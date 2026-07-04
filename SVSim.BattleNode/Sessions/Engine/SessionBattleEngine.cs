extern alias engine;
using System.Reflection;
using System.Runtime.Serialization;
using engine::SVSim.BattleEngine.Rng;
using SVSim.BattleNode.Protocol;
using NetworkBattleReceiver = engine::NetworkBattleReceiver;
using NetworkBattleDefine = engine::NetworkBattleDefine;
using BattleManagerBase = engine::BattleManagerBase;
using BattlePlayerBase = engine::BattlePlayerBase;
using BattleCardBase = engine::BattleCardBase;
using UnitBattleCard = engine::UnitBattleCard;
using ClassBattleCardBase = engine::ClassBattleCardBase;
using CardCreatorBase = engine::CardCreatorBase;
using CostAddModifier = engine::CostAddModifier;
using SBattleLoad = engine::SBattleLoad;
using CardTemplate = engine::CardTemplate;
using GameObject = engine::UnityEngine.GameObject;
using RealTimeNetworkAgent = engine::RealTimeNetworkAgent;
using NetworkNullLogger = engine::NetworkNullLogger;
using ToolboxGame = engine::Wizard.ToolboxGame;
using GameMgr = engine::GameMgr;
using BattleUIContainer = engine::BattleUIContainer;
using BackGroundBase = engine::BackGroundBase;
using NullPlayerEmotion = engine::Wizard.Battle.Player.Emotion.NullPlayerEmotion;
using NetworkMulliganPhase = engine::Wizard.Battle.Phase.NetworkMulliganPhase;
using MulliganInfoControl = engine::Wizard.Battle.Mulligan.MulliganInfoControl;
using UIWidget = engine::UIWidget;
using UISprite = engine::UISprite;
using NullDetailPanelControl = engine::NullDetailPanelControl;
using DetailPanelControl = engine::DetailPanelControl;
using BattleLogManager = engine::Wizard.Battle.UI.BattleLogManager;

namespace SVSim.BattleNode.Sessions.Engine;

/// <summary>One authoritative engine per BattleSession, seated as both players (design ND2). A faithful
/// SHADOW: it mirrors each client's resolved play, never overrides/rejects/originates (ND1). Ingest is
/// the engine's own NetworkBattleReceiver.ReceivedMessage (ND4); isPlayer selects the seat (F-N-2).
///
/// The headless wiring here is the production analogue of the test HeadlessFixture
/// (NewNetworkEmitBattle / SeedDeck / InitLeaderLife / InitCardTemplates). It deliberately omits the
/// emit-only RealTimeNetworkAgent scaffolding the test uses for the SEND path — the shadow engine only
/// RECEIVES (F-N-2), so no socket-agent is constructed. The engine's global init (CardMaster, GameMgr,
/// Wizard.Data) is the caller's responsibility (the test does HeadlessEngineEnv.EnsureInitialized;
/// the live node guards Setup in try/catch so an un-initialized host degrades to a no-op shadow).</summary>
internal sealed class SessionBattleEngine
{
    private const int DefaultLeaderLife = 20;

    // Phase-5 chunk 47: was `BattleAmbientContext _ctx` — an ambient-scoped bundle of
    // per-session state (GameMgr / ViewerId / IsForecast / IsRandomDraw / RecoveryInfo /
    // NetworkAgent) that engine code read via BattleAmbient.Current. The ambient is dead
    // (chunk 46 made GetIns() return null); everything that was on _ctx now lives on the mgr
    // instance directly. What remains is the per-session GameMgr, kept as a plain field so it
    // can be seeded (WirePerSessionGameMgr) BEFORE the mgr ctor reads it via the (cc, gm) overload.
    private readonly GameMgr _gameMgr = new();

    private HeadlessNetworkBattleMgr? _mgr;
    private NetworkBattleReceiver? _receiver;

    /// <summary>True once Setup has built the two-seat battle.</summary>
    public bool IsReady => _mgr is not null;

    /// <summary>Construct the two-seat network battle from both decks + the master seed (design F-N-5).
    /// <paramref name="seatADeck"/>/<paramref name="seatBDeck"/> are the per-side deck orders the node
    /// already computed (BattleSessionState.GetShuffledDeck) and handed each client.
    /// <paramref name="seatAClass"/>/<paramref name="seatBClass"/> are each seat's class ordinal (1..8,
    /// the <c>CardClass</c> int value); they select the leader's class via the all-8-class
    /// ClassCharacterList EngineGlobalInit installs (chara_id == class_id for 1..8). The 3-arg overload
    /// behavior is preserved by the defaults (1/2), matching the test-harness charaIds.
    /// <para>NOTE: GameMgr is per-session on the mgr instance (Phase-5 ambient rip, chunk 47); leader
    /// chara ids are set on the SESSION's <c>_gameMgr</c> (seeded pre-ctor by
    /// <c>EngineGlobalInit.WirePerSessionGameMgr</c>), not on a process-wide singleton. This is the
    /// multi-instancing payoff: concurrent sessions each own their own GameMgr + engine state, so the
    /// historical single-active-engine gate (deleted EngineSessionGate) is no longer needed.</para></summary>
    public void Setup(int masterSeed,
        IReadOnlyList<long> seatADeck, IReadOnlyList<long> seatBDeck,
        int seatAClass = 1, int seatBClass = 2)
    {
        SetupInternal(masterSeed, seatADeck, seatBDeck, seatAClass, seatBClass, rng: null);
    }

    /// <summary>TEST/DEBUG SEAM (Phase 4 Option-A viability PROBE — NOT a production fix). Identical to
    /// <see cref="Setup(int, IReadOnlyList{long}, IReadOnlyList{long}, int, int)"/> but installs a logging
    /// RNG source that, on EVERY <c>StableRandom</c>/<c>StableRandomDouble</c> roll, records a roll entry
    /// (call index, API, the seat signals readable from mgr state at roll time, and the live call stack).
    /// Lets a test answer: at roll time, is the ACTING SEAT determinable from mgr state alone, or only from
    /// the stack? No production path calls this.</summary>
    internal IReadOnlyList<RollEntry> DebugSetupWithRollLog(int masterSeed,
        IReadOnlyList<long> seatADeck, IReadOnlyList<long> seatBDeck,
        int seatAClass = 1, int seatBClass = 2)
    {
        var log = new List<RollEntry>();
        // The logger needs the mgr to read seat signals at roll time; the mgr is built inside Setup, so the
        // logger reads it lazily via a closure populated right after construction.
        HeadlessNetworkBattleMgr[] mgrBox = { null! };
        var rng = new RollLoggingRandomSource(new SeededRandomSource(masterSeed), log, () => mgrBox[0]);
        SetupInternal(masterSeed, seatADeck, seatBDeck, seatAClass, seatBClass, rng, mgrBox);
        return log;
    }

