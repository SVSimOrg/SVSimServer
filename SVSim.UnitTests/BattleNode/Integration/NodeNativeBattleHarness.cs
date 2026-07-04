using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Lifecycle;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Sessions;
using SVSim.BattleNode.Sessions.Dispatch;
using SVSim.BattleNode.Sessions.Engine;
using SVSim.BattleNode.Sessions.Participants;

namespace SVSim.UnitTests.BattleNode.Integration;

/// <summary>
/// Node-native battle harness for the Headless-Conductor milestones (M-HC-*). It reproduces what
/// <c>BattleSession.EnsureEngineSetup</c> does — shuffle each side's deck from a FIXED master seed and
/// <c>SessionBattleEngine.Setup</c> the two seats — then exposes the engine + state + participants so
/// later milestone tests can drive multi-frame sequences and assert on engine board state.
///
/// <para>WHY drive the engine directly (not a full <c>BattleSession</c>): the session's <c>_state</c>
/// and <c>_engine</c> are private with no fixed-seed injection point, and every milestone assertion is
/// on engine board state. The engine (<c>SessionBattleEngine</c>) is the unit under test, so we seat it
/// the same way the session does and skip the WS/dispatch scaffolding.</para>
///
/// <para>The oracle by construction: the node assigns idx = position in the shuffled order
/// (<see cref="BattleSessionState.GetShuffledDeck"/>), and the engine's headless draw is lowest-Index
/// first, so a FIXED seed makes the engine's draw order reproduce the node's BY CONSTRUCTION.</para>
///
/// <para>Engine globals (<c>CardMaster</c>, <c>GameMgr</c>, <c>Wizard.Data</c>) are primed by
/// <c>SessionBattleEngine.Setup</c> itself (it calls <c>EngineGlobalInit.EnsureInitialized()</c>, which
/// loads the full cards.json from <c>AppContext.BaseDirectory/Data/cards.json</c>). The harness adds no
/// global init of its own. Per-battle state is isolated via the engine's per-session
/// <c>BattleAmbientContext</c> (Task 7 of multi-instancing migration), so the historical
/// single-active-engine gate is gone — concurrent harnesses + sessions are now safe.</para>
/// </summary>
internal sealed class NodeNativeBattleHarness : IDisposable
{
    /// <summary>A deterministic master seed so deck shuffles (and the engine RNG stream born from it)
    /// are reproducible. Matches the value the engine construction tests use.</summary>
    public const int FixedMasterSeed = 12345;

    /// <summary>Default seat A viewer id — distinct from <see cref="DefaultSeatBViewerId"/> so the two
    /// sides shuffle independently (the shuffle seed mixes in the viewer id).</summary>
    public const long DefaultSeatAViewerId = 1001;
    public const long DefaultSeatBViewerId = 1002;

    /// <summary>Spellboost cost-reducer card (looking ahead to M-HC-3). Known id present in cards.json
    /// (sourced from tk2 battle capture / existing engine tests); a cards.json regeneration that drops
    /// it will produce a traceable failure here.</summary>
    public const long SpellboostCardId = 101314020;

    /// <summary>A second spellboost card seen in the tk2 capture. Known id present in cards.json
    /// (sourced from tk2 battle capture / existing engine tests); a cards.json regeneration that drops
    /// it will produce a traceable failure here.</summary>
    public const long SpellboostCardIdAlt = 100314020;

    /// <summary>A plain vanilla follower the engine resolution path proved out
    /// (HeadlessFixture.FollowerId). The bulk of the deterministic deck. Known id present in cards.json
    /// (sourced from tk2 battle capture / existing engine tests); a cards.json regeneration that drops
    /// it will produce a traceable failure here.</summary>
    public const long VanillaFollowerId = 100011010;

    /// <summary>A SECOND, distinct cost-1 vanilla follower (char_type 1, cost 1, no skill) — present +
    /// creatable in cards.json. Used by the opponent-reveal substitution test as the WIRE cardId that
    /// must override a seeded identity (it is deliberately NOT in any harness deck, so its only route
    /// onto the board is a reveal). Named here so card-id provenance stays traceable as ids accumulate
    /// (Task-4 review nit promoted in M-HC-3).</summary>
    public const long AltVanillaFollowerId = 101211120;

    /// <summary>A truly skill-less cost-1 vanilla follower with attack &gt;= life (a 1/1), so a mutual
    /// follower-vs-follower attack is a LETHAL trade (each deals 1, each has 1 life → both die). The
    /// proven vanillas <see cref="VanillaFollowerId"/>/<see cref="AltVanillaFollowerId"/> are 1/2, so they
    /// survive a single trade — this id is the one that exercises the death/removal arm of an attack
    /// (M-HC-4a follower trade). Present + creatable in cards.json (no skill, char_type 1, cost 1, 1/1).</summary>
    public const long VanillaOneOneFollowerId = 900011080;

