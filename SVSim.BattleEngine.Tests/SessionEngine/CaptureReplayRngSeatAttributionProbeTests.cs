using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Sessions.Engine;

namespace SVSim.BattleEngine.Tests.SessionEngine
{
    /// <summary>
    /// PHASE 4 — OPTION-A VIABILITY PROBE (TEST-ONLY; no production fix; no Engine/*.cs edits).
    ///
    /// QUESTION: can a per-seat RNG router in the headless engine reliably attribute each StableRandom roll
    /// to the correct seat — so two seats can draw from two independent same-seeded sub-streams (mirroring
    /// two real clients, each with its OWN _stableRandom)?
    ///
    /// METHOD: replay battle_test_fresh_cl1/cl2 through a <see cref="SessionBattleEngine"/> whose mgr RNG is
    /// a logging source. On EVERY roll it records (a) the seat signals the mgr can read from its own state
    /// (GetBattlePlayer(true/false).IsSelfTurn — the richest seat signal a mgr-level StableRandom override
    /// sees; there is NO "current operating seat" field on the mgr), and (b) the live call stack (where the
    /// acting seat is actually visible: MulliganCtrl._battlePlayer / BattlePlayerBase.LotteryRandomDrawCard /
    /// OperateReceive.StartOperate spin pre-roll). We dump the rolls for the mulligan lotteries, the first
    /// turn draws, and the spin pre-roll, and classify each — reporting whether the seat is UNAMBIGUOUS from
    /// mgr STATE vs only from the STACK.
    /// </summary>
    [TestFixture]
    [NonParallelizable]
    public class CaptureReplayRngSeatAttributionProbeTests
    {
        private const int SeatASeed = 1430655717; // cl1 Ready idxChangeSeed
        private const int SeatBSeed = 661650374;  // cl2 Ready idxChangeSeed
        private const int WireSpin = 243;

        private static readonly HashSet<string> SkipUris = new()
        {
            nameof(NetworkBattleUri.Echo),
            nameof(NetworkBattleUri.ChatStamp),
            nameof(NetworkBattleUri.Gungnir),
        };

        private static CapturedFrame Receive(IReadOnlyList<CapturedFrame> frames, string uri) =>
            frames.First(f => f.Direction == "receive" && f.Uri == uri);

        private static IEnumerable<(CapturedFrame Frame, bool Seat)> SendsInTsOrder(
            IReadOnlyList<CapturedFrame> cl1, IReadOnlyList<CapturedFrame> cl2) =>
            cl1.Where(f => f.Direction == "send").Select(f => (f, Seat: true))
               .Concat(cl2.Where(f => f.Direction == "send").Select(f => (f, Seat: false)))
               .OrderBy(x => x.f.Ts)
               .Select(x => (x.f, x.Seat));

