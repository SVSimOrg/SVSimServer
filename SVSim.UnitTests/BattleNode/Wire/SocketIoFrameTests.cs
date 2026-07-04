using System.Text.Json;
using NUnit.Framework;
using SVSim.BattleNode.Wire;

namespace SVSim.UnitTests.BattleNode.Wire;

[TestFixture]
public class SocketIoFrameTests
{
    [Test]
    public void Parse_ConnectPacket_HasNoPayload()
    {
        var frame = SocketIoFrame.Parse("0");
        Assert.That(frame.Type, Is.EqualTo(SocketIoPacketType.Connect));
        Assert.That(frame.AckId, Is.Null);
        Assert.That(frame.AttachmentCount, Is.EqualTo(0));
    }

    [Test]
    public void Parse_EventWithAck_ExtractsAckIdAndArgs()
    {
        var frame = SocketIoFrame.Parse("27[\"msg\",42]");
        Assert.That(frame.Type, Is.EqualTo(SocketIoPacketType.Event));
        Assert.That(frame.AckId, Is.EqualTo(7));
        Assert.That(frame.EventName, Is.EqualTo("msg"));
        Assert.That(frame.RawArgs[0].GetInt32(), Is.EqualTo(42));
    }

    [Test]
    public void Parse_BinaryEvent_RecordsAttachmentCount_WithAttachments_Assembles()
    {
        var attachment = new byte[] { 0x01, 0x02, 0x03 };
        var header = SocketIoFrame.Parse("51-[\"msg\",{\"_placeholder\":true,\"num\":0}]");
        var assembled = header.WithAttachments(new[] { attachment });

        Assert.That(assembled.Type, Is.EqualTo(SocketIoPacketType.BinaryEvent));
        Assert.That(assembled.AckId, Is.Null);
        Assert.That(assembled.AttachmentCount, Is.EqualTo(1));
        Assert.That(assembled.BinaryAttachments[0], Is.EqualTo(attachment));
        Assert.That(assembled.EventName, Is.EqualTo("msg"));
    }

    [Test]
    public void Parse_BinaryEventWithAckId_ExtractsBoth()
    {
        var frame = SocketIoFrame.Parse("51-3[\"msg\",{\"_placeholder\":true,\"num\":0}]");
        Assert.That(frame.Type, Is.EqualTo(SocketIoPacketType.BinaryEvent));
        Assert.That(frame.AckId, Is.EqualTo(3));
        Assert.That(frame.AttachmentCount, Is.EqualTo(1));
    }

    [Test]
    public void Parse_AckResponse_ExtractsIdAndIntArg()
    {
        var frame = SocketIoFrame.Parse("37[123]");
        Assert.That(frame.Type, Is.EqualTo(SocketIoPacketType.Ack));
        Assert.That(frame.AckId, Is.EqualTo(7));
        Assert.That(frame.RawArgs[0].GetInt32(), Is.EqualTo(123));
    }

    [Test]
    public void Encode_BinaryEventWithAttachment_EmitsCountDashAndPlaceholder()
    {
        var attachment = new byte[] { 0xff };
        var frame = SocketIoFrame.BinaryEventWithAttachments("synchronize", new[] { attachment });

        var (text, bins) = frame.Encode();
        Assert.That(text, Is.EqualTo("51-[\"synchronize\",{\"_placeholder\":true,\"num\":0}]"));
        Assert.That(bins.Single(), Is.EqualTo(attachment));
    }

    [Test]
    public void Encode_AckResponse_IsTypeIdAndArrayOfArgs()
    {
        var frame = SocketIoFrame.AckResponse(ackId: 7, pubSeqEcho: 123);
        var (text, bins) = frame.Encode();

        Assert.That(text, Is.EqualTo("37[123]"));
        Assert.That(bins, Is.Empty);
    }

    [Test]
    public void WithAttachments_CountMismatch_Throws()
    {
        var header = SocketIoFrame.Parse("51-[\"msg\",{\"_placeholder\":true,\"num\":0}]");
        Assert.Throws<ArgumentException>(() => header.WithAttachments(Array.Empty<byte[]>()));
    }

    [Test]
    public void Encode_ConnectPacket_HasNoBracketedArgs()
    {
        // Regression for an earlier bug where Encode always emitted "[]".
        var frame = SocketIoFrame.Parse("0");
        var (text, bins) = frame.Encode();
        Assert.That(text, Is.EqualTo("0"));
        Assert.That(bins, Is.Empty);
    }

    [Test]
    public void ParseThenEncode_EventWithAck_RoundTripsByteForByte()
    {
        const string wire = "27[\"msg\",42]";
        var frame = SocketIoFrame.Parse(wire);
        var (text, _) = frame.Encode();
        Assert.That(text, Is.EqualTo(wire));
    }

    [Test]
    public void Parse_NamespacePrefix_Throws()
    {
        // v1 only supports the default namespace. A "/foo," prefix used to be silently
        // skipped, which would route a frame meant for namespace /foo to the default
        // handler. Fail loud instead so we'd notice if the client ever started using one.
        var ex = Assert.Throws<ArgumentException>(() => SocketIoFrame.Parse("2/foo,[\"msg\"]"));
        Assert.That(ex!.Message, Does.Contain("/foo"));
    }

    [Test]
    public void Encode_EventNameWithSpecialChars_IsJsonEscaped()
    {
        var frame = SocketIoFrame.BinaryEventWithAttachments(
            eventName: "weird \"name\" with \\ backslash",
            attachments: new[] { new byte[] { 0x00 } });
        var (text, _) = frame.Encode();
        // The event name must be JSON-escaped: each " becomes \", and the literal \ becomes \\.
        Assert.That(text, Does.Contain("\"weird \\\"name\\\" with \\\\ backslash\""));
    }

    [Test]
    public void Parse_InvalidTypeChar_Throws()
    {
        var ex = Assert.Throws<ArgumentException>(() => SocketIoFrame.Parse("9[\"msg\"]"));
        Assert.That(ex!.Message, Does.Contain("Invalid SIO type char"));
    }

    [Test]
    public void Parse_OverflowingAckId_Throws()
    {
        Assert.Throws<ArgumentException>(() => SocketIoFrame.Parse("2999999999999[\"msg\"]"));
    }
}
