using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Sessions;
using SVSim.BattleNode.Sessions.Participants;

namespace SVSim.UnitTests.BattleNode.Sessions;

/// <summary>
/// In PvP a <see cref="BattleSession"/> subscribes to BOTH participants' FrameEmitted, and each
/// RealParticipant raises it from its own WebSocket read loop — i.e. two threads. The dispatch path
/// (ComputeFrames + the relay PushAsync calls) mutates shared, non-thread-safe session state, so it
/// must be serialized per session. This drives the two participants' dispatch concurrently and asserts
/// no two dispatches ever overlap.
/// </summary>
[TestFixture]
public class BattleSessionDispatchConcurrencyTests
{
    [Test]
    public async Task Concurrent_dispatch_from_both_participants_is_serialized()
    {
        var detector = new ConcurrencyDetector();
        var a = new ProbeParticipant(1001, CtxA(), detector);
        var b = new ProbeParticipant(2002, CtxB(), detector);
        var s = new BattleSession("bid-conc", BattleType.Pvp, a, b, NullLogger<BattleSession>.Instance);

        // Reach AfterReady single-threaded (ComputeFrames returns routes but never calls PushAsync,
        // so the detector is untouched during setup).
        DriveToAfterReady(s, a);
        DriveToAfterReady(s, b);

        detector.Arm();

        // Fire a gameplay frame from each side at the same instant. A's TurnStart routes to B.PushAsync
        // and B's to A.PushAsync, so both dispatches run their PushAsync concurrently unless the session
        // serializes them.
        using var gate = new ManualResetEventSlim(false);
        var ta = Task.Run(async () => { gate.Wait(); await a.RaiseAsync(Env(NetworkBattleUri.TurnStart)); });
        var tb = Task.Run(async () => { gate.Wait(); await b.RaiseAsync(Env(NetworkBattleUri.TurnStart)); });
        gate.Set();
        await Task.WhenAll(ta, tb);

        Assert.That(detector.MaxConcurrent, Is.EqualTo(1),
            "Two read-loop threads dispatched into shared session state concurrently; " +
            "HandleFrameAsync must serialize dispatch per session.");
    }

    private static void DriveToAfterReady(BattleSession s, ProbeParticipant p)
    {
        s.ComputeFrames(p, Env(NetworkBattleUri.InitNetwork));
        s.ComputeFrames(p, Env(NetworkBattleUri.InitBattle));
        s.ComputeFrames(p, Env(NetworkBattleUri.Loaded));
        s.ComputeFrames(p, Env(NetworkBattleUri.Swap));
    }

    private static MsgEnvelope Env(NetworkBattleUri uri) =>
        new(uri, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0,
            Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null,
            Body: new RawBody(new Dictionary<string, object?>()));

    private static MatchContext CtxA() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(_ => 100_011_010L).ToList(),
        ClassId: CardClass.Runecraft, CharaId: "3", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "PlayerA", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0, BattleModeId: BattleModes.TakeTwo);

    private static MatchContext CtxB() => new(
        SelfDeckCardIds: Enumerable.Range(1, 30).Select(_ => 200_011_010L).ToList(),
        ClassId: CardClass.Shadowcraft, CharaId: "5", CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Japan, UserName: "PlayerB", SleeveId: "3000022",
        EmblemId: "701441022", DegreeId: "300004", FieldId: 44, IsOfficial: 0, BattleModeId: BattleModes.TakeTwo);

    /// <summary>Tracks the peak number of dispatches in flight at once. Records the count under a
    /// short lock, then holds (outside the lock) to widen the overlap window so a serialization bug
    /// is observed deterministically rather than relied on to interleave by chance.</summary>
    private sealed class ConcurrencyDetector
    {
        private const int WidenMs = 50;
        private readonly object _lock = new();
        private int _current;
        private volatile bool _armed;
        public int MaxConcurrent { get; private set; }

        public void Arm() => _armed = true;

        public async Task EnterAsync()
        {
            if (!_armed) return;
            lock (_lock)
            {
                _current++;
                if (_current > MaxConcurrent) MaxConcurrent = _current;
            }
            await Task.Delay(WidenMs);
            lock (_lock) { _current--; }
        }
    }

    private sealed class ProbeParticipant : IBattleParticipant, IHasHandshakePhase
    {
        private readonly ConcurrencyDetector _detector;
        public long ViewerId { get; }
        public MatchContext Context { get; }
        public HandshakePhase Phase { get; set; } = HandshakePhase.AwaitingInitNetwork;
        public event Func<MsgEnvelope, CancellationToken, Task>? FrameEmitted;

        public ProbeParticipant(long viewerId, MatchContext context, ConcurrencyDetector detector)
        {
            ViewerId = viewerId;
            Context = context;
            _detector = detector;
        }

        public Task RaiseAsync(MsgEnvelope env) =>
            FrameEmitted?.Invoke(env, CancellationToken.None) ?? Task.CompletedTask;

        public Task PushAsync(MsgEnvelope env, Stock stock, CancellationToken ct) => _detector.EnterAsync();
        public Task RunAsync(CancellationToken ct) => Task.CompletedTask;
        public Task TerminateAsync(BattleFinishReason reason) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