        [Test]
        public void Roll_log_reveals_whether_acting_seat_is_attributable_from_state_or_only_stack()
        {
            var cl1 = CaptureReplay.Load("battle_test_fresh_cl1.ndjson");
            var cl2 = CaptureReplay.Load("battle_test_fresh_cl2.ndjson");

            var deckA = CaptureReplay.SelfDeckFrom(cl1);
            var deckB = CaptureReplay.SelfDeckFrom(cl2);

            // (5) seeds
            int seedA = CaptureReplay.SeedFrom(cl1);
            int seedB = CaptureReplay.SeedFrom(cl2);
            TestContext.WriteLine($"=== SEEDS (Matched.selfInfo.seed) ===");
            TestContext.WriteLine($"  cl1 seed = {seedA}");
            TestContext.WriteLine($"  cl2 seed = {seedB}");
            TestContext.WriteLine($"  SAME? {seedA == seedB}  (Ready idxChangeSeed cl1={SeatASeed} cl2={SeatBSeed} — DIFFERENT)");
            TestContext.WriteLine("");

            var engine = new SessionBattleEngine();
            var log = engine.DebugSetupWithRollLog(masterSeed: seedA, seatADeck: deckA, seatBDeck: deckB);
            Assert.That(engine.IsReady, Is.True);

            engine.DebugSeedIdxChange(SeatASeed, SeatBSeed);
            engine.DebugSetRandomDraw(true); // the gate that makes draws actually ROLL

            // mark roll-log boundaries so we can bucket the rolls by phase
            int Mark() => log.Count;

            int beforeDeal = Mark();
            engine.Receive(Receive(cl1, nameof(NetworkBattleUri.Deal)).Env, isPlayerSeat: true);
            int afterDeal = Mark();

            // seat A mulligan (Swap+Ready) then seat B mulligan
            engine.Receive(Receive(cl1, nameof(NetworkBattleUri.Swap)).Env, isPlayerSeat: true);
            int afterSwapA = Mark();
            engine.Receive(Receive(cl1, nameof(NetworkBattleUri.Ready)).Env, isPlayerSeat: true);
            int afterReadyA = Mark();
            engine.Receive(Receive(cl2, nameof(NetworkBattleUri.Swap)).Env, isPlayerSeat: false);
            int afterSwapB = Mark();
            engine.Receive(Receive(cl2, nameof(NetworkBattleUri.Ready)).Env, isPlayerSeat: false);
            int afterReadyB = Mark();

            // spin pre-roll (one client's 243 advance, applied once on the shared stream)
            engine.DebugSpinPreroll(WireSpin);
            int afterSpin = Mark();

            // replay both clients' interleaved sends (the plays + turn ops -> turn-start draws fire here)
            var sends = SendsInTsOrder(cl1, cl2).Where(x => !SkipUris.Contains(x.Frame.Uri)).ToList();
            foreach (var x in sends)
                engine.Receive(x.Frame.Env, isPlayerSeat: x.Seat);
            int afterSends = Mark();

            TestContext.WriteLine("=== ROLL-COUNT BY PHASE (IsRandomDraw=true) ===");
            TestContext.WriteLine($"  Deal              : {afterDeal - beforeDeal}");
            TestContext.WriteLine($"  Swap A            : {afterSwapA - afterDeal}");
            TestContext.WriteLine($"  Ready A (mulligan): {afterReadyA - afterSwapA}");
            TestContext.WriteLine($"  Swap B            : {afterSwapB - afterReadyA}");
            TestContext.WriteLine($"  Ready B (mulligan): {afterReadyB - afterSwapB}");
            TestContext.WriteLine($"  spin pre-roll     : {afterSpin - afterReadyB}  (expected {WireSpin})");
            TestContext.WriteLine($"  all sends/plays   : {afterSends - afterSpin}");
            TestContext.WriteLine($"  TOTAL             : {log.Count}");
            TestContext.WriteLine("");

            DumpRange("DEAL", log, beforeDeal, afterDeal);
            DumpRange("SWAP A (mulligan lottery, seat A)", log, afterDeal, afterSwapA);
            DumpRange("READY A (mulligan, seat A)", log, afterSwapA, afterReadyA);
            DumpRange("SWAP B (mulligan lottery, seat B)", log, afterReadyA, afterSwapB);
            DumpRange("READY B (mulligan, seat B)", log, afterSwapB, afterReadyB);
            DumpSpinSummary("SPIN PRE-ROLL", log, afterReadyB, afterSpin);
            // first ~12 of the play phase covers the early turn-start draws for both seats
            DumpRange("FIRST PLAY-PHASE ROLLS (turn draws + effects)", log, afterSpin,
                System.Math.Min(afterSpin + 12, afterSends));

            // === STATE-vs-STACK attribution analysis ===
            AnalyzeAttribution(log, afterSpin);

            Assert.Pass($"probe complete: {log.Count} rolls logged; see TestContext output for attribution analysis");
        }

        private static void DumpRange(string label, IReadOnlyList<SessionBattleEngine.RollEntry> log, int from, int to)
        {
            TestContext.WriteLine($"--- {label}  [rolls {from}..{to - 1}] ({to - from} rolls) ---");
            for (int i = from; i < to; i++)
            {
                var e = log[i];
                TestContext.WriteLine($"  #{e.Index} {e.Api}(arg={e.Arg}) | mgrState: self.IsSelfTurn={e.SelfIsSelfTurn} oppo.IsSelfTurn={e.OppoIsSelfTurn} | classify={Classify(e)}");
                TestContext.WriteLine($"        stack: {e.Stack}");
            }
            if (to - from == 0) TestContext.WriteLine("  (none)");
            TestContext.WriteLine("");
        }

        private static void DumpSpinSummary(string label, IReadOnlyList<SessionBattleEngine.RollEntry> log, int from, int to)
        {
            TestContext.WriteLine($"--- {label}  [rolls {from}..{to - 1}] ({to - from} rolls) ---");
            if (to - from > 0)
            {
                var first = log[from];
                TestContext.WriteLine($"  first spin roll #{first.Index}: self.IsSelfTurn={first.SelfIsSelfTurn} oppo.IsSelfTurn={first.OppoIsSelfTurn}");
                TestContext.WriteLine($"        stack: {first.Stack}");
                bool allStateIdentical = log.Skip(from).Take(to - from)
                    .All(e => e.SelfIsSelfTurn == first.SelfIsSelfTurn && e.OppoIsSelfTurn == first.OppoIsSelfTurn);
                bool allViaStartOperate = log.Skip(from).Take(to - from).All(e => e.Stack.Contains("StartOperate"));
                TestContext.WriteLine($"  all {to - from} spin rolls have identical mgr seat-state? {allStateIdentical}");
                TestContext.WriteLine($"  all {to - from} spin rolls routed via OperateReceive.StartOperate? {allViaStartOperate}");
            }
            TestContext.WriteLine("");
        }

