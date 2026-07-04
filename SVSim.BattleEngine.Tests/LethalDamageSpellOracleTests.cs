using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M8 (death VIA COMBAT MATH): a when_play TARGETED-DAMAGE spell whose amount is >= the target
    // follower's life resolves to correct authoritative state HEADLESS via the same IsForecast/
    // IsRecovery + ActionProcessor + selectedCards path M6/M7 proved. M3 proved `damage` to the LEADER
    // (life-delta, no death). M7 proved board-removal via UNCONDITIONAL `destroy`. M8 closes the gap
    // between them: the follower dies as a CONSEQUENCE of damage -> life<=0 -> the dead-check + the same
    // RemoveInplayCard/cemetery path M7 lit up — the dominant real-card removal mechanic (most "deal N
    // damage" cards), reached through combat math rather than a `destroy` skill.
    //
    // The spell is select_count=1 (proven in M6 — it hits ONLY the selected target), so the oracle is:
    // with two followers on the enemy board STRADDLING the 5 damage and the LETHAL one passed as
    // `selectedCards`, the selected follower (life 2 <= 5) DIES from combat math (enemy board -1, gone,
    // in CemeteryList — the M7 removal assertions, but reached via damage not `destroy`), while the
    // un-selected control (life 7 > 5) is UNTOUCHED (life unchanged, still on board — the M6 routing
    // assertion). The STRADDLE is what makes death-via-combat-math falsifiable: the load-bearing probe
    // (swap the selection to the 6/7) makes that follower SURVIVE at 2 (7-5) and NOBODY die — proving
    // the removal is gated on the SELECTED follower's life reaching <= 0 (combat math), not on
    // "selected gets removed" (which would be M7's unconditional `destroy`) or a blanket wipe.
    [TestFixture]
    public class LethalDamageSpellOracleTests
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
        public void Lethal_damage_spell_kills_the_selected_follower_and_chips_the_survivor()
        {
            BattleManagerBase.IsForecast = true;           // suppress VFX registration (F1)
            var mgr = HeadlessEngineEnv.NewSeededSingleBattleMgr();
            mgr.IsRecovery = true;                          // collapse wait delays to 0 (F1)

            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;

            // Minimal opponent/turn wiring (see M2-M7 oracles): opponent refs + active turn flag. The
            // spell's target resolver walks player -> opponent -> opponent's in-play followers.
            SetPrivateField(player, "_opponentBattlePlayer", enemy);
            SetPrivateField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            // Seed leader life so neither leader reads as a 0-life game-over state (blocks plays, M3).
            HeadlessEngineEnv.InitLeaderLife(mgr);

            // Put TWO vanilla followers on the ENEMY board STRADDLING the 5 damage: the SELECTED target
            // has life 2 (<= 5) so it dies; the un-selected control has life 7 (> 5) and, being a
            // select_count=1 spell's non-target, is untouched. (The straddle powers the load-bearing
            // probe: selecting the 6/7 instead makes it survive at 2 and nobody die.)
            var selected = HeadlessEngineEnv.PutFollowerInPlay(mgr, HeadlessEngineEnv.LethalTargetFollowerId, 0, isPlayer: false);
            var survivor = HeadlessEngineEnv.PutFollowerInPlay(mgr, HeadlessEngineEnv.SurvivorTargetFollowerId, 1, isPlayer: false);

            // Sanity: the chosen ids actually straddle the damage (one lethal, one not) at setup.
            Assert.That(selected.Life, Is.LessThanOrEqualTo(HeadlessEngineEnv.LethalDamage),
                "selected follower's life is not <= the spell damage (it would not die)");
            Assert.That(survivor.Life, Is.GreaterThan(HeadlessEngineEnv.LethalDamage),
                "survivor follower's life is not > the spell damage (it would not survive)");

            var cardParam = CardMaster.GetInstanceForBattle().GetCardParameterFromId(HeadlessEngineEnv.LethalDamageSpellId);

            // Place the lethal-damage spell in the active player's hand with PP to spare.
            var card = HeadlessEngineEnv.CreateHeadlessHandCard(HeadlessEngineEnv.LethalDamageSpellId, 1, isPlayer: true, mgr);
            player.HandCardList.Add(card);
            player.Pp = 10;

            // Pre-state snapshot.
            int ppBefore = player.Pp;
            int handBefore = player.HandCardList.Count;
            int playerInplayBefore = player.ClassAndInPlayCardList.Count;
            int enemyInplayBefore = enemy.ClassAndInPlayCardList.Count;
            int enemyCemeteryBefore = enemy.CemeteryList.Count;
            int survivorLifeBefore = survivor.Life;
            int enemyLeaderLifeBefore = enemy.ClassAndInPlayCardList[0].Life;

            // Resolve the play through the real engine, passing the chosen (lethal) target via selectedCards.
            var pair = mgr.GetBattlePlayerPair(isPlayer: true);
            var ap = new ActionProcessor(pair);
            Assert.DoesNotThrow(() => ap.PlayCard(card, selectedCards: new List<BattleCardBase> { selected }),
                "ActionProcessor.PlayCard threw on a lethal targeted-damage spell");

            Assert.Multiple(() =>
            {
                // PRIMARY M8 — death via combat math: the SELECTED follower (life <= damage) is removed
                // from the enemy board and lands in the cemetery (the M7 removal dimension, reached
                // through damage rather than `destroy`).
                Assert.That(enemy.ClassAndInPlayCardList, Does.Not.Contain(selected),
                    "lethal-damaged follower still on the enemy board (death-via-damage did not remove it)");
                Assert.That(enemy.ClassAndInPlayCardList.Count, Is.EqualTo(enemyInplayBefore - 1),
                    "enemy board count not -1 (lethal damage did not commit a removal, or hit the wrong count)");
                Assert.That(enemy.CemeteryList, Contains.Item(selected),
                    "lethal-damaged follower not in the enemy CemeteryList");
                Assert.That(enemy.CemeteryList.Count, Is.EqualTo(enemyCemeteryBefore + 1),
                    "enemy cemetery count not +1");

                // PRIMARY M8 — routing: the UN-SELECTED control (life > damage) is UNTOUCHED and stays on
                // the board (the M6 routing assertion; select_count=1 hits only the selected target, so
                // this proves the lethal removal was routed to the selection and is not a blanket wipe).
                Assert.That(enemy.ClassAndInPlayCardList, Contains.Item(survivor),
                    "un-selected follower was removed (effect not routed, or a blanket wipe)");
                Assert.That(survivor.Life, Is.EqualTo(survivorLifeBefore),
                    "un-selected follower took damage (effect not routed to the selection)");

                // Leader untouched (the spell targets a follower, not the face).
                Assert.That(enemy.ClassAndInPlayCardList[0].Life, Is.EqualTo(enemyLeaderLifeBefore),
                    "opponent leader life changed (damage hit the leader, not the selected follower)");

                // Cost paid; spell leaves hand and (being a spell) does NOT occupy the board.
                Assert.That(player.Pp, Is.EqualTo(ppBefore - cardParam.Cost), "PP not reduced by exactly cost");
                Assert.That(player.HandCardList, Does.Not.Contain(card), "spell still in hand");
                Assert.That(player.HandCardList.Count, Is.EqualTo(handBefore - 1), "hand count not -1");
                Assert.That(player.ClassAndInPlayCardList, Does.Not.Contain(card), "spell wrongly placed on the board");
                Assert.That(player.ClassAndInPlayCardList.Count, Is.EqualTo(playerInplayBefore), "player board count changed");
            });
        }
    }
}
