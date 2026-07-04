using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Wizard;
using Wizard.Battle;

namespace SVSim.BattleEngine.Tests
{
    // M9 (the §5 draw oracle): a when_play DRAW spell resolves to correct authoritative state HEADLESS
    // via the same IsForecast/IsRecovery + ActionProcessor path M2-M8 proved. The NEW oracle dimension
    // is the HAND/DECK DELTA — the deck->hand transfer no prior milestone read: M3/M4/M6/M8 moved
    // stats, M2/M5/M7 the board, M3 the leader. The spell's `draw 1` must pull the single seeded deck
    // card into the caster's hand (deck -1, that exact card now in hand) while the spell itself pays
    // its cost and leaves to the cemetery.
    //
    // RNG is neutralized structurally (see HeadlessEngineEnv.DrawSpellId): every real draw selects from
    // the deck via a `random_count` filter, so the deck is seeded with EXACTLY ONE known card — a
    // single-card pool makes `random_count=1` deterministic regardless of the RandomSeed. This rides
    // the M5 prefab card-creation path (the deck card is engine-created off the null-view seam) the same
    // way the summon-token milestone did.
    [TestFixture]
    public class DrawSpellOracleTests
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
        public void Draw_spell_moves_the_seeded_deck_card_into_hand()
        {
            BattleManagerBase.IsForecast = true;           // suppress VFX registration (F1)
            var mgr = HeadlessEngineEnv.NewSeededSingleBattleMgr();
            mgr.IsRecovery = true;                          // collapse wait delays to 0 (F1)

            var player = mgr.BattlePlayer;
            var enemy = mgr.BattleEnemy;

            // Minimal opponent/turn wiring (see M2-M8 oracles): opponent refs + active turn flag. The
            // draw resolves onto the active player's own hand/deck (the skill filter is character=me).
            SetPrivateField(player, "_opponentBattlePlayer", enemy);
            SetPrivateField(enemy, "_opponentBattlePlayer", player);
            player.IsSelfTurn = true;
            enemy.IsSelfTurn = false;

            // Seed leader life: this spell deals no damage, but the play-legality gate still rejects a
            // play when a leader reads as a 0-life game-over state (M3 learning).
            HeadlessEngineEnv.InitLeaderLife(mgr);

            // Seed the card-template prefabs the internal (createNullView:false) creation path clones —
            // the draw VFX touches the drawn card's view layer, so keep the M5 prefab surface available.
            HeadlessEngineEnv.InitCardTemplates(mgr);

            var cardParam = CardMaster.GetInstanceForBattle().GetCardParameterFromId(HeadlessEngineEnv.DrawSpellId);

            // Seed EXACTLY ONE known card on the caster's deck (forces the random_count=1 selection),
            // and place the draw spell in hand with PP to spare.
            var deckCard = HeadlessEngineEnv.SeedDeck(mgr, HeadlessEngineEnv.DeckSeedCardId, index: 2, isPlayer: true);
            var card = HeadlessEngineEnv.CreateHeadlessHandCard(HeadlessEngineEnv.DrawSpellId, 1, isPlayer: true, mgr);
            player.HandCardList.Add(card);
            player.Pp = 10;

            // Pre-state snapshot.
            int ppBefore = player.Pp;
            int handBefore = player.HandCardList.Count;
            int deckBefore = player.DeckCardList.Count;
            int cemeteryBefore = player.CemeteryList.Count;
            int playerInplayBefore = player.ClassAndInPlayCardList.Count;
            int enemyInplayBefore = enemy.ClassAndInPlayCardList.Count;

            // Sanity: the to-be-drawn card starts in the deck, not the hand.
            Assert.That(player.DeckCardList, Does.Contain(deckCard), "seeded card not in deck pre-play");
            Assert.That(player.HandCardList, Does.Not.Contain(deckCard), "seeded card already in hand pre-play");

            // Resolve the play through the real engine.
            var pair = mgr.GetBattlePlayerPair(isPlayer: true);
            var ap = new ActionProcessor(pair);
            Assert.DoesNotThrow(() => ap.PlayCard(card, selectedCards: null),
                "ActionProcessor.PlayCard threw on a draw spell");

            // Oracle: the deck->hand transfer is the new M9 dimension; the rest are the §5 spell-shaped
            // invariants proven by M3.
            Assert.Multiple(() =>
            {
                // Primary M9 assertion: the seeded deck card moved into the caster's hand...
                Assert.That(player.HandCardList, Does.Contain(deckCard),
                    "drawn card did not land in hand");
                Assert.That(player.HandCardList.Any(c => c.CardId == HeadlessEngineEnv.DeckSeedCardId), Is.True,
                    "no card with the seeded id is in hand");
                // ...and left the deck (deck -1, down to empty here).
                Assert.That(player.DeckCardList, Does.Not.Contain(deckCard), "drawn card still in deck");
                Assert.That(player.DeckCardList.Count, Is.EqualTo(deckBefore - 1), "deck count not -1");
                // The drawn card is the engine's OWN seeded deck object, not a fresh creation.
                Assert.That(deckCard.IsInHand, Is.True, "drawn card not marked in-hand");

                // The spell itself: pays exactly its cost...
                Assert.That(player.Pp, Is.EqualTo(ppBefore - cardParam.Cost), "PP not reduced by exactly cost");
                // ...leaves the hand (it is consumed, the drawn card replaces it -> net hand count flat)...
                Assert.That(player.HandCardList, Does.Not.Contain(card), "spell still in hand");
                Assert.That(player.HandCardList.Count, Is.EqualTo(handBefore), "hand count changed (spell -1 + draw +1 should net flat)");
                // ...resolves to the cemetery (a spell is not a follower; it never occupies the board).
                Assert.That(player.CemeteryList, Does.Contain(card), "spell did not resolve to the cemetery");
                Assert.That(player.CemeteryList.Count, Is.EqualTo(cemeteryBefore + 1), "cemetery count not +1");
                Assert.That(player.ClassAndInPlayCardList, Does.Not.Contain(card), "spell wrongly placed on the board");
                Assert.That(player.ClassAndInPlayCardList, Does.Not.Contain(deckCard), "drawn card wrongly placed on the board");
                Assert.That(player.ClassAndInPlayCardList.Count, Is.EqualTo(playerInplayBefore), "player board count changed");

                // Opponent untouched (the draw is character=me).
                Assert.That(enemy.ClassAndInPlayCardList.Count, Is.EqualTo(enemyInplayBefore), "opponent board changed");
            });
        }
    }
}
