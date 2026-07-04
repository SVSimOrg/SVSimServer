using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using NUnit.Framework;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Sessions.Engine;

namespace SVSim.BattleEngine.Tests.SessionEngine
{
    /// <summary>
    /// PHASE 4 STEP 1 — Tier 2 capture-replay root-cause VERIFICATION (NOT a fix).
    ///
    /// Replays the FRESH smoke captures (battle 907324319325) — battle_test_fresh_cl1/cl2.ndjson —
    /// through a <see cref="SessionBattleEngine"/>, then measures whether the per-seat <c>idxChangeSeed</c>
    /// the real Ready frame carries is what controls the "Target card was not found in hand cards"
    /// divergence symptom.
    ///
    /// FAITHFUL SETUP (the live ShadowIngest only feeds client SENDS, which contain NO Deal/Ready, so a
    /// bare send-only replay can't even seat a hand — that conflates "missing Deal" with "missing
    /// reshuffle"). To ISOLATE the reshuffle/seed effect we seat each seat's hand from its OWN client's
    /// RECEIVE Deal + Swap + Ready (the frames that establish the hand and reach mulligan-end), then replay
    /// both clients' interleaved SENDS (the plays). The Ready frame natively carries the real per-seat
    /// idxChangeSeed (cl1=1430655717, cl2=661650374), and the engine's recovery receiver calls
    /// <c>CreateXorShift</c> from it (NetworkBattleReceiver.cs:1125-1126). The A/B is then:
    ///   • WITH-SEED: ingest the Ready frame verbatim (idxChangeSeed present) -> XorShift active;
    ///   • SEED-STRIPPED: ingest the SAME Ready frame with idxChangeSeed forced to -1 -> XorShift inactive
    ///     (this is exactly the live shadow's effective state, since it never ingests the seed-bearing Ready).
    /// The ONLY difference between the two runs is whether the seed reaches CreateXorShift.
    ///
    /// DECK SETUP MECHANISM (feasibility crux, RESOLVED): each side's deck is reconstructed from the
    /// capture's <c>Matched.selfDeck</c> (idx-&gt;cardId, the exact shuffled order the node also handed the
    /// client) via <see cref="CaptureReplay.SelfDeckFrom"/>; the master seed from <c>Matched.selfInfo.seed</c>.
    /// The deck IS in the socket capture — no external fixture needed.
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public class CaptureReplayReshuffleRootCauseTests
    {
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

        private sealed record ReplayOutcome(
            int FrameCount, List<string> Divergences, bool AllDivergencesPostMulligan, bool SelfXorShiftActive);

        // Re-parse a captured frame, overriding the Ready body's idxChangeSeed (and oppoIdxChangeSeed if
        // present). Used to STRIP the seed (-1) to model the live shadow's seed-less state.
        private static MsgEnvelope OverrideReadySeed(CapturedFrame f, int newSeed)
        {
            var obj = JsonNode.Parse(f.RawBody)!.AsObject();
            obj["idxChangeSeed"] = newSeed;
            if (obj.ContainsKey("oppoIdxChangeSeed")) obj["oppoIdxChangeSeed"] = newSeed;
            return MsgEnvelope.FromJson(obj.ToJsonString());
        }

        /// <summary>Seat both hands from each client's receive Deal+Swap+Ready, then replay both clients'
        /// interleaved SENDS. <paramref name="stripSeed"/> forces the Ready idxChangeSeed to -1 (the live
        /// shadow's effective state). Returns divergences + the post-setup self XorShift state.</summary>
        private static ReplayOutcome Replay(bool stripSeed)
        {
            var cl1 = CaptureReplay.Load("battle_test_fresh_cl1.ndjson");
            var cl2 = CaptureReplay.Load("battle_test_fresh_cl2.ndjson");

            var deckA = CaptureReplay.SelfDeckFrom(cl1);
            var deckB = CaptureReplay.SelfDeckFrom(cl2);
            Assert.That(deckA, Is.Not.Empty, "cl1 Matched.selfDeck must reconstruct seat A's deck");
            Assert.That(deckB, Is.Not.Empty, "cl2 Matched.selfDeck must reconstruct seat B's deck");

            var engine = new SessionBattleEngine();
            engine.Setup(masterSeed: CaptureReplay.SeedFrom(cl1), seatADeck: deckA, seatBDeck: deckB);
            Assert.That(engine.IsReady, Is.True, "engine must seat from the captured decks + seed");

            var divergences = new List<string>();
            bool sawMulliganEnd = false;
            bool anyDivergencePreMulligan = false;

            void Ingest(MsgEnvelope env, bool seat, string uri)
            {
                if (MulliganUris.Contains(uri)) sawMulliganEnd = true;
                var r = engine.Receive(env, isPlayerSeat: seat);
                if (r.Diverged)
                {
                    divergences.Add($"seat={(seat ? "A" : "B")} {uri}: {Trim(r.RejectReason)}");
                    if (!sawMulliganEnd) anyDivergencePreMulligan = true;
                }
            }

            // --- Phase 1: seat both hands from the receive setup frames ----------------------------------
            // A single Deal seats BOTH opening hands (cl1's receive Deal carries self=A + oppo=B), so we
            // ingest Deal ONCE (as seat A) — ingesting both clients' Deals would double-deal (NRE / "Sequence
            // contains more than one"). Each seat's Swap then applies that seat's mulligan, and each seat's
            // Ready carries THAT seat's idxChangeSeed (cl1's for A, cl2's for B; the recovery receiver consumes
            // only the SELF seed per ingest, NetworkBattleReceiver.cs:1126), reaching mulligan-end per seat.
            CapturedFrame Receive(IReadOnlyList<CapturedFrame> frames, string uri) =>
                frames.First(f => f.Direction == "receive" && f.Uri == uri);

            // Deal once (seat A's receive Deal seats both hands).
            Ingest(Receive(cl1, nameof(NetworkBattleUri.Deal)).Env, seat: true, nameof(NetworkBattleUri.Deal));
            // Each seat's mulligan swap, then each seat's Ready (its own seed).
            foreach (var (frames, seat) in new[] { (cl1, true), (cl2, false) })
            {
                Ingest(Receive(frames, nameof(NetworkBattleUri.Swap)).Env, seat, nameof(NetworkBattleUri.Swap));
                var ready = Receive(frames, nameof(NetworkBattleUri.Ready));
                var readyEnv = stripSeed ? OverrideReadySeed(ready, -1) : ready.Env;
                Ingest(readyEnv, seat, nameof(NetworkBattleUri.Ready));
            }

            bool selfActive = engine.SelfXorShiftActive;

            // --- Phase 2: replay both clients' interleaved SENDS (the plays / turn ops) -------------------
            var sends = CaptureReplay.InterleavedSends(cl1, cl2)
                .Where(x => !SkipUris.Contains(x.Env.Uri.ToString()))
                .ToList();
            foreach (var (env, seat) in sends)
                Ingest(env, seat, env.Uri.ToString());

            return new ReplayOutcome(
                FrameCount: sends.Count, divergences, !anyDivergencePreMulligan, selfActive);
        }

        private static string Trim(string? s) =>
            (s ?? "").Split(" @ ")[0];

        [Test]
        public void Capture_replay_reproduces_post_mulligan_divergence_and_pins_what_the_seed_does_not_fix()
        {
            var withSeed = Replay(stripSeed: false);
            var stripped = Replay(stripSeed: true);

            TestContext.WriteLine($"WITH-SEED (Ready idxChangeSeed present): selfXorShiftActive={withSeed.SelfXorShiftActive} " +
                $"playFrames={withSeed.FrameCount} divergences={withSeed.Divergences.Count}");
            foreach (var d in withSeed.Divergences) TestContext.WriteLine("  DIVERGE " + d);
            TestContext.WriteLine($"SEED-STRIPPED (idxChangeSeed=-1, the live shadow state): selfXorShiftActive={stripped.SelfXorShiftActive} " +
                $"playFrames={stripped.FrameCount} divergences={stripped.Divergences.Count}");
            foreach (var d in stripped.Divergences) TestContext.WriteLine("  DIVERGE " + d);

            Assert.Multiple(() =>
            {
                // (1) The reported symptom reproduces DETERMINISTICALLY from the captures: the replay diverges,
                //     including the verbatim "Target card was not found in hand cards" exception.
                Assert.That(withSeed.Divergences, Is.Not.Empty,
                    "the capture replay must reproduce the divergence symptom");
                Assert.That(withSeed.Divergences.Any(d => d.Contains("not found in hand")), Is.True,
                    "the reported 'Target card was not found in hand cards' symptom must reproduce");

                // (2) All divergences occur AFTER the mulligan barrier — consistent with a post-mulligan cause.
                Assert.That(withSeed.AllDivergencesPostMulligan, Is.True, "with-seed divergences are post-mulligan");
                Assert.That(stripped.AllDivergencesPostMulligan, Is.True, "stripped divergences are post-mulligan");

                // (3) The wire seed DOES drive the engine's XorShift gate (NetworkBattleReceiver.cs:1126):
                //     present -> active, stripped (the live shadow's state) -> inactive.
                Assert.That(withSeed.SelfXorShiftActive, Is.True,
                    "ingesting the real Ready (idxChangeSeed present) activates the engine's XorShift");
                Assert.That(stripped.SelfXorShiftActive, Is.False,
                    "stripping idxChangeSeed (the live shadow's state) leaves the XorShift inactive");

                // (4) THE KEY VERIFICATION FINDING — activating the XorShift via the wire seed does NOT, on its
                //     own, change the divergence count. The engine's recovery/watch RECEIVE path never performs
                //     the post-mulligan full-deck reshuffle the live client does: the XorShift's GetChangeInt is
                //     consumed ONLY by AddToDeckCardIndexChange (BattlePlayerBase.cs:3079) for cards added to the
                //     deck AFTER mulligan-end, and the per-turn draw is engine-computed off the (un-reshuffled)
                //     deck order, not driven by the wire's `move idx`. So "feed the seed" alone does NOT fix the
                //     desync headless — the eventual fix must also make the engine reshuffle the deck post-
                //     mulligan to match the client (or drive the draw from the wire idx). We PIN this here.
                Assert.That(stripped.Divergences.Count, Is.EqualTo(withSeed.Divergences.Count),
                    "VERIFIED: activating the XorShift via the wire seed alone does NOT change the divergence " +
                    "count — the engine's receive path does not reshuffle the deck, so the seed is necessary " +
                    "but NOT sufficient (the fix needs the reshuffle too, not just the seed)");
            });
        }
    }
}
