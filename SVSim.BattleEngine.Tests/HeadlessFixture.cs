using System.Reflection;
using SVSim.BattleEngine.Rng;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Phase;
using Wizard.Battle.Recovery;
using Wizard.Battle.Replay;
using Wizard.Battle.Resource;
using Wizard.Battle.View.Vfx;
using Wizard.BattleMgr;

namespace SVSim.BattleEngine.Tests
{
    // Initializes the global engine state a headless battle assumes exists. In the real client this
    // is populated from /load/index at login; here we author the minimum the resolution path reads.
    public static class HeadlessEngineEnv
    {
        // Simplest zero-skill vanilla follower in cards.json: neutral (clan 0), cost 1, 1/2, no skill.
        public const int FollowerId = 100011010;

        // M3 next-hardest deterministic card: a fixed-damage spell. 900124030 is an ELF (clan 1, matches
        // PlayerClassId) cost-3 spell whose sole skill is `when_play` `damage=3` to `card_type=class`
        // (the enemy leader) — auto-targeted (no select_count), no RNG. Deterministic burn to the face.
        public const int SpellId = 900124030;

        // M4 next-hardest deterministic card: a when_play SELF-BUFF follower. 103111050 is an ELF
        // (clan 1) cost-1 1/1 whose sole non-evo skill is `when_play` `powerup` `add_offense=1&add_life=1`
        // with skill_target `character=me&target=self` — it buffs ITSELF, so no target selection (the
        // fanfare auto-resolves). Fixed +1/+1 => a deterministic stat-delta oracle. The skill is gated on
        // `play_count>2`; the headless harness seeds that via the public AddCurrentTrunPlayCount (see the
        // oracle test). Base 1/1 -> 2/2 after the fanfare.
        public const int BuffFollowerId = 103111050;
        public const int BuffAddOffense = 1;
        public const int BuffAddLife = 1;

        // M5 next-hardest deterministic card: a when_play SUMMON_TOKEN spell. 800134010 is an ELF
        // (clan 1) cost-1 spell whose sole skill is `when_play` `summon_token=100011020` with
        // `skill_target=none` and an UNGATED condition (`character=me`, trivially the caster): it
        // summons exactly ONE neutral 2/2 follower TOKEN onto the caster's board — no target
        // selection, no RNG (Skill_summon_token's random branch is `num >= 0 && !IsForecast`, and
        // this option carries no `random_count`, so num=-1 => the deterministic literal-id path).
        // The new oracle dimension over M2/M3/M4 is a BOARD-COUNT DELTA from a SKILL-CREATED card:
        // a token that was never in the hand/deck appears in play. This is also the first headless
        // exercise of the PUBLIC prefab card-creation path (CardCreatorBase.CreateCard,
        // createNullView:false, via BattlePlayerBase.CreateNextIndexCard) — class-card construction
        // hits `default: return null` and the M2-M4 hand cards used the private null-view seam, so
        // the view-building creation path is genuinely new here.
        public const int TokenSpellId = 800134010;
        public const int SummonedTokenId = 100011020; // neutral 2/2 follower token
        public const int SummonedTokenAtk = 2;
        public const int SummonedTokenLife = 2;

        // M6 next milestone: the first card requiring TARGET SELECTION — exercises the selectedCards
        // path of ActionProcessor.PlayCard (dormant through M2-M5, all of which played
        // selectedCards: null). 800134020 is an ELF (clan 1) cost-1 SPELL whose sole skill is
        // `when_play` `damage=5` to a SELECTED enemy follower
        // (skill_target=character=op&target=inplay&card_type=unit&select_count=1), ungated
        // (character=me), no RNG, no dynamic `{}` value. The new oracle dimension is SELECTION
        // ROUTING: with TWO followers on the enemy board and ONE passed as selectedCards, only the
        // selected follower takes the 5 damage and the un-selected one is untouched.
        public const int TargetSpellId = 800134020;
        public const int TargetSpellDamage = 5;

        // Two zero-skill vanilla NEUTRAL followers placed on the ENEMY board. Both have life > the
        // 5 damage so they SURVIVE — this gives a differential life-delta oracle (selected -5,
        // un-selected -0) that reads the authoritative damage path M3 already proved, without
        // depending on follower death/board-removal timing (a separate, unproven mechanic). Distinct
        // base life (13 vs 7) so the two post-states can't coincidentally match.
        public const int SelectTargetFollowerId = 900041010;   // neutral 13/13
        public const int UnselectTargetFollowerId = 102011010; // neutral 6/7