    /// <summary>A SIMPLE single-target when_play DAMAGE spell (M-HC-4c fixture). cards.json id 100414020:
    /// char_type 4 (spell), clan 4 (Dragoncraft), cost 1, skill <c>damage</c> / skill_timing
    /// <c>when_play</c> / skill_target <c>character=op&amp;target=inplay&amp;card_type=unit&amp;select_count=1</c>
    /// / skill_option <c>damage=2</c> — i.e. "deal 2 damage to a selected enemy follower". Concrete sane
    /// cost (1), no board-state-dependent magnitude, no condition beyond an enemy unit existing — the
    /// cleanest targeted-play fixture in the current dump. Present + creatable in cards.json.</summary>
    public const long SingleTargetDamageSpellId = 100414020;

    /// <summary>The flat damage magnitude of <see cref="SingleTargetDamageSpellId"/> (skill_option
    /// <c>damage=2</c>). The targeted-play test asserts the enemy follower's life drops by exactly this.</summary>
    public const int SingleTargetDamageAmount = 2;

    /// <summary>A high-life vanilla follower (M-HC-4c damage TARGET). cards.json id 101411060: char_type 1,
    /// clan 4, cost 2, 1/4, no skill. A 1/4 body takes <see cref="SingleTargetDamageAmount"/> (2) and
    /// SURVIVES at life 2 — so the targeted-damage assertion reads a clean life DROP (not a death/removal,
    /// which would only prove BoardCount). Present + creatable in cards.json.</summary>
    public const long HighLifeVanillaFollowerId = 101411060;

    /// <summary>Base life of <see cref="HighLifeVanillaFollowerId"/> (4). Pre-damage pin for the target.</summary>
    public const int HighLifeVanillaFollowerLife = 4;

    /// <summary>A SIMPLE CHOICE card (M-HC-4c choice fixture). cards.json id 127011010: char_type 1
    /// (follower), clan 0 (Neutral — playable under any seat class), cost 1, 1/2, skill
    /// <c>choice,token_draw</c> / skill_timing <c>when_choice_play,when_play</c> / skill_option
    /// <c>card_id=121011010:120011010,...</c> — i.e. "choose ONE of two tokens to add to hand"
    /// (<see cref="ChoiceTokenA"/> / <see cref="ChoiceTokenB"/>). The choice OUTCOME is directly
    /// observable: the chosen token lands in the caster's hand, so a test can assert which branch
    /// resolved by the new hand card's identity. (The token resolves into HAND — confirmed against the
    /// capture's <c>orderList.add{to:20}</c> hand-zone op — despite the skill_option <c>summon_side=me</c>
    /// superficially reading like a summon-to-board.) Present + creatable in cards.json.</summary>
    public const long ChoiceCardId = 127011010;

    /// <summary>The first choice option of <see cref="ChoiceCardId"/> (token added to hand).</summary>
    public const long ChoiceTokenA = 121011010;

    /// <summary>The second choice option of <see cref="ChoiceCardId"/> (token added to hand).</summary>
    public const long ChoiceTokenB = 120011010;

    /// <summary>A BOARD-DEPENDENT cost-reducer follower (M-HC-4d fixture). cards.json id 127011020:
    /// char_type 1 (follower), clan 0 (Neutral — playable under any seat class), base cost 6, 3/3, skill
    /// <c>cost_change,rush</c> / skill_timing <c>when_evolve_other,when_change_inplay</c> / skill_option
    /// <c>set=1,none</c> / skill_condition (cost_change) <c>turn=self&amp;{me.hand_self.unit.count}&gt;0&amp;
    /// character=me&amp;target=evolution_card&amp;card_type=unit</c> / skill_target <c>character=me&amp;target=self
    /// &amp;card_type=unit</c> — i.e. "WHILE in hand, when ANOTHER of your followers evolves on your turn (and you
    /// hold at least one other unit in hand), SET this card's cost to 1." The engine's evolve path
    /// (<c>UnitBattleCard</c> non-skill evolve) scans the evolving player's HAND for cards whose skills have
    /// <c>OnWhenEvolveOtherStart != 0</c> and registers them via <c>SkillCollectionBase.CreateWhenEvolveOtherInfo</c>;
    /// <c>Skill_cost_change</c> then applies a <c>CostSetModifier(1)</c> to this card, so its resolved
    /// <c>Cost</c> drops 6 → 1. Because the node reads opponent-facing cost straight off the resolved engine
    /// (<c>SessionBattleEngine.PlayedCardCost</c>, M-HC-3), this board-dependent reduction is captured BY
    /// CONSTRUCTION once evolve resolves headless (M-HC-4b) — this card validates that. Present + creatable in
    /// cards.json.</summary>
    public const long BoardDependentCostCardId = 127011020;

    /// <summary>Base cost of <see cref="BoardDependentCostCardId"/> (6) — the pre-evolve resolved cost.</summary>
    public const int BoardDependentCostBase = 6;

