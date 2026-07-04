using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;
using SVSim.BattleNode.Sessions;
using SVSim.BattleNode.Sessions.Participants;
using SVSim.UnitTests.BattleNode.Infrastructure;

namespace SVSim.UnitTests.BattleNode.Sessions;

/// <summary>
/// Audit Md11 — confirms <see cref="BattleSession.RunAsync"/> drops the per-RealParticipant
/// <see cref="SVSim.BattleNode.Reliability.OutboundSequencer"/> archive when the session
/// terminates. The NoOp bot has no outbound archive of its own, so the test uses a Bot
/// session (one Real, one NoOpBot) and asserts only the Real side's archive is cleared.
/// </summary>
[TestFixture]
public class BattleSessionTerminateCascadeTests
{
    [Test, Timeout(10_000)]
    public async Task RunAsync_clears_real_participant_archive_on_terminate()
    {
        var ws = new TestWebSocket();
        var real = new RealParticipant(
            ws, viewerId: 1, MakeFakeContext(), NullLogger<RealParticipant>.Instance);
        var bot = new NoOpBotParticipant();

        // Pre-load the archive so we can prove it was cleared (not just empty).
        real.Outbound.AssignAndArchive(MakeEnvelope(NetworkBattleUri.Matched));
        real.Outbound.AssignAndArchive(MakeEnvelope(NetworkBattleUri.BattleStart));
        Assume.That(real.Outbound.Archive.Count, Is.EqualTo(2), "Precondition: archive populated.");

        var session = new BattleSession(
            battleId: "test-bid", type: BattleType.Bot,
            a: real, b: bot, log: NullLogger<BattleSession>.Instance);

        // Drive RunAsync to completion: closing the incoming side causes
        // RealParticipant's read loop to return Close → RunAsync exits → terminate
        // cascade fires.
        var runTask = session.RunAsync(CancellationToken.None);
        ws.CompleteIncoming();
        await runTask;

        Assert.That(real.Outbound.Archive, Is.Empty,
            "RealParticipant's outbound archive must be cleared by the terminate cascade.");
    }

    private static MsgEnvelope MakeEnvelope(NetworkBattleUri uri) =>
        new(uri, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0, Cat: EmitCategory.Battle,
            PubSeq: null, PlaySeq: null, Body: new RawBody(new Dictionary<string, object?>()));

    private static MatchContext MakeFakeContext() => new(
        SelfDeckCardIds: Array.Empty<long>(),
        ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "card_master_node_10015",
        CountryCode: "JP", UserName: "Test", SleeveId: "0",
        EmblemId: "0", DegreeId: "0", FieldId: 0, IsOfficial: 0, BattleModeId: BattleModes.TakeTwo);
}
