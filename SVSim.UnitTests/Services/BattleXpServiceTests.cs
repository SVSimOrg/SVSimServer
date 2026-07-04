using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.Database.Repositories.Globals;
using SVSim.Database.Services;
using SVSim.Database.Services.BattleXp;

namespace SVSim.UnitTests.Services;

public class BattleXpServiceTests
{
    private sealed class FakeConfig : IGameConfigService
    {
        private readonly BattleXpConfig _cfg;
        public FakeConfig(BattleXpConfig cfg) { _cfg = cfg; }
        public T Get<T>() where T : class, new() => (T)(object)_cfg;
    }

    private static IGlobalsRepository CurveRepo(params (int Level, int NecessaryExp)[] rows)
    {
        var mock = new Mock<IGlobalsRepository>(MockBehavior.Strict);
        mock.Setup(x => x.GetClassExpCurve()).ReturnsAsync(
            rows.Select(r => new ClassExpEntry { Id = r.Level, NecessaryExp = r.NecessaryExp }).ToList());
        return mock.Object;
    }

    private static Viewer NewViewerWithClass(int classId, int level = 1, int exp = 0)
    {
        var cls = new ClassEntry { Id = classId, Name = $"Class{classId}" };
        return new Viewer
        {
            Id = 1,
            DisplayName = "v",
            Classes = { new ViewerClassData { Class = cls, Level = level, Exp = exp } },
        };
    }

    private static BattleXpService NewService(BattleXpConfig cfg, params (int Level, int NecessaryExp)[] curve)
    {
        var rows = curve.Length > 0
            ? curve
            : new[] { (1, 100), (2, 150), (3, 250) };
        return new BattleXpService(CurveRepo(rows), new FakeConfig(cfg), NullLogger<BattleXpService>.Instance);
    }

    [Test]
    public async Task Win_uses_XpPerWin_and_accumulates_exp()
    {
        var svc = NewService(new BattleXpConfig { XpPerWin = 40, XpPerLoss = 5 });
        var v = NewViewerWithClass(1);

        var r = await svc.GrantAsync(v, classId: 1, isWin: true, BattleXpMode.Rank);

        Assert.That(r.GetXp, Is.EqualTo(40));
        Assert.That(r.TotalXp, Is.EqualTo(40));
        Assert.That(r.Level, Is.EqualTo(1));
        Assert.That(v.Classes.Single().Exp, Is.EqualTo(40));
        Assert.That(v.Classes.Single().Level, Is.EqualTo(1));
    }

    [Test]
    public async Task Loss_uses_XpPerLoss()
    {
        var svc = NewService(new BattleXpConfig { XpPerWin = 40, XpPerLoss = 5 });
        var v = NewViewerWithClass(1);

        var r = await svc.GrantAsync(v, classId: 1, isWin: false, BattleXpMode.Rank);

        Assert.That(r.GetXp, Is.EqualTo(5));
        Assert.That(r.TotalXp, Is.EqualTo(5));
    }

    [Test]
    public async Task Per_mode_override_wins_over_global()
    {
        var svc = NewService(new BattleXpConfig
        {
            XpPerWin = 40, XpPerLoss = 5,
            PracticeXpPerWin = 200, PracticeXpPerLoss = 20,
        });

        var vw = NewViewerWithClass(1);
        var win = await svc.GrantAsync(vw, 1, isWin: true, BattleXpMode.Practice);
        Assert.That(win.GetXp, Is.EqualTo(200));

        var vl = NewViewerWithClass(1);
        var loss = await svc.GrantAsync(vl, 1, isWin: false, BattleXpMode.Practice);
        Assert.That(loss.GetXp, Is.EqualTo(20));
    }

    [Test]
    public async Task Rank_mode_falls_back_to_global_when_override_null()
    {
        var svc = NewService(new BattleXpConfig
        {
            XpPerWin = 40, XpPerLoss = 5,
            PracticeXpPerWin = 200, // Rank has no override — must fall back.
        });
        var v = NewViewerWithClass(1);

        var r = await svc.GrantAsync(v, 1, isWin: true, BattleXpMode.Rank);

        Assert.That(r.GetXp, Is.EqualTo(40));
    }

    [Test]
    public async Task Story_uses_StoryXpPerClear_when_set_ignoring_isWin()
    {
        var svc = NewService(new BattleXpConfig
        {
            XpPerWin = 40, XpPerLoss = 5,
            StoryXpPerClear = 300,
        });
        var v = NewViewerWithClass(1);

        var r = await svc.GrantAsync(v, 1, isWin: false, BattleXpMode.Story);

        Assert.That(r.GetXp, Is.EqualTo(300));
    }

