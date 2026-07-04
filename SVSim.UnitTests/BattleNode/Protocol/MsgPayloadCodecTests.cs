using NUnit.Framework;
using SVSim.BattleNode.Protocol;

namespace SVSim.UnitTests.BattleNode.Protocol;

[TestFixture]
public class MsgPayloadCodecTests
{
    private static string FreshKey()
    {
        var seq = 0;
        return SVSim.BattleNode.Wire.NodeCrypto.GenerateKey(() => (seq++ * 7) % 16);
    }

    [Test]
    public void Roundtrip_PreservesEnvelope()
    {
        var env = new MsgEnvelope(
            Uri: NetworkBattleUri.Loaded,
            ViewerId: 906243102,
            Uuid: "udid",
            Bid: "1234",
            RetryAttempt: 0,
            Cat: EmitCategory.Battle,
            PubSeq: 3,
            PlaySeq: null,
            Body: new RawBody(new Dictionary<string, object?>()));

        var bytes = MsgPayloadCodec.Encode(env, key: FreshKey());
        var back = MsgPayloadCodec.Decode(bytes);

        Assert.That(back.Uri, Is.EqualTo(NetworkBattleUri.Loaded));
        Assert.That(back.PubSeq, Is.EqualTo(3));
        Assert.That(back.Bid, Is.EqualTo("1234"));
    }

    [Test]
    public void Decode_KnownEnvelope_ReturnsExpectedUriAndBody()
    {
        // The captures only contain decoded JSON, so we build the encrypted-msgpack representation
        // ourselves with the same JSON and a known key — this confirms the full chain end-to-end.
        var key = FreshKey();
        var originalJson = "{\"uri\":\"InitNetwork\",\"viewerId\":1,\"uuid\":\"u\",\"try\":0,\"cat\":99,\"resultCode\":1}";
        var encrypted = SVSim.BattleNode.Wire.NodeCrypto.EncryptForNode(originalJson, key);
        var bytes = MessagePack.MessagePackSerializer.Serialize(encrypted);

        var env = MsgPayloadCodec.Decode(bytes);

        Assert.That(env.Uri, Is.EqualTo(NetworkBattleUri.InitNetwork));
        Assert.That(env.Cat, Is.EqualTo(EmitCategory.General));
        Assert.That(((RawBody)env.Body).Entries["resultCode"], Is.EqualTo(1L));
    }
}
