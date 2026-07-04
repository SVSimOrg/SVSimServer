using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M6 (the first TARGET-SELECTION card): a when_play TARGETED-DAMAGE spell resolves to correct
    // authoritative state HEADLESS via the same IsForecast/IsRecovery + ActionProcessor path the
    // M2-M5 cards proved — but for the FIRST time exercising the `selectedCards` path of
    // ActionProcessor.PlayCard (Engine/Wizard.Battle/ActionProcessor.cs:401, dormant until now;
    // M2-M5 all passed selectedCards: null). The new oracle dimension is SELECTION ROUTING: with
    // TWO followers on the enemy board and ONE passed as `selectedCards`, the spell's `damage=5`
    // must hit the SELECTED follower and leave the un-selected one untouched. A plain "a follower
    // took damage" assertion would false-pass; reading the differential (selected -5, un-selected 0)
    // is what proves the selectedCards path routes the effect to the chosen target. Load-bearing is
    // confirmed by swapping which follower is selected and watching the damage follow the selection.
    [TestFixture]
    public class TargetedDamageSpellOracleTests
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
        public void Targeted_damage_spell_hits_only_the_selected_enemy_follower()
        {
            BattleManagerBase.IsForecast = true;           // suppress VFX registration (F1)
            var mgr = HeadlessEngineEnv.NewSeededSingleBattleMgr();
            mgr.IsRecovery = true;                          // collapse wait delays to 0 (F1)

            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;

            // Minimal opponent/turn wiring (see M2-M5 oracles): opponent refs + active turn flag. The
            // spell's target resolver walks player -> opponent -> opponent's in-play followers.
            SetPrivateField(player, "_opponentBattlePlayer", enemy);
            SetPrivateField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            // Seed leader life so neither leader reads as a 0-life game-over state (blocks plays, M3).
            HeadlessEngineEnv.InitLeaderLife(mgr);

            // Put TWO vanilla followers on the ENEMY board (the new M6 setup). Both survive the 5
            // damage, so the oracle reads a differential life-delta rather than depending on death.
            var selected = HeadlessEngineEnv.PutFollowerInPlay(mgr, HeadlessEngineEnv.SelectTargetFollowerId, 0, isPlayer: false);
            var unselected = HeadlessEngineEnv.PutFollowerInPlay(mgr, HeadlessEngineEnv.UnselectTargetFollowerId, 1, isPlayer: false);

            var cardParam = CardMaster.GetInstanceForBattle().GetCardParameterFromId(HeadlessEngineEnv.TargetSpellId);

            // Place the targeted-damage spell in the active player's hand with PP to spare.
            var card = HeadlessEngineEnv.CreateHeadlessHandCard(HeadlessEngineEnv.TargetSpellId, 1, isPlayer: true, mgr);
            player.HandCardList.Add(card);
            player.Pp = 10;

            // Pre-state snapshot.
            int ppBefore = player.Pp;
            int handBefore = player.HandCardList.Count;
            int playerInplayBefore = player.ClassAndInPlayCardList.Count;
            int enemyInplayBefore = enemy.ClassAndInPlayCardList.Count;
            int selectedLifeBefore = selected.Life;
            int unselectedLifeBefore = unselected.Life;
            int enemyLeaderLifeBefore = enemy.ClassAndInPlayCardList[0].Life;

            // Resolve the play through the real engine, passing the chosen target via selectedCards
            // (the M6 first — every prior milestone passed null).
            var pair = mgr.GetBattlePlayerPair(isPlayer: true);
            var ap = new ActionProcessor(pair);
            Assert.DoesNotThrow(() => ap.PlayCard(card, selectedCards: new List<BattleCardBase> { selected }),
                "ActionProcessor.PlayCard threw on a targeted-damage spell");

            // Oracle: selection routing is the new M6 dimension; the rest are the §5 spell-shaped invariants.
            Assert.Multiple(() =>
            {
                // PRIMARY M6 assertions: the SELECTED follower takes exactly the spell's damage...
                Assert.That(selected.Life, Is.EqualTo(selectedLifeBefore - HeadlessEngineEnv.TargetSpellDamage),
                    "selected follower did not take the spell's damage");
                // ...and the UN-SELECTED follower is untouched (proves routing, not a blanket hit).
                Assert.That(unselected.Life, Is.EqualTo(unselectedLifeBefore),
                    "un-selected follower was damaged (effect not routed to the selection)");
                // Both followers survive => still on the enemy board; leader unchanged.
                Assert.That(enemy.ClassAndInPlayCardList.Count, Is.EqualTo(enemyInplayBefore),
                    "enemy board count changed (a target unexpectedly left the board)");
                Assert.That(enemy.ClassAndInPlayCardList[0].Life, Is.EqualTo(enemyLeaderLifeBefore),
                    "opponent leader life changed (damage hit the leader, not the selected follower)");
                // Cost paid.
                Assert.That(player.Pp, Is.EqualTo(ppBefore - cardParam.Cost), "PP not reduced by exactly cost");
                // Spell leaves hand and (being a spell) does NOT occupy the board.
                Assert.That(player.HandCardList, Does.Not.Contain(card), "spell still in hand");
                Assert.That(player.HandCardList.Count, Is.EqualTo(handBefore - 1), "hand count not -1");
                Assert.That(player.ClassAndInPlayCardList, Does.Not.Contain(card), "spell wrongly placed on the board");
                Assert.That(player.ClassAndInPlayCardList.Count, Is.EqualTo(playerInplayBefore), "player board count changed");
            });
        }
    }
}