    /// <summary>The flat cost <see cref="BoardDependentCostCardId"/> resolves to AFTER another follower evolves
    /// on the controller's turn (skill_option <c>set=1</c> → <c>CostSetModifier(1)</c>). Independent of how many
    /// followers evolved (a SET, not an add) — exactly 1.</summary>
    public const int BoardDependentCostReduced = 1;

    /// <summary>A non-trivial CLAN+TRIBE fixture follower (M-HC-4e). cards.json id 900231030: char_type 1
    /// (follower), clan 2 (ROYAL / Swordcraft), tribe 2 (LEGION), cost 0, 2/2. Cost 0 makes it playable on
    /// turn-1 PP 1; its clan (2) matches <see cref="CardClass.Swordcraft"/> so it is legal under a Swordcraft
    /// seat. Its clan/tribe (2 / "2") are concretely non-zero so the engine-sourced clan/tribe read +
    /// knownList emit assert REAL values (not the 0/"0" no-tribe default). Verified against cards.json AND the
    /// prod wire form (comma-joined int TribeType as string: tribe 2 → "2"). Present + creatable in
    /// cards.json.</summary>
    public const long ClanTribeFollowerId = 900231030;

    /// <summary>The engine-resolved clan of <see cref="ClanTribeFollowerId"/> as the wire int (ROYAL ==
    /// ClanType 2). The M-HC-4e knownList emit asserts <c>clan</c> equals this.</summary>
    public const int ClanTribeFollowerClan = 2;

    /// <summary>The engine-resolved tribe of <see cref="ClanTribeFollowerId"/> in the EXACT prod wire string
    /// form (LEGION == TribeType 2 → the single-element comma-join "2"). The M-HC-4e knownList emit asserts
    /// <c>tribe</c> equals this.</summary>
    public const string ClanTribeFollowerTribe = "2";

    public BattleSessionState State { get; }
    public StubParticipant SeatA { get; }
    public StubParticipant SeatB { get; }
    public SessionBattleEngine Engine { get; }

    /// <summary>This side's deck in the node's shuffled order (idx == position + 1).</summary>
    public IReadOnlyList<long> SeatADeck { get; }
    public IReadOnlyList<long> SeatBDeck { get; }

    private NodeNativeBattleHarness(
        BattleSessionState state, StubParticipant a, StubParticipant b, SessionBattleEngine engine,
        IReadOnlyList<long> seatADeck, IReadOnlyList<long> seatBDeck)
    {
        State = state;
        SeatA = a;
        SeatB = b;
        Engine = engine;
        SeatADeck = seatADeck;
        SeatBDeck = seatBDeck;
    }

    /// <summary>Build a 30-card deck: mostly the vanilla follower plus a couple of spellboost cards
    /// (so later milestones have a cost-reducer to play). All ids exist in cards.json.</summary>
    public static IReadOnlyList<long> DefaultDeck()
    {
        var deck = new List<long>(30) { SpellboostCardId, SpellboostCardIdAlt };
        deck.AddRange(Enumerable.Repeat(VanillaFollowerId, 30 - deck.Count));
        return deck;
    }

    /// <summary>A deck for the M-HC-4d board-dependent-cost test: an alternating mix of the vanilla
    /// follower (to play turn 1 and EVOLVE on seat A's evolve turn) and the <see cref="BoardDependentCostCardId"/>
    /// (the <c>when_evolve_other set=1</c> cost-reducer that must sit IN HAND across the evolve). Alternating
    /// 15/15 guarantees BOTH identities populate the opening hand + early draws regardless of the fixed shuffle;
    /// the test locates each by identity (not a shuffle-dependent position). The cost-reducer's condition
    /// <c>{me.hand_self.unit.count}&gt;0</c> (another unit in hand) is satisfied because copies of BOTH followers
    /// remain in hand at the evolve.</summary>
    public static IReadOnlyList<long> BoardDependentCostDeck()
    {
        var deck = new List<long>(30);
        for (int i = 0; i < 15; i++) { deck.Add(VanillaFollowerId); deck.Add(BoardDependentCostCardId); }
        return deck;
    }

    /// <summary>A 30-card deck of the <see cref="ClanTribeFollowerId"/> clan+tribe fixture (M-HC-4e). All one
    /// identity, all cost 0 — so the opening hand reliably holds a copy to play turn 1, regardless of shuffle,
    /// and the engine-resolved clan/tribe read off the played card is unambiguous.</summary>
    public static IReadOnlyList<long> ClanTribeDeck() =>
        Enumerable.Repeat(ClanTribeFollowerId, 30).ToList();