    private void SetupInternal(int masterSeed,
        IReadOnlyList<long> seatADeck, IReadOnlyList<long> seatBDeck,
        int seatAClass, int seatBClass,
        IRandomSource? rng, HeadlessNetworkBattleMgr[]? mgrBox = null)
    {
        // Prime the engine's process-global statics (CardMaster, Wizard.Data, all-8-class Master,
        // GameMgr/netUser/udid). Idempotent (process-once); makes the LIVE host ready so Setup succeeds
        // here rather than throwing into the shadow's no-op path (Phase 2 N2, carried-risk A).
        EngineGlobalInit.EnsureInitialized();

        // Phase-5 chunk 46: seed the session's own GameMgr (chara ids + net-user) BEFORE mgr
        // construction so the base ctor's BattlePlayer/DataMgr reads resolve. Was piggybacked on
        // EnsureInitialized via ambient reach; now explicit and per-session.
        EngineGlobalInit.WirePerSessionGameMgr(_gameMgr);

        // rng defaults to SeededRandomSource(masterSeed) inside the mgr — masterSeed here is the
        // engine's StableRandom seed (parameter name preserved for API compatibility; callers pass
        // BattleSeeds.Stable(rootMasterSeed) so the stream is born aligned with the seed the node
        // ships to both clients in Matched.seed). F-N-5; O-N-2 "bit-aligned anyway".
        // Phase-5 chunk 45: seed the mgr's GameMgr via _ctx (the session's ambient-attached GameMgr)
        // and hand it to the mgr's pre-seeded ctor. Eliminates the "ambient reaches into mgr ctor"
        // step; equivalent behavior since _gameMgr is the same instance the node reads later.
        var mgr = new HeadlessNetworkBattleMgr(new SessionContentsCreator(masterSeed), rng, _gameMgr);
        if (mgrBox is not null) mgrBox[0] = mgr; // publish for the test roll-logger closure (DebugSetupWithRollLog)

        // Publish the mgr on the ambient EARLY (Phase 5a, 2026-07-02). Pre-Phase-5a the ambient
        // stored NetworkAgent/RecoveryInfo/IsForecast/etc. directly, so late-attach was fine.
        // Post-5a those fields are instance-backed on the mgr, and every accessor routes through
        // BattleManagerBase.GetIns() → ambient.Mgr — so any subsequent setup call that reaches for
        // one of them (InstallHeadlessNetworkAgent, SetGameMgrCharaIds, etc.) needs the mgr already
        // attached. The later `_mgr = mgr; // Phase-5 chunk 47: was _ctx.Mgr = _mgr — ambient publish is dead.` line is now a no-op re-assignment;
        // kept there for readability at the "wire mulligan" seam it originally guarded.
        // Phase-5 chunk 47: was _ctx.Mgr = mgr — ambient publish is dead (GetIns() returns null unconditionally).
        // Recovery mode is the engine's OWN headless replay path: the live view/UI touches on the
        // receive cycle (BattleUIContainer.DisableMenu, turn-control UI, card-view creation, VFX
        // waits) are all gated `!IsRecovery` (BattleUIContainer.cs:130, BattleManagerBase.cs:1499+),
        // so this collapses them to no-ops without changing authoritative state. Set AFTER construction
        // so the ctor still wired the LIVE NetworkBattleReceiver (ND4) rather than the replay receiver.
        // Safe for shadow: the only thing !IsRecovery additionally enables is EMIT, which a pure shadow
        // never does (it never originates a send).
        mgr.IsRecovery = true;

        // Seat each player as the other's opponent (private field on BattlePlayerBase, as the real
        // match-load does). Mirrors HeadlessFixture.NewNetworkEmitBattle.
        BattlePlayerBase player = mgr.GetBattlePlayer(isPlayer: true);
        BattlePlayerBase enemy = mgr.GetBattlePlayer(isPlayer: false);
        SetField(player, "_opponentBattlePlayer", enemy);
        SetField(enemy, "_opponentBattlePlayer", player);
        player.IsSelfTurn = true;
        enemy.IsSelfTurn = false;

        // Participant A always goes first (LoadedHandler gives A TurnState.First). The engine's
        // BattlePlayer = isPlayer=true = seat A, so doesPlayerGoFirst must be true. This controls:
        //   (1) SetupEvolCount: first player gets FIRST_PLAYER_EP (2) + wait 5,
        //       second player gets SECOND_PLAYER_EP (3) + wait 4
        //   (2) IsFirst → BattlePlayer.IsGameFirst / BattleEnemy.IsGameFirst → turn-1 draw count:
        //       first player draws 1, second draws 2 (BattlePlayerBase.TurnStartDrawCard)
        mgr.IsFirst = true;
        mgr.SetupEvolCount(doesPlayerGoFirst: true);

        // The real match-load's SetupInitialGameState(areCardsRandomlyDrawn:true) sets this flag
        // (BattleManagerBase.cs:1110), routing LotteryRandomDrawCard through seeded StableRandom
        // instead of top-of-deck. Without it the shadow draws DeckCardList[0] every time while
        // clients draw seeded-random — desynchronizing the hand and every downstream field.
        mgr.InstanceIsRandomDraw = true; // Phase-5 chunk 43: was BattleManagerBase.IsRandomDraw static

        InitLeaderLife(mgr);       // a 0-life leader reads as game-over and silently blocks plays
        InitCardTemplates(mgr);    // play/draw resolution touches the (no-op) card view layer
        InitHeadlessViews(mgr);    // turn/play cycle dereferences UI-container + emotion refs
        InstallHeadlessNetworkAgent(mgr); // turn-flow resolve reads mgr.InstanceNetworkAgent

        // Per-session leader class: chara_id == class_id for 1..8 in the all-8-class ClassCharacterList,
        // so writing the seats' class ordinals into the SESSION's GameMgr DataMgr (resolved through the
        // ambient — see Setup remarks) resolves each leader's correct class.
        SetGameMgrCharaIds(mgr.GameMgr, seatAClass, seatBClass);

        SeedDeck(mgr, seatADeck, isPlayer: true);
        SeedDeck(mgr, seatBDeck, isPlayer: false);

        // Publish the mgr on the per-session ambient BEFORE wiring the mulligan phase: that ctor
        // chains into MulliganInfoControl.InitMulliganInfo, which reads BattleManagerBase.GetIns()
        // (MulliganInfoControl.cs:259). With the fallback gone (Task 8), an unset ambient.Mgr would
        // resolve to null and NRE on the very next field read. Set ambient.Mgr here so the wiring
        // resolves the per-session mgr cleanly.
        _mgr = mgr;
        // Phase-5 chunk 47: was _ctx.Mgr = _mgr — ambient publish is dead.

        WireMulliganPhase(mgr);    // wire OperateReceive.OnReceiveDeal -> StartDeal (deal seats the hand)

        // Use the mgr's OWN receiver — the ctor already wired it to the mgr's OperateReceive +
        // NetworkBattleData (NetworkBattleManagerBase.cs:266, non-recovery branch). This is the same
        // receiver the engine's RecoveryDataHandler drives when replaying recorded frames.
        _receiver = mgr.GetNetworkBattleReceiver();
    }

    /// <summary>Ingest one client frame into the engine for the given seat. <paramref name="isPlayerSeat"/>
    /// maps the sender to the engine's player(true)/opponent(false) seat (F-N-2). A throw/reject is
    /// returned as a detected-desync EVENT (ND6), never silently absorbed.</summary>
    public EngineIngestResult Receive(MsgEnvelope env, bool isPlayerSeat)
    {
        if (_mgr is null || _receiver is null)
            throw new InvalidOperationException("Receive before Setup.");

        var dict = ToEngineDict((env.Body as RawBody)?.Entries);
        TranslateTargetOwners(dict, isPlayerSeat);
        TranslateChoiceKeyAction(dict);
        var uri = MapUri(env.Uri);

        try
        {
            // Mirror the engine's own recorded-frame replay (RecoveryDataHandler.cs:283): every
            // ingested action resolves through the isHaveSequence ConductReceiveData path, and
            // checkBreakData:false so a partial/handshake frame is not rejected as a break.
            bool accepted = _receiver.ReceivedMessage(
                uri, isHaveSequence: true, dict, isPlayerSeat, handler: null, checkBreakData: false);
            return accepted ? EngineIngestResult.Ok() : EngineIngestResult.Reject($"receiver rejected {env.Uri}");
        }
        catch (Exception ex)
        {
            // Keep the first few frames: a headless-gap NRE/ANE is almost always diagnosable from the
            // call chain (the throwing leaf is often a ThrowHelper, so one frame is too few).
            var site = string.Join(" || ", (ex.StackTrace ?? "").Split('\n').Take(4).Select(s => s.Trim()));
            return EngineIngestResult.Reject($"{env.Uri} threw: {ex.GetType().Name}: {ex.Message} @ {site}");
        }
    }