        // M7 next milestone: targeted DESTROY — the first card proving follower DEATH / board-removal
        // resolves in the AUTHORITATIVE (committed) part of PlayCard headless, not the cosmetic
        // post-Process tail. 800144120 is an ELF (clan 1) cost-0 SPELL whose sole skill is `when_play`
        // `destroy` of a SELECTED enemy follower
        // (skill_target=character=op&target=inplay&card_type=unit&select_count=1), ungated
        // (skill_condition=character=me), no RNG, no dynamic value. `destroy` is UNCONDITIONAL removal
        // (vs `damage` needing a >=life amount), so the oracle is the cleanest possible "card left the
        // board": selected follower gone + enemy board count -1 + selected card in CemeteryList, while
        // the un-selected follower stays (routing, M6's lesson, confirmed load-bearing by swapping the
        // selection). Reuses the two M2/M6 vanilla followers as the target board (destroy is
        // unconditional so their stats are irrelevant — distinct ids only so selected vs un-selected
        // can't be confused). InitCardTemplates is NOT needed (destroy creates no card).
        public const int DestroySpellId = 800144120;
        public const int DestroyTargetFollowerId = FollowerId;             // neutral 1/2 (the selected, destroyed one)
        public const int DestroyOtherFollowerId = UnselectTargetFollowerId; // neutral 6/7 (the un-selected survivor)

        // M8 next milestone: LETHAL damage — proves follower DEATH VIA COMBAT MATH (damage >= life ->
        // 0 life -> the same RemoveInplayCard/cemetery death path M7 lit up via `destroy`, but reached
        // through the dominant real-card mechanic: "deal N damage"). Reuses the M6 damage=5 spell
        // (800134020) but with target followers STRADDLING 5 life so the SAME spell kills one and merely
        // chips the other in a single oracle: the SELECTED target has life <= 5 and dies (board -1 +
        // cemetery +1, the M7 assertions), while the UN-SELECTED control has life > 5 and survives at
        // reduced life (the M6 life-delta assertion). This combines M7's removal dimension with M6's
        // life-delta + routing, and distinguishes death-via-damage from the unconditional `destroy`.
        public const int LethalDamageSpellId = TargetSpellId;                 // 800134020, when_play damage=5
        public const int LethalDamage = TargetSpellDamage;                    // 5
        public const int LethalTargetFollowerId = FollowerId;                 // neutral 1/2 (life 2 <= 5 -> dies)
        public const int SurvivorTargetFollowerId = UnselectTargetFollowerId; // neutral 6/7 (life 7 > 5 -> survives at 2)

        // M9 next milestone: when_play DRAW — proves the HAND/DECK DELTA dimension (design §5's draw
        // oracle): the last deterministic, non-RNG card-effect class no prior milestone touched (M3/M4/
        // M6/M8 moved stats, M2/M5/M7 the board, M3 the leader — none read the deck->hand transfer).
        // 800114010 is an ELF (clan 1) cost-1 SPELL whose sole skill is `when_play` `draw` of ONE card
        // from the caster's own deck (skill_target=character=me&target=deck&card_type=all&random_count=1),
        // ungated (skill_condition=character=me), no evo skill, no preprocess, no dynamic `{}` value.
        //
        // ADAPTATION FROM THE RESUME-GUIDE SHAPE: the guide asked for a `skill_target=none` draw with
        // "no RNG", but no such card exists in cards.json — EVERY draw selects from the deck via a
        // `random_count=N` target filter (skill_option is always literally `none`; the count lives in
        // skill_target). The RNG is neutralized structurally instead: seed the deck with EXACTLY ONE
        // known card, so `random_count=1` over a single-card pool is deterministic regardless of the
        // RandomSeed. This keeps the oracle decisive (drawn id is forced) while exercising the real
        // draw path. Like the summon token, a drawn card is engine-CREATED off the deck the M5 prefab
        // way; unlike summon, the card already exists (we seed it) and the skill only MOVES it deck->hand.
        public const int DrawSpellId = 800114010;
        public const int DeckSeedCardId = FollowerId; // the single known deck card (neutral 1/2 vanilla)