    /// <summary>Seat the engine exactly as <c>BattleSession.EnsureEngineSetup</c> does: shuffle each
    /// side's deck from the fixed seed via <see cref="BattleSessionState.GetShuffledDeck"/>, then
    /// <c>SessionBattleEngine.Setup(seed, deckA, deckB, classA, classB)</c>.</summary>
    public static NodeNativeBattleHarness Create(
        IReadOnlyList<long>? seatADeck = null,
        IReadOnlyList<long>? seatBDeck = null,
        CardClass seatAClass = CardClass.Forestcraft,
        CardClass seatBClass = CardClass.Swordcraft,
        int masterSeed = FixedMasterSeed)
    {
        var state = new BattleSessionState(masterSeed);

        var a = new StubParticipant(DefaultSeatAViewerId, MakeCtx(seatADeck ?? DefaultDeck(), seatAClass));
        var b = new StubParticipant(DefaultSeatBViewerId, MakeCtx(seatBDeck ?? DefaultDeck(), seatBClass));

        var shuffledA = state.GetShuffledDeck(a);
        var shuffledB = state.GetShuffledDeck(b);

        var engine = new SessionBattleEngine();
        // Mirror BattleSession.EnsureEngineSetup: engine's StableRandom is seeded with
        // BattleSeeds.Stable(MasterSeed), the value the Matched frame ships to clients
        // (InitBattleHandler.cs:28). See BattleSession.cs for the full root-cause comment.
        engine.Setup(BattleSeeds.Stable(state.MasterSeed), shuffledA, shuffledB,
            (int)a.Context.ClassId, (int)b.Context.ClassId);

        return new NodeNativeBattleHarness(state, a, b, engine, shuffledA, shuffledB);
    }

    private static MatchContext MakeCtx(IReadOnlyList<long> deck, CardClass cls) => new(
        SelfDeckCardIds: deck,
        ClassId: cls, CharaId: ((int)cls).ToString(), CardMasterName: "card_master_node_10015",
        CountryCode: CountryCodes.Korea, UserName: "Player", SleeveId: "3000011",
        EmblemId: "701441011", DegreeId: "300003", FieldId: 43, IsOfficial: 0,
        BattleModeId: BattleModes.TakeTwo);

    // --- engine board-state pass-throughs (seat:true == player A, false == opponent B) ----------

    public bool IsReady => Engine.IsReady;
    public int LeaderLife(bool playerSeat) => Engine.LeaderLife(playerSeat);
    public int Pp(bool playerSeat) => Engine.Pp(playerSeat);
    public int HandCount(bool playerSeat) => Engine.HandCount(playerSeat);
    public int BoardCount(bool playerSeat) => Engine.BoardCount(playerSeat);
    public int DeckCount(bool playerSeat) => Engine.DeckCount(playerSeat);
    public int Turn(bool playerSeat) => Engine.Turn(playerSeat);

    /// <summary>The engine-resolved wire <c>cardId</c> of the card at engine <paramref name="idx"/> on the
    /// given seat (M-HC-4f). Pass-through to <c>SessionBattleEngine.PlayedCardId</c> — the TRUE id the engine
    /// seated (deck id / token id / chosen-token id / copied id), the value the handler now sources for the
    /// opponent-facing knownList instead of the wire-mined map.</summary>
    public long PlayedCardId(bool playerSeat, int idx, long fallback = 0) => Engine.PlayedCardId(playerSeat, idx, fallback);

    /// <summary>The engine Index of seat A's hand card at <paramref name="handPos"/> (the playIdx a
    /// Play frame would carry to play it).</summary>
    public int PlayerHandCardIndex(int handPos) => Engine.HandCardIndex(playerSeat: true, handPos);

    /// <summary>The wire CardId of the hand card at <paramref name="handPos"/> on the given seat. Lets a
    /// test find a specific card (e.g. the spellboost reducer) in a shuffled opening hand by identity.</summary>
    public int HandCardId(bool playerSeat, int handPos) => Engine.HandCardId(playerSeat, handPos);

    /// <summary>The engine Index of the hand card at <paramref name="handPos"/> on the given seat.</summary>
    public int HandCardIndex(bool playerSeat, int handPos) => Engine.HandCardIndex(playerSeat, handPos);

    /// <summary>TEST/DEBUG: pull one value from the engine's shared <c>_stableRandom</c> stream. Mirrors the
    /// engine's <see cref="SessionBattleEngine.DebugStableRandomDouble"/> seam; lets a regression test
    /// assert seed alignment with the wire (clients seed their <c>_stableRandom</c> with the Matched.seed,
    /// which is <c>BattleSeeds.Stable(masterSeed)</c>).</summary>
    public double DebugStableRandomDouble() => Engine.DebugStableRandomDouble();

    /// <summary>TEST/DEBUG: read the seat's auto-assign Index counter (<c>cardTotalNum</c>). After
    /// Setup it must equal <c>deck.Count + 1</c> so the next skill-generated token gets an Index
    /// clear of the deck-loaded 1..40 (= the real client's SBattleLoad behavior).</summary>
    public int DebugCardTotalNum(bool playerSeat) => Engine.DebugCardTotalNum(playerSeat);

