using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Sessions.Dispatch;
using SVSim.BattleNode.Sessions.Dispatch.Handlers;
using SVSim.BattleNode.Sessions.Participants;

namespace SVSim.BattleNode.Sessions;

/// <summary>
/// v2 broker session. Holds two participants and brokers between them. Subscribes
/// to each participant's <see cref="IBattleParticipant.FrameEmitted"/>; on each frame,
/// runs <see cref="ComputeFrames"/> to determine the routing (target + frame + <see cref="Stock"/>
/// flag) and dispatches via <see cref="IBattleParticipant.PushAsync"/>.
/// </summary>
/// <remarks>
/// Wires both battle modes: Pvp (broadcast Matched/BattleStart per-perspective, forward
/// gameplay frames between the two real participants) and Bot (ack-only, NoOp opponent).
/// </remarks>
public sealed class BattleSession
{
    private readonly ILogger<BattleSession> _log;

    private readonly BattleSessionState _state = new();

    /// <summary>One authoritative shadow engine per session (design ND2). Fed both clients' frames in
    /// pure shadow (N1): it tracks state but emits nothing and changes no route. N2+ flips outbound
    /// fields to engine reads. Constructed unconditionally; <see cref="EnsureEngineSetup"/> seats it
    /// once both decks are known, and every interaction is guarded so a shadow failure can never break
    /// live dispatch (ND6: log, never throw into the relay).</summary>
    private readonly Engine.SessionBattleEngine _engine = new();

    /// <summary>Setup is attempted exactly once. A shadow engine that can't seat headless in this host
    /// (e.g. engine global state not initialized) stays not-ready and the shadow silently no-ops —
    /// never retried, never fatal.</summary>
    private bool _engineSetupAttempted;

    /// <summary>Guards: server-generated Deal is fed to the shadow engine exactly once (the first
    /// occurrence from either LoadedHandler invocation). Deal + Ready are server-generated frames the
    /// engine needs to drive the mulligan: Deal → StartDeal (cards deck→hand for the player seat,
    /// _firstDrawList for the opponent), Ready → CompleteMulligan → EnemyChangeCardVfx → opponent
    /// DrawFirstMulliganCard. Without them the engine's hand stays empty and every play throws
    /// "Target card was not found in hand cards".</summary>
    private bool _engineDealFed;

    /// <summary>Guards: server-generated Ready is fed to the shadow engine exactly once (the first
    /// Ready addressed to participant A). Fed as isPlayerSeat=false so the recovery path's
    /// OperateMulligan enters the OperateOppoMulligan branch — the only branch that invokes
    /// ReceiveOpponentMulligan → EnemyChangeCardVfx → DrawFirstMulliganCard. The player's mulligan
    /// was already processed during the Swap feed.</summary>
    private bool _engineReadyFed;

    /// <summary>Serializes dispatch. Both participants' read loops raise FrameEmitted on their own
    /// threads, and a dispatch (<see cref="ComputeFrames"/> + the relay <c>PushAsync</c> calls) mutates
    /// shared, non-thread-safe state — the <see cref="BattleSessionState"/> dictionaries and each
    /// participant's <c>OutboundSequencer</c>. This gate funnels both threads through one critical
    /// section so concurrent frames can't corrupt that state.</summary>
    private readonly SemaphoreSlim _dispatchGate = new(1, 1);

    /// <summary>The per-battle master seed (see <see cref="BattleSessionState.MasterSeed"/>).
    /// Exposed for logging + future replay persistence.</summary>
    public int MasterSeed => _state.MasterSeed;

    public string BattleId { get; }
    public BattleType Type { get; }
    public IBattleParticipant A { get; }
    public IBattleParticipant B { get; }
    public SessionLifecycle Lifecycle => _state.Lifecycle;

    // Per-URI dispatch table. All 14 inbound URIs are registered (Tasks 5-14); unknown
    // URIs are dropped with a LogDebug in ComputeFrames.
    private static readonly IReadOnlyDictionary<NetworkBattleUri, IFrameHandler> Handlers = BuildHandlers();

