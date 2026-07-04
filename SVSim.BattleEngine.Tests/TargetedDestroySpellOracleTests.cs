using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M7 (the first card to prove follower DEATH / board-removal): a when_play TARGETED-DESTROY spell
    // resolves to correct authoritative state HEADLESS via the same IsForecast/IsRecovery +
    // ActionProcessor + selectedCards path M6 proved — but for the FIRST time exercising a mechanic
    // that REMOVES a card from the board. M2-M6 only ever ADDED to / mutated stats of cards already in
    // play; none proved the engine commits board REMOVAL inside the authoritative part of PlayCard
    // (rather than the cosmetic post-Process tail the prior docs flag). The new oracle dimension is
    // BOARD REMOVAL: with TWO followers on the enemy board and ONE passed as `selectedCards`, the
    // `destroy` must remove exactly the SELECTED follower (enemy board count -1, selected gone, landed
    // in the enemy CemeteryList) while leaving the un-selected follower on the board. The un-selected-
    // survives assertion is load-bearing the same way M4's delta-vs-base and M6's differential were:
    // it distinguishes "the destroy was routed to the selection" from "a blanket board wipe" — and is
    // confirmed by the routing already proven in M6 (the effect follows the selectedCards entry).
    [TestFixture]
    public class TargetedDestroySpellOracleTests
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
        public void Targeted_destroy_spell_removes_only_the_selected_enemy_follower()
        {
            BattleManagerBase.IsForecast = true;           // suppress VFX registration (F1)
            var mgr = HeadlessEngineEnv.NewSeededSingleBattleMgr();
            mgr.IsRecovery = true;                          // collapse wait delays to 0 (F1)

            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;

            // Minimal opponent/turn wiring (see M2-M6 oracles): opponent refs + active turn flag. The
            // destroy's target resolver walks player -> opponent -> opponent's in-play followers.
            SetPrivateField(player, "_opponentBattlePlayer", enemy);
            SetPrivateField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            // Seed leader life so neither leader reads as a 0-life game-over state (blocks plays, M3).
            HeadlessEngineEnv.InitLeaderLife(mgr);

            // Put TWO vanilla followers on the ENEMY board (the M6 setup). destroy is unconditional, so
            // their stats are irrelevant — distinct ids only so the selected vs un-selected can't be
            // confused. The selected one is destroyed; the un-selected one must survive.
            var selected = HeadlessEngineEnv.PutFollowerInPlay(mgr, HeadlessEngineEnv.DestroyTargetFollowerId, 0, isPlayer: false);
            var unselected = HeadlessEngineEnv.PutFollowerInPlay(mgr, HeadlessEngineEnv.DestroyOtherFollowerId, 1, isPlayer: false);

            var cardParam = CardMaster.GetInstanceForBattle().GetCardParameterFromId(HeadlessEngineEnv.DestroySpellId);

            // Place the targeted-destroy spell in the active player's hand with PP to spare.
            var card = HeadlessEngineEnv.CreateHeadlessHandCard(HeadlessEngineEnv.DestroySpellId, 1, isPlayer: true, mgr);
            player.HandCardList.Add(card);
            player.Pp = 10;

            // Pre-state snapshot.
            int ppBefore = player.Pp;
            int handBefore = player.HandCardList.Count;
            int playerInplayBefore = player.ClassAndInPlayCardList.Count;
            int enemyInplayBefore = enemy.ClassAndInPlayCardList.Count;
            int enemyCemeteryBefore = enemy.CemeteryList.Count;
            int enemyLeaderLifeBefore = enemy.ClassAndInPlayCardList[0].Life;

            // Resolve the play through the real engine, passing the chosen target via selectedCards.
            var pair = mgr.GetBattlePlayerPair(isPlayer: true);
            var ap = new ActionProcessor(pair);
            Assert.DoesNotThrow(() => ap.PlayCard(card, selectedCards: new List<BattleCardBase> { selected }),
                "ActionProcessor.PlayCard threw on a targeted-destroy spell");

            // Oracle: board removal is the new M7 dimension; the rest are the §5 spell-shaped invariants.
            Assert.Multiple(() =>
            {
                // PRIMARY M7 assertions: the SELECTED follower is removed from the enemy board...
                Assert.That(enemy.ClassAndInPlayCardList, Does.Not.Contain(selected),
                    "selected follower still on the enemy board (destroy did not remove it)");
                Assert.That(enemy.ClassAndInPlayCardList.Count, Is.EqualTo(enemyInplayBefore - 1),
                    "enemy board count not -1 (a destroy did not commit, or hit the wrong number of cards)");
                // ...and it landed in the enemy's CemeteryList (the engine's destroy/death path).
                Assert.That(enemy.CemeteryList, Contains.Item(selected),
                    "destroyed follower not in the enemy CemeteryList");
                Assert.That(enemy.CemeteryList.Count, Is.EqualTo(enemyCemeteryBefore + 1),
                    "enemy cemetery count not +1");
                // ...while the UN-SELECTED follower stays on the board (proves routing, not a board wipe).
                Assert.That(enemy.ClassAndInPlayCardList, Contains.Item(unselected),
                    "un-selected follower was destroyed (effect not routed to the selection)");
                // Leader untouched (destroy targets a follower, not the face).
                Assert.That(enemy.ClassAndInPlayCardList[0].Life, Is.EqualTo(enemyLeaderLifeBefore),
                    "opponent leader life changed (destroy hit the leader, not the selected follower)");
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