    /// <summary>The real wire <c>CardId</c> of the in-play follower at <paramref name="boardPos"/> on the
    /// given seat (0-based, leader excluded). Asserts an opponent reveal seated the substituted identity
    /// (M-HC-2).</summary>
    public int InPlayCardId(bool playerSeat, int boardPos) => Engine.InPlayCardId(playerSeat, boardPos);

    /// <summary>The engine <c>Index</c> of the in-play follower at <paramref name="boardPos"/> — the wire
    /// <c>playIdx</c> an ATTACK frame carries to address that follower as the attacker (M-HC-4a).</summary>
    public int InPlayCardIndex(bool playerSeat, int boardPos) => Engine.InPlayCardIndex(playerSeat, boardPos);

    /// <summary>The current life/health of the in-play follower at <paramref name="boardPos"/>.</summary>
    public int InPlayCardLife(bool playerSeat, int boardPos) => Engine.InPlayCardLife(playerSeat, boardPos);

    /// <summary>The attack stat of the in-play follower at <paramref name="boardPos"/>.</summary>
    public int InPlayCardAtk(bool playerSeat, int boardPos) => Engine.InPlayCardAtk(playerSeat, boardPos);

    /// <summary>True while the in-play follower at <paramref name="boardPos"/> can still attack this turn.</summary>
    public bool InPlayCardAttackable(bool playerSeat, int boardPos) => Engine.InPlayCardAttackable(playerSeat, boardPos);

    /// <summary>True once the in-play follower at <paramref name="boardPos"/> has evolved (M-HC-4b).</summary>
    public bool IsEvolved(bool playerSeat, int boardPos) => Engine.IsEvolved(playerSeat, boardPos);

    /// <summary>The seat's current evolve-point count (M-HC-4b). An evolve spends one EP.</summary>
    public int EpCount(bool playerSeat) => Engine.EpCount(playerSeat);

    /// <summary>Turns remaining until the seat may evolve (0 == unlocked) (M-HC-4b).</summary>
    public int EvolveWaitTurnCount(bool playerSeat) => Engine.EvolveWaitTurnCount(playerSeat);

    // --- TEST/DEBUG seams (Phase 4 root-cause verification: post-mulligan reshuffle) ---------------

    /// <summary>TEST/DEBUG: is the engine's SELF-seat XorShift idx-change RNG active (the gate the
    /// post-mulligan reshuffle checks)? Live recovery setup leaves it FALSE.</summary>
    public bool SelfXorShiftActive => Engine.SelfXorShiftActive;

    /// <summary>TEST/DEBUG: opponent-seat XorShift active state.</summary>
    public bool OppoXorShiftActive => Engine.OppoXorShiftActive;

    /// <summary>TEST/DEBUG: inject the per-seat idxChange seeds (call before the Ready mulligan-end frame
    /// to activate the engine's own post-mulligan reshuffle).</summary>
    public void DebugSeedIdxChange(int selfSeed, int oppoSeed) => Engine.DebugSeedIdxChange(selfSeed, oppoSeed);

    /// <summary>Build an envelope for <paramref name="body"/> and ingest it into the engine for the
    /// given seat (player == seat A). Mirrors <c>BattleNodeFlowTests.MakeEnvelopeWith</c> +
    /// <c>SessionBattleEngine.Receive</c>.</summary>
    public EngineIngestResult Push(NetworkBattleUri uri, Dictionary<string, object?> body, bool isPlayerSeat)
    {
        var seat = isPlayerSeat ? SeatA : SeatB;
        var env = new MsgEnvelope(
            uri, ViewerId: seat.ViewerId, Uuid: "udid-test", Bid: null, RetryAttempt: 0,
            Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null,
            Body: new RawBody(body));
        return Engine.Receive(env, isPlayerSeat);
    }

    /// <summary>The engine's <c>NetworkBattleDefine.PlayActionType.ATTACK</c> opcode — confirmed
    /// <c>= 10</c> in <c>SVSim.BattleEngine/Engine/NetworkBattleDefine.cs</c> (NOT 31, which is
    /// PLAY_HAND_SELECT). The receiver maps the wire <c>type</c> int straight to the enum
    /// (NetworkBattleReceiver.cs:1093).</summary>
    public const int AttackOpcode = 10;

