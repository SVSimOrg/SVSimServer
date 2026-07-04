using NUnit.Framework;
using SVSim.BattleNode.Wire;

namespace SVSim.UnitTests.BattleNode.Wire;

[TestFixture]
public class NodeCryptoTests
{
    [Test]
    public void EncryptThenDecrypt_RoundTripsArbitraryString()
    {
        const string plaintext = "{\"uri\":\"InitNetwork\",\"viewerId\":906243102,\"try\":0,\"cat\":99}";
        const string key = "Y2FmZWJhYmU3ZmY3ZmY3ZmY3ZmY3ZmY3"; // 32 chars

        var encrypted = NodeCrypto.EncryptForNode(plaintext, key);
        var decrypted = NodeCrypto.DecryptForNode(encrypted);

        Assert.That(decrypted, Is.EqualTo(plaintext));
    }

    [Test]
    public void GenerateKey_WithDeterministicSource_ProducesStable32CharOutput()
    {
        var seq = 0;
        string key = NodeCrypto.GenerateKey(() => seq++ % 16);

        Assert.That(key.Length, Is.EqualTo(32));
        // Two calls with the same source produce the same output.
        seq = 0;
        Assert.That(NodeCrypto.GenerateKey(() => seq++ % 16), Is.EqualTo(key));
    }

    [Test]
    public void EncryptForNode_NonStandardKeyLength_Throws()
    {
        Assert.Throws<ArgumentException>(() => NodeCrypto.EncryptForNode("x", "tooshort"));
    }

    [Test]
    public void DecryptForNode_TooShortInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => NodeCrypto.DecryptForNode("tooshort"));
    }

    [Test]
    public void GenerateKey_RandSourceReturnsOutOfRange_MasksToLowFourBits()
    {
        // Defensive: misbehaving caller returns 31 (binary 11111). Internal contract is
        // "each call produces one hex digit"; without masking, 31 widens to "1f" (two
        // chars) which throws off the base64 length math. After masking with & 0xF,
        // 31 becomes 15 — one hex digit "f". This pair distinguishes because
        // base64-of-repeated-"1f" and base64-of-repeated-"f" differ at every position.
        var keyFromThirtyOne = NodeCrypto.GenerateKey(() => 31);
        var keyFromFifteen = NodeCrypto.GenerateKey(() => 15);

        Assert.That(keyFromThirtyOne, Is.EqualTo(keyFromFifteen),
            "31 & 0xF == 15 — GenerateKey must mask out-of-range bits, not let them widen the hex digit.");
    }

    [Test]
    public void EncryptForNode_FixedVector_ProducesStableOutput()
    {
        // Pinned wire-format regression: any change to encoding/padding/IV derivation
        // that drifts in both directions would still pass the roundtrip test but break
        // this hardcoded vector — and break interop with the real client.
        const string plaintext = "hello, node!";
        const string key = "01234567890123456789012345678901";
        const string expected = "012345678901234567890123456789015mEezM5MgR7UUEkmx5OzPQ==";

        Assert.That(NodeCrypto.EncryptForNode(plaintext, key), Is.EqualTo(expected));
        Assert.That(NodeCrypto.DecryptForNode(expected), Is.EqualTo(plaintext));
    }
}
