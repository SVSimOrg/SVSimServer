#nullable enable
using NUnit.Framework;
using SVSim.BattleNode.Sessions.Engine;

namespace SVSim.BattleEngine.Tests;

/// <summary>Confirms that constructing engine B after engine A has finished does not inherit
/// any leftover ambient/static state from engine A. Catches the static-not-cleared family of
/// bugs that the multi-instance Task.Run tests don't exercise (their scopes never overlap
/// AsyncLocal flow). Backstop for engine shim cleanup Task 5.</summary>
[TestFixture, NonParallelizable]
public class SequentialBattleTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp() => HeadlessEngineEnv.EnsureProcessGlobals();

    [Test]
    public void Sequential_SecondBattle_StartsCleanAfterFirstCompletes()
    {
        var engineA = new SessionBattleEngine();
        engineA.Setup(masterSeed: 333, HeadlessEngineEnv.SampleDeck(), HeadlessEngineEnv.SampleDeck(),
            seatAClass: 1, seatBClass: 2);
        _ = engineA.LeaderLife(true);
        _ = engineA.Pp(true);
        _ = engineA.HandCount(true);
        // Drop engineA. No explicit Dispose today; we rely on scope-exit semantics in SessionBattleEngine.

        // Now construct engine B in the same AppDomain.
        var engineB = new SessionBattleEngine();
        engineB.Setup(masterSeed: 444, HeadlessEngineEnv.SampleDeck(), HeadlessEngineEnv.SampleDeck(),
            seatAClass: 3, seatBClass: 4);

        Assert.That(engineB.LeaderLife(true), Is.EqualTo(20),
            "engineB starting life leaked from engineA — sequential static-state bug.");
        Assert.That(engineB.Pp(true), Is.EqualTo(0));
        Assert.That(engineB.HandCount(true), Is.EqualTo(0));

        // Re-read engineA after engineB exists, to confirm engineB's setup didn't poison engineA's reads.
        Assert.That(engineA.LeaderLife(true), Is.EqualTo(20),
            "engineA reads changed after engineB.Setup — cross-contamination bug.");
    }

    [Test]
    public void Sequential_ThirdBattle_AfterTwoCompletes_StillClean()
    {
        for (int i = 0; i < 3; i++)
        {
            var e = new SessionBattleEngine();
            e.Setup(masterSeed: 500 + i, HeadlessEngineEnv.SampleDeck(), HeadlessEngineEnv.SampleDeck());
            Assert.That(e.LeaderLife(true), Is.EqualTo(20));
            Assert.That(e.Pp(true), Is.EqualTo(0));
        }
    }
}