    // NOTE (live-fidelity migration): the target-builders below emit the REAL client wire shape —
    // a sender-relative <c>isSelf</c> flag on each targetList entry — NOT the engine's internal
    // <c>vid</c> stamp. Real client-sent attack/evolve/targeted-play frames carry
    // <c>{targetIdx, isSelf, selectSkillIndex}</c> (verified in the client-send captures, e.g.
    // data_dumps/captures/battle_test/battle-traffic_cl1.ndjson); the previous <c>vid</c> shape was a
    // harness workaround that masked a missing ingest translation. SessionBattleEngine.Receive now
    // translates isSelf → the engine vid on the engine's OWN dict copy (the engine's IsRecovery target
    // parse derives owner from <c>vid != PlayerStaticData.UserViewerID</c>, NetworkBattleReceiver.cs:2186),
    // so the harness drives the live contract end-to-end.
    //
    // isSelf is relative to the FRAME's SENDER: <c>isSelf:1</c> = the target sits on the sender's own
    // seat; <c>isSelf:0</c> = it sits on the OTHER seat. The builders take <paramref name="targetOnEnemySeat"/>
    // (stable signature) and map it to <c>isSelf:0</c> (true) / <c>isSelf:1</c> (false), since every
    // builder is driven by seat A attacking/targeting seat B's card (targetOnEnemySeat:true) or its own
    // (false).
    private static long IsSelfFlag(bool targetOnEnemySeat) => targetOnEnemySeat ? 0 : 1;