        // M10 next milestone: the first DYNAMIC `{}`-VALUE card — proves the engine COMPUTES an effect
        // magnitude from live game state (a value the wire can't carry; per memory
        // project_battle_relay_nontargeted_effects this state-derived-value problem is exactly what
        // broke the PvP relay, so proving the engine resolves it headless is the direct validation that
        // the port — not a relay — is the necessary path). Still non-RNG: a seeded state makes the value
        // deterministic. 112134010 is an ELF (clan 1) cost-2 SPELL whose sole skill is `when_play`
        // `damage={me.play_count}-1` to `character=both&target=inplay&card_type=unit` (with a
        // `base_card_id!=900111010|900111020` exclusion) — an AoE over BOTH boards' units, auto-targeted
        // (no select_count, so selectedCards: null like M2-M5), ungated (skill_condition=character=me).
        //
        // The `{}` value resolves (SkillOptionValue.ParseInt) as
        // `_filterVariable.Parse("me.play_count") - 1`, where Parse routes to
        // SkillEnvironmentalPlayCount.Filtering -> playerInfo.GetCurrentTurnPlayCount() (the
        // `isPrePlay=false` resolution path). That is the SAME per-turn counter the public
        // AddCurrentTrunPlayCount feeds (M4 proved this seam drove the play_count>2 GATE; M10 proves it
        // also feeds the `{}` VALUE). The per-play auto-increment AddCurrentTrunPlayCount(1) lives in
        // ActionProcessor's OnBeforePlayCard (BattlePlayerBase.cs:1400), subscribed by
        // SetupActionProcessorEvent — which is ONLY called on the OperateMgr/Prediction/OperationSimulator
        // paths, NOT on the direct `new ActionProcessor(pair).PlayCard` (DP4) path this harness uses. So
        // the headless play does NOT self-bump the per-turn count: the skill reads EXACTLY the seeded
        // GetCurrentTurnPlayCount() and the damage == seeded - 1. The oracle derives the expected
        // magnitude from the engine's OWN live GetCurrentTurnPlayCount(), not from a hardcoded literal,
        // which is the M10 dimension (engine-computed value, not a wire-carried constant).
        //
        // The target is the M6 vanilla NEUTRAL 13/13 follower (SelectTargetFollowerId, already loaded):
        // life 13 > any reasonable seeded count, so it SURVIVES for a clean life-delta read (reusing the
        // M3/M6/M8 damage->life path), and `card_type=unit` excludes both leaders (asserted untouched).
        public const int DynamicDamageSpellId = 112134010;
        public const int DynamicDamageTargetFollowerId = SelectTargetFollowerId; // neutral 13/13 (survives, clean delta)
        // A deliberately non-trivial seeded per-turn play count so the computed damage (== this value)
        // is an obvious state read, not a coincidence with a small literal. The load-bearing probe
        // (M4/M6/M8 discipline) varies this and watches the damage track it.
        public const int DynamicSeededPlayCount = 4;

        // M12 (the design §5 RNG oracle): reuse the M9 draw spell (800114010, when_play `draw` 1 from the
        // caster's deck via a random_count=1 filter) but over a MULTI-card deck with IsRandomDraw=true.
        // M9 passed only because IsRandomDraw=false takes BattlePlayerBase.LotteryRandomDrawCard's
        // top-of-deck `else` branch (BattlePlayerBase.cs:3174-3185) — a 1-card pool made index 0 the only
        // card. With IsRandomDraw=true the selection runs through SkillRandomSelectFilter.Filtering, which
        // calls BattleManagerBase.GetIns().StableRandom(poolCount) per pick (SkillRandomSelectFilter.cs:42,
        // gated on IsRandomDraw) — the chokepoint HeadlessBattleMgr overrides. So the scripted source picks
        // exactly which deck card is drawn, proving a GENUINE multi-outcome roll (the dimension M9's
        // one-card pool deliberately avoided).
        //
        // Three distinguishable deck cards seeded at consecutive indices; SkillRandomSelectFilter orders
        // the pool by Index (line 34), so the pick index maps to position in this order:
        //   index 0 -> RngDeckCardA (100011010), index 1 -> RngDeckCardB (103111050), index 2 -> RngDeckCardC (100011020)
        // All three are already loaded by HeadlessCardMaster.Load via EnsureInitialized (FollowerId,
        // BuffFollowerId, SummonedTokenId), so no Load change is needed.
        public const int RngDrawSpellId = DrawSpellId;        // 800114010, when_play draw 1 (random_count=1)
        public const int RngDeckCardA = FollowerId;           // neutral 1/2   -> Index-order position 0
        public const int RngDeckCardB = BuffFollowerId;       // ELF 1/1       -> Index-order position 1
        public const int RngDeckCardC = SummonedTokenId;      // neutral 2/2   -> Index-order position 2

        private static bool _done;
        private static readonly object _processGlobalsGate = new object();

        // Process-globals only: load card master, install master data, seed LoadDetail/Crossover,
        // seed Certification.udid. Per-battle/per-test state (chara ids on the DataMgr,
        // NetworkUserInfoData) is seeded on the mgr's own GameMgr via NewSeededSingleBattleMgr /
        // NewSeededHeadlessBattleMgr / NewSeededHeadlessNetworkBattleMgr in the fixture entry points
        // — no shared state to race here. Thread-safe (assembly-level Parallelizable(Fixtures) means
        // many fixtures' [SetUp] race this one function).
        public static void EnsureProcessGlobals()
        {
            if (_done) return;
            lock (_processGlobalsGate)
            {
                if (_done) return;
                EnsureProcessGlobalsCore();
                _done = true;
            }
        }