    private static IReadOnlyDictionary<NetworkBattleUri, IFrameHandler> BuildHandlers()
    {
        var retireKill = new RetireKillHandler();
        var forwardWhenReady = new ForwardWhenBothReadyHandler();
        return new Dictionary<NetworkBattleUri, IFrameHandler>
        {
            [NetworkBattleUri.InitNetwork] = new InitNetworkHandler(),
            [NetworkBattleUri.InitBattle] = new InitBattleHandler(),
            [NetworkBattleUri.Loaded] = new LoadedHandler(),
            [NetworkBattleUri.Swap] = new SwapHandler(),
            [NetworkBattleUri.TurnEnd] = new TurnEndHandler(),
            [NetworkBattleUri.TurnEndFinal] = new TurnEndFinalHandler(),
            [NetworkBattleUri.Retire] = retireKill,
            [NetworkBattleUri.Kill] = retireKill,
            [NetworkBattleUri.TurnStart] = new TurnStartHandler(),
            [NetworkBattleUri.Judge] = new JudgeHandler(),
            [NetworkBattleUri.PlayActions] = new PlayActionsHandler(),
            [NetworkBattleUri.Echo] = new EchoHandler(),
            [NetworkBattleUri.TurnEndActions] = new TurnEndActionsHandler(),
            [NetworkBattleUri.JudgeResult] = forwardWhenReady,
        };
    }

    private FrameDispatchContext BuildContext(IBattleParticipant from, MsgEnvelope env) =>
        new()
        {
            A = A, B = B, From = from, Other = ReferenceEquals(from, A) ? B : A,
            Env = env, BattleId = BattleId, State = _state, Engine = _engine,
        };

    public BattleSession(string battleId, BattleType type, IBattleParticipant a, IBattleParticipant b,
        ILogger<BattleSession> log)
    {
        BattleId = battleId;
        Type = type;
        A = a;
        B = b;
        _log = log;

        _log.LogInformation("BattleSession {Bid}: master seed {Seed}", BattleId, _state.MasterSeed);

        // Subscribe to both participants' emissions.
        A.FrameEmitted += OnFrameFromA;
        B.FrameEmitted += OnFrameFromB;
    }

    public async Task RunAsync(CancellationToken cancellation)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        var aTask = A.RunAsync(cts.Token);
        var bTask = B.RunAsync(cts.Token);