    /// <summary>Build a PlayActions ATTACK frame in the REAL client wire shape. <paramref name="attackerIdx"/>
    /// is the attacker's in-play engine <c>Index</c> (the wire <c>playIdx</c>); the target is described in
    /// <c>targetList</c> as <c>{targetIdx, isSelf, selectSkillIndex}</c> — the sender-relative <c>isSelf</c>
    /// flag a live client actually sends (see <see cref="IsSelfFlag"/>).
    /// <para>The dispatch reads <c>(_isPlayer ? PlayerTargetDataList : OpponentTargetDataList)</c>
    /// (WatchOperationCollection.InPlayActionOperation), and the <c>targetList</c> key populates the seat's
    /// list matching the ingest's <c>isPlayer</c> — so a seat-A (<c>isPlayer:true</c>) attack correctly fills
    /// <c>PlayerTargetDataList</c>. The target's OWNER is resolved by
    /// <c>NetworkBattleGenericTool.LookForActionDataToTargetCard</c> with fixed-seat semantics:
    /// the engine's IsRecovery parse derives owner from a <c>vid</c> stamp, which
    /// <c>SessionBattleEngine.TranslateTargetOwners</c> writes on ingest from this <c>isSelf</c> flag —
    /// so <paramref name="targetOnEnemySeat"/> drives the absolute target seat through the live contract.</para>
    /// <para>For a seat-A attack on seat B's leader: <c>targetIdx = 0</c> (the leader/Class card is Index 0)
    /// and <c>targetOnEnemySeat = true</c>.</para></summary>
    public static Dictionary<string, object?> AttackBody(int attackerIdx, int targetIdx, bool targetOnEnemySeat) => new()
    {
        ["playIdx"] = attackerIdx,
        ["type"] = AttackOpcode,
        ["targetList"] = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["targetIdx"] = (long)targetIdx,
                ["isSelf"] = IsSelfFlag(targetOnEnemySeat),
                ["selectSkillIndex"] = new List<object?>(),
            },
        },
    };

    /// <summary>The engine's <c>NetworkBattleDefine.PlayActionType.PLAY_HAND_SELECT</c> opcode — confirmed
    /// <c>= 31</c> in <c>SVSim.BattleEngine/Engine/NetworkBattleDefine.cs</c>. A TARGETED hand play (a
    /// when_play spell/fanfare that selects a target) carries this opcode (the "_SELECT" suffix), as
    /// opposed to the plain <c>PLAY_HAND = 30</c> a vanilla play uses. The recovery receive path branches
    /// on it to <c>RecoveryOperationCollection.PlaySkillSelectHandCardOperation</c> →
    /// <c>PlayHandCardReflection.PlayAction</c>, which resolves the target from <c>targetList</c> via
    /// <c>NetworkBattleGenericTool.LookForActionDataToTargetCard</c> (seat A) before applying the skill.</summary>
    public const int PlayHandSelectOpcode = 31;

    /// <summary>Build a PlayActions PLAY_HAND_SELECT (targeted hand-play) frame. <paramref name="playIdx"/>
    /// is the played hand card's engine <c>Index</c> (the wire <c>playIdx</c>); the single target is
    /// described in <c>targetList</c> in the SAME real <c>{targetIdx, isSelf, selectSkillIndex}</c> shape as
    /// <see cref="AttackBody"/>/<see cref="EvolveSelectBody"/> (the receive parse reads it identically —
    /// <c>CreateTargetList</c> in NetworkBattleReceiver.cs:2164 — into the seat's TargetDataList, and under
    /// IsRecovery resolves the target's owner from the <c>vid</c> that
    /// <c>SessionBattleEngine.TranslateTargetOwners</c> derives from this <c>isSelf</c> flag on ingest).
    /// <para>For a seat-A spell targeting an enemy follower: <paramref name="targetIdx"/> = the enemy
    /// follower's in-play engine Index and <paramref name="targetOnEnemySeat"/> = <c>true</c> (<c>isSelf:0</c>
    /// → translated to the seat-B vid → <c>LookForActionDataToTargetCard</c> resolves it on
    /// <c>BattleEnemy.ClassAndInPlayCardList</c>).</para></summary>
    public static Dictionary<string, object?> TargetedPlayBody(int playIdx, int targetIdx, bool targetOnEnemySeat) => new()
    {
        ["playIdx"] = playIdx,
        ["type"] = PlayHandSelectOpcode,
        ["targetList"] = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["targetIdx"] = (long)targetIdx,
                ["isSelf"] = IsSelfFlag(targetOnEnemySeat),
                ["selectSkillIndex"] = new List<object?>(),
            },
        },
    };

    /// <summary>Build a PlayActions CHOICE hand-play frame. A choice play carries the plain
    /// <c>PLAY_HAND = 30</c> opcode plus a <c>keyAction</c> list that the receiver parses
    /// (NetworkBattleReceiver.cs:1176-1228) into <c>keyActionType=Choice</c> (→ <c>ReceiveData.IsChoice</c>)
    /// and <c>choiceIdList</c> = the chosen token id(s). Each entry is
    /// <c>{ type:"Choice", cardId:&lt;played card id&gt;, selectCard:[&lt;tokenId&gt;] }</c>. The receiver reads
    /// <c>selectCard</c> via <c>ConvertToListInt</c> (NetworkBattleReceiver.cs:1202), i.e. it consumes a
    /// FLAT list of the chosen token id(s). (The verbatim CLIENT-SEND capture of THIS card —
    /// <c>data_dumps/captures/battle_test/rng/battle-traffic_cl1.ndjson</c> — wraps it as
    /// <c>selectCard:{cardId:[121011010],open:0}</c>; that wrapper is unwrapped before the node's
    /// server-authored receive frame, which is what the receiver — and this driver — consume.)
    /// <paramref name="playIdx"/> is the choice card's hand engine <c>Index</c>; <paramref name="playedCardId"/>
    /// its wire id; <paramref name="chosenTokenId"/> the selected option.</summary>
    public static Dictionary<string, object?> ChoicePlayBody(int playIdx, long playedCardId, long chosenTokenId) => new()
    {
        ["playIdx"] = playIdx,
        ["type"] = 30, // PLAY_HAND — choice is signalled via keyAction, not a distinct opcode
        ["keyAction"] = new List<object?>
        {
            new Dictionary<string, object?>
            {
                // The real capture sends type:1 (int); "Choice" (string) is equivalent — the receiver does
                // Enum.Parse(KeyActionType, type.ToString()) and KeyActionType.Choice == 1, so the string and
                // the int both parse to the same enum value.
                ["type"] = "Choice",
                ["cardId"] = playedCardId,
                // The RECEIVE parse reads selectCard via ConvertToListInt (NetworkBattleReceiver.cs:1202),
                // i.e. a FLAT list of the chosen token id(s). (The verbatim CLIENT-SEND capture wraps it as
                // {cardId:[...],open:0}, but that wrapper is unwrapped before the node's server-authored
                // receive frame; the receiver consumes the flat list.)
                ["selectCard"] = new List<object?> { chosenTokenId },
            },
        },
    };

    /// <summary>VERBATIM CLIENT-SEND Choice play shape — the wrapped form
    /// <c>selectCard:{cardId:[&lt;tokenId&gt;], open:&lt;0|1&gt;}</c> the sender's wire actually carries
    /// (data_dumps/captures/battle_test/cl1/battle-traffic.ndjson, live bid 131549100204:
    /// <c>"selectCard":{"cardId":[121011010],"open":0}</c>). The shadow engine's ingest receives this
    /// wrapper directly (the node strips selectCard from the opponent broadcast, so opponent-facing
    /// frames never see it); <see cref="Engine.SessionBattleEngine.TranslateChoiceKeyAction"/>
    /// unwraps it on the engine's own dict copy before the receiver parses keyAction. This driver
    /// exists so a regression test can pin that unwrap end-to-end against the SAME shape the live
    /// wire delivers, distinct from <see cref="ChoicePlayBody"/> which fast-paths the flat list.
    /// <paramref name="open"/> defaults to 0 (choice hidden from opponent) — the value the live
    /// capture carries; flag is dropped by the unwrap and irrelevant to resolution.</summary>
    public static Dictionary<string, object?> ChoicePlayBodyWrapped(int playIdx, long playedCardId, long chosenTokenId, int open = 0) => new()
    {
        ["playIdx"] = playIdx,
        ["type"] = 30,
        ["keyAction"] = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["type"] = "Choice",
                ["cardId"] = playedCardId,
                ["selectCard"] = new Dictionary<string, object?>
                {
                    ["cardId"] = new List<object?> { chosenTokenId },
                    ["open"] = open,
                },
            },
        },
    };

    /// <summary>The engine's <c>NetworkBattleDefine.PlayActionType.EVOLUTION</c> opcode — confirmed
    /// <c>= 20</c> in <c>SVSim.BattleEngine/Engine/NetworkBattleDefine.cs</c> (EVOLUTION_SELECT is 21). The
    /// receiver maps the wire <c>type</c> int straight to the enum; EVOLUTION/EVOLUTION_SELECT route through
    /// the SAME InPlayAction dispatch arm as ATTACK (NetworkOperationCollection.cs:163-170).</summary>
    public const int EvolutionOpcode = 20;

    /// <summary>The engine's <c>NetworkBattleDefine.PlayActionType.EVOLUTION_SELECT</c> opcode — confirmed
    /// <c>= 21</c> in <c>SVSim.BattleEngine/Engine/NetworkBattleDefine.cs</c>.</summary>
    public const int EvolutionSelectOpcode = 21;

    /// <summary>Build a PlayActions EVOLUTION frame for the in-play follower addressed by its engine
    /// <c>Index</c> (<paramref name="cardIdx"/> == the wire <c>playIdx</c>). A plain (non-targeted) evolve
    /// carries no targetList — the dispatch's <c>list</c> stays empty and the engine evolves the card in
    /// place (InPlayCardReflection.Evol).</summary>
    public static Dictionary<string, object?> EvolveBody(int cardIdx) => new()
    {
        ["playIdx"] = cardIdx,
        ["type"] = EvolutionOpcode,
    };

    /// <summary>Build a PlayActions EVOLUTION_SELECT frame: the follower at engine <c>Index</c>
    /// <paramref name="cardIdx"/> evolves and targets the card at <paramref name="targetIdx"/>. The target is
    /// described in the SAME real <c>{targetIdx, isSelf, selectSkillIndex}</c> shape as <see cref="AttackBody"/>
    /// (the dispatch resolves the target's owner from the <c>vid</c> that
    /// <c>SessionBattleEngine.TranslateTargetOwners</c> derives from this <c>isSelf</c> on ingest);
    /// <paramref name="targetOnEnemySeat"/> selects the isSelf flag.</summary>
    public static Dictionary<string, object?> EvolveSelectBody(int cardIdx, int targetIdx, bool targetOnEnemySeat) => new()
    {
        ["playIdx"] = cardIdx,
        ["type"] = EvolutionSelectOpcode,
        ["targetList"] = new List<object?>
        {
            new Dictionary<string, object?>
            {
                ["targetIdx"] = (long)targetIdx,
                ["isSelf"] = IsSelfFlag(targetOnEnemySeat),
                ["selectSkillIndex"] = new List<object?>(),
            },
        },
    };

    public void Dispose() { /* engine holds no unmanaged resources; nothing to release. */ }

    /// <summary>Minimal test-only <see cref="IBattleParticipant"/> exposing only the
    /// <see cref="ViewerId"/> + <see cref="Context"/> that the harness reads. Broker members
    /// (<c>PushAsync</c>, <c>RunAsync</c>, <c>TerminateAsync</c>) throw <see cref="NotSupportedException"/>
    /// — the harness drives the engine directly, so a frame must never reach the participant relay.
    /// Silent no-ops would let a misrouted push pass undetected.</summary>
    internal sealed class StubParticipant : IBattleParticipant, IHasHandshakePhase
    {
        public long ViewerId { get; }
        public MatchContext Context { get; }

        /// <summary>Handshake cursor (M-HC-3a handler-emit test). Implementing
        /// <see cref="IHasHandshakePhase"/> lets a test build a <c>FrameDispatchContext</c> over two
        /// StubParticipants and advance both to <see cref="HandshakePhase.AfterReady"/> so
        /// <c>BothSidesAfterReady()</c> passes (the PvP relay gate). Harness tests that drive the engine
        /// directly never read this; it defaults to the pre-handshake state and is harmless to them.</summary>
        public HandshakePhase Phase { get; set; } = HandshakePhase.AwaitingInitNetwork;

        public StubParticipant(long viewerId, MatchContext context)
        {
            ViewerId = viewerId;
            Context = context;
        }

#pragma warning disable CS0067 // FrameEmitted is part of the interface but the stub never raises it.
        public event Func<MsgEnvelope, CancellationToken, Task>? FrameEmitted;
#pragma warning restore CS0067

        public Task PushAsync(MsgEnvelope envelope, Stock stock, CancellationToken ct) =>
            throw new NotSupportedException("StubParticipant.PushAsync — harness drives the engine directly; a frame must not reach the participant relay.");
        public Task RunAsync(CancellationToken ct) =>
            throw new NotSupportedException("StubParticipant.RunAsync should not be called in harness tests.");
        public Task TerminateAsync(BattleFinishReason reason) =>
            throw new NotSupportedException("StubParticipant.TerminateAsync should not be called in harness tests.");
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