        // Best-effort classification from the STACK (the ground truth of who is rolling).
        private static string Classify(SessionBattleEngine.RollEntry e)
        {
            string s = e.Stack;
            if (s.Contains("StartOperate")) return "SPIN-PREROLL";
            if (s.Contains("_LotMulliganCardIndex") || s.Contains("MulliganCtrl")) return "MULLIGAN-LOTTERY";
            if (s.Contains("LotteryRandomDrawCard") || s.Contains("RandomCardDraw")) return "TURN/EFFECT-DRAW";
            if (s.Contains("SkillRandomSelectFilter")) return "SKILL-FILTER-DRAW";
            return "OTHER-EFFECT";
        }

        private static void AnalyzeAttribution(IReadOnlyList<SessionBattleEngine.RollEntry> log, int playPhaseStart)
        {
            TestContext.WriteLine("=== STATE-vs-STACK ATTRIBUTION ANALYSIS ===");

            // 1) Does mgr-state (IsSelfTurn flags) ever change across the whole replay? If both flags are
            //    pinned at setup values (self=true/oppo=false) the entire time, mgr-state CANNOT distinguish
            //    seats — every roll looks identical from mgr state.
            var distinctStates = log
                .Select(e => (e.SelfIsSelfTurn, e.OppoIsSelfTurn))
                .Distinct()
                .ToList();
            TestContext.WriteLine($"  distinct mgr seat-states observed across ALL {log.Count} rolls: {distinctStates.Count}");
            foreach (var st in distinctStates)
                TestContext.WriteLine($"    (self.IsSelfTurn={st.Item1}, oppo.IsSelfTurn={st.Item2})");

            // 2) For the mulligan lotteries: seat A's 6 rolls then seat B's 6 rolls happen back-to-back. Are
            //    their mgr-states distinguishable? (They should NOT be — IsSelfTurn isn't toggled during
            //    mulligan; both lotteries run with the same setup-time flags.)
            var mulliganRolls = log.Where(e => Classify(e) == "MULLIGAN-LOTTERY").ToList();
            var mulliganStates = mulliganRolls.Select(e => (e.SelfIsSelfTurn, e.OppoIsSelfTurn)).Distinct().Count();
            TestContext.WriteLine($"  mulligan-lottery rolls: {mulliganRolls.Count}; distinct mgr seat-states among them: {mulliganStates}");
            TestContext.WriteLine($"    -> seat attributable from mgr STATE alone? {(mulliganStates >= 2 ? "MAYBE" : "NO (state identical for both seats' lotteries)")}");
            bool mulliganSeatInStack = mulliganRolls.All(e => e.Stack.Contains("Mulligan") || e.Stack.Contains("_LotMulligan"));
            TestContext.WriteLine($"    -> mulligan rolls carry a MulliganCtrl frame on the stack? {mulliganSeatInStack}");

            // 3) For the play-phase draws: are turn-start draws present at all, and do their mgr-states track
            //    the acting seat (i.e. does IsSelfTurn flip to identify whose turn/draw it is)?
            var drawRolls = log.Skip(playPhaseStart)
                .Where(e => Classify(e) is "TURN/EFFECT-DRAW" or "SKILL-FILTER-DRAW")
                .ToList();
            TestContext.WriteLine($"  play-phase draw/filter rolls: {drawRolls.Count}");
            if (drawRolls.Count > 0)
            {
                var drawStates = drawRolls.Select(e => (e.SelfIsSelfTurn, e.OppoIsSelfTurn)).Distinct().Count();
                TestContext.WriteLine($"    distinct mgr seat-states among draw rolls: {drawStates}");
            }

            TestContext.WriteLine("");
            TestContext.WriteLine("  INTERPRETATION:");
            TestContext.WriteLine("   * If distinct mgr seat-states == 1 for a phase, the StableRandom override CANNOT");
            TestContext.WriteLine("     attribute that phase's rolls to a seat from mgr state — only the call STACK");
            TestContext.WriteLine("     (MulliganCtrl._battlePlayer / BattlePlayerBase 'this' / OperateReceive._isPlayer)");
            TestContext.WriteLine("     names the acting seat.");
        }
    }
}