    [Test]
    public async Task Story_falls_back_to_XpPerWin_when_StoryXpPerClear_null()
    {
        var svc = NewService(new BattleXpConfig { XpPerWin = 40, XpPerLoss = 5 });
        var v = NewViewerWithClass(1);

        var r = await svc.GrantAsync(v, 1, isWin: true, BattleXpMode.Story);

        Assert.That(r.GetXp, Is.EqualTo(40));
    }

    [Test]
    public async Task Single_grant_crossing_one_threshold_bumps_level_and_carries_overflow()
    {
        // Curve: L1 needs 100 to reach L2. Grant 130 → level 2, Exp 30 carry.
        var svc = NewService(new BattleXpConfig { XpPerWin = 130 });
        var v = NewViewerWithClass(1);

        var r = await svc.GrantAsync(v, 1, isWin: true, BattleXpMode.Rank);

        Assert.That(r.Level, Is.EqualTo(2));
        Assert.That(r.TotalXp, Is.EqualTo(30));
        Assert.That(v.Classes.Single().Level, Is.EqualTo(2));
        Assert.That(v.Classes.Single().Exp, Is.EqualTo(30));
    }

    [Test]
    public async Task Single_grant_crossing_multiple_thresholds_bumps_multiple_levels()
    {
        // Curve: L1=100, L2=150, L3=250.
        // Grant 500 → L1(100)→L2 (400 left) → L2(150)→L3 (250 left) → stop at maxLevel=3.
        // Final: Level=3, Exp=250.
        var svc = NewService(new BattleXpConfig { XpPerWin = 500 });
        var v = NewViewerWithClass(1);

        var r = await svc.GrantAsync(v, 1, isWin: true, BattleXpMode.Rank);

        Assert.That(r.Level, Is.EqualTo(3));
        Assert.That(r.TotalXp, Is.EqualTo(250));
    }

    [Test]
    public async Task Saturates_at_max_curve_level_with_excess_in_exp()
    {
        // Max curve level = 3. Grant enough to overshoot far.
        var svc = NewService(new BattleXpConfig { XpPerWin = 10_000 });
        var v = NewViewerWithClass(1);

        var r = await svc.GrantAsync(v, 1, isWin: true, BattleXpMode.Rank);

        Assert.That(r.Level, Is.EqualTo(3));
        // 10000 - 100 (L1→L2) - 150 (L2→L3) = 9750 sitting in Exp at L3.
        Assert.That(r.TotalXp, Is.EqualTo(9750));
    }

    [Test]
    public async Task Unknown_classId_returns_zero_and_does_not_mutate_viewer()
    {
        var svc = NewService(new BattleXpConfig { XpPerWin = 40 });
        var v = NewViewerWithClass(classId: 1);

        var r = await svc.GrantAsync(v, classId: 99, isWin: true, BattleXpMode.Rank);

        Assert.That(r.GetXp, Is.EqualTo(0));
        Assert.That(r.TotalXp, Is.EqualTo(0));
        Assert.That(r.Level, Is.EqualTo(1));
        Assert.That(r.LeveledUp, Is.False);
        Assert.That(v.Classes.Single().Exp, Is.EqualTo(0),
            "Class 1's row must not have been touched.");
    }

    // ---- LeveledUp signal ----

    [Test]
    public async Task LeveledUp_is_false_when_grant_stays_within_same_level()
    {
        // L1 requires 100 to reach L2; grant only 40.
        var svc = NewService(new BattleXpConfig { XpPerWin = 40 });
        var v = NewViewerWithClass(1);

        var r = await svc.GrantAsync(v, 1, isWin: true, BattleXpMode.Rank);

        Assert.That(r.Level, Is.EqualTo(1));
        Assert.That(r.LeveledUp, Is.False);
    }

    [Test]
    public async Task LeveledUp_is_true_when_grant_crosses_a_threshold()
    {
        // L1=100 → grant 130 → L2 with 30 carry.
        var svc = NewService(new BattleXpConfig { XpPerWin = 130 });
        var v = NewViewerWithClass(1);

        var r = await svc.GrantAsync(v, 1, isWin: true, BattleXpMode.Rank);

        Assert.That(r.Level, Is.EqualTo(2));
        Assert.That(r.LeveledUp, Is.True);
    }

    [Test]
    public async Task LeveledUp_stays_true_across_multiple_bumps_in_one_grant()
    {
        // Grant crosses L1→L2→L3 in one go.
        var svc = NewService(new BattleXpConfig { XpPerWin = 500 });
        var v = NewViewerWithClass(1);

        var r = await svc.GrantAsync(v, 1, isWin: true, BattleXpMode.Rank);

        Assert.That(r.Level, Is.EqualTo(3));
        Assert.That(r.LeveledUp, Is.True);
    }
}