    // --- live isSelf -> engine-vid target-owner translation (live PvP ingest fidelity) -------------
    //
    // THE GAP this closes: real clients send each targetList entry as {targetIdx, isSelf, selectSkillIndex}
    // (verified in client-send captures, e.g. data_dumps/captures/battle_test/battle-traffic_cl1.ndjson),
    // where `isSelf` is the SENDER's perspective flag (isSelf:1 = target on the sender's own seat;
    // isSelf:0 = target on the OTHER seat). But the engine receive path the node drives is IsRecovery, and
    // its recovery targetList parse (NetworkBattleReceiver.CreateTargetList, isWatch:true branch,
    // NetworkBattleReceiver.cs:2180-2188) derives a target's owner from a `vid` stamp:
    //   isSelf_engine = (vid != PlayerStaticData.UserViewerID)         // UserViewerID == EngineGlobalInit.ThisViewerId
    // and the downstream resolver (NetworkBattleGenericTool.LookForActionDataToTargetCard:133) routes
    //   isSelf_engine == false -> BattlePlayer (engine seat A);  isSelf_engine == true -> BattleEnemy (seat B).
    // So the engine vid encodes the target's ABSOLUTE seat: seat A == ThisViewerId, seat B != it.
    //
    // Without a translation a real `isSelf` frame carries no `vid`, so the recovery parse leaves
    // isSelf_engine=false (vid defaults 0 != ThisViewerId would even read TRUE, but with no key it's the
    // default-0 TargetData) and the target mis-resolves -> a targeted attack/spell/evolution silently
    // misses. We translate on the ENGINE's OWN dict copy only (ToEngineDict re-boxed a fresh dict; the
    // node's relay/mining read the ORIGINAL env.Body, which KnownListBuilder/RecordTokensFrom consume as
    // `isSelf` and must keep), so the node-side isSelf bookkeeping is untouched.
    //
    // ONLY engine-vid field on the live targeted frames: `targetList[].vid`. The recovery parse reads `vid`
    // exclusively in the isWatch:true `targetList` branch (the ONLY `vid` read on the receiver,
    // NetworkBattleReceiver.cs:2182); `oppoTargetList` parses `isSelf` directly (isWatch:false) but the node
    // never sends it. Non-targeted frames (deal/play/turn/mulligan) carry no targetList and pass through
    // unchanged.
    //
    // The (isPlayerSeat, isSelf) -> vid mapping (oracle: the harness's known-good SelfSeatVid/EnemySeatVid):
    //   target is on seat A  <=>  isPlayerSeat == (isSelf == 1)   // sender-relative isSelf -> absolute seat
    //   seat A -> ThisViewerId ;  seat B -> ThisViewerId + 1
    private static void TranslateTargetOwners(Dictionary<string, object> dict, bool isPlayerSeat)
    {
        if (!dict.TryGetValue(TargetListKey, out var raw) || raw is not List<object> entries)
            return;

        foreach (var e in entries)
        {
            if (e is not Dictionary<string, object> entry) continue;
            // Tolerate a vid already present (idempotent): leave the engine shape as-is. The primary
            // contract is the real isSelf shape, but a frame that already carries vid resolves directly.
            if (entry.ContainsKey(VidKey)) continue;
            if (!entry.TryGetValue(IsSelfKey, out var isSelfRaw)) continue;

            bool isSelf = ToInt(isSelfRaw) == 1;
            bool targetOnSeatA = isPlayerSeat == isSelf;
            entry[VidKey] = targetOnSeatA ? EngineGlobalInit.ThisViewerId : EngineGlobalInit.ThisViewerId + 1;
            // Drop isSelf on the ENGINE copy: the isWatch:true recovery parse reads vid, not isSelf, so the
            // key is dead weight on this copy. (The node's relay/mining copy is a different dict and keeps it.)
            entry.Remove(IsSelfKey);
        }
    }

    private const string TargetListKey = "targetList";
    private const string VidKey = "vid";
    private const string IsSelfKey = "isSelf";
    private const string KeyActionKey = "keyAction";
    private const string SelectCardKey = "selectCard";
    private const string CardIdKey = "cardId";

    // --- live Choice-keyAction shape translation (live PvP ingest fidelity) ------------------------
    //
    // THE GAP this closes: a Choice play's wire keyAction entry on the SENDER's send is the wrapped
    // shape `{type:1, cardId:<choiceCardId>, selectCard:{cardId:[<chosenId>...], open:0|1}}` (verified
    // in client-send captures, e.g. data_dumps/captures/battle_test/cl1/battle-traffic.ndjson live
    // Resonance play). The engine's receive parser (NetworkBattleReceiver.cs:1202) reads the
    // `selectCard` value through `ConvertToListInt`, which does `value as List<object>` — a Dictionary
    // value casts to null and the inner `foreach (... in null)` throws NRE. The whole
    // ConvertReceiveDataToMakeData is wrapped in a swallow-catch (NetworkBattleReceiver.cs:1255-1260)
    // that logs to Debug.LogError + LocalLog — both shimmed/no-op'd headlessly — and returns false.
    // SessionBattleEngine.Receive calls ReceivedMessage with checkBreakData:false, so the false isn't
    // surfaced; the engine continues with `choiceIdList=[]`, the choice never resolves, and the played
    // card never moves from hand to board. Then any LATER frame that addresses the un-resolved card
    // by Index sees a stale hand entry — silently for a turn or two, until a TARGETED play looks for
    // it on the board (where it should be per wire) and gets `null` from LookForActionDataToTargetCard
    // → ActionProcessor.PlayCard:407 NRE on `selectedCard.SelfBattlePlayer`.
    //
    // OPPONENT-FACING relay shape is different: the node strips selectCard entirely from the opponent
    // broadcast (verified: cl2 receives `keyAction:[{type:1, cardId:127011010}]`), so the opponent
    // never needs this transform. Only the shadow engine — which ingests the SENDER's raw send — does.
    //
    // The fix: walk keyAction on the ENGINE's own dict copy (TranslateTargetOwners' pattern) and
    // unwrap selectCard. `{cardId:[121011010], open:0}` → `[121011010]`. The `open` flag (was this
    // choice revealed to the opponent) is irrelevant to the engine's resolution. The flat-list shape
    // is what `ConvertToListInt` consumes successfully, AND what the existing test harness
    // (NodeNativeBattleHarness.ChoicePlayBody) already supplies — that test passes, proving the rest
    // of the Choice resolution path works given the right shape. Idempotent: an already-flat list
    // (no wrapping dict) is left alone, so a future relay frame that happens to carry the flat form
    // also resolves directly.
    //
    // Live regression: bid 131549100204, B's Resonance (127011010) play of idx 20 at error.txt:1642.
    // Without the unwrap, idx 20 stays in B's hand; later A's 6-cost bounce targets B's "board" idx 20,
    // engine can't find it on the board, ActionProcessor.PlayCard NRE's at the foreach over a list
    // containing a null target.
    private static void TranslateChoiceKeyAction(Dictionary<string, object> dict)
    {
        if (!dict.TryGetValue(KeyActionKey, out var raw) || raw is not List<object> entries)
            return;

        foreach (var e in entries)
        {
            if (e is not Dictionary<string, object> entry) continue;
            if (!entry.TryGetValue(SelectCardKey, out var sel)) continue;
            // Already-flat (a List): no transform needed. Idempotent guard.
            if (sel is List<object>) continue;
            // Wrapped (a Dict): unwrap to the inner cardId list.
            if (sel is Dictionary<string, object> wrap
                && wrap.TryGetValue(CardIdKey, out var inner)
                && inner is List<object> flat)
            {
                entry[SelectCardKey] = flat;
            }
            else
            {
                // Unrecognized shape — drop the key so the parse doesn't NRE; the play will resolve
                // with an empty choice list, and the divergence (if any) will surface downstream
                // rather than crash the receiver.
                entry.Remove(SelectCardKey);
            }
        }
    }

    // The decoded wire value may be a boxed long/int/bool depending on the codec; normalize to int.
    private static int ToInt(object v) => v switch
    {
        bool b => b ? 1 : 0,
        long l => (int)l,
        int i => i,
        _ => Convert.ToInt32(v),
    };

    // --- live board-state reads (N1 oracle surface; design F-N-4 board-state reads) ----------------
    // Each returns LIVE engine state off the seated player, mirroring the Phase-1 oracle reads
    // (VanillaFollowerOracleTests: player.Pp, player.HandCardList.Count, ClassAndInPlayCardList,
    // leader == the Class card). seat:true == player, false == opponent (F-N-2).
    //
    // INVARIANT (two accessor bands, different null-engine policy):
    //   • This "oracle" band (down to EvolveWaitTurnCount) goes through Seat(), which THROWS if the
    //     engine isn't seated for this session. It is TEST-ONLY — called solely from the
    //     node-native harness/tests, where the engine is always seated. Do NOT call these from a wire
    //     handler.
    //   • The wire-path band below (PlayedCardCost/Spellboost/Clan/Tribe/Id) DEGRADES to a fallback
    //     when _mgr is null (Setup failed and the ComputeFrames try/catch swallowed it, ND6), so a
    //     non-engine session never crashes. Production handlers read ONLY that band.

    public int LeaderLife(bool playerSeat) => Seat(playerSeat).Class.Life;
    public int Pp(bool playerSeat) => Seat(playerSeat).Pp;
    public int HandCount(bool playerSeat) => Seat(playerSeat).HandCardList.Count;
    public int DeckCount(bool playerSeat) => Seat(playerSeat).DeckCardList.Count;
    public int Turn(bool playerSeat) => Seat(playerSeat).Turn;

