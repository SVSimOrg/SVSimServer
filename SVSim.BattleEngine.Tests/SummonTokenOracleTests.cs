using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M5 (next-hardest deterministic card): a when_play SUMMON_TOKEN spell resolves to correct
    // authoritative state HEADLESS via the same IsForecast/IsRecovery + ActionProcessor path the
    // M2 vanilla follower / M3 fixed-damage spell / M4 self-buff follower proved (design §5 / DP4 +
    // M3+ resume recipe). The new oracle dimension over M2-M4 is a BOARD-COUNT DELTA from a
    // SKILL-CREATED card: the spell's `summon_token=100011020` must place exactly one NEW follower
    // token (id 100011020, a neutral 2/2) onto the caster's board — a card that was never in the
    // hand or deck. This is the first headless run of the PUBLIC prefab card-creation path
    // (CardCreatorBase.CreateCard, createNullView:false), so it stresses the view shim in a way the
    // earlier null-view-seam milestones did not.
    [TestFixture]
    public class SummonTokenOracleTests
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
        public void Summon_token_spell_places_a_new_token_on_the_board()
        {
            BattleManagerBase.IsForecast = true;           // suppress VFX registration (F1)
            var mgr = HeadlessEngineEnv.NewSeededSingleBattleMgr();
            mgr.IsRecovery = true;                          // collapse wait delays to 0 (F1)

            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;

            // Minimal opponent/turn wiring (see M2-M4 oracles): opponent refs + active turn flag.
            // The summon resolves onto the active player's own board (`summon_side` defaults to self).
            SetPrivateField(player, "_opponentBattlePlayer", enemy);
            SetPrivateField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            // Seed leader life: this spell deals no damage, but the play-legality gate still rejects a
            // play when a leader reads as a 0-life game-over state (M3 learning).
            HeadlessEngineEnv.InitLeaderLife(mgr);

            // Seed the card-template prefabs the engine's internal (createNullView:false) summon
            // creation path clones — the bare construction path leaves SBattleLoad null.
            HeadlessEngineEnv.InitCardTemplates(mgr);

            var cardParam = CardMaster.GetInstanceForBattle().GetCardParameterFromId(HeadlessEngineEnv.TokenSpellId);

            // Place the summon-token spell in the active player's hand with PP to spare; empty board.
            var card = HeadlessEngineEnv.CreateHeadlessHandCard(HeadlessEngineEnv.TokenSpellId, 1, isPlayer: true, mgr);
            player.HandCardList.Add(card);
            player.Pp = 10;

            // Pre-state snapshot. ClassAndInPlayCardList holds the leader (index 0) on an empty board.
            int ppBefore = player.Pp;
            int handBefore = player.HandCardList.Count;
            int playerInplayBefore = player.ClassAndInPlayCardList.Count;
            int enemyHandBefore = enemy.HandCardList.Count;
            int enemyInplayBefore = enemy.ClassAndInPlayCardList.Count;
            int enemyLeaderLifeBefore = enemy.ClassAndInPlayCardList[0].Life;

            // Resolve the play through the real engine.
            var pair = mgr.GetBattlePlayerPair(isPlayer: true);
            var ap = new ActionProcessor(pair);
            Assert.DoesNotThrow(() => ap.PlayCard(card, selectedCards: null),
                "ActionProcessor.PlayCard threw on a summon-token spell");

            // Oracle: the board-count delta + summoned token identity is the new M5 dimension; the rest
            // are the §5 spell-shaped invariants proven by M3.
            Assert.Multiple(() =>
            {
                // Primary M5 assertion: exactly one NEW card is on the player's board (the summoned
                // token), and it is the token id with its CardCSVData base stats — proving the skill
                // CREATED a card, not just moved the played one.
                Assert.That(player.ClassAndInPlayCardList.Count, Is.EqualTo(playerInplayBefore + 1),
                    "player board count not +1 (the summoned token did not land)");
                var token = player.ClassAndInPlayCardList
                    .SingleOrDefault(c => c.CardId == HeadlessEngineEnv.SummonedTokenId);
                Assert.That(token, Is.Not.Null, "summoned token (id 100011020) not found on the board");
                Assert.That(token.Atk, Is.EqualTo(HeadlessEngineEnv.SummonedTokenAtk), "token atk != base");
                Assert.That(token.Life, Is.EqualTo(HeadlessEngineEnv.SummonedTokenLife), "token life != base");
                // The summoned token is NOT the played card.
                Assert.That(token, Is.Not.SameAs(card), "summoned token is the played spell itself");
                // Cost paid.
                Assert.That(player.Pp, Is.EqualTo(ppBefore - cardParam.Cost), "PP not reduced by exactly cost");
                // The spell leaves hand and (being a spell) does NOT itself occupy the board.
                Assert.That(player.HandCardList, Does.Not.Contain(card), "spell still in hand");
                Assert.That(player.HandCardList.Count, Is.EqualTo(handBefore - 1), "hand count not -1");
                Assert.That(player.ClassAndInPlayCardList, Does.Not.Contain(card), "spell wrongly placed on the board");
                // Opponent unchanged (the summon targets the caster's own board).
                Assert.That(enemy.HandCardList.Count, Is.EqualTo(enemyHandBefore), "opponent hand changed");
                Assert.That(enemy.ClassAndInPlayCardList.Count, Is.EqualTo(enemyInplayBefore), "opponent board changed");
                Assert.That(enemy.ClassAndInPlayCardList[0].Life, Is.EqualTo(enemyLeaderLifeBefore), "opponent leader life changed");
            });
        }
    }
}
