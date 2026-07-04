extern alias engine;
using System.IO;
using System.Linq;
using NUnit.Framework;
using SVSim.BattleEngine.Tests.Coverage;

namespace SVSim.BattleEngine.Tests.Coverage;

[TestFixture, NonParallelizable]
public class CoverageRecorderTests
{
    [SetUp] public void SetUp() => CoverageRecorder.Reset();

    [Test]
    public void Install_RecordsAtLeastOneHit_AfterCallingAnEngineType()
    {
        CoverageRecorder.Install();
        // Phase-5 chunk 47: was `BattleAmbient.Current` (deleted). Call any engine-type method
        // with a body so the Harmony prefix records a hit; Certification.ViewerId's getter is a
        // small non-inlined engine method that still ships (façade retained; consumers all read
        // per-mgr now).
        var _ = engine::Cute.Certification.ViewerId;
        Assert.That(CoverageRecorder.HitCount, Is.GreaterThan(0));
    }

    [Test]
    public void Dump_WritesSortedFqns_OneLineEach()
    {
        CoverageRecorder.Install();
        // Phase-5 chunk 47: was `BattleAmbient.Current` (deleted). Call any engine-type method
        // with a body so the Harmony prefix records a hit; Certification.ViewerId's getter is a
        // small non-inlined engine method that still ships (façade retained; consumers all read
        // per-mgr now).
        var _ = engine::Cute.Certification.ViewerId;
        var path = Path.Combine(Path.GetTempPath(), "coverage-recorder-test.txt");
        CoverageRecorder.Dump(path);
        var lines = File.ReadAllLines(path).ToList();
        Assert.That(lines, Is.Not.Empty);
        Assert.That(lines, Is.Ordered);
        Assert.That(lines.All(l => l.Contains(".")), Is.True);
    }
}