    /// <summary>Followers in play, excluding the leader (the Class card occupies one slot of
    /// ClassAndInPlayCardList).</summary>
    public int BoardCount(bool playerSeat) => Math.Max(0, Seat(playerSeat).ClassAndInPlayCardList.Count - 1);

    /// <summary>The engine <c>Index</c> of the hand card at the given hand position. The receive-path
    /// Play frame addresses a card by its engine Index (playIdx), which equals deck position + 1 for
    /// a card dealt from the seeded deck.</summary>
    public int HandCardIndex(bool playerSeat, int handPos) => Seat(playerSeat).HandCardList[handPos].Index;

    /// <summary>The real <c>CardId</c> (wire identity) of the hand card at <paramref name="handPos"/>. Lets a
    /// test locate a specific card in a SHUFFLED opening hand by identity (then read its <see cref="HandCardIndex"/>
    /// to drive a play), without depending on which shuffled position the card landed at.</summary>
    public int HandCardId(bool playerSeat, int handPos) => Seat(playerSeat).HandCardList[handPos].CardId;

    /// <summary>The real <c>CardId</c> (wire identity) of the in-play follower at <paramref name="boardPos"/>
    /// (0-based, skipping the leader/Class card at ClassAndInPlayCardList[0] — same convention as
    /// <see cref="BoardCount"/>). Used to assert an opponent reveal seated the substituted card with its
    /// true identity (M-HC-2): before the reveal the slot holds a hidden dummy (cardId 0); after, the
    /// engine-resolved actual card carries the wire cardId.</summary>
    public int InPlayCardId(bool playerSeat, int boardPos)
    {
        return Seat(playerSeat).ClassAndInPlayCardList[boardPos + 1].CardId;
    }

    /// <summary>The engine <c>Index</c> of the in-play follower at <paramref name="boardPos"/> (0-based,
    /// leader excluded — same convention as <see cref="BoardCount"/>/<see cref="InPlayCardId"/>). An ATTACK
    /// frame addresses the attacker by this in-play Index (the wire <c>playIdx</c>), so a test reads it after
    /// a follower resolves onto the board to build the attack (M-HC-4a).</summary>
    public int InPlayCardIndex(bool playerSeat, int boardPos)
    {
        return Seat(playerSeat).ClassAndInPlayCardList[boardPos + 1].Index;
    }

    /// <summary>The current life/health of the in-play follower at <paramref name="boardPos"/> (0-based,
    /// leader excluded). Reads <see cref="BattleCardBase.Life"/> (skill-resolved current health). Lets an
    /// attack test assert a follower took the attacker's damage (M-HC-4a follower-vs-follower trade).</summary>
    public int InPlayCardLife(bool playerSeat, int boardPos)
    {
        return Seat(playerSeat).ClassAndInPlayCardList[boardPos + 1].Life;
    }

    /// <summary>The attack stat of the in-play follower at <paramref name="boardPos"/> (skill-resolved
    /// <see cref="BattleCardBase.Atk"/>). The damage it deals when it attacks.</summary>
    public int InPlayCardAtk(bool playerSeat, int boardPos)
    {
        return Seat(playerSeat).ClassAndInPlayCardList[boardPos + 1].Atk;
    }

    /// <summary>True when the in-play follower at <paramref name="boardPos"/> can still attack this turn
    /// (<see cref="BattleCardBase.Attackable"/>). After it attacks (consuming its single attack) this reads
    /// false — the "attacker is spent" assertion (M-HC-4a).</summary>
    public bool InPlayCardAttackable(bool playerSeat, int boardPos)
    {
        return Seat(playerSeat).ClassAndInPlayCardList[boardPos + 1].Attackable;
    }

    /// <summary>True once the in-play follower at <paramref name="boardPos"/> (0-based, leader excluded)
    /// has evolved (<see cref="UnitBattleCard.IsEvolution"/>, set true inside the engine's own
    /// <c>UnitBattleCard.Evolution</c> mutation). Only <see cref="UnitBattleCard"/> followers carry the
    /// flag; a non-follower (or the leader) reads false. The evolve test's decisive engine-state assertion
    /// (M-HC-4b).</summary>
    public bool IsEvolved(bool playerSeat, int boardPos)
    {
        return (Seat(playerSeat).ClassAndInPlayCardList[boardPos + 1] as UnitBattleCard)?.IsEvolution ?? false;
    }

    /// <summary>The seat's current evolve-point count (<see cref="BattlePlayerBase.CurrentEpCount"/>). An
    /// evolve spends one EP, so the evolve test asserts this decrements by 1. EP is granted at setup by
    /// the engine's <c>SetupEvolCount</c> (2 for the game-first seat, 3 for the second) and unlocks once
    /// <c>EvolveWaitTurnCount</c> has counted down (M-HC-4b).</summary>
    public int EpCount(bool playerSeat) => Seat(playerSeat).CurrentEpCount;

    /// <summary>Turns remaining until <paramref name="playerSeat"/> may evolve
    /// (<see cref="BattlePlayerBase.EvolveWaitTurnCount"/>); 0 means evolve is unlocked. Lets a test ramp to
    /// the evolve-enabled turn deterministically (M-HC-4b).</summary>
    public int EvolveWaitTurnCount(bool playerSeat) => Seat(playerSeat).EvolveWaitTurnCount;

    /// <summary>The engine-RESOLVED play-time cost of the card whose engine <c>Index</c> == <paramref name="idx"/>
    /// on <paramref name="playerSeat"/> (M-HC-3a). This is the discounted cost the play actually paid —
    /// spellboost reduction, board-dependent modifiers and all — read straight off the engine, so the
    /// opponent-facing knownList carries the SAME cost the engine charged (closing the spellboost
    /// cost-desync BY CONSTRUCTION: no bookkeeping, the engine already knows).
    /// <para>READ-MOMENT: the conductor's <c>ShadowIngest</c> runs <c>engine.Receive</c> (→ resolves the
    /// play) BEFORE the handler runs, so at read time the played card has LEFT the hand — a follower sits
    /// in <c>ClassAndInPlayCardList</c>, a spell in <c>CemeteryList</c>. <see cref="BattleCardBase.PlayCard"/>
    /// captures <c>_playedCost = useCost</c> (== the fully-resolved <c>Cost</c> at the moment of play,
    /// incl. every CostModifier) onto the card object, which persists after the card leaves the hand —
    /// so <see cref="BattleCardBase.PlayedCost"/> is the authoritative play-time discounted cost. We search
    /// the seat's post-resolution zones (in-play, cemetery) by <c>Index</c>, then fall back to the hand
    /// (a not-yet-resolved card, e.g. a degenerate test path) reading the live <c>Cost</c> there.</para>
    /// <para>Degrades to <paramref name="fallback"/> when the engine is not set up (Setup failed and the
    /// ComputeFrames try/catch swallowed it, ND6) or the idx resolves to no card — so a non-engine
    /// session never crashes and a vanilla play simply emits its base cost via the caller's fallback.</para></summary>
    public int PlayedCardCost(bool playerSeat, int idx, int fallback = 0)
    {
        if (_mgr is null) return fallback;
        var card = FindByIndex(Seat(playerSeat), idx);
        if (card is null) return fallback;
        // PlayedCost is set (>= 0) once PlayCard resolved the play; before that (a card still in hand on a
        // degenerate path) read the live Cost, which already folds in any registered CostModifier.
        return card.PlayedCost >= 0 ? card.PlayedCost : card.Cost;
    }

