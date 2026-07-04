using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Sessions.Engine;

namespace SVSim.BattleEngine.Tests.SessionEngine
{
    /// <summary>
    /// PHASE 4 — DECISIVE VERIFICATION (TEST-ONLY, no production fix, no Engine/*.cs edits).
    ///
    /// QUESTION: does feeding the headless shadow engine the FULL client inputs (server-authored
    /// Deal/Swap/Ready setup frames for BOTH seats + the real per-seat <c>idxChangeSeed</c>) make its
    /// recovery-mode draw recompute faithful, so the "Target card was not found in hand cards"
    /// divergences vanish?
    ///
    /// This builds the explicit 2x2 {setup-frames ingested: yes/no} x {real seed: yes/no} divergence
    /// table over the SAME fresh battle (907324319325, battle_test_fresh_cl1/cl2.ndjson), and — at the
    /// FIRST remaining divergence — dumps the engine's hand indices/ids vs the wire's <c>playIdx</c>.
    ///
    /// SEEDING MECHANISM (clean, both seats): the seat-B <c>Ready</c> ingest throws an NRE headless (the
    /// recovery deal path isn't headless-clean for the opponent seat), so the wire <c>Ready</c> cannot be
    /// relied on to seat seat B's XorShift. To inject the real seed FAITHFULLY for BOTH seats without
    /// depending on the throwing Ready, we call the test seam <see cref="SessionBattleEngine"/>.
    /// <c>DebugSeedIdxChange(self, oppo)</c> (-> <c>BattleManagerBase.CreateXorShift</c>) BEFORE the
    /// mulligan-end frame, with the real per-seat seeds (seat A = cl1's Ready idxChangeSeed = 1430655717,
    /// seat B = cl2's = 661650374). We ASSERT both <c>SelfXorShiftActive</c> and <c>OppoXorShiftActive</c>
    /// are true after.
    ///
    /// SETUP-FRAME INGEST: identical mechanism to <see cref="CaptureReplayReshuffleRootCauseTests"/> — a
    /// single <c>Deal</c> (cl1's receive Deal seats BOTH hands), each seat's <c>Swap</c> (its mulligan),
    /// each seat's <c>Ready</c> (mulligan-end). The {no-setup-frames} row SKIPS Deal/Swap/Ready entirely:
    /// the engine's autonomous Setup hand stands, and we replay only the plays.
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public class CaptureReplayFullInputDivergenceExperimentTests
    {
        // Real per-seat idxChangeSeed carried by each client's Ready frame (given in the experiment brief;
        // re-confirmed below against the captures).
        private const int SeatASeed = 1430655717; // cl1 / seat A / player
        private const int SeatBSeed = 661650374;  // cl2 / seat B / opponent

        private static readonly HashSet<string> SkipUris = new()
        {
            nameof(NetworkBattleUri.Echo),
            nameof(NetworkBattleUri.ChatStamp),
            nameof(NetworkBattleUri.Gungnir),
        };

        private static readonly HashSet<string> MulliganUris = new()
        {
            nameof(NetworkBattleUri.Deal),
            nameof(NetworkBattleUri.Swap),
            nameof(NetworkBattleUri.Ready),
        };

        private sealed record HandDump(string Seat, int PlayIdx, string Uri, string Reason,
            IReadOnlyList<(int Index, int CardId)> SelfHand,
            IReadOnlyList<(int Index, int CardId)> OppoHand,
            bool PlayIdxInSelfHand, bool PlayIdxInOppoHand);

        private sealed record Cell(
            bool SetupFrames, bool RealSeed,
            int Divergences, bool SelfXorActive, bool OppoXorActive,
            HandDump? FirstNotFoundDump);

        private static int ReadPlayIdx(string rawBody)
        {
            using var doc = JsonDocument.Parse(rawBody);
            return doc.RootElement.TryGetProperty("playIdx", out var p) && p.TryGetInt32(out var v) ? v : -1;
        }

        // Snapshot a seat's hand as (engine Index, CardId) pairs. Reads through the SessionBattleEngine
        // oracle accessors (HandCount/HandCardIndex/HandCardId).
        private static List<(int, int)> HandSnapshot(SessionBattleEngine engine, bool seat)
        {
            var list = new List<(int, int)>();
            int n = engine.HandCount(seat);
            for (int i = 0; i < n; i++)
                list.Add((engine.HandCardIndex(seat, i), engine.HandCardId(seat, i)));
            return list;
        }

        private static Cell Run(bool setupFrames, bool realSeed)
        {
            var cl1 = CaptureReplay.Load("battle_test_fresh_cl1.ndjson");
            var cl2 = CaptureReplay.Load("battle_test_fresh_cl2.ndjson");

            var deckA = CaptureReplay.SelfDeckFrom(cl1);
            var deckB = CaptureReplay.SelfDeckFrom(cl2);
            Assert.That(deckA, Is.Not.Empty);
            Assert.That(deckB, Is.Not.Empty);

            var engine = new SessionBattleEngine();
            engine.Setup(masterSeed: CaptureReplay.SeedFrom(cl1), seatADeck: deckA, seatBDeck: deckB);
            Assert.That(engine.IsReady, Is.True);

            // Inject the real per-seat seed BEFORE mulligan-end (Ready). Clean both-seat activation via the
            // CreateXorShift seam, sidestepping the seat-B Ready NRE.
            if (realSeed)
                engine.DebugSeedIdxChange(SeatASeed, SeatBSeed);

            int divergences = 0;
            HandDump? firstNotFound = null;

            void Ingest(MsgEnvelope env, bool seat, string uri, string rawBody)
            {
                var r = engine.Receive(env, isPlayerSeat: seat);
                if (!r.Diverged) return;
                divergences++;
                if (firstNotFound is null && (r.RejectReason ?? "").Contains("not found in hand"))
                {
                    int playIdx = ReadPlayIdx(rawBody);
                    var self = HandSnapshot(engine, seat);
                    var oppo = HandSnapshot(engine, !seat);
                    firstNotFound = new HandDump(
                        seat ? "A" : "B", playIdx, uri, Trim(r.RejectReason),
                        self, oppo,
                        self.Any(h => h.Item1 == playIdx), oppo.Any(h => h.Item1 == playIdx));
                }
            }

            CapturedFrame Receive(IReadOnlyList<CapturedFrame> frames, string uri) =>
                frames.First(f => f.Direction == "receive" && f.Uri == uri);

            // --- Phase 1: setup frames (optional) ---------------------------------------------------------
            if (setupFrames)
            {
                var deal = Receive(cl1, nameof(NetworkBattleUri.Deal));
                Ingest(deal.Env, seat: true, nameof(NetworkBattleUri.Deal), deal.RawBody);
                foreach (var (frames, seat) in new[] { (cl1, true), (cl2, false) })
                {
                    var swap = Receive(frames, nameof(NetworkBattleUri.Swap));
                    Ingest(swap.Env, seat, nameof(NetworkBattleUri.Swap), swap.RawBody);
                    var ready = Receive(frames, nameof(NetworkBattleUri.Ready));
                    Ingest(ready.Env, seat, nameof(NetworkBattleUri.Ready), ready.RawBody);
                }
            }

            bool selfActive = engine.SelfXorShiftActive;
            bool oppoActive = engine.OppoXorShiftActive;

            // Snapshot the engine's post-setup hands (after Deal/Swap/Ready) for the full-inputs cell, so the
            // report can compare the engine's mulligan-resolved hand against the wire's Swap/Ready move list.
            if (setupFrames && realSeed)
            {
                TestContext.WriteLine("  [post-setup] engine SELF (seat A) hand: " +
                    string.Join(" ", HandSnapshot(engine, true).Select(h => $"(idx={h.Item1},cid={h.Item2})")));
                TestContext.WriteLine("  [post-setup] engine OPPO (seat B) hand: " +
                    string.Join(" ", HandSnapshot(engine, false).Select(h => $"(idx={h.Item1},cid={h.Item2})")));
            }

            // --- Phase 2: replay both clients' interleaved SENDS (the plays) ------------------------------
            var sends = SendsWithRawBody(cl1, cl2)
                .Where(x => !SkipUris.Contains(x.Frame.Uri))
                .ToList();
            foreach (var x in sends)
                Ingest(x.Frame.Env, x.Seat, x.Frame.Uri, x.Frame.RawBody);

            return new Cell(setupFrames, realSeed, divergences, selfActive, oppoActive, firstNotFound);
        }

        private static IEnumerable<(CapturedFrame Frame, bool Seat)> SendsWithRawBody(
            IReadOnlyList<CapturedFrame> cl1, IReadOnlyList<CapturedFrame> cl2)
        {
            return cl1.Where(f => f.Direction == "send").Select(f => (f, Seat: true))
                .Concat(cl2.Where(f => f.Direction == "send").Select(f => (f, Seat: false)))
                .OrderBy(x => x.f.Ts)
                .Select(x => (x.f, x.Seat));
        }

        private static string Trim(string? s) => (s ?? "").Split(" @ ")[0];

        [Test]
        public void Full_input_2x2_divergence_table_and_first_remaining_divergence_dump()
        {
            // Confirm the brief's per-seat seeds match the captures' Ready frames before relying on them.
            ConfirmReadySeeds();

            var cells = new[]
            {
                Run(setupFrames: false, realSeed: false), // baseline-ish: autonomous Setup hand, seed -1
                Run(setupFrames: false, realSeed: true),
                Run(setupFrames: true,  realSeed: false),
                Run(setupFrames: true,  realSeed: true),  // FULL INPUTS
            };

            TestContext.WriteLine("=== 2x2 DIVERGENCE TABLE (setup-frames x real-seed) ===");
            TestContext.WriteLine("setupFrames | realSeed | divergences | selfXor | oppoXor");
            foreach (var c in cells)
                TestContext.WriteLine(
                    $"   {(c.SetupFrames ? "YES" : "no ")}      |   {(c.RealSeed ? "YES" : "no ")}    |     {c.Divergences,2}      |  {c.SelfXorActive,-5} |  {c.OppoXorActive,-5}");

            var full = cells.Single(c => c.SetupFrames && c.RealSeed);
            TestContext.WriteLine("");
            TestContext.WriteLine($"FULL-INPUTS cell: setupFrames=YES realSeed=YES -> divergences={full.Divergences} " +
                $"selfXorActive={full.SelfXorActive} oppoXorActive={full.OppoXorActive}");

            if (full.FirstNotFoundDump is { } d)
            {
                TestContext.WriteLine("");
                TestContext.WriteLine("=== FIRST 'not found in hand' DIVERGENCE (full-inputs cell) ===");
                TestContext.WriteLine($"  seat={d.Seat} uri={d.Uri} wire playIdx={d.PlayIdx}  reason={d.Reason}");
                TestContext.WriteLine($"  playIdx in self hand? {d.PlayIdxInSelfHand}   in oppo hand? {d.PlayIdxInOppoHand}");
                TestContext.WriteLine($"  engine SELF (seat {d.Seat}) hand [{d.SelfHand.Count}]: " +
                    string.Join(" ", d.SelfHand.Select(h => $"(idx={h.Index},cid={h.CardId})")));
                TestContext.WriteLine($"  engine OPPO hand [{d.OppoHand.Count}]: " +
                    string.Join(" ", d.OppoHand.Select(h => $"(idx={h.Index},cid={h.CardId})")));
            }
            else
            {
                TestContext.WriteLine("");
                TestContext.WriteLine("FULL-INPUTS cell produced NO 'not found in hand' divergence.");
            }

            // EVIDENCE ASSERTIONS (pin the experiment's reproducibility, not a desired fix outcome):
            Assert.Multiple(() =>
            {
                // The seed seam activates BOTH seats' XorShift in every realSeed cell.
                foreach (var c in cells.Where(c => c.RealSeed))
                {
                    Assert.That(c.SelfXorActive, Is.True,
                        $"realSeed cell (setup={c.SetupFrames}) must activate self XorShift");
                    Assert.That(c.OppoXorActive, Is.True,
                        $"realSeed cell (setup={c.SetupFrames}) must activate oppo XorShift");
                }
                // With NO seed seam AND NO setup frames (the live shadow's effective state — never
                // ingests the seed-bearing Ready), BOTH seats' XorShift stay inactive.
                var bare = cells.Single(c => !c.RealSeed && !c.SetupFrames);
                Assert.That(bare.SelfXorActive, Is.False, "no-seed/no-setup leaves self XorShift inactive");
                Assert.That(bare.OppoXorActive, Is.False, "no-seed/no-setup leaves oppo XorShift inactive");

                // With setup frames but no seam, the seat-A Ready frame's own idxChangeSeed activates the
                // SELF XorShift (seat B's Ready NREs before it can seat oppo) — so self is active, oppo isn't.
                var setupNoSeam = cells.Single(c => !c.RealSeed && c.SetupFrames);
                Assert.That(setupNoSeam.SelfXorActive, Is.True,
                    "setup-frames cell: seat-A Ready idxChangeSeed activates self XorShift");
                Assert.That(setupNoSeam.OppoXorActive, Is.False,
                    "setup-frames cell: seat-B Ready NREs before seating oppo XorShift");

                // THE DECISIVE FINDING: full inputs (setup frames + real seed, both seats' XorShift active)
                // do NOT eliminate the divergences — they stay at the 14 baseline.
                var full2 = cells.Single(c => c.SetupFrames && c.RealSeed);
                Assert.That(full2.SelfXorActive && full2.OppoXorActive, Is.True,
                    "full-inputs cell has both seats' XorShift active");
                Assert.That(full2.Divergences, Is.GreaterThan(0),
                    "REFUTED: full inputs do NOT make the recovery recompute faithful — divergences remain");
            });
        }

        // Re-confirm the brief's per-seat seeds against the captured Ready frames (fail loudly if the
        // fixtures ever drift from the assumed seeds).
        private static void ConfirmReadySeeds()
        {
            var cl1 = CaptureReplay.Load("battle_test_fresh_cl1.ndjson");
            var cl2 = CaptureReplay.Load("battle_test_fresh_cl2.ndjson");
            int a = ReadReadySeed(cl1);
            int b = ReadReadySeed(cl2);
            TestContext.WriteLine($"Confirmed Ready idxChangeSeed: cl1(seatA)={a} cl2(seatB)={b}");
            Assert.That(a, Is.EqualTo(SeatASeed), "cl1 Ready idxChangeSeed must equal the brief's seat-A seed");
            Assert.That(b, Is.EqualTo(SeatBSeed), "cl2 Ready idxChangeSeed must equal the brief's seat-B seed");
        }

        private static int ReadReadySeed(IReadOnlyList<CapturedFrame> frames)
        {
            var ready = frames.First(f => f.Direction == "receive" && f.Uri == nameof(NetworkBattleUri.Ready));
            var obj = JsonNode.Parse(ready.RawBody)!.AsObject();
            return (int)obj["idxChangeSeed"]!;
        }
    }
}
