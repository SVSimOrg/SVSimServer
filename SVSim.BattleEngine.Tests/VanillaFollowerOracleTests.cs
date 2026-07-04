using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M2 first-green (go/no-go step 2): a single zero-skill vanilla follower resolves to correct
    // authoritative state HEADLESS via the proven IsForecast/IsRecovery + ActionProcessor path
    // (design §5 / DP4). No Unity runtime, no VFX clock.
    [TestFixture]
    public class VanillaFollowerOracleTests
    {

        [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

        private static void SetPrivateField(object obj, string name, object value)
        {
            var f = obj.GetType().GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            // Walk up the hierarchy if declared on a base type.
            var t = obj.GetType();
            while (f == null && t.BaseType != null) { t = t.BaseType; f = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic); }
            Assert.That(f, Is.Not.Null, $"field {name} not found on {obj.GetType().Name}");
            f.SetValue(obj, value);
        }

        [Test]
        public void Vanilla_follower_resolves_to_correct_state()
        {
            BattleManagerBase.IsForecast = true;           // suppress VFX registration (F1)
            var mgr = HeadlessEngineEnv.NewSeededSingleBattleMgr();
            mgr.IsRecovery = true;                          // collapse wait delays to 0 (F1)

            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;

            // Wire the opponent links + active turn. The full BattlePlayerBase.Setup(opponent) does
            // this but cascades into UI/manager init irrelevant to the resolution path, so set the
            // minimal state directly: each player's opponent ref, and the active player's turn flag
            // (the on-enter-play skill sweep reads opponent.IsSelfTurn / IsGameFirst).
            SetPrivateField(player, "_opponentBattlePlayer", enemy);
            SetPrivateField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            var cardParam = CardMaster.GetInstanceForBattle().GetCardParameterFromId(HeadlessEngineEnv.FollowerId);

            // Place the follower in the active player's hand with PP to spare; empty board otherwise.
            var card = HeadlessEngineEnv.CreateHeadlessHandCard(HeadlessEngineEnv.FollowerId, 1, isPlayer: true, mgr);
            player.HandCardList.Add(card);
            player.Pp = 10;

            // Pre-state snapshot.
            int ppBefore = player.Pp;
            int handBefore = player.HandCardList.Count;
            int inplayBefore = player.ClassAndInPlayCardList.Count;
            int enemyHandBefore = enemy.HandCardList.Count;
            int enemyInplayBefore = enemy.ClassAndInPlayCardList.Count;
            int enemyLeaderLifeBefore = enemy.ClassAndInPlayCardList[0].Life;

            // Resolve the play through the real engine.
            var pair = mgr.GetBattlePlayerPair(isPlayer: true);
            var ap = new ActionProcessor(pair);
            Assert.DoesNotThrow(() => ap.PlayCard(card, selectedCards: null),
                "ActionProcessor.PlayCard threw on a vanilla follower");

            // Oracle (§5 invariants).
            Assert.Multiple(() =>
            {
                Assert.That(player.Pp, Is.EqualTo(ppBefore - cardParam.Cost), "PP not reduced by exactly cost");
                Assert.That(player.HandCardList, Does.Not.Contain(card), "card still in hand");
                Assert.That(player.HandCardList.Count, Is.EqualTo(handBefore - 1), "hand count not -1");
                Assert.That(player.ClassAndInPlayCardList, Contains.Item(card), "card not in play");
                Assert.That(player.ClassAndInPlayCardList.Count, Is.EqualTo(inplayBefore + 1), "in-play count not +1");
                Assert.That(card.Atk, Is.EqualTo(cardParam.Atk), "follower atk != CardCSVData base");
                Assert.That(card.Life, Is.EqualTo(cardParam.Life), "follower life != CardCSVData base");
                // Opponent unchanged.
                Assert.That(enemy.HandCardList.Count, Is.EqualTo(enemyHandBefore), "opponent hand changed");
                Assert.That(enemy.ClassAndInPlayCardList.Count, Is.EqualTo(enemyInplayBefore), "opponent board changed");
                Assert.That(enemy.ClassAndInPlayCardList[0].Life, Is.EqualTo(enemyLeaderLifeBefore), "opponent leader life changed");
                // §5 "zero VFX registered with VfxMgr": structural here — the shim VfxMgr is a pure
                // no-op (RegisterImmediate/SequentialVfx do nothing) and IsForecast suppresses
                // registration in the real engine, so no VFX is ever played headless. Covered by the
                // DoesNotThrow above; there is no meaningful count to assert against the no-op shim.
            });
        }
    }
}