    /// <summary>The engine-RESOLVED spellboost (spell-charge) COUNT of the card whose engine <c>Index</c> ==
    /// <paramref name="idx"/> on <paramref name="playerSeat"/> (M-HC-3b). The engine accumulates this count
    /// for real on the receive path (each spell play that targets the card runs the card's own
    /// <c>Skill_spell_charge.AddSpellChargeCount</c>), so this is the same authoritative count prod sends —
    /// emitted on the opponent-facing knownList so the wire stays prod-faithful now that the wire-derived
    /// spellboost bookkeeping is retired (cost itself is engine-sourced via <see cref="PlayedCardCost"/>).
    /// <para>READ-MOMENT (persist-post-play): <see cref="BattleCardBase.SpellChargeCount"/> is set to 0 only
    /// in the ctor (re-init, BattleCardBase.cs:2042) and in <c>ReturnCard</c> (bounce-to-hand,
    /// BattleCardBase.cs:2681); <see cref="BattleCardBase.PlayCard"/> never touches it. So the count PERSISTS
    /// on the played card object after it leaves the hand (follower in-play, spell in cemetery) — the same
    /// persist-after-play property <see cref="BattleCardBase.PlayedCost"/> has. We therefore use the SAME
    /// post-resolution zone search (<see cref="FindByIndex"/>: in-play → cemetery → hand) and read
    /// <c>SpellChargeCount</c> directly — no separate receive-capture is needed.</para>
    /// <para>Degrades to <paramref name="fallback"/> when the engine is not set up or the idx resolves to no
    /// card — so a non-engine session never crashes and a vanilla play emits 0 via the caller's fallback.</para></summary>
    public int PlayedCardSpellboost(bool playerSeat, int idx, int fallback = 0)
    {
        if (_mgr is null) return fallback;
        var card = FindByIndex(Seat(playerSeat), idx);
        return card?.SpellChargeCount ?? fallback;
    }

    /// <summary>The engine-RESOLVED card identity (wire <c>cardId</c>) of the card whose engine <c>Index</c> ==
    /// <paramref name="idx"/> on <paramref name="playerSeat"/> (M-HC-4f), read straight off
    /// <see cref="BattleCardBase.CardId"/> — the TRUE id the engine resolved during the conductor's
    /// <c>ShadowIngest</c> (<c>engine.Receive</c> ran BEFORE this read). This is the authoritative identity for
    /// EVERY card the engine seats, retiring the wire-mined idx→cardId bookkeeping for the played card:
    /// <list type="bullet">
    /// <item>a DECK card carries its dealt id (the seeded shuffled-deck identity);</item>
    /// <item>a GENERATED token carries the wire id <c>CreateActualCard</c>/<c>ReplaceReceivedCards</c> stamped on it
    ///   (M-HC-2 proved reveal seats the wire cardId);</item>
    /// <item>a CHOICE/Discover token carries the CHOSEN id (M-HC-4c proved the chosen token lands with its true id);</item>
    /// <item>a COPY/clone token carries the COPIED id (the engine copies the source card at baseIdx).</item>
    /// </list>
    /// Same post-resolution zone search + degrade-to-<paramref name="fallback"/> contract as
    /// <see cref="PlayedCardCost"/>: no engine / no card → <paramref name="fallback"/>, so a non-engine session
    /// (Setup failed and the ComputeFrames try/catch swallowed it, ND6) keeps emitting the deck-map id via
    /// the caller's fallback, never crashing.</summary>
    public long PlayedCardId(bool playerSeat, int idx, long fallback = 0)
    {
        if (_mgr is null) return fallback;
        var card = FindByIndex(Seat(playerSeat), idx);
        return card is null ? fallback : card.CardId;
    }

    /// <summary>The engine-RESOLVED clan of the card whose engine <c>Index</c> == <paramref name="idx"/> on
    /// <paramref name="playerSeat"/> (M-HC-4e), as the int <c>ClanType</c> ordinal prod sends on the
    /// knownList entry (e.g. <c>clan:8</c> in the tk2 capture). Reads <see cref="BattleCardBase.Clan"/>, whose
    /// getter returns the skill-applied clan (<c>SkillApplyInformation.ClanSkinInfo.Last()</c> when a skill
    /// changed it, else <c>BaseParameter.Clan</c>) — so a <c>change_affiliation</c> is reflected, which is WHY
    /// the engine value (not the static card-master clan) is the faithful one to emit.
    /// <para>Same post-resolution zone search + degrade-to-<paramref name="fallback"/> contract as
    /// <see cref="PlayedCardCost"/>: no engine / no card → fallback, so a non-engine session never crashes.</para></summary>
    public int PlayedCardClan(bool playerSeat, int idx, int fallback = 0)
    {
        if (_mgr is null) return fallback;
        var card = FindByIndex(Seat(playerSeat), idx);
        return card is null ? fallback : (int)card.Clan;
    }

    /// <summary>The engine-RESOLVED tribe of the card whose engine <c>Index</c> == <paramref name="idx"/> on
    /// <paramref name="playerSeat"/> (M-HC-4e), in the EXACT wire string form prod sends: the comma-joined
    /// int <c>TribeType</c> ordinals (e.g. <c>tribe:"7,16"</c> for MACHINE+SCHOOL in the tk2 capture), and
    /// <c>"0"</c> when the card has no tribe (== <c>TribeType.ALL == 0</c> — prod never sends empty/omitted;
    /// the client reads it via <c>item.Value.ToString()</c>, NetworkBattleReceiver.cs:2382). Reads
    /// <see cref="BattleCardBase.Tribe"/>, whose getter folds in any skill-applied tribe CHANGE/ADD over
    /// <c>BaseParameter.Tribe</c> (and drops ALL when the resolved list has ≥2 entries) — so the wire carries
    /// the LIVE tribe, the faithful value over the static card-master one.
    /// <para>Same post-resolution zone search + degrade-to-<paramref name="fallback"/> contract as
    /// <see cref="PlayedCardClan"/>: no engine / no card → <paramref name="fallback"/> (default <c>"0"</c>, the
    /// prod no-tribe form — NEVER empty, which is wire-illegal: prod always sends tribe as a non-empty string,
    /// the client reads it via <c>item.Value.ToString()</c> at NetworkBattleReceiver.cs:2382). The degrade is
    /// LIVE, not dead: a session whose Setup failed (the ComputeFrames try/catch swallowed it, ND6) has
    /// <c>_mgr is null</c> yet still emits a knownList entry (the handler resolves the identity via the
    /// deck-map/mined fallback when the engine read degrades, so BuildPlayedCard still synthesizes an
    /// entry), so this path must hand back a legal wire value.</para></summary>
    public string PlayedCardTribe(bool playerSeat, int idx, string fallback = "0")
    {
        if (_mgr is null) return fallback;
        var card = FindByIndex(Seat(playerSeat), idx);
        if (card is null) return fallback;
        var tribe = card.Tribe;
        // Prod's no-tribe form is the single "0" (TribeType.ALL == 0), never an empty string; an empty list
        // (defensive) renders the same "0".
        return tribe is null || tribe.Count == 0
            ? "0"
            : string.Join(",", tribe.Select(t => (int)t));
    }

    // Locate the card with the given engine Index across the seat's post-resolution zones. Order matters
    // only for disambiguation; Index is unique per card so the first hit is the card. In-play (followers)
    // and cemetery (spells) are where a just-resolved play lands; hand is the pre-resolution fallback.
    private static BattleCardBase? FindByIndex(BattlePlayerBase seat, int idx)
    {
        foreach (var c in seat.ClassAndInPlayCardList)
            if (c.Index == idx) return c;
        foreach (var c in seat.CemeteryList)
            if (c.Index == idx) return c;
        foreach (var c in seat.HandCardList)
            if (c.Index == idx) return c;
        return null;
    }

    /// <summary>TEST SEAM (M-HC-3a validation): register a cost-reducing modifier on the hand card at
    /// engine <c>Index</c> == <paramref name="idx"/>, mimicking what card 101314020's <c>when_spell_charge</c>
    /// <c>cost_change add=ADD_CHARGE_COUNT*-1</c> skill does once it has accumulated <paramref name="charge"/>
    /// spellboost charges (each charge adds a <c>CostAddModifier(-1)</c>; the engine's own
    /// <see cref="Skill_cost_change"/> builds exactly this). Used to drive the count→cost resolution
    /// deterministically headless without pumping the (VFX-coupled) spell-charge skill chain through a
    /// real multi-spell sequence — the engine's authentic <see cref="BattleCardBase.Cost"/> getter then
    /// resolves the discount, and <see cref="BattleCardBase.PlayCard"/> captures it as PlayedCost on the
    /// next play. Returns the resolved hand-card Cost AFTER seeding (base − charge) for the caller to pin.
    /// No-op-returns -1 if the engine isn't set up or no hand card has that Index.</summary>
    internal int SeedHandCardSpellboostCost(bool playerSeat, int idx, int charge)
    {
        if (_mgr is null) return -1;
        BattleCardBase? card = null;
        foreach (var c in Seat(playerSeat).HandCardList)
            if (c.Index == idx) { card = c; break; }
        if (card is null) return -1;
        for (int i = 0; i < charge; i++)
            card.AddCostModifier(new CostAddModifier(-1), null, eventCall: false);
        card.SetSpellChargeCount(charge); // keep the charge count consistent with the modifiers (cosmetic here)
        return card.Cost;
    }

