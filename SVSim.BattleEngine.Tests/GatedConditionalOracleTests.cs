using System.Reflection;
using NUnit.Framework;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M11 (the GATE itself is the oracle): every prior milestone either had no skill_condition or
    // seeded its gate TRUE so the effect fires (M4 seeded play_count>2; M10 seeded a play_count
    // VALUE). None proved the engine SUPPRESSES an effect when a skill_condition evaluates FALSE —
    // the dual of "effect fires". M11 proves conditional BRANCHING resolves headless by asserting
    // BOTH directions of the SAME gated card in ONE fixture (design "M11 — NEXT" resume guide):
    //
    //   * gate TRUE  (play_count > 2, seeded via the public AddCurrentTrunPlayCount seam M4/M10 use)
    //                -> the when_play powerup fires -> the follower is buffed over its base stats.
    //   * gate FALSE (play_count <= 2, the bare-construction default)
    //                -> the powerup is a NO-OP: zero stat delta, BUT the card still pays its cost
    //                   and still leaves hand -> board (the gate suppresses the EFFECT, not the PLAY).
    //
    // Card: 103111050 — the M4 self-buff follower (ELF clan-1 cost-1 base 1/1, sole non-evo skill
    // `when_play` `powerup` `add_offense=1&add_life=1` to `character=me&target=self`), whose
    // skill_condition is `character=me&target=self&play_count>2` (verified in cards.json). The gate
    // reads BattlePlayerBase.GetCurrentTurnPlayCount(), seedable past/below the threshold via the
    // public AddCurrentTrunPlayCount. Reusing the M4-proven buff DIMENSION means the only NEW thing
    // under test is the CONDITIONAL — exactly the resume-guide's "proven effect dimension, gate is
    // the oracle" prescription.
    //
    // Why one fixture, both branches, ONE card is decisive: the two assertions are jointly
    // satisfiable ONLY by a correctly-gating engine. An "always-buffs" engine fails the FALSE branch
    // (would buff with play_count=0); a "never-buffs" engine fails the TRUE branch (M4's gate seed
    // wouldn't fire). M4 already demonstrated this split as a manual load-bearing probe (remove the
    // seed -> buff vanishes); M11 promotes it to the PRIMARY assertion.
    [TestFixture]
    public class GatedConditionalOracleTests
    {
        // A clearly super-threshold seed (play_count 5 > 2): the gate evaluates TRUE, fanfare fires.
        private const int GateTrueSeed = 5;
        // The bare-construction default is play_count 0 (<= 2 -> gate FALSE); we seed nothing for the
        // FALSE branch, exactly as M4's load-bearing probe did when it removed its seed.
        private const int GateFalseSeed = 0;


        [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

        private static void SetPrivateField(object obj, string name, object value)
        {
            var t = obj.GetType();
            var f = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            while (f == null && t.BaseType != null) { t = t.BaseType; f = t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic); }
            Assert.That(f, Is.Not.Null, $"field {name} not found on {obj.GetType().Name}");
            f.SetValue(obj, value);
        }

        // Resolve the gated self-buff follower on a FRESH battle with the per-turn play count seeded
        // to `seededPlayCount`, and report the play's outcome. A fresh mgr per branch is required:
        // play_count is per-mgr state and a resolved play mutates the board, so the two branches must
        // not share a battle. Mirrors the M4 BuffFollowerOracleTests setup verbatim, parameterized on
        // the seed (which is the only thing M11 varies between branches).
        private (BattleCardBase card, CardParameter param, int ppBefore, int ppAfter,
                        int handBefore, bool inHandAfter, int inplayBefore, bool onBoardAfter, int inplayAfter)
            PlayGatedSelfBuff(int seededPlayCount)
        {
            BattleManagerBase.IsForecast = true;           // suppress VFX registration (F1)
            var mgr = HeadlessEngineEnv.NewSeededSingleBattleMgr();
            mgr.IsRecovery = true;                          // collapse wait delays to 0 (F1)

            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;

            // Minimal opponent/turn wiring (M2/M3/M4 oracles): opponent refs + active turn flag. The
            // self-buff target resolver (`character=me&target=self`) reads the active player's own
            // in-play card, so the turn flag must be set before the fanfare sweeps.
            SetPrivateField(player, "_opponentBattlePlayer", enemy);
            SetPrivateField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            // Seed leader life so neither leader reads as a 0-life game-over that silently blocks the
            // play (M3 learning). This card deals no damage but the play-legality gate still checks it.
            HeadlessEngineEnv.InitLeaderLife(mgr);

            // THE GATE SEED — the one knob M11 turns between branches. The skill_condition
            // `play_count>2` reads BattlePlayerBase.GetCurrentTurnPlayCount(); seed it via the public
            // AddCurrentTrunPlayCount (M4/M10 seam). For the FALSE branch we leave the bare default 0.
            if (seededPlayCount > 0) player.AddCurrentTrunPlayCount(seededPlayCount);

            var cardParam = CardMaster.GetInstanceForBattle().GetCardParameterFromId(HeadlessEngineEnv.BuffFollowerId);

            // Place the gated self-buff follower in the active player's hand with PP to spare; empty board.
            var card = HeadlessEngineEnv.CreateHeadlessHandCard(HeadlessEngineEnv.BuffFollowerId, 1, isPlayer: true, mgr);
            player.HandCardList.Add(card);
            player.Pp = 10;

            int ppBefore = player.Pp;
            int handBefore = player.HandCardList.Count;
            int inplayBefore = player.ClassAndInPlayCardList.Count;

            var pair = mgr.GetBattlePlayerPair(isPlayer: true);
            var ap = new ActionProcessor(pair);
            Assert.DoesNotThrow(() => ap.PlayCard(card, selectedCards: null),
                $"ActionProcessor.PlayCard threw on the gated self-buff (seed={seededPlayCount})");

            return (card, cardParam, ppBefore, player.Pp,
                    handBefore, player.HandCardList.Contains(card),
                    inplayBefore, player.ClassAndInPlayCardList.Contains(card),
                    player.ClassAndInPlayCardList.Count);
        }

        [Test]
        public void Gated_fanfare_fires_when_seeded_true_and_is_suppressed_when_false()
        {
            // ----- Branch 1: gate TRUE (play_count 5 > 2) -> the fanfare FIRES (M4 dimension). -----
            var t = PlayGatedSelfBuff(GateTrueSeed);

            // ----- Branch 2: gate FALSE (play_count 0 <= 2) -> the fanfare is SUPPRESSED. -----
            var f = PlayGatedSelfBuff(GateFalseSeed);

            Assert.Multiple(() =>
            {
                // PRIMARY M11 assertion — the gate itself: SAME card, opposite stat outcomes driven
                // ONLY by the seeded condition.
                //   TRUE  -> buffed: base 1/1 + 1/1 = 2/2.
                Assert.That(t.card.Atk, Is.EqualTo(t.param.Atk + HeadlessEngineEnv.BuffAddOffense),
                    "[gate TRUE] atk != base + add_offense (fanfare should have fired)");
                Assert.That(t.card.Life, Is.EqualTo(t.param.Life + HeadlessEngineEnv.BuffAddLife),
                    "[gate TRUE] life != base + add_life (fanfare should have fired)");
                //   FALSE -> unbuffed: stays at the CardCSVData base 1/1 (effect suppressed).
                Assert.That(f.card.Atk, Is.EqualTo(f.param.Atk),
                    "[gate FALSE] atk != base (fanfare should have been gated out)");
                Assert.That(f.card.Life, Is.EqualTo(f.param.Life),
                    "[gate FALSE] life != base (fanfare should have been gated out)");

                // The gate suppresses the EFFECT, not the PLAY: in BOTH branches the card still pays
                // its cost and still moves hand -> board like any follower.
                // TRUE branch:
                Assert.That(t.ppAfter, Is.EqualTo(t.ppBefore - t.param.Cost), "[gate TRUE] PP not reduced by cost");
                Assert.That(t.inHandAfter, Is.False, "[gate TRUE] card still in hand");
                Assert.That(t.onBoardAfter, Is.True, "[gate TRUE] card not on board");
                Assert.That(t.inplayAfter, Is.EqualTo(t.inplayBefore + 1), "[gate TRUE] in-play count not +1");
                // FALSE branch — the M11 crux: cost STILL paid + card STILL resolves despite the no-op effect.
                Assert.That(f.ppAfter, Is.EqualTo(f.ppBefore - f.param.Cost), "[gate FALSE] PP not reduced by cost");
                Assert.That(f.inHandAfter, Is.False, "[gate FALSE] card still in hand");
                Assert.That(f.onBoardAfter, Is.True, "[gate FALSE] card not on board");
                Assert.That(f.inplayAfter, Is.EqualTo(f.inplayBefore + 1), "[gate FALSE] in-play count not +1");
            });
        }
    }
}
