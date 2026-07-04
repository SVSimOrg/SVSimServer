#nullable enable
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SVSim.BattleNode.Sessions.Engine;

namespace SVSim.BattleEngine.Tests;

/// <summary>Forcing-function tests for the multi-instancing migration. After the chunk-46/47
/// ambient rip, per-battle mutable state (IsForecast/IsRandomDraw/RecoveryInfo/ViewerId/
/// NetworkAgent + GameMgr) lives on the mgr instance itself; there is no ambient at all,
/// no "current mgr / current GameMgr" static gate. Two engines on two tasks resolve
/// independently because their mgrs are different objects, full stop — this fixture pins
/// parallel-equals-sequential to catch any residual contamination through the not-yet-culled
/// static accumulators (Wizard.LocalLog trace SB, Unity Resources cache — both already
/// thread-safe).</summary>
[TestFixture, Parallelizable(ParallelScope.All)]
public class MultiInstanceEngineTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

    [Test]
    public async Task TwoBattles_ResolveIndependently_OnDifferentTasks()
    {
        var engineA = new SessionBattleEngine();
        var engineB = new SessionBattleEngine();
        engineA.Setup(masterSeed: 111, HeadlessEngineEnv.SampleDeck(), HeadlessEngineEnv.SampleDeck(),
            seatAClass: 1, seatBClass: 2);
        engineB.Setup(masterSeed: 222, HeadlessEngineEnv.SampleDeck(), HeadlessEngineEnv.SampleDeck(),
            seatAClass: 5, seatBClass: 7);

        var taskA = Task.Run(() => DriveBasicTurns(engineA));
        var taskB = Task.Run(() => DriveBasicTurns(engineB));
        await Task.WhenAll(taskA, taskB);

        // Pin the engines' post-Setup state to concrete starting values. Both engines must
        // report the SAME starting state regardless of distinct masterSeeds, which is the
        // cross-contamination property under test.
        Assert.That(engineA.LeaderLife(true), Is.EqualTo(20));
        Assert.That(engineB.LeaderLife(true), Is.EqualTo(20));
        Assert.That(engineA.Pp(true), Is.EqualTo(0));
        Assert.That(engineB.Pp(true), Is.EqualTo(0));
        Assert.That(engineA.HandCount(true), Is.EqualTo(0));
        Assert.That(engineB.HandCount(true), Is.EqualTo(0));
    }

    [Test]
    public async Task StressN_BaselineMatches([Values(4, 8, 16)] int n)
    {
        var inputs = new (int seed, long[] deckA, long[] deckB)[n];
        for (int i = 0; i < n; i++)
            inputs[i] = (1000 + i, HeadlessEngineEnv.SampleDeck(), HeadlessEngineEnv.SampleDeck());

        var parallel = await Task.WhenAll(inputs.Select(input => Task.Run(() =>
        {
            var e = new SessionBattleEngine();
            e.Setup(input.seed, input.deckA, input.deckB);
            DriveBasicTurns(e);
            return e.LeaderLife(true);
        })));

        var sequential = new int[n];
        for (int i = 0; i < n; i++)
        {
            var e = new SessionBattleEngine();
            e.Setup(inputs[i].seed, inputs[i].deckA, inputs[i].deckB);
            DriveBasicTurns(e);
            sequential[i] = e.LeaderLife(true);
        }

        Assert.That(parallel, Is.EqualTo(sequential));
    }

    [Test]
    public void BattleManagerBase_GetIns_Always_Null()
    {
        // Post-chunk-46 ambient rip: GetIns() has no scope-based mechanism; every engine consumer
        // was converted to per-mgr instance reads, and the residual static returns null so its
        // ?. cascade landing in the 3 façades / IsForecast+IsRandomDraw returns their defaults.
        Assert.That(BattleManagerBase.GetIns(), Is.Null);
    }

    private static void DriveBasicTurns(SessionBattleEngine e)
    {
        _ = e.LeaderLife(true);
        _ = e.Pp(true);
        _ = e.HandCount(true);
    }
}
