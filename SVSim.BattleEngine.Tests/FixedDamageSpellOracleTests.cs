using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M3 (next-hardest deterministic card): a FIXED-DAMAGE SPELL resolves to correct authoritative
    // state HEADLESS via the same IsForecast/IsRecovery + ActionProcessor path the M2 vanilla
    // follower proved (design §5 / DP4 + M3 resume recipe). The new oracle dimension over M2 is the
    // OPPONENT LEADER-LIFE DELTA: the spell's when_play `damage=3` to the enemy leader must reduce
    // that leader's Life by exactly 3, with the spell consuming its cost and NOT entering the board.
    [TestFixture]
    public class FixedDamageSpellOracleTests
    {

        [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

        // The spell's sole skill is `damage=3` to the enemy leader (cards.json skill_option for 900124030).
        private const int ExpectedLeaderDamage = 3;

        private static void SetPrivateField(object obj, string name, object value)
        {
            var t = obj.GetType();
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            while (f == null && t.BaseType != null) { t = t.BaseType; f = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic); }
            Assert.That(f, Is.Not.Null, $"field {name} not found on {obj.GetType().Name}");
            f.SetValue(obj, value);
        }

        [Test]
        public void Fixed_damage_spell_reduces_opponent_leader_life()
        {
            BattleManagerBase.IsForecast = true;           // suppress VFX registration (F1)
            var mgr = HeadlessEngineEnv.NewSeededSingleBattleMgr();
            mgr.IsRecovery = true;                          // collapse wait delays to 0 (F1)

            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;

            // Minimal opponent/turn wiring (see M2 oracle): opponent refs + active turn flag. The
            // spell's target resolver walks player -> opponent -> opponent's class card (the leader).
            SetPrivateField(player, "_opponentBattlePlayer", enemy);
            SetPrivateField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            // Seed leader life (engine's InitializeClassLife subset) so the enemy leader is a live,
            // damageable target rather than a 0-life game-over state that blocks the play.
            HeadlessEngineEnv.InitLeaderLife(mgr);

            var cardParam = CardMaster.GetInstanceForBattle().GetCardParameterFromId(HeadlessEngineEnv.SpellId);

            // Place the spell in the active player's hand with PP to spare; empty board otherwise.
            var card = HeadlessEngineEnv.CreateHeadlessHandCard(HeadlessEngineEnv.SpellId, 1, isPlayer: true, mgr);
            player.HandCardList.Add(card);
            player.Pp = 10;

            // Pre-state snapshot.
            int ppBefore = player.Pp;
            int handBefore = player.HandCardList.Count;
            int playerInplayBefore = player.ClassAndInPlayCardList.Count;
            int enemyInplayBefore = enemy.ClassAndInPlayCardList.Count;
            int enemyLeaderLifeBefore = enemy.ClassAndInPlayCardList[0].Life;

            // Resolve the play through the real engine.
            var pair = mgr.GetBattlePlayerPair(isPlayer: true);
            var ap = new ActionProcessor(pair);
            Assert.DoesNotThrow(() => ap.PlayCard(card, selectedCards: null),
                "ActionProcessor.PlayCard threw on a fixed-damage spell");

            // Oracle: the leader-life delta is the new M3 dimension; the rest are the §5 spell-shaped invariants.
            Assert.Multiple(() =>
            {
                // Primary M3 assertion: opponent leader takes exactly the spell's fixed damage.
                Assert.That(enemy.ClassAndInPlayCardList[0].Life,
                    Is.EqualTo(enemyLeaderLifeBefore - ExpectedLeaderDamage),
                    "opponent leader life not reduced by the spell's fixed damage");
                // Cost paid.
                Assert.That(player.Pp, Is.EqualTo(ppBefore - cardParam.Cost), "PP not reduced by exactly cost");
                // Spell leaves hand.
                Assert.That(player.HandCardList, Does.Not.Contain(card), "spell still in hand");
                Assert.That(player.HandCardList.Count, Is.EqualTo(handBefore - 1), "hand count not -1");
                // A spell is not a follower: it must NOT occupy the board (resolves to graveyard).
                Assert.That(player.ClassAndInPlayCardList, Does.Not.Contain(card), "spell wrongly placed on the board");
                Assert.That(player.ClassAndInPlayCardList.Count, Is.EqualTo(playerInplayBefore), "player board count changed");
                // Opponent board (leader card only) count unchanged — only its life moved.
                Assert.That(enemy.ClassAndInPlayCardList.Count, Is.EqualTo(enemyInplayBefore), "opponent board count changed");
            });
        }
    }
}