    // === TEST/DEBUG SEAMS (Phase 4 root-cause verification — NOT a production fix) =================
    // These exist solely to PROVE the post-mulligan reshuffle root cause from a test. They read/poke
    // the engine's XorShift idx-change RNG, which the live recovery path leaves null/inactive (seed -1).
    // No production code path calls them. Remove (or fold into the real seeding) when the fix lands.

    /// <summary>TEST/DEBUG: is the engine's SELF-seat XorShift idx-change RNG active? Mirrors the gate the
    /// post-mulligan deck reshuffle/re-index checks (<c>BattleMgr.XorShiftRandom(true) != null &amp;&amp;
    /// .IsActive</c>, BattlePlayerBase.cs:3049/3073). Under the live recovery setup
    /// (<c>CreateXorShift(-1,-1)</c> via NullRecoveryManager.IdxChangeSeed == -1) this is FALSE, so the
    /// engine SKIPS the reshuffle the real clients performed.</summary>
    internal bool SelfXorShiftActive
    {
        get => (_mgr?.XorShiftRandom(isSelf: true)?.IsActive) ?? false;
    }

    /// <summary>TEST/DEBUG: same as <see cref="SelfXorShiftActive"/> for the OPPONENT seat.</summary>
    internal bool OppoXorShiftActive
    {
        get => (_mgr?.XorShiftRandom(isSelf: false)?.IsActive) ?? false;
    }

    /// <summary>DIAGNOSTIC: check if OnReceiveDeal is wired and report deck/hand counts.</summary>
    internal string DiagnoseDealState()
    {
        if (_mgr is null) return "mgr=null";
        var or = _mgr.OperateReceive;
        bool dealWired = or.OnReceiveDeal != null;
        var p = _mgr.GetBattlePlayer(true);
        var e = _mgr.GetBattlePlayer(false);
        return $"OnReceiveDeal={(dealWired ? "wired" : "NULL")}, " +
               $"playerDeck={p.DeckCardList.Count}, playerHand={p.HandCardList.Count}, " +
               $"enemyDeck={e.DeckCardList.Count}, enemyHand={e.HandCardList.Count}";
    }

    /// <summary>Seed the opponent seat's XorShift for post-mulligan deck reshuffle. The Ready frame's
    /// <c>idxChangeSeed</c> seeds the self seat (BattlePlayer/A) automatically via the receiver. The
    /// opponent seat (BattleEnemy/B) needs its seed injected separately because the Ready frame sent
    /// to A doesn't carry B's seed. Called from <see cref="BattleSession.ShadowFeedServerFrames"/>
    /// after feeding the Ready.</summary>
    internal void SeedOppoIdxChange(int oppoSeed)
    {
        _mgr?.CreateXorShift(-1, oppoSeed);
    }

    /// <summary>TEST/DEBUG: inject BOTH per-seat idxChange seeds at once (the verification seam the
    /// PostMulliganReshuffleRootCauseTests use). Production code uses the Ready frame for the self
    /// seed + <see cref="SeedOppoIdxChange"/> for the opponent seed.</summary>
    internal void DebugSeedIdxChange(int selfSeed, int oppoSeed)
    {
        if (_mgr is null) throw new InvalidOperationException("DebugSeedIdxChange before Setup.");
        _mgr.CreateXorShift(selfSeed, oppoSeed);
    }

    /// <summary>TEST/DEBUG: override the engine's process-global <c>BattleManagerBase.IsRandomDraw</c>
    /// flag. Production Setup now sets this true (matching the real match-load's
    /// <c>SetupInitialGameState(areCardsRandomlyDrawn:true)</c>). This seam exists so tests can
    /// force it false to reproduce the old top-of-deck bug. Static field → set per run under
    /// [NonParallelizable].</summary>
    internal void DebugSetRandomDraw(bool value)
    {
        // Phase-5 chunk 43: was ambient-scoped BattleManagerBase.IsRandomDraw = value; now
        // writes the mgr instance directly. _mgr is the session's authoritative mgr.
        if (_mgr is not null) _mgr.InstanceIsRandomDraw = value;
    }

    /// <summary>TEST/DEBUG (Phase 4 draw-recompute hypothesis): advance the SHARED <c>_stableRandom</c>
    /// stream by <paramref name="n"/> draws, exactly as <c>OperateReceive.StartOperate</c> does on a
    /// received frame carrying <c>spin=n</c> (OperateReceive.cs:80-83 loops <c>StableRandomDouble()</c>
    /// n times). The live shadow never ingests the Ready frame that carries the wire spin, so its stream
    /// is offset; this applies the pre-roll at the same point the real client would.</summary>
    internal void DebugSpinPreroll(int n)
    {
        if (_mgr is null) throw new InvalidOperationException("DebugSpinPreroll before Setup.");
        for (int i = 0; i < n; i++) _mgr.StableRandomDouble();
    }

    /// <summary>TEST/DEBUG: consume one value from the shared <c>_stableRandom</c> stream and return it.
    /// Lets a regression test assert engine seed alignment against the wire — the very first
    /// <c>StableRandom.NextDouble()</c> the engine produces must equal the first <c>NextDouble()</c> of a
    /// fresh <c>System.Random(BattleSeeds.Stable(masterSeed))</c>, since clients seed
    /// <c>_stableRandom = new System.Random(Matched.seed)</c> with the SAME value
    /// (BattleManagerBase.cs:721; Matched.seed == BattleSeeds.Stable(masterSeed),
    /// InitBattleHandler.cs:28).</summary>
    internal double DebugStableRandomDouble()
    {
        if (_mgr is null) throw new InvalidOperationException("DebugStableRandomDouble before Setup.");
        return _mgr.StableRandomDouble();
    }

    /// <summary>TEST/DEBUG: read the per-seat <c>cardTotalNum</c> counter that drives auto-assigned
    /// Index for skill-generated tokens (BattleManagerBase.SetupCardIndex uses this when
    /// <c>addIndex == -1</c>). After Setup it must equal <c>deck.Count + 1</c> on both seats (matches
    /// the real client's <c>SBattleLoad.InitPlayer</c> tail, SBattleLoad.cs:1292), so the FIRST
    /// generated token gets Index 41 — clear of deck-loaded indices 1..40 — and matches the wire
    /// <c>add.idx</c>. A stale value of 0 causes tokens to take Index 0, 1, ... and collide.</summary>
    internal int DebugCardTotalNum(bool playerSeat)
    {
        return _mgr is null ? -1 : _mgr.GetBattlePlayer(playerSeat).cardTotalNum;
    }

