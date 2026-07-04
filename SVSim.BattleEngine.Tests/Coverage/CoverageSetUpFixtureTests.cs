using NUnit.Framework;
using SVSim.BattleEngine.Tests;

namespace SVSim.BattleEngine.Tests.Coverage;

[TestFixture]
public class CoverageSetUpFixtureTests
{
    [Test]
    public void DumpPath_IsObjCoverageBattleEngineLiveTxt()
    {
        Assert.That(
            CoverageSetUpFixture.DumpPath,
            Does.EndWith("obj/coverage/SVSim.BattleEngine.Tests.live.txt")
                .Or.EndWith(@"obj\coverage\SVSim.BattleEngine.Tests.live.txt"));
    }

    [Test]
    public void IsEnabled_ReflectsEnvVar()
    {
        var prior = System.Environment.GetEnvironmentVariable("SVSIM_COVERAGE");
        try
        {
            System.Environment.SetEnvironmentVariable("SVSIM_COVERAGE", "1");
            Assert.That(CoverageSetUpFixture.IsEnabled, Is.True);
            System.Environment.SetEnvironmentVariable("SVSIM_COVERAGE", null);
            Assert.That(CoverageSetUpFixture.IsEnabled, Is.False);
        }
        finally
        {
            System.Environment.SetEnvironmentVariable("SVSIM_COVERAGE", prior);
        }
    }
}
