using System;
using System.IO;
using NUnit.Framework;
using SVSim.BattleEngine.Tests.Coverage;

namespace SVSim.BattleEngine.Tests;

/// <summary>Assembly-level NUnit fixture. When SVSIM_COVERAGE=1, installs
/// CoverageRecorder before any test runs and dumps the recorded FQNs after
/// the assembly finishes. Outside SVSIM_COVERAGE=1, this fixture is a no-op
/// — production test runs are unaffected.</summary>
[SetUpFixture]
public class CoverageSetUpFixture
{
    public static bool IsEnabled =>
        Environment.GetEnvironmentVariable("SVSIM_COVERAGE") == "1";

    public static string DumpPath =>
        Path.Combine(TestContext.CurrentContext.TestDirectory,
                     "..", "..", "..", "obj", "coverage",
                     "SVSim.BattleEngine.Tests.live.txt");

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        if (!IsEnabled) return;
        CoverageRecorder.Install();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (!IsEnabled) return;
        CoverageRecorder.Dump(DumpPath);
        TestContext.Progress.WriteLine($"[coverage] dumped {CoverageRecorder.HitCount} FQNs to {DumpPath}");
    }
}
