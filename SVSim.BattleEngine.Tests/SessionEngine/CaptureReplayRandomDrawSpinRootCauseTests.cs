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
    /// PHASE 4 — DRAW-RECOMPUTE ROOT-CAUSE VALIDATION (TEST-ONLY; no production fix; no Engine/*.cs edits).
    ///
    /// HYPOTHESIS (from the experiment brief): the shadow diverges ("Target card was not found in hand
    /// cards", post-mulligan) because the per-turn network DRAW is a SEEDED-RANDOM pick from the deck via
    /// <c>mgr.StableRandom(...)</c> (SkillRandomSelectFilter.Filtering:49/58), gated by the process-global
    /// <c>BattleManagerBase.IsRandomDraw</c> — which the real match-load sets true via
    /// <c>StartOpening → SetupInitialGameState(areCardsRandomlyDrawn:true)</c> (BattleManagerBase.cs:1098/1110).
    /// The headless <see cref="SessionBattleEngine"/>.Setup never runs SetupInitialGameState, so IsRandomDraw
    /// stays FALSE and the shadow draws TOP-OF-DECK while the clients draw seeded-random → mismatch.
    /// AND the shared <c>_stableRandom</c> stream must be advanced by the wire <c>spin</c> pre-roll the Ready
    /// frame carries (spin=243), which <c>OperateReceive.StartOperate:80-83</c> applies but the shadow never
    /// ingests — so without it the stream is offset.
    ///
    /// ISOLATION MATRIX (this is the report's headline): setup frames + real seed are held CONSTANT (the
    /// faithful baseline the prior FullInput experiment pinned at 14); the two NEW variables are toggled:
    ///   • {IsRandomDraw=false, no spin}  = baseline (top-of-deck draws; the live shadow's effective state)
    ///   • {IsRandomDraw=true,  no spin}  = random-draw active but stream MIS-aligned (expect WORSE)
    ///   • {IsRandomDraw=true,  +spin}    = random-draw active AND stream aligned (the hypothesised fix)
    ///
    /// SPIN APPLICATION: spin=243 appears on the Ready frame in BOTH captures (each client applies its own
    /// once). Our shadow shares ONE <c>_stableRandom</c> across both seats (seated as both players), and a
    /// single client's stream sits 243 draws in after ITS Ready — so we apply spin=243 ONCE, after the
    /// Deal/Swap/Ready setup frames and before the plays, exactly where the real client's StartOperate would.
    /// (A scan of both fixtures confirms Ready is the ONLY frame carrying a non-zero spin.)
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public class CaptureReplayRandomDrawSpinRootCauseTests
    {
        private const int SeatASeed = 1430655717; // cl1 / seat A / player  (Ready idxChangeSeed)
        private const int SeatBSeed = 661650374;  // cl2 / seat B / opponent
        private const int WireSpin = 243;         // both captures' Ready frame spin

        private static readonly HashSet<string> SkipUris = new()
        {
            nameof(NetworkBattleUri.Echo),
            nameof(NetworkBattleUri.ChatStamp),
            nameof(NetworkBattleUri.Gungnir),
        };

        private sealed record HandDump(string Seat, int PlayIdx, string Uri, string Reason,
            int StableRandomCount,
            IReadOnlyList<(int Index, int CardId)> SelfHand,
            IReadOnlyList<(int Index, int CardId)> OppoHand,
            bool PlayIdxInSelfHand, bool PlayIdxInOppoHand);

        private sealed record Cell(bool RandomDraw, bool Spin, int Divergences, HandDump? FirstNotFound);

        private static int ReadPlayIdx(string rawBody)
        {
            using var doc = JsonDocument.Parse(rawBody);
            return doc.RootElement.TryGetProperty("playIdx", out var p) && p.TryGetInt32(out var v) ? v : -1;
        }

        private static List<(int, int)> HandSnapshot(SessionBattleEngine engine, bool seat)
        {
            var list = new List<(int, int)>();
            int n = engine.HandCount(seat);
            for (int i = 0; i < n; i++)
                list.Add((engine.HandCardIndex(seat, i), engine.HandCardId(seat, i)));
            return list;
        }

        private static CapturedFrame Receive(IReadOnlyList<CapturedFrame> frames, string uri) =>
            frames.First(f => f.Direction == "receive" && f.Uri == uri);

        private static Cell Run(bool randomDraw, bool spin)
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

            // CONSTANT across all cells: faithful seed seam (both seats' XorShift active), sidestepping the
            // seat-B Ready NRE — identical to the FullInput experiment's full-inputs cell.
            engine.DebugSeedIdxChange(SeatASeed, SeatBSeed);

            // NEW VARIABLE 1: the IsRandomDraw gate. Set BEFORE any draw (deal is the first draw).
            engine.DebugSetRandomDraw(randomDraw);

            int divergences = 0;
            HandDump? firstNotFound = null;

            void Ingest(MsgEnvelope env, bool seat, string uri, string rawBody)
            {
                var r = engine.Receive(env, isPlayerSeat: seat);
                if (!r.Diverged) return;
                divergences++;
                if (firstNotFound is null && (r.RejectReason ?? "").Contains("not found in hand"))
                {
                    var self = HandSnapshot(engine, seat);
                    var oppo = HandSnapshot(engine, !seat);
                    int playIdx = ReadPlayIdx(rawBody);
                    firstNotFound = new HandDump(
                        seat ? "A" : "B", playIdx, uri, Trim(r.RejectReason),
                        engine.DebugStableRandomCount, self, oppo,
                        self.Any(h => h.Item1 == playIdx), oppo.Any(h => h.Item1 == playIdx));
                }
            }

            // --- Phase 1: setup frames (CONSTANT: Deal once + each seat's Swap + Ready) -------------------
            var deal = Receive(cl1, nameof(NetworkBattleUri.Deal));
            Ingest(deal.Env, seat: true, nameof(NetworkBattleUri.Deal), deal.RawBody);
            foreach (var (frames, seat) in new[] { (cl1, true), (cl2, false) })
            {
                var swap = Receive(frames, nameof(NetworkBattleUri.Swap));
                Ingest(swap.Env, seat, nameof(NetworkBattleUri.Swap), swap.RawBody);
                var ready = Receive(frames, nameof(NetworkBattleUri.Ready));
                Ingest(ready.Env, seat, nameof(NetworkBattleUri.Ready), ready.RawBody);
            }

            // NEW VARIABLE 2: the spin pre-roll, applied at mulligan-end (after Ready, before the first
            // turn-start draw) — where OperateReceive.StartOperate applies the Ready's spin in production.
            // ONE application of 243 (shared stream, one client's worth of advance).
            if (spin)
                engine.DebugSpinPreroll(WireSpin);

            // --- Phase 2: replay both clients' interleaved SENDS (the plays) ------------------------------
            var sends = SendsWithRawBody(cl1, cl2)
                .Where(x => !SkipUris.Contains(x.Frame.Uri))
                .ToList();
            foreach (var x in sends)
                Ingest(x.Frame.Env, x.Seat, x.Frame.Uri, x.Frame.RawBody);

            return new Cell(randomDraw, spin, divergences, firstNotFound);
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
        public void IsRandomDraw_plus_spin_preroll_isolation_matrix()
        {
            try
            {
                ConfirmSpin();

                var baseline = Run(randomDraw: false, spin: false);
                var rdOnly = Run(randomDraw: true, spin: false);
                var rdSpin = Run(randomDraw: true, spin: true);

                TestContext.WriteLine("=== ISOLATION MATRIX (setup-frames + real-seed held CONSTANT) ===");
                TestContext.WriteLine("IsRandomDraw | spin | divergences");
                TestContext.WriteLine($"   false     | no   |     {baseline.Divergences}");
                TestContext.WriteLine($"   true      | no   |     {rdOnly.Divergences}");
                TestContext.WriteLine($"   true      | +243 |     {rdSpin.Divergences}");

                DumpFirst("baseline {false,no}", baseline);
                DumpFirst("rd-only  {true,no}", rdOnly);
                DumpFirst("rd+spin  {true,+243}", rdSpin);

                Assert.Pass(
                    $"MATRIX baseline={baseline.Divergences} rdOnly={rdOnly.Divergences} rdSpin={rdSpin.Divergences}");
            }
            catch (SuccessException) { throw; }
            catch (Exception ex)
            {
                TestContext.WriteLine("EXPERIMENT THREW: " + ex);
                throw;
            }
        }

        private static void DumpFirst(string label, Cell c)
        {
            if (c.FirstNotFound is not { } d)
            {
                TestContext.WriteLine($"[{label}] no 'not found in hand' divergence.");
                return;
            }
            TestContext.WriteLine($"[{label}] FIRST 'not found in hand': seat={d.Seat} uri={d.Uri} " +
                $"wire playIdx={d.PlayIdx} stableRandomCount={d.StableRandomCount} reason={d.Reason}");
            TestContext.WriteLine($"   playIdx in self hand? {d.PlayIdxInSelfHand}  in oppo hand? {d.PlayIdxInOppoHand}");
            TestContext.WriteLine($"   SELF (seat {d.Seat}) hand [{d.SelfHand.Count}]: " +
                string.Join(" ", d.SelfHand.Select(h => $"(idx={h.Index},cid={h.CardId})")));
            TestContext.WriteLine($"   OPPO hand [{d.OppoHand.Count}]: " +
                string.Join(" ", d.OppoHand.Select(h => $"(idx={h.Index},cid={h.CardId})")));
        }

        /// <summary>STEP 4 (payoff check): with the hypothesised fix applied {IsRandomDraw=true, +spin},
        /// does the engine reach and RESOLVE cl1's spellboost play so PlayedCardSpellboost/PlayedCardCost
        /// return real (non-zero) values? cl1's deck carries the spellboost-scaling follower 101314020 at
        /// deck idx 10/21/25. We replay the {true,+243} cell and, after each accepted seat-A PlayActions,
        /// probe whether any in-play/cemetery card has that id with a resolved cost/spellboost. We report
        /// whether the spellboost play was ever reached at all.</summary>
        [Test]
        public void Spellboost_play_resolution_under_random_draw_plus_spin()
        {
            const int SpellboostCardId = 101314020;

            var cl1 = CaptureReplay.Load("battle_test_fresh_cl1.ndjson");
            var cl2 = CaptureReplay.Load("battle_test_fresh_cl2.ndjson");
            var deckA = CaptureReplay.SelfDeckFrom(cl1);
            var deckB = CaptureReplay.SelfDeckFrom(cl2);

            var engine = new SessionBattleEngine();
            engine.Setup(masterSeed: CaptureReplay.SeedFrom(cl1), seatADeck: deckA, seatBDeck: deckB);
            engine.DebugSeedIdxChange(SeatASeed, SeatBSeed);
            engine.DebugSetRandomDraw(true);

            // setup frames
            engine.Receive(Receive(cl1, nameof(NetworkBattleUri.Deal)).Env, isPlayerSeat: true);
            foreach (var (frames, seat) in new[] { (cl1, true), (cl2, false) })
            {
                engine.Receive(Receive(frames, nameof(NetworkBattleUri.Swap)).Env, isPlayerSeat: seat);
                engine.Receive(Receive(frames, nameof(NetworkBattleUri.Ready)).Env, isPlayerSeat: seat);
            }
            engine.DebugSpinPreroll(WireSpin);

            int acceptedSeatAPlays = 0, divergedBeforeFirstPlay = 0;
            bool spellboostResolved = false;
            int sbCost = -999, sbCharge = -999;

            var sends = SendsWithRawBody(cl1, cl2).Where(x => !SkipUris.Contains(x.Frame.Uri)).ToList();
            bool sawFirstPlay = false;
            foreach (var x in sends)
            {
                bool isPlay = x.Frame.Uri == nameof(NetworkBattleUri.PlayActions);
                var r = engine.Receive(x.Frame.Env, isPlayerSeat: x.Seat);
                if (isPlay && !sawFirstPlay) { sawFirstPlay = true; if (r.Diverged) divergedBeforeFirstPlay++; }
                if (isPlay && x.Seat && !r.Diverged)
                {
                    acceptedSeatAPlays++;
                    int playIdx = ReadPlayIdx(x.Frame.RawBody);
                    long id = engine.PlayedCardId(true, playIdx, 0);
                    if (id == SpellboostCardId)
                    {
                        spellboostResolved = true;
                        sbCost = engine.PlayedCardCost(true, playIdx, -1);
                        sbCharge = engine.PlayedCardSpellboost(true, playIdx, -1);
                        break;
                    }
                }
            }

            TestContext.WriteLine($"[spellboost payoff] acceptedSeatAPlays={acceptedSeatAPlays} " +
                $"divergedAtFirstPlay={divergedBeforeFirstPlay} spellboostResolved={spellboostResolved} " +
                $"cost={sbCost} charge={sbCharge}");

            // The replay diverges at the FIRST seat-A play (matrix shows playIdx=8 not in hand), so the
            // engine never advances to the later spellboost play — the visible spellboost symptom is NOT
            // fixed by {IsRandomDraw+spin} because the prerequisite (aligned draws) is not met.
            Assert.That(divergedBeforeFirstPlay, Is.EqualTo(1),
                "the FIRST seat-A play already diverges under {IsRandomDraw=true,+spin}");
            Assert.That(spellboostResolved, Is.False,
                "the spellboost play is never reached because the replay diverges at the first play");
        }

        private static void ConfirmSpin()
        {
            foreach (var fn in new[] { "battle_test_fresh_cl1.ndjson", "battle_test_fresh_cl2.ndjson" })
            {
                var frames = CaptureReplay.Load(fn);
                var ready = Receive(frames, nameof(NetworkBattleUri.Ready));
                var obj = JsonNode.Parse(ready.RawBody)!.AsObject();
                int spin = obj.TryGetPropertyValue("spin", out var s) ? (int)s! : 0;
                TestContext.WriteLine($"Confirmed {fn} Ready spin={spin}");
                Assert.That(spin, Is.EqualTo(WireSpin), $"{fn} Ready spin must equal {WireSpin}");
            }
        }
    }
}