        private static void EnsureProcessGlobalsCore()
        {
            // Wizard.Data.Load: static /load/index snapshot. The ctor's CreateBackgroundId reads
            // Data.Load.data._userTutorial (LoadDetail self-inits _userTutorial). Suppress VFX too.
            Wizard.Data.Load = new Load { data = new LoadDetail() };
            // CardParameter(CardCSVData) reads Data.Crossover.RestrictedCard for deck-limit calc;
            // an empty Crossover returns the default count (no restriction). Private setter -> reflect.
            typeof(Wizard.Data).GetProperty("Crossover",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
                .SetValue(null, new Wizard.Crossover());
            // CardMaster must be non-null before construction (the leader/class card looks up id 0).
            // Load the M2 vanilla follower + the M3 fixed-damage spell + the M4 self-buff follower +
            // the M5 summon-token spell AND the token it summons so each oracle can create + look up
            // real stats. The summoned token id must be present: Skill_summon_token resolves it
            // through CardMaster.GetCardParameterFromId during creation.
            HeadlessCardMaster.Load(FollowerId, SpellId, BuffFollowerId, TokenSpellId, SummonedTokenId,
                TargetSpellId, SelectTargetFollowerId, UnselectTargetFollowerId, DestroySpellId, DrawSpellId,
                DynamicDamageSpellId);
            // Master reference data (class-character list) for leader/class card resolution.
            HeadlessMasterData.Install();

            // The network emit path's payload builder (RealTimeNetworkAgent.CreateEmitData) reads
            // Cute.Certification.Udid (RealTimeNetworkAgent.cs:1407). The Udid getter lazily decodes from
            // Toolbox.SavedataManager (Certification.cs:35), which is null headless. Seed the private static
            // backing field with a non-empty placeholder so the getter short-circuits before touching the
            // savedata manager. The value is opaque to the engine (it's just echoed into the emit dict).
            typeof(Cute.Certification)
                .GetField("udid", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .SetValue(null, "headless-udid");
        }

        // Simple deterministic 40-card deck for multi-instance tests: every slot is the same vanilla
        // FollowerId. Card 100011010 is loaded as part of EnsureProcessGlobals' HeadlessCardMaster.Load
        // batch so SessionBattleEngine.Setup resolves each entry without re-loading. Kept a single
        // shape — the multi-instance property being verified (per-session ambient isolation across
        // parallel battles) is driven by distinct masterSeeds on the engines, not by deck variation.
        public static long[] SampleDeck()
        {
            var deck = new long[40];
            for (int i = 0; i < 40; i++) deck[i] = FollowerId;
            return deck;
        }

        // Per-GameMgr chara-id seeder. Fixtures construct a GameMgr up-front and pass it to the
        // mgr ctor (chunk-45 overload); this stamps the player/enemy leader chara ids on its DataMgr.
        // Set the backing fields directly: the public SetPlayerCharaId() also pulls MyRotation /
        // AvatarBattle info (more null statics) the resolution path doesn't need.
        public static void SeedCharaIds(GameMgr gm)
        {
            var dm = gm.GetDataMgr();
            SetField(dm, "_playerCharaId", HeadlessMasterData.PlayerCharaId);
            SetField(dm, "_enemyCharaId", HeadlessMasterData.EnemyCharaId);
        }

        // Per-GameMgr net-user seeder. NetworkBattleManagerBase.CreateBackgroundId reads
        // gm.GetNetworkUserInfoData().GetFieldId() when the RecoveryManager yields no bg id; the bare
        // ctor path leaves _netUser null (no lazy init). Seed a no-op instance whose _selfInfo carries
        // fieldId=1 (== ForestField, valid). Only satisfies the bg lookup — no game-state effect.
        public static void SeedNetUser(GameMgr gm)
        {
            var netUser = new NetworkUserInfoData();
            netUser.SetSelfInfo(
                new System.Collections.Generic.Dictionary<string, object> { ["fieldId"] = 1 },
                isWatchReplayRecovery: false);
            gm.SetNetworkUserInfoData(netUser);
        }

        // Phase-5 chunk 46: canonical seeded SingleBattleMgr factory. Replaces the historical
        // `new SingleBattleMgr(new HeadlessContentsCreator())` pattern from every oracle test,
        // which relied on the ambient bridge to reach a chara-id/net-user-seeded GameMgr. Now the
        // GameMgr is built + seeded here and passed to the mgr's chunk-45 ctor overload directly.
        public static SingleBattleMgr NewSeededSingleBattleMgr()
        {
            var gm = new GameMgr();
            SeedCharaIds(gm);
            SeedNetUser(gm);
            return new SingleBattleMgr(new HeadlessContentsCreator(), gm);
        }

        // Same idea for the RNG-injectable HeadlessBattleMgr / HeadlessNetworkBattleMgr twins.
        public static HeadlessBattleMgr NewSeededHeadlessBattleMgr(IRandomSource rng = null)
        {
            var gm = new GameMgr();
            SeedCharaIds(gm);
            SeedNetUser(gm);
            return new HeadlessBattleMgr(new HeadlessContentsCreator(), rng, gm);
        }

        public static HeadlessNetworkBattleMgr NewSeededHeadlessNetworkBattleMgr(IRandomSource rng = null)
        {
            var gm = new GameMgr();
            SeedCharaIds(gm);
            SeedNetUser(gm);
            return new HeadlessNetworkBattleMgr(new HeadlessContentsCreator(), rng, gm);
        }

        // Seed each leader's starting life on a freshly-constructed mgr. The engine does this in
        // BattleManagerBase.SetupInitialGameState -> InitializeClassLife (InitBaseMaxLife per leader),
        // but the full SetupInitialGameState also cascades into rotation/avatar/turn-panel UI init
        // that is irrelevant (and hostile) to a headless resolution test, so apply just the
        // InitializeClassLife subset. Without this a leader's BaseMaxLife defaults to 0 — which reads
        // as already-dead/game-over and silently blocks any card play (the M2 follower oracle never
        // noticed because it only asserted leader life *unchanged*, and 0 == 0).
        public const int DefaultLeaderLife = 20;

        public static void InitLeaderLife(BattleManagerBase mgr, int life = DefaultLeaderLife)
        {
            ((ClassBattleCardBase)mgr.BattlePlayer.Class).InitBaseMaxLife(life);
            ((ClassBattleCardBase)mgr.BattleEnemy.Class).InitBaseMaxLife(life);
        }

        // The PUBLIC prefab card-creation path (CardCreatorBase.CreateCard, createNullView:false) —
        // used by anything the engine creates INTERNALLY (summons, token-draws, etc.), as opposed to
        // the test's direct private null-view seam for hand cards — clones card-template prefabs held
        // on BattleManagerBase.SBattleLoad. The real async battle load (CoLoad) builds these; the bare
        // `new SingleBattleMgr(...)` construction path leaves SBattleLoad null (the M2 NRE was here).
        // Seed it with non-null no-op CardTemplates: their `.gameObject` is a lazy shim no-op, and the
        // shim's CloneObjectToParent + self-consistent object graph carry the rest. Nothing here
        // computes game state — the token's authoritative stats come from CardCSVData, not the view.
        public static void InitCardTemplates(BattleManagerBase mgr)
        {
            mgr.SBattleLoad = new SBattleLoad
            {
                UnitCardTemplate = new CardTemplate(),
                SpellCardTemplate = new CardTemplate(),
                FieldCardTemplate = new CardTemplate(),
            };
            // The created card's transform is positioned/parented under the battle's 3D scene-graph
            // containers (CardCreatorBase.CreateCardTypeBuildInfo reads ins.CardHolder/ECardHolder/
            // PCardPlace/Battle3DContainer). The real battle load instantiates these; seed non-null
            // no-op GameObjects so the positioning resolves (no-op transforms; nothing rendered).
            mgr.Battle3DContainer = new GameObject();
            mgr.CardHolder = new GameObject();
            mgr.ECardHolder = new GameObject();
            mgr.PCardPlace = new GameObject();
            mgr.ChoiceCardHolder = new GameObject();
            mgr.EvolveCardHolder = new GameObject();
        }

        // The shared headless card-creation primitive. CardCreatorBase.CreateCardWithoutResources is
        // the engine's own null-view creation path (CreateBase -> new *BattleCard(buildInfo).Setup(
        // createNullView:true)); it's private, so reflect it rather than reimplement the 14-arg
        // BuildInfo wiring. The public CardCreatorBase.CreateCard goes through prefab cloning.
        //
        // The engine's CreateCard also calls owner.SetupCardEvent(card); the raw
        // CreateCardWithoutResources seam skips it, so we fold it in here. SetupCardEvent wires the
        // per-card play events (BattlePlayerBase.cs:1452): for a SPELL/amulet it attaches
        // OnPlay -> RemoveSpellCardFromHand and OnFinishWhenPlaySkill -> AddSpellCardToCemetery, which
        // are how a non-follower leaves the hand at all (a follower's hand->field move is intrinsic to
        // SetUpInplay, not event-driven). For a follower SetupCardEvent only attaches an OnEvolve hook
        // that never fires on a vanilla play, so folding it in is a no-op there — making this a single
        // primitive both follower and non-follower oracles can share.
        public static BattleCardBase CreateHeadlessHandCard(int cardId, int index, bool isPlayer, BattleManagerBase mgr)
        {
            var io = mgr.CreatePlayerInnerOptionsBuilder();
            var m = typeof(CardCreatorBase).GetMethod("CreateCardWithoutResources",
                BindingFlags.NonPublic | BindingFlags.Static);
            var card = (BattleCardBase)m.Invoke(null, new object[] { cardId, index, isPlayer, mgr, io });
            BattlePlayerBase owner = isPlayer ? (BattlePlayerBase)mgr.BattlePlayer : mgr.BattleEnemy;
            owner.SetupCardEvent(card);
            return card;
        }

        // Put a follower DIRECTLY onto a player's board headless (vs as a side-effect of PlayCard),
        // for setting up a target board state. Create it through the shared null-view seam, then drive
        // the engine's own hand->field move: HandCardToField requires the card to be in HandCardList,
        // then AddInplayCards it + removes it from hand (BattlePlayerBase.cs:2568). For a vanilla
        // follower the OnAddPlayCard/StopBattleHandCard/OnSummonAfter events it fires are no-ops (no
        // fanfare), so the follower lands on the board at its CardCSVData base stats. M2 proved the
        // hand->field placement path resolves headless.
        public static BattleCardBase PutFollowerInPlay(BattleManagerBase mgr, int cardId, int index, bool isPlayer)
        {
            var card = CreateHeadlessHandCard(cardId, index, isPlayer, mgr);
            BattlePlayerBase owner = isPlayer ? (BattlePlayerBase)mgr.BattlePlayer : mgr.BattleEnemy;
            owner.HandCardList.Add(card);
            owner.HandCardToField(card);
            return card;
        }

        // Push a known card onto a player's DECK headless (the M9 draw oracle's setup primitive). The
        // bare `new SingleBattleMgr(...)` construction leaves DeckCardList non-null-but-empty (ctor at
        // BattlePlayerBase.cs:1050), and a card's deck membership IS its `IsInDeck` (BattleCardBase.cs:970
        // `=> SelfBattlePlayer.DeckCardList.Contains(this)`) — so no separate "in deck" flag is needed.
        // Create the card through the same null-view seam hand/board cards use, then drive the engine's
        // own AddToDeck (BattlePlayerBase.cs:3038): for a vanilla follower it is just DeckCardList.Add
        // (HasDeckSelfSkill is false; the XorShiftRandom/IsMulliganEnd reshuffle bookkeeping short-
        // circuits on the null/inactive headless RNG). The drawn card is then the engine's own deck
        // object, so the oracle can assert deck->hand identity by reference, not just by id.
        public static BattleCardBase SeedDeck(BattleManagerBase mgr, int cardId, int index, bool isPlayer)
        {
            var card = CreateHeadlessHandCard(cardId, index, isPlayer, mgr);
            BattlePlayerBase owner = isPlayer ? (BattlePlayerBase)mgr.BattlePlayer : mgr.BattleEnemy;
            owner.AddToDeck(card);
            return card;
        }

        // Build a headless battle wired for AUTHORITATIVE RNG: real rolls under IsForecast (via the
        // injected source on HeadlessBattleMgr) AND IsRandomDraw=true (the second gate — without it the
        // random-select filters bypass the roll and pick index 0; BattleManagerBase.cs:415,
        // SkillRandomSelectFilter.cs:42). Mirrors the opponent/turn/leader-life wiring every oracle does.
        // Returns the constructed HeadlessBattleMgr; the caller seeds hands/decks/boards and plays.
        public static HeadlessBattleMgr NewAuthoritativeBattle(IRandomSource rng)
        {
            EnsureProcessGlobals();                       // sets IsForecast = true among other globals
            // Phase-5 chunk 45: build a pre-seeded GameMgr and hand it to the mgr ctor, bypassing
            // the ambient bridge. The base ctor's BattlePlayer construction reads chara-id/net-user
            // through GameMgr.GetIns() (which routes to mgr.GameMgr once base ctor runs), so the
            // GameMgr must be seeded BEFORE the mgr ctor completes.
            var gm = new GameMgr();
            SeedCharaIds(gm);
            SeedNetUser(gm);
            var mgr = new HeadlessBattleMgr(new HeadlessContentsCreator(), rng, gm);
            // Phase-5 chunk 42: write InstanceIsRandomDraw directly (mgr not yet ambient-attached).
            mgr.InstanceIsRandomDraw = true;                       // the second RNG gate (F-RNG-2)
            mgr.IsRecovery = true;                      // collapse wait delays to 0 (F1)

            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;
            SetField(player, "_opponentBattlePlayer", enemy);
            SetField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            InitLeaderLife(mgr);                         // a 0-life leader reads as game-over and blocks plays
            InitCardTemplates(mgr);                      // the draw VFX touches the drawn card's view layer
            return mgr;
        }

        // M13 emit-path read. Builds a HeadlessNetworkBattleMgr (the emitting twin of the
        // HeadlessBattleMgr NewAuthoritativeBattle returns) and stands up the OnEmit capture seam: the
        // engine's own RealTimeNetworkAgent.OnEmit event (RealTimeNetworkAgent.cs:1270) fires the played
        // URI before both emit guards, so capturing it needs no Engine/shim edit — just an injected agent.
        // Returns (mgr, emitted-URI list). The caller seeds the hand and drives mgr.OperateMgr.PlayCard.
        public static (HeadlessNetworkBattleMgr mgr, System.Collections.Generic.List<NetworkBattleDefine.NetworkBattleURI> emitted)
            NewNetworkEmitBattle(IRandomSource rng = null)
        {
            EnsureProcessGlobals();                         // sets IsForecast = true among other globals
            // Phase-5 chunk 45: build + seed a per-mgr GameMgr up-front (no ambient bridge).
            var gm = new GameMgr();
            SeedCharaIds(gm);
            SeedNetUser(gm);
            var mgr = new HeadlessNetworkBattleMgr(new HeadlessContentsCreator(), rng, gm);
            // NOTE: IsRecovery is left FALSE here (unlike the solo NewAuthoritativeBattle). The network
            // emit path is gated on !IsRecovery in BOTH places: NetworkStandardBattleMgr.SendPlayCard
            // (NetworkStandardBattleMgr.cs:155) and the OnSetCardComplete->SendPlayCard subscription in
            // SetUpNetworkOperateEvent (NetworkBattleManagerBase.cs:927, which early-returns under
            // IsRecovery). With IsRecovery=true the play would resolve state but never emit. (The solo
            // NewAuthoritativeBattle uses IsRecovery=true only to collapse VFX wait delays; here the no-op
            // view shims absorb the real view layer instead — see the IsForecast=false block below.)

            // IsForecast MUST be false on the network emit path. BattleManagerBase.IsVirtualBattle is
            // `=> IsForecast` (BattleManagerBase.cs:657), and NetworkStandardBattleMgr.SendPlayCard is gated
            // on `!IsVirtualBattle` (NetworkStandardBattleMgr.cs:155) — under IsForecast=true the play
            // resolves state but the emit is suppressed. EnsureInitialized leaves IsForecast=true (correct
            // for the direct-ActionProcessor solo oracles, where it suppresses VFX); clear it here so the
            // genuine emit fires. The cost is that VFX registration is no longer short-circuited, so the
            // play exercises the real view layer — those view touches are satisfied by the no-op view shims
            // (InitCardTemplates, the HandView/DetailPanel fills below). M3's damage is literal, immune to
            // any play-count bump the OperateMgr path adds vs the direct path.
            // Phase-5 chunk 42: mgr isn't yet attached to the ambient at this point (_scope.Ctx.Mgr
            // is set by the caller after this fixture returns), so BattleManagerBase.IsForecast=false
            // would silently no-op (ambient bridge is gone). Write InstanceIsForecast directly.
            mgr.InstanceIsForecast = false;
            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;
            SetField(player, "_opponentBattlePlayer", enemy);
            SetField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            InitLeaderLife(mgr);                          // a 0-life leader reads as game-over and blocks plays
            InitCardTemplates(mgr);                       // play/draw VFX touches the card view layer
            // The OperateMgr emit path runs SetupActionProcessorEvent (skipped by the direct-ActionProcessor
            // solo oracles), which subscribes BattleMgr.DetailMgr.DetailPanelControl.UpdateCardDescriptionOnEvent
            // to OnPlayComplete (BattlePlayerBase.cs:1431). DetailMgr is created in CreateManager but its
            // DetailPanelControl (a UI control) is null headless. Seed the engine's own NullDetailPanelControl
            // no-op so the play-complete event resolves without touching the UI.
            mgr.DetailMgr.DetailPanelControl = new NullDetailPanelControl();

            // Inject a headless RealTimeNetworkAgent so NetworkBattleSender's ToolboxGame.RealTimeNetworkAgent
            // .* calls resolve, and subscribe OnEmit. GetUninitializedObject skips the MonoBehaviour Awake.
            var agent = (RealTimeNetworkAgent)System.Runtime.Serialization.FormatterServices
                .GetUninitializedObject(typeof(RealTimeNetworkAgent));
            // CurrentMatchingStatus has a protected setter; seed it non-Disconnected so EmitMsgPack does not
            // early-return at RealTimeNetworkAgent.cs:1272 (needed only for the best-effort payload read, Task 4;
            // OnEmit fires regardless). The default on the uninitialized object is OffLine (0), which clears the
            // SetCurrentMatchingStatus guards; the only side effect is a static-StringBuilder trace log, so the
            // public setter runs cleanly headless. Prepared (50) is the real enum member (RealTimeNetworkAgent.cs:35).
            agent.SetCurrentMatchingStatus(RealTimeNetworkAgent.MatchingStatus.Prepared);

            // The engine RTA is a pass-7 stub with no-op method bodies. AddActionSequence /
            // EmitMsgPack / OnEmit-plumbing were dropped; the historical Gungnir + _notEmit seeds
            // are no longer needed. NetworkLogger stays as a NetworkNullLogger since it's a
            // typed property (INetworkLogger<NetworkLog>) that some tests may still inspect.
            SetProperty(agent, "NetworkLogger", new NetworkNullLogger());

            var emitted = new System.Collections.Generic.List<NetworkBattleDefine.NetworkBattleURI>();
            agent.OnEmit += uri => emitted.Add(uri);
            // Phase-5 chunk 40: ambient fallback removed from ToolboxGame.SetRealTimeNetworkBattle;
            // fixture has mgr in scope, so seed it directly instead of via the ambient-routed static.
            mgr.InstanceNetworkAgent = agent;

            return (mgr, emitted);
        }

        // M13 Task 4 best-effort: read the emit payload back out of the agent's stock sequencer. With
        // _notEmit=true (NewNetworkEmitBattle terminates the path that way), EmitMsgUriPack short-circuits
        // BEFORE stockEmitMessageMgr.StockData (RealTimeNetworkAgent.cs:1438 vs :1461), so the stock is
        // expected to be null/empty — return null on any null/throw so the test degrades to Inconclusive
        // rather than failing. Field `stockEmitMessageMgr` (:103) + `GetSequenceAllData()`
        // (StockEmitMgr.cs:81, returns List<Dictionary<string,object>>) verified against the copied engine.
        // Precondition: this is expected-null ONLY while NewNetworkEmitBattle sets _notEmit=true and leaves
        // stockEmitMessageMgr unconstructed. If that harness setup changes, revisit — a non-null stock should
        // then make the test ASSERT on the payload rather than defer to Inconclusive.
        public static System.Collections.IList TryReadStockedEmitData(RealTimeNetworkAgent agent)
        {
            try
            {
                var f = typeof(RealTimeNetworkAgent).GetField("stockEmitMessageMgr",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var stock = f?.GetValue(agent);
                if (stock == null) return null;
                var m = stock.GetType().GetMethod("GetSequenceAllData");
                return m?.Invoke(stock, null) as System.Collections.IList;
            }
            catch { return null; }
        }

        private static void SetField(object obj, string name, object value)
        {
            var f = obj.GetType().GetField(name,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Public);
            if (f == null) throw new System.InvalidOperationException(
                $"{obj.GetType().Name} has no field '{name}'");
            f.SetValue(obj, value);
        }

        // Set a property whose setter is non-public (e.g. RealTimeNetworkAgent.NetworkLogger has a
        // protected setter). Walks the type hierarchy because the declaring type may be a base class.
        private static void SetProperty(object obj, string name, object value)
        {
            var t = obj.GetType();
            System.Reflection.PropertyInfo p = null;
            while (t != null && p == null)
            {
                p = t.GetProperty(name,
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Public);
                t = t.BaseType;
            }
            if (p == null) throw new System.InvalidOperationException(
                $"{obj.GetType().Name} has no property '{name}'");
            p.SetValue(obj, value);
        }
    }

    // Test-side replica of the engine's own StandardBattleMgrContentsCreator (the practice/solo
    // init path: GameMgr.cs:244 `new SingleBattleMgr(new StandardBattleMgrContentsCreator(null, null))`).
    // Authored here (not copied) so we control the seed deterministically; uses the real engine
    // managers verbatim. The real StandardBattleMgrContentsCreator + SingleBattlePhaseCreator were
    // cut from the M1 copy set (entry-point constructors), so we reproduce them minimally.
    public sealed class HeadlessContentsCreator : IBattleMgrContentsCreator
    {
        public int RandomSeed => 12345; // fixed; vanilla follower has no RNG so value is irrelevant

        // No-op managers (vs the practice path's file-backed SingleBattleRecoveryRecordManager):
        // the ctor's FirstRecoverySetting/FirstReplaySetting dereference these, and recovery/replay
        // recording is irrelevant to the M2 oracle, so use the engine's own null implementations.
        public IRecoveryManager RecoveryManager { get; } = new NullRecoveryManager();
        public IRecoveryRecordManager RecoveryRecordManager { get; } = new NullRecoveryRecordManager();
        public IReplayRecordManager ReplayRecordManager { get; } = new NullReplayRecordManager();

        public IBattleResourceMgr CreateResourceMgr() => new BattleResourceMgr();
        public VfxMgr CreateVfxMgr() => new VfxMgr();
        public IPhaseCreator CreatePhaseCreator(BattleManagerBase battleMgr) =>
            new HeadlessPhaseCreator(battleMgr);
    }

    // Equivalent of the engine's SingleBattlePhaseCreator: inherits PhaseCreatorBase wholesale.
    public sealed class HeadlessPhaseCreator : PhaseCreatorBase
    {
        public HeadlessPhaseCreator(BattleManagerBase battleMgr) : base(battleMgr) { }
    }
}
