using System.Reflection;
using NUnit.Framework;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M4 (next-hardest deterministic card): a when_play SELF-BUFF follower resolves to correct
    // authoritative state HEADLESS via the same IsForecast/IsRecovery + ActionProcessor path the M2
    // vanilla follower and M3 fixed-damage spell proved (design §5 / DP4 + M3 resume recipe). The new
    // oracle dimension over M2/M3 is the PLAYED CARD'S OWN STAT DELTA: the fanfare `powerup`
    // `add_offense=1&add_life=1` to `target=self` must raise the follower's Atk and Life by exactly
    // those amounts over its CardCSVData base — a self-buff, so no target selection is involved.
    [TestFixture]
    public class BuffFollowerOracleTests
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
        public void Self_buff_fanfare_raises_own_atk_and_life()
        {
            BattleManagerBase.IsForecast = true;           // suppress VFX registration (F1)
            var mgr = HeadlessEngineEnv.NewSeededSingleBattleMgr();
            mgr.IsRecovery = true;                          // collapse wait delays to 0 (F1)

            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;

            // Minimal opponent/turn wiring (see M2/M3 oracles): opponent refs + active turn flag. The
            // self-buff's target resolver (`character=me&target=self`) reads the active player's own
            // in-play card, so the turn flag must be set before the fanfare sweeps.
            SetPrivateField(player, "_opponentBattlePlayer", enemy);
            SetPrivateField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            // Seed leader life so neither leader reads as a 0-life game-over state that silently blocks
            // the play (M3 learning); this card deals no damage but the play-legality gate still checks it.
            HeadlessEngineEnv.InitLeaderLife(mgr);

            // The card's fanfare is gated on `play_count>2` (cards.json skill_condition for 103111050).
            // The engine reads this from BattlePlayerBase.GetCurrentTurnPlayCount(); seed it past the
            // threshold via the public AddCurrentTrunPlayCount so the powerup actually fires. (Without
            // this the card resolves to the board but takes no buff — the delta-vs-base oracle is what
            // distinguishes "buff applied" from "fanfare silently gated out".)
            player.AddCurrentTrunPlayCount(5);

            var cardParam = CardMaster.GetInstanceForBattle().GetCardParameterFromId(HeadlessEngineEnv.BuffFollowerId);

            // Place the self-buff follower in the active player's hand with PP to spare; empty board.
            var card = HeadlessEngineEnv.CreateHeadlessHandCard(HeadlessEngineEnv.BuffFollowerId, 1, isPlayer: true, mgr);
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
                "ActionProcessor.PlayCard threw on a self-buff fanfare follower");

            // Oracle: the own-stat delta is the new M4 dimension; the rest are the §5 follower invariants.
            Assert.Multiple(() =>
            {
                // Primary M4 assertion: the fanfare powerup raised the follower's own stats by exactly
                // the buff amounts over its CardCSVData base (1/1 -> 2/2).
                Assert.That(card.Atk, Is.EqualTo(cardParam.Atk + HeadlessEngineEnv.BuffAddOffense),
                    "follower atk != base + fanfare add_offense");
                Assert.That(card.Life, Is.EqualTo(cardParam.Life + HeadlessEngineEnv.BuffAddLife),
                    "follower life != base + fanfare add_life");
                // Cost paid.
                Assert.That(player.Pp, Is.EqualTo(ppBefore - cardParam.Cost), "PP not reduced by exactly cost");
                // Follower moved hand -> board.
                Assert.That(player.HandCardList, Does.Not.Contain(card), "card still in hand");
                Assert.That(player.HandCardList.Count, Is.EqualTo(handBefore - 1), "hand count not -1");
                Assert.That(player.ClassAndInPlayCardList, Contains.Item(card), "card not in play");
                Assert.That(player.ClassAndInPlayCardList.Count, Is.EqualTo(inplayBefore + 1), "in-play count not +1");
                // Opponent unchanged (the buff targets self, not the opponent).
                Assert.That(enemy.HandCardList.Count, Is.EqualTo(enemyHandBefore), "opponent hand changed");
                Assert.That(enemy.ClassAndInPlayCardList.Count, Is.EqualTo(enemyInplayBefore), "opponent board changed");
                Assert.That(enemy.ClassAndInPlayCardList[0].Life, Is.EqualTo(enemyLeaderLifeBefore), "opponent leader life changed");
            });
        }
    }
}