        if (Type == BattleType.Pvp)
        {
            // WhenAny: first WS drop / first graceful close triggers cascade. Pvp has two
            // RealParticipants; we synthesize a BattleFinish for the survivor if either side
            // terminates first.
            var first = await Task.WhenAny(aTask, bTask).ConfigureAwait(false);
            var survivor = first == aTask ? B : A;

            if (Lifecycle != SessionLifecycle.Terminal)
            {
                // Involuntary drop (no graceful Retire): synthesize BattleFinish(DisconnectWin)
                // to survivor. DisconnectWin=201 → client renders "opponent disconnected" →
                // WIN UI; the legacy Win=1 used here previously rendered "no contest".
                try
                {
                    await survivor.PushAsync(
                        BattleFrames.BuildBattleFinish(BattleResult.DisconnectWin), Stock.Bypass, cancellation)
                        .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _log.LogWarning(ex,
                        "BattleSession {Bid}: failed to push BattleFinish to survivor (their WS may also be closed)",
                        BattleId);
                }
                _state.Lifecycle = SessionLifecycle.Terminal;
            }

            cts.Cancel(); // unblock the survivor's RunAsync read loop
            try { await Task.WhenAll(aTask, bTask).ConfigureAwait(false); }
            catch (Exception ex) when (ex is OperationCanceledException or WebSocketException) { }
            catch (AggregateException ex) when (ex.Flatten().InnerExceptions.All(
                e => e is OperationCanceledException or WebSocketException)) { }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "BattleSession {Bid}: unexpected exception from WhenAll (PvP drain)", BattleId);
            }
        }
        else
        {
            // Bot mode: the NoOp opponent's RunAsync returns immediately; wait for the real
            // participant. The session keeps running for the real one.
            try { await Task.WhenAll(aTask, bTask).ConfigureAwait(false); }
            catch (Exception ex) when (ex is OperationCanceledException or WebSocketException) { }
            catch (AggregateException ex) when (ex.Flatten().InnerExceptions.All(
                e => e is OperationCanceledException or WebSocketException)) { }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "BattleSession {Bid}: unexpected exception from WhenAll (Bot drain)", BattleId);
            }
        }

        // Unsubscribe event handlers so the session + state aren't pinned by live delegates.
        A.FrameEmitted -= OnFrameFromA;
        B.FrameEmitted -= OnFrameFromB;

        // Release per-participant outbound archives at battle-end
        // (only RealParticipant has one; bots don't archive).
        if (A is RealParticipant rpA) rpA.Outbound.Clear();
        if (B is RealParticipant rpB) rpB.Outbound.Clear();

        // Per-session mgr instance (Phase-5 ambient rip, chunk 47) isolates per-battle state across
        // concurrent sessions, so the historical single-active-engine gate (and its matching
        // try/finally Release) is gone — engine setup is unconditional per session, and there is no
        // teardown obligation that must run on a throw from the participant tear-down.
        await Task.WhenAll(
            A.TerminateAsync(BattleFinishReason.NormalFinish),
            B.TerminateAsync(BattleFinishReason.NormalFinish))
            .ConfigureAwait(false);

        await A.DisposeAsync().ConfigureAwait(false);
        await B.DisposeAsync().ConfigureAwait(false);
        _dispatchGate.Dispose();
    }

    private Task OnFrameFromA(MsgEnvelope env, CancellationToken ct) => HandleFrameAsync(A, env, ct);
    private Task OnFrameFromB(MsgEnvelope env, CancellationToken ct) => HandleFrameAsync(B, env, ct);

    private async Task HandleFrameAsync(IBattleParticipant from, MsgEnvelope env, CancellationToken ct)
    {
        await _dispatchGate.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            var routes = ComputeFrames(from, env);
            foreach (var (target, frame, stock) in routes)
            {
                await target.PushAsync(frame, stock, ct);
            }
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "BattleSession {Bid}: unhandled in HandleFrameAsync", BattleId);
        }
        finally
        {
            _dispatchGate.Release();
        }
    }

    /// <summary>
    /// Pure-logic dispatch: given an inbound frame from one participant, return the list
    /// of (target, frame, stock) routes the session should dispatch. Transitions
    /// <see cref="Phase"/>. Extracted so unit tests can drive the dispatch without
    /// standing up real participants.
    /// </summary>
    internal IReadOnlyList<DispatchRoute> ComputeFrames(IBattleParticipant from, MsgEnvelope env)
    {
        // Shadow engine (N1): seat-once then ingest this frame, fully isolated from dispatch. The
        // wire output below is byte-for-byte unchanged — routes still come from the existing handlers;
        // the engine only observes (ND1). A shadow failure is logged and swallowed (ND6), never thrown
        // into the relay.
        try
        {
            EnsureEngineSetup();
            ShadowIngest(from, env);
        }
        catch (Exception ex)
        {
            _log.LogWarning(ex, "BattleSession {Bid}: shadow engine error (ignored)", BattleId);
        }

        if (Handlers.TryGetValue(env.Uri, out var handler))
        {
            var routes = handler.Handle(BuildContext(from, env));
            try { ShadowFeedServerFrames(routes); }
            catch (Exception ex)
            {
                _log.LogWarning(ex, "BattleSession {Bid}: shadow engine error feeding server frames (ignored)", BattleId);
            }
            return routes;
        }

        _log.LogDebug("BattleSession {Bid}: dropping uri={Uri} in lifecycle={Lifecycle} from vid={Vid}",
            BattleId, env.Uri, Lifecycle, from.ViewerId);
        return Array.Empty<DispatchRoute>();
    }

    /// <summary>Feed server-generated mulligan frames (Deal, Swap response, Ready) into the shadow
    /// engine. These frames are produced by LoadedHandler/SwapHandler and dispatched only to clients
    /// — they never enter <see cref="ShadowIngest"/> because they're not client-sent. But the engine
    /// needs them to drive the mulligan: Deal seats the hand, Ready completes the opponent's hand.
    /// The test harness (<c>NodeNativeBattleHarness</c>) feeds these directly; this method is the
    /// live-session equivalent.</summary>
    private void ShadowFeedServerFrames(IReadOnlyList<DispatchRoute> routes)
    {
        if (!_engine.IsReady) return;

        foreach (var (target, frame, _) in routes)
        {
            switch (frame.Uri)
            {
                case NetworkBattleUri.Deal when !_engineDealFed:
                    _engineDealFed = true;
                    _log.LogDebug("BattleSession {Bid}: DEAL diag BEFORE: {Diag}",
                        BattleId, _engine.DiagnoseDealState());
                    ShadowFeed(frame, isPlayerSeat: true, "Deal");
                    _log.LogDebug("BattleSession {Bid}: DEAL diag AFTER: {Diag}",
                        BattleId, _engine.DiagnoseDealState());
                    break;

                case NetworkBattleUri.Swap:
                    // The Swap RESPONSE (server-authored, carries post-mulligan self hand as
                    // pos→idx) must go to the engine for the correct seat. The client-sent Swap
                    // ({idxList}) also enters ShadowIngest but is harmless — its selfIdxList
                    // parses to null (no "self" key) so FirstMulliganOperation no-ops.
                    bool swapIsPlayer = ReferenceEquals(target, A);
                    ShadowFeed(frame, swapIsPlayer, $"SwapResponse({(swapIsPlayer ? "A" : "B")})");
                    break;

                case NetworkBattleUri.Ready when !_engineReadyFed && ReferenceEquals(target, A):
                    _engineReadyFed = true;
                    // Feed A's Ready (carries A's idxChangeSeed → receiver seeds _selfXorShiftRandom).
                    ShadowFeed(frame, isPlayerSeat: false, "Ready");
                    // Seed B's XorShift separately — A's Ready doesn't carry B's seed.
                    _engine.SeedOppoIdxChange(BattleSeeds.IdxChange(_state.MasterSeed, B.ViewerId));
                    break;
            }
        }
    }

    private void ShadowFeed(MsgEnvelope frame, bool isPlayerSeat, string label)
    {
        var engineFrame = frame.Body is RawBody ? frame : frame with { Body = ToRawBody(frame.Body) };
        var r = _engine.Receive(engineFrame, isPlayerSeat);
        if (r.Diverged)
            _log.LogWarning("BattleSession {Bid}: shadow engine diverged on {Label} feed: {Reason}",
                BattleId, label, r.RejectReason);
        if (frame.Uri is NetworkBattleUri.Deal or NetworkBattleUri.Swap or NetworkBattleUri.Ready)
            LogEngineHandState(frame.Uri, $"ShadowFeed({label})");
    }

    private static readonly JsonSerializerOptions _bodyJsonOptions = Wire.WireJsonOptions.CamelCase;

    /// <summary>Convert a typed body record (DealBody, SwapResponseBody, ReadyBody, etc.) to the
    /// <see cref="RawBody"/> the engine receiver expects. Serialize → JsonElement → ToObject (the
    /// same deep-conversion MsgEnvelope.FromJson uses for incoming wire frames).</summary>
    private static RawBody ToRawBody(IMsgBody? body)
    {
        if (body is null) return new RawBody(new Dictionary<string, object?>());
        var el = JsonSerializer.SerializeToElement(body, body.GetType(), _bodyJsonOptions);
        var dict = el.EnumerateObject()
            .ToDictionary(p => p.Name, p => MsgEnvelope.ToObject(p.Value));
        return new RawBody(dict);
    }

    /// <summary>Seat the shadow engine once, from the master seed + both deterministically-shuffled
    /// decks the node already computed (F-N-5). Attempted a single time; if the host can't seat the
    /// engine headless, it stays not-ready and the shadow no-ops for the rest of the battle.</summary>
    private void EnsureEngineSetup()
    {
        if (_engineSetupAttempted) return;
        _engineSetupAttempted = true;

        // Per-session mgr instance (Phase-5 ambient rip, chunk 47) isolates per-battle state across
        // concurrent sessions, so the historical single-active-engine gate (EngineSessionGate.TryAcquire)
        // is gone and engine setup is unconditional. A genuine setup failure still surfaces via
        // ComputeFrames' shadow-engine try/catch (it logs + swallows so the relay never sees an engine
        // exception, ND6), and IsReady stays false in that case so ShadowIngest/ShadowFeedServerFrames
        // no-op for the rest of the battle.
        //
        // Seed the engine's StableRandom with BattleSeeds.Stable(MasterSeed) — the SAME value the
        // Matched frame ships to both clients (InitBattleHandler.cs:28). The clients seed their
        // System.Random with Matched.seed (BattleManagerBase.cs:721), so the engine's stream must
        // share that derivation to track. MasterSeed itself is a root only — every wire-facing seed
        // (Stable, IdxChange, DeckShuffle) is a BattleSeeds.Derive(...) of it; the engine never
        // consumes the root directly. Live regression: bid 654473755566 had MasterSeed=1184631275
        // and Stable=1543475792 (the Matched.seed); seeding the engine with the raw root made every
        // turn-1+ draw pick a different deck position than the clients, so the opponent's first
        // non-mulligan play addressed a card the engine never drew → HandCardToField threw.
        _engine.Setup(BattleSeeds.Stable(_state.MasterSeed),
            _state.GetShuffledDeck(A), _state.GetShuffledDeck(B),
            (int)A.Context.ClassId, (int)B.Context.ClassId);
    }

    private void ShadowIngest(IBattleParticipant from, MsgEnvelope env)
    {
        if (!_engine.IsReady) return;
        // Node-only handshake URIs (InitNetwork, InitBattle) have no counterpart on the engine's
        // NetworkBattleURI enum — they're pure relay plumbing (see InitNetworkHandler /
        // InitBattleHandler). Feeding them to the engine's MapUri throws ArgumentException that
        // the outer try/catch swallows, but produces a per-frame warn+stack-trace for every
        // battle setup. Skip at the ingest source rather than plumb an ignore-branch through the
        // engine (the whole point of the shadow being separate is that node protocol changes
        // shouldn't require engine touches).
        if (env.Uri is NetworkBattleUri.InitNetwork or NetworkBattleUri.InitBattle) return;
        bool isPlayerSeat = ReferenceEquals(from, A);
        var r = _engine.Receive(env, isPlayerSeat);
        if (r.Diverged)
            _log.LogWarning("BattleSession {Bid}: shadow engine diverged on {Uri}: {Reason}",
                BattleId, env.Uri, r.RejectReason);
        if (env.Uri is NetworkBattleUri.Swap or NetworkBattleUri.TurnStart or NetworkBattleUri.PlayActions)
            LogEngineHandState(env.Uri, $"ShadowIngest(seat={(isPlayerSeat ? "A" : "B")})");
    }

    private void LogEngineHandState(NetworkBattleUri uri, string label)
    {
        if (!_engine.IsReady) return;
        var aIdxs = string.Join(",", Enumerable.Range(0, _engine.HandCount(true))
            .Select(i => _engine.HandCardIndex(true, i)));
        var bIdxs = string.Join(",", Enumerable.Range(0, _engine.HandCount(false))
            .Select(i => _engine.HandCardIndex(false, i)));
        _log.LogInformation("BattleSession {Bid}: engine hand after {Uri} {Label}: A=[{AHand}] B=[{BHand}]",
            BattleId, uri, label, aIdxs, bIdxs);
    }

}
