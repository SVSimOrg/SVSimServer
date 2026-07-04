using System.Linq;
using NUnit.Framework;
using SVSim.BattleNode.Sessions.Engine;

namespace SVSim.BattleEngine.Tests.SessionEngine
{
    [TestFixture]
    public class SessionEngineConstructionTests
    {

        [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

        [Test]
        public void SessionBattleEngine_instantiates_and_is_not_ready_before_setup()
        {
            var engine = new SessionBattleEngine();
            Assert.That(engine.IsReady, Is.False);
        }

        [Test]
        public void Setup_builds_two_seat_network_battle_headless()
        {
            // Load every card id the two test decks reference so CardMaster can resolve them.
            var deckA = Enumerable.Repeat(100011010L, 40).ToList(); // vanilla 1/2 follower x40
            var deckB = Enumerable.Repeat(100011010L, 40).ToList();
            HeadlessCardMaster.Load(100011010);

            var engine = new SessionBattleEngine();
            Assert.DoesNotThrow(() => engine.Setup(masterSeed: 12345, seatADeck: deckA, seatBDeck: deckB));
            Assert.That(engine.IsReady, Is.True);
        }

        [Test]
        public void Receive_one_playactions_resolves_headless()
        {
            // SUPERSEDED by the node-native oracle (SVSim.UnitTests HeadlessConductorTests). This test
            // predates the M-HC-0b view-untangle: before it, the receive conductor resolved NOTHING
            // headless (every InstantVfx the conductor fused the mutation into was no-op'd by the shared
            // VfxMgr, and OperateReceive.OnReceiveDeal was never wired), so a play "ingested" without
            // touching state and trivially did not reject. Now the conductor RESOLVES (HeadlessConductor
            // VfxMgr runs the InstantVfx; the deal seats the hand). This test feeds the first captured
            // `send PlayActions` WITHOUT first replaying the capture's Deal/mulligan, so the played card
            // is not in the seated hand and the now-live resolution correctly rejects
            // (RemoveSpellCardFromHand: not found). Replaying the capture's Deal first does NOT fix it:
            // the seated deck order can't reproduce the capture's post-mulligan idx references (the
            // documented capture-replay draw-misalignment artifact — see memory
            // project_battle_headless_conductor: "validate via node-native battles"). The valid headless
            // play oracle is now HeadlessConductorTests.Vanilla_play_resolves_on_engine_state_headless.
            Assert.Ignore("Superseded by node-native HeadlessConductorTests (M-HC-0b). Capture-replay " +
                "draw-misalignment makes a captured play unresolvable against a node-seated deck; the " +
                "node-native harness is the post-M-HC-0b oracle. Revive if capture-replay alignment lands.");

            var cl1 = CaptureReplay.Load("battle_test_cl1.ndjson");
            var deck = CaptureReplay.SelfDeckFrom(cl1);
            // Load ALL deck ids in ONE call: HeadlessCardMaster.Load replaces the static CardMaster each
            // call, so a per-id loop would leave only the last card resolvable.
            HeadlessCardMaster.Load(deck.Select(x => (int)x).Distinct().ToArray());

            var engine = new SessionBattleEngine();
            engine.Setup(CaptureReplay.SeedFrom(cl1), seatADeck: deck, seatBDeck: deck);

            var firstPlay = cl1.First(f => f.Direction == "send" && f.Uri == "PlayActions");
            var result = engine.Receive(firstPlay.Env, isPlayerSeat: true);

            Assert.That(result.RejectReason, Is.Null, $"ingest threw/rejected: {result.RejectReason}");
            Assert.That(result.Accepted, Is.True);
        }
    }
}
