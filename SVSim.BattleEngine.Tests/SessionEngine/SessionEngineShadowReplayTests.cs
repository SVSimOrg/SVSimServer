using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Sessions.Engine;

namespace SVSim.BattleEngine.Tests.SessionEngine
{
    [TestFixture]
    public class SessionEngineShadowReplayTests
    {

        [SetUp] public void SetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

        // Frames that are transport/keepalive, not game actions — not ingested.
        private static readonly HashSet<string> SkipUris = new()
        {
            nameof(NetworkBattleUri.Echo),
            nameof(NetworkBattleUri.ChatStamp),
            nameof(NetworkBattleUri.Gungnir),
        };

        [Test]
        public void Shadow_replay_of_captured_battle_tracks_state_without_desync()
        {
            // SUPERSEDED by the node-native oracle (SVSim.UnitTests HeadlessConductorTests). This test's
            // "0 rejects" used to pass VACUOUSLY: before the M-HC-0b view-untangle the receive conductor
            // resolved NOTHING headless (InstantVfx mutations no-op'd; OnReceiveDeal unwired), so no
            // captured frame could diverge because none was applied. The retracted "shadow tracks the
            // capture" claim is documented in memory project_battle_node_engine_shadow / _headless_conductor.
            // Now that the conductor RESOLVES, replaying a captured stream against a node-seated deck hits
            // the documented capture-replay draw-misalignment: the seated deck order can't reproduce the
            // capture's post-mulligan idx references, so played cards aren't in the seated hand
            // (HandCardToField/RemoveSpellCardFromHand: not found). The decision (memory
            // project_battle_headless_conductor) is to validate headless resolution via NODE-NATIVE
            // battles, not capture replay. The node-native oracle now covers Deal+Play.
            Assert.Ignore("Superseded by node-native HeadlessConductorTests (M-HC-0b). Capture-replay " +
                "against a node-seated deck hits the documented draw-misalignment artifact once the " +
                "receive path actually resolves. Revive if a capture-replay alignment path lands.");

            var cl1 = CaptureReplay.Load("battle_test_cl1.ndjson");
            var cl2 = CaptureReplay.Load("battle_test_cl2.ndjson");
            var deckA = CaptureReplay.SelfDeckFrom(cl1);
            var deckB = CaptureReplay.SelfDeckFrom(cl2);
            // One Load call with every id — Load replaces the static master each call.
            HeadlessCardMaster.Load(deckA.Concat(deckB).Select(x => (int)x).Distinct().ToArray());

            var engine = new SessionBattleEngine();
            engine.Setup(masterSeed: CaptureReplay.SeedFrom(cl1), seatADeck: deckA, seatBDeck: deckB);

            // Single-client full-stream replay (cl1 as the player seat): cl1's SENT frames are its own
            // actions (seat=true); its RECEIVED frames are the opponent/server actions (seat=false),
            // incl. the Deal that establishes both hands. This is exactly the stream cl1's receiver
            // processed, in capture (ts) order. (The node-side both-clients-sends model is exercised
            // live in Task 7; here we validate engine tracking against ground truth.)
            var stream = cl1.Where(f => !SkipUris.Contains(f.Uri))
                            .OrderBy(f => f.Ts)
                            .ToList();

            var rejects = new List<string>();
            var violations = new List<string>();

            foreach (var f in stream)
            {
                bool seat = f.Direction == "send";
                var r = engine.Receive(f.Env, isPlayerSeat: seat);
                if (r.RejectReason is not null)
                    rejects.Add($"{f.Direction} {f.Uri}: {r.RejectReason}");

                if (f.Uri == nameof(NetworkBattleUri.TurnEnd))
                    CheckInvariants(engine, violations, atUri: f.Uri);
            }

            foreach (var line in rejects) TestContext.WriteLine("REJECT " + line);
            foreach (var line in violations) TestContext.WriteLine("VIOLATION " + line);
            TestContext.WriteLine($"frames={stream.Count} rejects={rejects.Count} violations={violations.Count}");

            Assert.Multiple(() =>
            {
                Assert.That(rejects, Is.Empty, "engine diverged / rejected a captured frame");
                Assert.That(violations, Is.Empty, "engine state left a structural invariant");
            });
        }

        private static void CheckInvariants(SessionBattleEngine engine, List<string> violations, string atUri)
        {
            foreach (var seat in new[] { true, false })
            {
                int life = engine.LeaderLife(seat), pp = engine.Pp(seat);
                int board = engine.BoardCount(seat), hand = engine.HandCount(seat);
                if (life is < 0 or > 20) violations.Add($"{atUri} seat={seat} life={life}");
                if (pp is < 0 or > 10) violations.Add($"{atUri} seat={seat} pp={pp}");
                if (board is < 0 or > 7) violations.Add($"{atUri} seat={seat} board={board}");
                if (hand is < 0 or > 9) violations.Add($"{atUri} seat={seat} hand={hand}");
            }
        }
    }
}