    /// <summary>TEST/DEBUG: the engine's running <c>StableRandom</c>/<c>StableRandomDouble</c> call count
    /// (private <c>BattleManagerBase.stableRandomCount</c>), so a divergence dump can report how far the
    /// shared stream has advanced at the moment of a mismatch.</summary>
    internal int DebugStableRandomCount
    {
        get
        {
                return _mgr is null ? -1
                : (int)(typeof(BattleManagerBase)
                    .GetField("stableRandomCount", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .GetValue(_mgr) ?? -1);
        }
    }

    private engine::BattlePlayerBase Seat(bool playerSeat) =>
        (_mgr ?? throw new InvalidOperationException("read before Setup")).GetBattlePlayer(playerSeat);

    private static NetworkBattleDefine.NetworkBattleURI MapUri(NetworkBattleUri uri)
        => Enum.Parse<NetworkBattleDefine.NetworkBattleURI>(uri.ToString());

    // The receiver reads keys via Enum.IsDefined over NetworkParameter and casts nested values to
    // List<object> / Dictionary<string,object>; the node decodes nested data as the nullable
    // List<object?> / Dictionary<string,object?>. Rebox to the non-nullable shape, dropping nulls
    // (the receiver presence-checks keys, so an absent key is the correct encoding of a null).
    private static Dictionary<string, object> ToEngineDict(Dictionary<string, object?>? entries)
    {
        var result = new Dictionary<string, object>();
        if (entries is null) return result;
        foreach (var (k, v) in entries)
            if (v is not null) result[k] = Rebox(v);
        return result;
    }

    private static object Rebox(object v) => v switch
    {
        Dictionary<string, object?> d => d.Where(kv => kv.Value is not null)
                                          .ToDictionary(kv => kv.Key, kv => Rebox(kv.Value!)),
        List<object?> l => l.Where(x => x is not null).Select(x => Rebox(x!)).ToList(),
        _ => v,
    };

    // --- headless wiring (production analogue of HeadlessFixture) -----------------------------------

    private static void InitLeaderLife(BattleManagerBase mgr, int life = DefaultLeaderLife)
    {
        ((ClassBattleCardBase)mgr.GetBattlePlayer(true).Class).InitBaseMaxLife(life);
        ((ClassBattleCardBase)mgr.GetBattlePlayer(false).Class).InitBaseMaxLife(life);
    }

    private static void InitCardTemplates(BattleManagerBase mgr)
    {
        mgr.SBattleLoad = new SBattleLoad
        {
            UnitCardTemplate = new CardTemplate(),
            SpellCardTemplate = new CardTemplate(),
            FieldCardTemplate = new CardTemplate(),
        };
        mgr.Battle3DContainer = new GameObject();
        mgr.CardHolder = new GameObject();
        mgr.ECardHolder = new GameObject();
        mgr.PCardPlace = new GameObject();
        mgr.ChoiceCardHolder = new GameObject();
        mgr.EvolveCardHolder = new GameObject();
    }

    // Seed the no-op UI refs the receive/turn cycle dereferences. Under IsRecovery the methods on
    // these (e.g. BattleUIContainer.DisableMenu) no-op, but the receiver still CALLS them, so the
    // references must be non-null. PlayerEmotion is the engine's own NullPlayerEmotion.
    private static void InitHeadlessViews(BattleManagerBase mgr)
    {
        mgr.BattleUIContainer = (BattleUIContainer)FormatterServices.GetUninitializedObject(typeof(BattleUIContainer));
        // Revealed-card creation (ReplaceReceivedCard.CreateActualCard -> CreateBaseCardGameObject)
        // clones the card prefab under _backGround.m_Battle3DContainer — a field distinct from
        // mgr.Battle3DContainer. Seed a no-op BackGround with a non-null container.
        var bg = (BackGroundBase)FormatterServices.GetUninitializedObject(typeof(BackGroundBase));
        SetProperty(bg, "m_Battle3DContainer", new GameObject());
        SetField(mgr, "_backGround", bg);
        // PlayerEmotion is declared on BattlePlayer (the player seat); BattleEnemy has none — set
        // where present.
        TrySetProperty(mgr.GetBattlePlayer(true), "PlayerEmotion", new NullPlayerEmotion());
        TrySetProperty(mgr.GetBattlePlayer(false), "PlayerEmotion", new NullPlayerEmotion());

        // The receive play path runs SetupActionProcessorEvent (BattlePlayerBase.cs:1431/1438), which
        // wires BattleMgr.DetailMgr.DetailPanelControl.UpdateCardDescription* into OnPlayComplete/
        // OnEvolutionComplete. DetailMgr is created in CreateManager but its panel controls are null
        // headless. Seed the engine's own NullDetailPanelControl no-op (IDetailPanelControl) + an
        // uninitialized SubDetailPanelControl (concrete DetailPanelControl, read on other action arms).
        mgr.DetailMgr.DetailPanelControl = new NullDetailPanelControl();
        mgr.DetailMgr.SubDetailPanelControl =
            (DetailPanelControl)FormatterServices.GetUninitializedObject(typeof(DetailPanelControl));
    }

    // Hold a strong reference to the wired mulligan phase: its StartDeal closure is what
    // OperateReceive.OnReceiveDeal invokes, and it stores the mulligan mgr/controls that seat the hand.
    private NetworkMulliganPhase? _mulliganPhase;

    // Wire the receive path's deal handler. In production the phase machine advances to
    // NetworkMulliganPhase, whose Setup/MulliganEventSetting wires OperateReceive.OnReceiveDeal ->
    // MulliganPhaseBase.StartDeal (NetworkMulliganPhase.cs:91). The node never pumps the phase machine
    // (BattleManagerBase.Update is never called), and the node's PhaseCreator yields no NetworkMulligan
    // phase anyway — so construct the phase directly and run MulliganEventSetting() to install that
    // delegate. The phase ctor's Initialize builds the player/opponent mulligan controls (PlayerMlgCtrl
    // via InitMulligan) off the no-op view leaves the shim GameObject lazily materializes. The DEAL
    // mutation (cards deck->hand) happens synchronously inside StartDeal -> CreateMulliganDealList +
    // DrawFirstMulliganCard; the VFX it returns are cosmetic (dropped by HeadlessConductorVfxMgr).
    private void WireMulliganPhase(HeadlessNetworkBattleMgr mgr)
    {
        // The phase ctor's Initialize does NGUITools.AddChild(Battle3DContainer,
        // GetPrefabMgr().Get("Prefab/UI/MulliganInfo")).GetComponent<MulliganInfoControl>(). PrefabMgr.Get
        // returns null for an unregistered prefab (engine logic — not editable), and AddChild(parent,
        // null) -> Instantiate(null) -> null -> NRE on GetComponent. Seed a no-op GameObject under that
        // key so AddChild clones it and the shim GameObject lazily materializes a no-op
        // MulliganInfoControl. Node seed (allowed); the control is never shown/updated headless.
        var prefab = new GameObject();
        SeedMulliganInfoControl(prefab);
        var prefabData = mgr.GameMgr.GetPrefabMgr().GetPrefabData();
        prefabData["Prefab/UI/MulliganInfo"] = prefab;

        var phase = new NetworkMulliganPhase(mgr, mgr.NetworkSender);
        phase.MulliganEventSetting();
        _mulliganPhase = phase;
    }

    // Materialize a no-op MulliganInfoControl on the prefab GameObject and seed the view-leaf fields the
    // phase ctor's PlayerMulliganView ctor -> MulliganInfoControl.InitMulliganInfo reads:
    //   _partsPlayer/_partsOpponent (private nested MulliganParts) — each needs a non-null _exchangeMark
    //     array (read for .Length in InitMulliganInfo) plus non-null _keepZone/_abandonZone UIWidgets
    //     (read for .gameObject elsewhere on the mulligan path).
    // The shim GameObject lazily creates the MulliganInfoControl but does NOT fill the MulliganParts
    // (it isn't a Component, so WireComponentFields skips it). Node seed (allowed) — pure no-op view leaves.
    private static void SeedMulliganInfoControl(GameObject prefab)
    {
        var ctrl = prefab.GetComponent<MulliganInfoControl>(); // Shim GameObject.GetComponent<T>() lazily materialises a no-op component — not a real Unity scene; this is intentional and will not NRE.
        var partsType = typeof(MulliganInfoControl)
            .GetNestedType("MulliganParts", BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("MulliganInfoControl.MulliganParts nested type not found");
        SetField(ctrl, "_partsPlayer", BuildMulliganParts(partsType));
        SetField(ctrl, "_partsOpponent", BuildMulliganParts(partsType));
    }

    private static object BuildMulliganParts(Type partsType)
    {
        var parts = FormatterServices.GetUninitializedObject(partsType);
        SetField(parts, "_exchangeMark", Array.CreateInstance(typeof(UISprite), 0));
        SetField(parts, "_keepZone", NewUiWidget());
        SetField(parts, "_abandonZone", NewUiWidget());
        return parts;
    }

    // A UIWidget is read for .gameObject (Component.gameObject) on the mulligan path; create one on a
    // fresh GameObject so its gameObject backref resolves.
    private static UIWidget NewUiWidget() => new GameObject().GetComponent<UIWidget>();

    /// <summary>Seat one side's full deck in order (idx == list position + 1). Each card is created
    /// through the engine's own null-view seam and pushed via AddToDeck — the SeedDeck primitive the
    /// test harness proved (HeadlessFixture.SeedDeck).
    /// <para>Mirrors the real client's <c>SBattleLoad.InitPlayer</c>/<c>InitEnemy</c> tail: after
    /// loading the 40-card deck at indices 1..40, set <c>cardTotalNum = deck.Count + 1</c> so the
    /// next skill-generated token gets Index 41 (matches the wire's <c>add.idx</c>). Without this,
    /// <c>cardTotalNum</c> stays at the property default (0) and the auto-assign path
    /// (<c>SetupCardIndex(_, -1)</c> in BattleManagerBase.cs:1770) hands tokens Index 0, 1, ...,
    /// which COLLIDES with deck-loaded cards' Index 1..40. The collision is silent until something
    /// plays the deck card with the colliding Index (e.g. Hoverboarder at deck idx 1 with a token
    /// at engine Index 1): <c>GetBattleCardIdx</c>'s <c>SingleOrDefault</c> finds two matches and
    /// throws "Sequence contains more than one matching element". Also pin
    /// <c>BattleStartDeckCardList</c> like the real client, so any skill that reads the starting
    /// deck (e.g. tribe filters) sees the seeded deck instead of an empty list.</para></summary>
    private static void SeedDeck(BattleManagerBase mgr, IReadOnlyList<long> deck, bool isPlayer)
    {
        BattlePlayerBase owner = mgr.GetBattlePlayer(isPlayer);
        for (int i = 0; i < deck.Count; i++)
        {
            var card = CreateHeadlessCard(mgr, (int)deck[i], index: i + 1, isPlayer);
            owner.AddToDeck(card);
        }
        owner.cardTotalNum = deck.Count + 1;
        owner.BattleStartDeckCardList = new List<BattleCardBase>(owner.DeckCardList);
    }

    private static readonly MethodInfo CreateCardWithoutResources =
        typeof(CardCreatorBase).GetMethod("CreateCardWithoutResources",
            BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("CardCreatorBase.CreateCardWithoutResources not found");

    private static BattleCardBase CreateHeadlessCard(BattleManagerBase mgr, int cardId, int index, bool isPlayer)
    {
        var io = mgr.CreatePlayerInnerOptionsBuilder();
        var card = (BattleCardBase)CreateCardWithoutResources.Invoke(
            null, new object[] { cardId, index, isPlayer, mgr, io })!;
        mgr.GetBattlePlayer(isPlayer).SetupCardEvent(card);
        return card;
    }

    // The turn-flow + emit bookkeeping reads the global ToolboxGame.RealTimeNetworkAgent (e.g.
    // RealTimeNetworkAgent.GetIsFirstPlayer/GetTurnState, which delegate to GameMgr's
    // NetworkUserInfoData.TurnState). Headless there is no socket agent, so seed a no-op one —
    // mirroring HeadlessFixture.NewNetworkEmitBattle. Since the engine RTA is now a stub with
    // no-op method bodies (pass-7 engine cleanup), the historical internal seeds (_gungnir field,
    // _notEmit short-circuit) are no longer needed — the stub can't NRE inside its own methods.
    // NOTE: this is a process-global; one engine per process is assumed for the shadow (revisit for
    // live multi-session — see design O-N status). Idempotent enough for the per-battle setup.
    private static void InstallHeadlessNetworkAgent(HeadlessNetworkBattleMgr mgr)
    {
        var agent = (RealTimeNetworkAgent)FormatterServices.GetUninitializedObject(typeof(RealTimeNetworkAgent));
        agent.SetCurrentMatchingStatus(RealTimeNetworkAgent.MatchingStatus.Prepared);
        SetProperty(agent, "NetworkLogger", new NetworkNullLogger());
        mgr.InstanceNetworkAgent = agent; // Phase-5 chunk 41: was ToolboxGame.SetRealTimeNetworkBattle(agent)
    }

    // Write the two seats' class ordinals into the SESSION's GameMgr DataMgr leader chara ids. Mirrors
    // the test seam HeadlessFixture.cs (SetField(dm, "_playerCharaId"/"_enemyCharaId", ...)). chara_id
    // == class_id for 1..8 in EngineGlobalInit's all-8-class ClassCharacterList, so the ordinal selects
    // the class. A non-positive ordinal (e.g. CardClass.None == 0) clamps to the default seat (1/2).
    // GameMgr is per-session (mgr.GameMgr, seeded pre-ctor from _gameMgr).
    private static void SetGameMgrCharaIds(GameMgr gm, int a, int b)
    {
        var dm = gm.GetDataMgr();
        SetField(dm, "_playerCharaId", a <= 0 ? 1 : a);
        SetField(dm, "_enemyCharaId", b <= 0 ? 2 : b);
    }

    private static void SetField(object obj, string name, object value)
    {
        var f = obj.GetType().GetField(name,
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            ?? throw new InvalidOperationException($"{obj.GetType().Name} has no field '{name}'");
        f.SetValue(obj, value);
    }

    private static void SetProperty(object obj, string name, object value)
    {
        var t = obj.GetType();
        PropertyInfo? p = null;
        while (t is not null && p is null)
        {
            p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            t = t.BaseType;
        }
        (p ?? throw new InvalidOperationException($"{obj.GetType().Name} has no property '{name}'"))
            .SetValue(obj, value);
    }

    private static void TrySetProperty(object obj, string name, object value)
    {
        var t = obj.GetType();
        while (t is not null)
        {
            var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (p is not null) { p.SetValue(obj, value); return; }
            t = t.BaseType;
        }
    }

    // === TEST/DEBUG: per-roll attribution probe (Phase 4 Option-A viability) =======================
    // Captures, at the EXACT moment of each StableRandom*/StableRandomDouble roll, the seat signals the
    // mgr can read from its own state, plus the live call stack. The decisive question: can the acting
    // seat be attributed from mgr STATE alone (a router could route on it), or only by reading the STACK?

    /// <summary>One recorded RNG roll. <paramref name="SelfIsSelfTurn"/>/<paramref name="OppoIsSelfTurn"/>
    /// are the mgr-readable seat-turn flags at roll time; <paramref name="Stack"/> is the trimmed call
    /// stack (the only place the acting seat is sometimes visible).</summary>
    internal sealed record RollEntry(
        int Index, string Api, int Arg,
        bool SelfIsSelfTurn, bool OppoIsSelfTurn,
        string Stack);

    // A logging IRandomSource: delegates to the real seeded source but records each roll. Reads the mgr's
    // seat-turn flags (the richest seat signal a mgr-level StableRandom override can see — there is no
    // "current operating seat" field on the mgr) and the call stack at the call site.
    private sealed class RollLoggingRandomSource : IRandomSource
    {
        private readonly IRandomSource _inner;
        private readonly List<RollEntry> _log;
        private readonly Func<HeadlessNetworkBattleMgr?> _mgr;
        private int _i;

        public RollLoggingRandomSource(IRandomSource inner, List<RollEntry> log, Func<HeadlessNetworkBattleMgr?> mgr)
        {
            _inner = inner; _log = log; _mgr = mgr;
        }

        public double NextUnit() { Record("NextUnit", -1); return _inner.NextUnit(); }
        public int NextSelf(int max) { Record("NextSelf", max); return _inner.NextSelf(max); }

        private void Record(string api, int arg)
        {
            bool selfTurn = false, oppoTurn = false;
            try
            {
                var mgr = _mgr();
                if (mgr is not null)
                {
                    selfTurn = mgr.GetBattlePlayer(true).IsSelfTurn;
                    oppoTurn = mgr.GetBattlePlayer(false).IsSelfTurn;
                }
            }
            catch { /* read-only probe; never let a state read abort the roll */ }

            string stack = TrimStack(System.Environment.StackTrace);
            _log.Add(new RollEntry(_i++, api, arg, selfTurn, oppoTurn, stack));
        }

        // Keep the frames that reveal WHO is rolling (mulligan lottery vs draw vs filter vs spin pre-roll),
        // dropping the logger's own frames and System.Environment.
        private static string TrimStack(string raw)
        {
            var lines = (raw ?? "").Split('\n')
                .Select(s => s.Trim())
                .Where(s => s.Length > 0
                    && !s.Contains("RollLoggingRandomSource")
                    && !s.Contains("Environment.get_StackTrace")
                    && !s.Contains("Environment.GetStackTrace"))
                .Select(Shorten)
                .Take(8);
            return string.Join(" <- ", lines);
        }

        // "at Namespace.Type.Method(args) in file:line N" -> "Type.Method" (keep it scannable).
        private static string Shorten(string frame)
        {
            string s = frame.StartsWith("at ") ? frame.Substring(3) : frame;
            int paren = s.IndexOf('(');
            if (paren >= 0) s = s.Substring(0, paren);
            var parts = s.Split('.');
            return parts.Length >= 2 ? parts[^2] + "." + parts[^1] : s;
        }
    }
}
