using System.Linq;
using NUnit.Framework;

namespace SVSim.BattleEngine.Tests.SessionEngine
{
    [TestFixture]
    public class CaptureReplayTests
    {
        [Test]
        public void Load_parses_frames_and_extracts_self_deck()
        {
            var frames = CaptureReplay.Load("battle_test_cl1.ndjson");
            Assert.That(frames, Is.Not.Empty);

            var deck = CaptureReplay.SelfDeckFrom(frames);
            Assert.That(deck, Is.Not.Empty, "Matched.selfDeck should parse");
            Assert.That(deck.Count, Is.EqualTo(40), "a standard deck is 40 cards");

            // Send PlayActions carry their URI at the top level (body.uri == None); the helper must
            // resolve it correctly, not drop it to None.
            Assert.That(frames.Any(f => f.Direction == "send" && f.Uri == "PlayActions"),
                Is.True, "send PlayActions URI resolved from top level");

            Assert.That(CaptureReplay.SeedFrom(frames), Is.GreaterThan(0), "Matched.selfInfo.seed parsed");
        }
    }
}
