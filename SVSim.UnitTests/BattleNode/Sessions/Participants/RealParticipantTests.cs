using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;
using SVSim.BattleNode.Sessions;
using SVSim.BattleNode.Sessions.Participants;
using SVSim.UnitTests.BattleNode.Infrastructure;

namespace SVSim.UnitTests.BattleNode.Sessions.Participants;

[TestFixture]
public class RealParticipantTests
{
    [Test]
    public void PushAsync_ordered_assigns_playSeq_via_OutboundSequencer()
    {
        var ws = new TestWebSocket();
        var p = new RealParticipant(ws, viewerId: 1, FixtureCtx(),
            NullLogger<RealParticipant>.Instance);

        // First ordered push gets playSeq = 1; second = 2; etc.
        // Inspect the participant's outbound sequencer state via its public Archive.
        var env = NewEnvelope(NetworkBattleUri.Matched);
        p.PushAsync(env, Stock.Normal, CancellationToken.None).Wait();
        p.PushAsync(env, Stock.Normal, CancellationToken.None).Wait();

        Assert.That(p.Outbound.Archive.Count, Is.EqualTo(2));
        Assert.That(p.Outbound.Archive[1].PlaySeq, Is.EqualTo(1));
        Assert.That(p.Outbound.Archive[2].PlaySeq, Is.EqualTo(2));
    }

    [Test]
    public void PushAsync_noStock_omits_playSeq()
    {
        var ws = new TestWebSocket();
        var p = new RealParticipant(ws, viewerId: 1, FixtureCtx(),
            NullLogger<RealParticipant>.Instance);

        p.PushAsync(NewEnvelope(NetworkBattleUri.BattleFinish), Stock.Bypass, CancellationToken.None).Wait();

        // No playSeq archive entry for no-stock pushes.
        Assert.That(p.Outbound.Archive.Count, Is.EqualTo(0));
    }

    [Test]
    public void ViewerId_and_Context_are_exposed()
    {
        var ws = new TestWebSocket();
        var ctx = FixtureCtx();
        var p = new RealParticipant(ws, viewerId: 906243102L, ctx,
            NullLogger<RealParticipant>.Instance);

        Assert.That(p.ViewerId, Is.EqualTo(906243102L));
        Assert.That(p.Context, Is.SameAs(ctx));
    }

    [Test]
    public void ClipAckArg_InRange_ReturnsArgUnchanged()
    {
        var result = RealParticipant.ClipAckArg(42L, NullLogger<RealParticipant>.Instance, viewerId: 1);
        Assert.That(result, Is.EqualTo(42));
    }

    [Test]
    public void ClipAckArg_AboveIntMax_ClipsToIntMaxValue()
    {
        var result = RealParticipant.ClipAckArg((long)int.MaxValue + 1L, NullLogger<RealParticipant>.Instance, viewerId: 1);
        Assert.That(result, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void ClipAckArg_BelowIntMin_ClipsToIntMinValue()
    {
        var result = RealParticipant.ClipAckArg((long)int.MinValue - 1L, NullLogger<RealParticipant>.Instance, viewerId: 1);
        Assert.That(result, Is.EqualTo(int.MinValue));
    }

    [Test]
    public void ClipAckArg_AtIntMaxBoundary_ReturnsIntMaxValue()
    {
        var result = RealParticipant.ClipAckArg((long)int.MaxValue, NullLogger<RealParticipant>.Instance, viewerId: 1);
        Assert.That(result, Is.EqualTo(int.MaxValue));
    }

    [Test]
    public void ClipAckArg_AtIntMinBoundary_ReturnsIntMinValue()
    {
        var result = RealParticipant.ClipAckArg((long)int.MinValue, NullLogger<RealParticipant>.Instance, viewerId: 1);
        Assert.That(result, Is.EqualTo(int.MinValue));
    }

    [Test]
    public void Phase_defaults_to_AwaitingInitNetwork()
    {
        var ws = new TestWebSocket();
        var p = new RealParticipant(ws, viewerId: 1, FixtureCtx(),
            NullLogger<RealParticipant>.Instance);

        Assert.That(p.Phase, Is.EqualTo(SVSim.BattleNode.Sessions.HandshakePhase.AwaitingInitNetwork));
    }

    [Test]
    public void Phase_setter_is_visible_to_same_assembly()
    {
        var ws = new TestWebSocket();
        var p = new RealParticipant(ws, viewerId: 1, FixtureCtx(),
            NullLogger<RealParticipant>.Instance);

        // Setter is `internal`; SVSim.UnitTests has InternalsVisibleTo on SVSim.BattleNode.
        p.Phase = SVSim.BattleNode.Sessions.HandshakePhase.AfterReady;

        Assert.That(p.Phase, Is.EqualTo(SVSim.BattleNode.Sessions.HandshakePhase.AfterReady));
    }

    [Test]
    public void Phase_is_per_instance_not_shared()
    {
        var wsA = new TestWebSocket();
        var wsB = new TestWebSocket();
        var a = new RealParticipant(wsA, viewerId: 1, FixtureCtx(), NullLogger<RealParticipant>.Instance);
        var b = new RealParticipant(wsB, viewerId: 2, FixtureCtx(), NullLogger<RealParticipant>.Instance);

        a.Phase = SVSim.BattleNode.Sessions.HandshakePhase.AfterReady;

        Assert.That(b.Phase, Is.EqualTo(SVSim.BattleNode.Sessions.HandshakePhase.AwaitingInitNetwork),
            "B's Phase must not change when A's Phase is set.");
    }

    [Test]
    public async Task AwaitSessionFinishedAsync_returns_when_MarkSessionFinished_fires()
    {
        var ws = new TestWebSocket();
        var p = new RealParticipant(ws, viewerId: 1, FixtureCtx(),
            NullLogger<RealParticipant>.Instance);

        var awaiter = p.AwaitSessionFinishedAsync(CancellationToken.None);
        p.MarkSessionFinished();

        await awaiter; // should complete promptly
        Assert.Pass();
    }

    [Test]
    public void AwaitSessionFinishedAsync_cancels_on_token()
    {
        var ws = new TestWebSocket();
        var p = new RealParticipant(ws, viewerId: 1, FixtureCtx(),
            NullLogger<RealParticipant>.Instance);

        using var cts = new CancellationTokenSource();
        var awaiter = p.AwaitSessionFinishedAsync(cts.Token);
        cts.Cancel();

        Assert.That(async () => await awaiter, Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public async Task MarkSessionFinished_is_idempotent()
    {
        var ws = new TestWebSocket();
        var p = new RealParticipant(ws, viewerId: 1, FixtureCtx(),
            NullLogger<RealParticipant>.Instance);

        p.MarkSessionFinished();
        p.MarkSessionFinished(); // should not throw

        await p.AwaitSessionFinishedAsync(CancellationToken.None);
        Assert.Pass();
    }

    private static MatchContext FixtureCtx() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(_ => 100_011_010L).ToList(),
        ClassId: CardClass.Forestcraft, CharaId: "1", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "Player", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);

    private static MsgEnvelope NewEnvelope(NetworkBattleUri uri) =>
        new(uri, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0,
            Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null,
            Body: new ResultCodeOnlyBody());
}
