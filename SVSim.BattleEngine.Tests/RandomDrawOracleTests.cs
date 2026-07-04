using System.Linq;
using NUnit.Framework;
using SVSim.BattleEngine.Rng;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M12: the first card whose outcome is a GENUINE RNG roll. The M9 draw spell over a 3-card deck with
    // IsRandomDraw=true selects via SkillRandomSelectFilter -> GetIns().StableRandom(poolCount), which
    // HeadlessBattleMgr routes to the injected ScriptedRandomSource. The oracle asserts the engine drew
    // EXACTLY the card the scripted roll selects, and (load-bearing) that the pick TRACKS the script:
    // a different scripted unit draws a different card. This is the multi-outcome roll M9's one-card pool
    // deliberately neutralized — it requires the F2 decoupling (real rolls under IsForecast) AND the
    // IsRandomDraw=true second gate, both delivered by NewAuthoritativeBattle.
    [TestFixture]
    public class RandomDrawOracleTests
    {

        [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

        [TearDown]
        public void ResetRandomDrawGate()
        {
            // NewAuthoritativeBattle sets BattleManagerBase.IsRandomDraw = true on the mgr; the static
            // setter now no-ops without a mgr in scope (Phase-5 chunk 46), so this reset became a
            // per-mgr concern rather than a global fixture-hygiene one. Kept for symmetry.
            BattleManagerBase.IsRandomDraw = false;
        }

        // Draw with a single scripted unit; return (drawnCardId, deckCountAfter). The deck is seeded with
        // three distinguishable cards at indices 2,3,4 -> Index-order positions 0,1,2 map to
        // RngDeckCardA/B/C. The draw makes one StableRandom(3) call -> index = floor(3*unit).
        private (int drawnId, int deckAfter) DrawWith(double unit)
        {
            var mgr = HeadlessEngineEnv.NewAuthoritativeBattle(new ScriptedRandomSource(new[] { unit }));
            var player = mgr.BattlePlayer;

            HeadlessEngineEnv.SeedDeck(mgr, HeadlessEngineEnv.RngDeckCardA, index: 2, isPlayer: true);
            HeadlessEngineEnv.SeedDeck(mgr, HeadlessEngineEnv.RngDeckCardB, index: 3, isPlayer: true);
            HeadlessEngineEnv.SeedDeck(mgr, HeadlessEngineEnv.RngDeckCardC, index: 4, isPlayer: true);

            var spell = HeadlessEngineEnv.CreateHeadlessHandCard(HeadlessEngineEnv.RngDrawSpellId, 1, isPlayer: true, mgr);
            player.HandCardList.Add(spell);
            player.Pp = 10;

            var pair = mgr.GetBattlePlayerPair(isPlayer: true);
            var ap = new ActionProcessor(pair);
            Assert.DoesNotThrow(() => ap.PlayCard(spell, selectedCards: null), "PlayCard threw on the random draw");

            // The drawn card is the new hand entry that is not the spell.
            var drawn = player.HandCardList.Single(c => c.CardId != HeadlessEngineEnv.RngDrawSpellId);
            return (drawn.CardId, player.DeckCardList.Count);
        }

        [Test]
        public void Random_draw_picks_the_scripted_card()
        {
            // unit 0.5 -> floor(3*0.5)=1 -> Index-order position 1 -> RngDeckCardB.
            var (drawnId, deckAfter) = DrawWith(0.5);
            Assert.Multiple(() =>
            {
                Assert.That(drawnId, Is.EqualTo(HeadlessEngineEnv.RngDeckCardB),
                    "scripted roll 0.5 should draw the middle (Index-order position 1) deck card");
                Assert.That(deckAfter, Is.EqualTo(2), "deck should be 3 -> 2 after drawing one");
            });
        }

        [Test]
        public void Random_draw_pick_tracks_the_scripted_roll()
        {
            // Load-bearing: varying the scripted unit must move the pick across all three positions.
            // floor(3*0.0)=0 -> A ; floor(3*0.5)=1 -> B ; floor(3*0.9)=2 -> C.
            Assert.That(DrawWith(0.0).drawnId, Is.EqualTo(HeadlessEngineEnv.RngDeckCardA), "0.0 -> position 0");
            Assert.That(DrawWith(0.5).drawnId, Is.EqualTo(HeadlessEngineEnv.RngDeckCardB), "0.5 -> position 1");
            Assert.That(DrawWith(0.9).drawnId, Is.EqualTo(HeadlessEngineEnv.RngDeckCardC), "0.9 -> position 2");
        }
    }
}
