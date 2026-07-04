using NUnit.Framework;
using SVSim.BattleNode.Wire;

namespace SVSim.UnitTests.BattleNode.Wire;

[TestFixture]
public class EngineIoFrameTests
{
    [TestCase("0{\"sid\":\"abc\"}", EngineIoPacketType.Open, "{\"sid\":\"abc\"}")]
    [TestCase("2", EngineIoPacketType.Ping, "")]
    [TestCase("3", EngineIoPacketType.Pong, "")]
    [TestCase("4hello", EngineIoPacketType.Message, "hello")]
    public void Parse_RecognizesTypeAndPayload(string raw, EngineIoPacketType expectedType, string expectedPayload)
    {
        var frame = EngineIoFrame.Parse(raw);

        Assert.That(frame.Type, Is.EqualTo(expectedType));
        Assert.That(frame.Payload, Is.EqualTo(expectedPayload));
    }

    [TestCase(EngineIoPacketType.Open, "{\"sid\":\"abc\"}", "0{\"sid\":\"abc\"}")]
    [TestCase(EngineIoPacketType.Pong, "", "3")]
    [TestCase(EngineIoPacketType.Message, "2[\"msg\",{}]", "42[\"msg\",{}]")]
    public void Encode_EmitsTypeDigitFollowedByPayload(EngineIoPacketType type, string payload, string expected)
    {
        var frame = new EngineIoFrame(type, payload);

        Assert.That(frame.Encode(), Is.EqualTo(expected));
    }

    [Test]
    public void Parse_EmptyInput_Throws()
    {
        Assert.Throws<ArgumentException>(() => EngineIoFrame.Parse(""));
    }

    [Test]
    public void Parse_NonDigitFirstChar_Throws()
    {
        Assert.Throws<ArgumentException>(() => EngineIoFrame.Parse("xfoo"));
    }

    [Test]
    public void EngineIoHandshake_SerializesWithCamelCaseKeys()
    {
        var hs = new EngineIoHandshake("abc123", Array.Empty<string>(), 25000, 60000);

        Assert.That(hs.ToJson(),
            Is.EqualTo("{\"sid\":\"abc123\",\"upgrades\":[],\"pingInterval\":25000,\"pingTimeout\":60000}"));
    }
}
