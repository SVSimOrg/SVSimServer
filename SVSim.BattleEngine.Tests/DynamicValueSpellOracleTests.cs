using System.Reflection;
using NUnit.Framework;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M10 (the first DYNAMIC `{}`-VALUE card — the first deliberate step beyond the four §5-named
    // oracle dimensions M2-M9 closed): a when_play spell whose effect MAGNITUDE is COMPUTED by the
    // engine from live game state, not carried as a literal. 112134010's sole skill is
    // `when_play damage={me.play_count}-1` to units; the `{}` resolves
    // (SkillOptionValue.ParseInt -> SkillFilterVariable.Parse -> SkillEnvironmentalPlayCount.Filtering)
    // to `GetCurrentTurnPlayCount() - 1`. That GetCurrentTurnPlayCount() is the SAME per-turn counter
    // M4 seeded via the public AddCurrentTrunPlayCount to drive a play_count GATE — M10 proves the seam
    // also feeds the effect VALUE.
    //
    // The new oracle dimension over every prior milestone is the ENGINE-COMPUTED VALUE: the asserted
    // damage is derived from the engine's OWN live play-count accessor (GetCurrentTurnPlayCount() - 1),
    // never a hardcoded literal. Per memory project_battle_relay_nontargeted_effects, a state-derived
    // value that the wire could NOT carry (spellboost cost) is exactly what desynced the PvP relay;
    // proving the engine resolves a `{}` value headless is the direct validation that the port (not a
    // relay) is the necessary path.
    //
    // Timing note (the M10 first-unknown, RESOLVED empirically by the first RED): the per-play
    // auto-increment AddCurrentTrunPlayCount(1) lives in ActionProcessor's OnBeforePlayCard
    // (BattlePlayerBase.cs:1400), which is subscribed by SetupActionProcessorEvent — and that is only
    // called on the OperateMgr / Prediction / OperationSimulator paths, NOT on the direct
    // `new ActionProcessor(pair).PlayCard` (DP4) path this harness uses. So the headless play does NOT
    // self-bump the per-turn play count: the skill reads EXACTLY the seeded GetCurrentTurnPlayCount()
    // and the damage == seeded - 1. (The first RED expected a +1 that this path never applies; the
    // state-derived primary assertion below was right regardless, and the concrete pins were corrected
    // to the observed no-bump behavior.)
    [TestFixture]
    public class DynamicValueSpellOracleTests
    {

        [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

        private static void SetPrivateField(object obj, string name, object value)
        {
            var t = obj.GetType();
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            while (f == null && t.BaseType != null) { t = t.BaseType; f = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic); }
            Assert.That(f, Is.Not.Null, $"field {name} not found on {obj.GetType().Name}");
            f.SetValue(obj, value);
        }

        [Test]
        public void Dynamic_damage_spell_deals_engine_computed_play_count_value()
        {
            BattleManagerBase.IsForecast = true;           // suppress VFX registration (F1)
            var mgr = HeadlessEngineEnv.NewSeededSingleBattleMgr();
            mgr.IsRecovery = true;                          // collapse wait delays to 0 (F1)

            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;

            // Minimal opponent/turn wiring (see M2-M6 oracles): opponent refs + active turn flag. The
            // spell's target resolver walks player -> opponent -> opponent's in-play units; the
            // `{me.play_count}` read keys on the active player's current turn.
            SetPrivateField(player, "_opponentBattlePlayer", enemy);
            SetPrivateField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            // Seed leader life so neither leader reads as a 0-life game-over state (blocks plays, M3).
            HeadlessEngineEnv.InitLeaderLife(mgr);

            // Put ONE vanilla follower on the ENEMY board. The spell is `character=both` (AoE over both
            // boards' units), but with no player-side units the only matched target is this enemy
            // follower; its base life (13) exceeds any seeded play count so it SURVIVES -> clean
            // life-delta read (no dependence on death/removal). card_type=unit excludes both leaders.
            var target = HeadlessEngineEnv.PutFollowerInPlay(mgr, HeadlessEngineEnv.DynamicDamageTargetFollowerId, 0, isPlayer: false);

            // Seed the live game state the `{}` value reads: the active player's current-turn play
            // count. This is the M4 seam (AddCurrentTrunPlayCount), here driving the VALUE not a gate.
            player.AddCurrentTrunPlayCount(HeadlessEngineEnv.DynamicSeededPlayCount);

            var cardParam = CardMaster.GetInstanceForBattle().GetCardParameterFromId(HeadlessEngineEnv.DynamicDamageSpellId);

            // Place the dynamic-value spell in the active player's hand with PP to spare.
            var card = HeadlessEngineEnv.CreateHeadlessHandCard(HeadlessEngineEnv.DynamicDamageSpellId, 1, isPlayer: true, mgr);
            player.HandCardList.Add(card);
            player.Pp = 10;

            // Pre-state snapshot.
            int ppBefore = player.Pp;
            int handBefore = player.HandCardList.Count;
            int playerInplayBefore = player.ClassAndInPlayCardList.Count;
            int enemyInplayBefore = enemy.ClassAndInPlayCardList.Count;
            int targetLifeBefore = target.Life;
            int playerLeaderLifeBefore = player.ClassAndInPlayCardList[0].Life;
            int enemyLeaderLifeBefore = enemy.ClassAndInPlayCardList[0].Life;

            // Resolve the play through the real engine (auto-targeted AoE -> selectedCards: null).
            var pair = mgr.GetBattlePlayerPair(isPlayer: true);
            var ap = new ActionProcessor(pair);
            Assert.DoesNotThrow(() => ap.PlayCard(card, selectedCards: null),
                "ActionProcessor.PlayCard threw on a dynamic {}-value damage spell");

            // The engine-computed value, derived from the engine's OWN live play-count accessor (the
            // direct-ActionProcessor path does not self-bump it, so this reads the seeded value) —
            // exactly the value the skill's `{me.play_count}-1` resolved against. NOT a hardcoded
            // literal: this is the M10 dimension (effect magnitude computed from state the wire can't
            // carry).
            int playCountAtResolution = player.GetCurrentTurnPlayCount();
            int expectedDamage = playCountAtResolution - 1;
            int actualDamage = targetLifeBefore - target.Life;

            Assert.Multiple(() =>
            {
                // PRIMARY M10 assertion: the damage dealt equals the engine-COMPUTED {me.play_count}-1,
                // read from live state — proving the engine resolved the `{}` expression, not a literal.
                Assert.That(actualDamage, Is.EqualTo(expectedDamage),
                    "damage dealt did not equal the engine-computed {me.play_count}-1 value");
                // Concrete pins (catch a silent state-read failure where play_count would default to 0,
                // making damage -1 -> 0): the direct-ActionProcessor path applies no self-play bump, so
                // the resolution-time count is exactly the seeded value and the damage is seeded - 1.
                Assert.That(playCountAtResolution, Is.EqualTo(HeadlessEngineEnv.DynamicSeededPlayCount),
                    "play count was not read as the seeded current-turn value");
                Assert.That(actualDamage, Is.EqualTo(HeadlessEngineEnv.DynamicSeededPlayCount - 1),
                    "net damage did not equal seeded play_count - 1 ({me.play_count}-1 mis-resolved)");
                // Target survives (life > damage) and stays on the board; both leaders untouched
                // (card_type=unit excludes class cards).
                Assert.That(target.Life, Is.EqualTo(targetLifeBefore - expectedDamage), "target life delta wrong");
                Assert.That(enemy.ClassAndInPlayCardList, Does.Contain(target), "target unexpectedly left the board");
                Assert.That(enemy.ClassAndInPlayCardList.Count, Is.EqualTo(enemyInplayBefore), "enemy board count changed");
                Assert.That(player.ClassAndInPlayCardList[0].Life, Is.EqualTo(playerLeaderLifeBefore), "player leader damaged (unit-only AoE hit a leader)");
                Assert.That(enemy.ClassAndInPlayCardList[0].Life, Is.EqualTo(enemyLeaderLifeBefore), "enemy leader damaged (unit-only AoE hit a leader)");
                // §5 spell-shaped invariants: cost paid, spell leaves hand, does NOT occupy the board.
                Assert.That(player.Pp, Is.EqualTo(ppBefore - cardParam.Cost), "PP not reduced by exactly cost");
                Assert.That(player.HandCardList, Does.Not.Contain(card), "spell still in hand");
                Assert.That(player.HandCardList.Count, Is.EqualTo(handBefore - 1), "hand count not -1");
                Assert.That(player.ClassAndInPlayCardList, Does.Not.Contain(card), "spell wrongly placed on the board");
                Assert.That(player.ClassAndInPlayCardList.Count, Is.EqualTo(playerInplayBefore), "player board count changed");
            });
        }
    }
}
