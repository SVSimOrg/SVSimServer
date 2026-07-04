using System.Net.WebSockets;
using System.Text;
using MessagePack;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Sessions;
using SVSim.BattleNode.Sessions.Participants;
using SVSim.BattleNode.Wire;
using SVSim.UnitTests.BattleNode.Infrastructure;

namespace SVSim.UnitTests.BattleNode.Sessions.Participants;

/// <summary>
/// Regression tests for the <c>"hand"</c> SIO event handler. The wire shape verified at
/// <c>RealTimeNetworkAgent.CreatePackEmitHandData:815-817</c>:
/// <code>
///   return MessagePackSerializer.Serialize(JsonMapper.ToJson(info));  // info = List&lt;object&gt;, NOT encrypted
/// </code>
/// is the source of truth this test must match. (An earlier version of this test
/// wrapped the body in an encrypted dict shape — that was wrong and shipped a handler
/// that softlocked in prod despite passing the test. See
/// <c>docs/audits/battle-node-sio-events-2026-06-02.md</c>.)
/// </summary>
[TestFixture]
public class RealParticipantHandEventTests
{
    [Test]
    public async Task Stocked_hand_event_acks_with_array_index3_pubSeq()
    {
        // Prod wire shape per EmitMsgUriPack:1454-1458 — Insert(3, num) puts pubSeq at index 3:
        //   [uri_int, viewerId, udid, pubSeq, ...select-skill params]
        const long expectedPubSeq = 42L;
        var bodyJson = $"[2,906243102,\"d08367be-1152-4009-aaaf-2d47d1d9112c\",{expectedPubSeq},1,false,false]";

        var ws = new TestWebSocket();
        var p = new RealParticipant(ws, viewerId: 906_243_102L, FixtureCtx(),
            NullLogger<RealParticipant>.Instance);
        EnqueueHandFrame(ws, ackId: 26, bodyJson: bodyJson);
        ws.CompleteIncoming();

        await p.RunAsync(CancellationToken.None);

        var ackFrame = FindAckFrame(ws, ackId: 26);
        Assert.That(ackFrame, Is.Not.Null,
            $"Expected an SIO Ack frame for ackId=26 in outbound sends; got: [{string.Join(", ", AllTextSends(ws))}]");
        Assert.That(ackFrame, Does.Contain($"[{expectedPubSeq}]"),
            "Ack arg must echo the body's pubSeq (array index 3) so client's stockEmitMessageMgr.GetSelectData succeeds.");
    }

    [Test]
    public async Task Stocked_hand_event_with_dict_root_acks_with_top_level_pubSeq()
    {
        // Defensive: not what the client sends today, but the StockHandData dict shape
        // exists client-side and could surface on the wire with a future format change.
        const long expectedPubSeq = 17L;
        var bodyJson = $"{{\"StockHandData\":[2,1,\"u\",{expectedPubSeq}],\"try\":0,\"pubSeq\":{expectedPubSeq}}}";

        var ws = new TestWebSocket();
        var p = new RealParticipant(ws, viewerId: 1, FixtureCtx(),
            NullLogger<RealParticipant>.Instance);
        EnqueueHandFrame(ws, ackId: 33, bodyJson: bodyJson);
        ws.CompleteIncoming();

        await p.RunAsync(CancellationToken.None);

        var ackFrame = FindAckFrame(ws, ackId: 33);
        Assert.That(ackFrame, Is.Not.Null);
        Assert.That(ackFrame, Does.Contain($"[{expectedPubSeq}]"));
    }

    [Test]
    public async Task Hand_event_without_ackId_is_swallowed_silently_no_ack_sent()
    {
        // Fire-and-forget hand emits (TOUCH_URI, SELECT_OBJECT_URI, TURN_END_READY_URI) arrive
        // without an ack-id and don't block the client's emit queue. We should swallow them
        // without decoding or acking.
        var ws = new TestWebSocket();
        var p = new RealParticipant(ws, viewerId: 1, FixtureCtx(),
            NullLogger<RealParticipant>.Instance);

        EnqueueHandFrame(ws, ackId: null, bodyJson: "[1,1,\"u\",0,0]");
        ws.CompleteIncoming();

        await p.RunAsync(CancellationToken.None);

        var ackFrames = AllTextSends(ws).Where(s => s.StartsWith("43")).ToList();
        Assert.That(ackFrames, Is.Empty,
            $"No-ack-id hand frame must not produce an Ack; got: [{string.Join(", ", ackFrames)}]");
    }

    [Test]
    public async Task Hand_event_with_unparseable_pubSeq_position_falls_back_to_ack_arg_0()
    {
        // If a stocked hand frame ever arrives with an array shorter than 4 elements (or a
        // non-numeric index 3), we still ack so the client doesn't softlock — but with
        // arg=0. The client's GetSelectData lookup misses and OnAck fires with null
        // selectData, which is a normal cache-miss path (not a deadlock).
        var bodyJson = "[2,1,\"u\"]"; // length 3, no index-3 pubSeq

        var ws = new TestWebSocket();
        var p = new RealParticipant(ws, viewerId: 1, FixtureCtx(),
            NullLogger<RealParticipant>.Instance);
        EnqueueHandFrame(ws, ackId: 99, bodyJson: bodyJson);
        ws.CompleteIncoming();

        await p.RunAsync(CancellationToken.None);

        var ackFrame = FindAckFrame(ws, ackId: 99);
        Assert.That(ackFrame, Is.Not.Null,
            "Malformed hand body should still ack (arg=0), not silently swallow.");
        Assert.That(ackFrame, Does.Contain("[0]"),
            "Fallback ack arg should be 0.");
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Enqueue an SIO BinaryEvent("hand", {placeholder}) text frame followed by its single
    /// binary attachment (msgpack-string of the raw JSON, <b>not</b> encrypted —
    /// CreatePackEmitHandData:815-817 does not call CryptAES.encryptForNode).
    /// </summary>
    private static void EnqueueHandFrame(TestWebSocket ws, int? ackId, string bodyJson)
    {
        var ackPart = ackId.HasValue ? ackId.Value.ToString() : "";
        var text = $"451-{ackPart}[\"hand\",{{\"_placeholder\":true,\"num\":0}}]";
        ws.EnqueueIncoming(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text);

        // Binary attachment: EIO Message prefix (0x04) + msgpack-string(bodyJson).
        var msgpackBytes = MessagePackSerializer.Serialize(bodyJson);
        var prefixed = new byte[msgpackBytes.Length + 1];
        prefixed[0] = (byte)EngineIoPacketType.Message;
        Buffer.BlockCopy(msgpackBytes, 0, prefixed, 1, msgpackBytes.Length);
        ws.EnqueueIncoming(prefixed, WebSocketMessageType.Binary);
    }

    private static IEnumerable<string> AllTextSends(TestWebSocket ws) =>
        ws.Sends
            .Where(f => f.Type == WebSocketMessageType.Text)
            .Select(f => Encoding.UTF8.GetString(f.Payload));

    private static string? FindAckFrame(TestWebSocket ws, int ackId) =>
        AllTextSends(ws).FirstOrDefault(s => s.StartsWith($"43{ackId}["));

    private static MatchContext FixtureCtx() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(_ => 100_011_010L).ToList(),
        ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "Player", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);
}
