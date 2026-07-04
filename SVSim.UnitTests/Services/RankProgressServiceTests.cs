using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Globals;
using SVSim.Database.Services.RankProgress;

namespace SVSim.UnitTests.Services;

public class RankProgressServiceTests
{
    // Values mirror SVSim.Bootstrap/Data/ranks.csv verbatim: RankId, NecessaryPoint,
    // AccumulatePoint, LowerLimitPoint, AccumulateMasterPoint.
    private static readonly (int Id, int Necessary, int Accumulate, int Lower, int AccMp)[] Ranks =
    {
        (1,  100,   100,     0,     0), (2,  100,   200,     0,     0),
        (3,  500,   700,     0,     0), (4,  500,  1200,     0,     0),
        (5,  750,  1950,  1200,     0), (6,  750,  2700,  1200,     0),
        (7,  800,  3500,  1200,     0), (8, 1000,  4500,  1200,     0),
        (9, 1500,  6000,  4500,     0), (10,1500,  7500,  4500,     0),
        (11,1750,  9250,  4500,     0), (12,1750, 11000,  4500,     0),
        (13,2000, 13000, 11000,     0), (14,2000, 15000, 11000,     0),
        (15,2000, 17000, 11000,     0), (16,2000, 19000, 11000,     0),
        (17,3000, 23000, 20000,     0), (18,3000, 26000, 20000,     0),
        (19,3000, 29000, 20000,     0), (20,3000, 32000, 20000,     0),
        (21,4000, 37000, 33000,     0), (22,4000, 42000, 33000,     0),
        (23,4000, 46000, 33000,     0), (24,4000, 50000, 33000,     0),
        (25,   0,     0, 50000,  5000),
        (26,   0,     0, 50000, 15000),
        (27,   0,     0, 50000, 25000),
        (28,   0,     0, 50000, 35000),
        (29,   0,     0, 50000,     0),
    };

    private static IGlobalsRepository RankRepo()
    {
        var mock = new Mock<IGlobalsRepository>(MockBehavior.Loose);
        mock.Setup(x => x.GetRankInfo()).ReturnsAsync(
            Ranks.Select(r => new RankInfoEntry
            {
                Id = r.Id,
                NecessaryPoint = r.Necessary,
                AccumulatePoint = r.Accumulate,
                LowerLimitPoint = r.Lower,
                AccumulateMasterPoint = r.AccMp,
            }).ToList());
        return mock.Object;
    }

    private static Viewer NewViewer(Format format, int point, int masterPoint)
    {
        var v = new Viewer { Id = 1, DisplayName = "v" };
        v.RankProgress.Add(new ViewerRankProgress
        {
            Format = format, Point = point, MasterPoint = masterPoint,
        });
        return v;
    }

    private static RankProgressService NewService() =>
        new(RankRepo(), NullLogger<RankProgressService>.Instance);

    [Test]
    public async Task First_win_from_scratch_awards_100_and_promotes_to_Beginner1()
    {
        var svc = NewService();
        var v = new Viewer { Id = 1, DisplayName = "v" };

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: true);

        Assert.That(r.Rank, Is.EqualTo(2));
        Assert.That(r.AfterBattlePoint, Is.EqualTo(100));
        Assert.That(r.BattlePoint, Is.EqualTo(100));
        Assert.That(r.AfterMasterPoint, Is.EqualTo(0));
        Assert.That(r.IsMasterRank, Is.False);
        Assert.That(r.IsGrandMasterRank, Is.False);
        var row = v.RankProgress.Single();
        Assert.That(row.Format, Is.EqualTo(Format.Rotation));
        Assert.That(row.Point, Is.EqualTo(100));
    }

    [Test]
    public async Task Loss_at_0_points_floors_at_0()
    {
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 0, masterPoint: 0);

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: false);

        Assert.That(r.Rank, Is.EqualTo(1));
        Assert.That(r.AfterBattlePoint, Is.EqualTo(0));
        Assert.That(r.BattlePoint, Is.EqualTo(0));
    }

    [Test]
    public async Task Loss_within_beginner_can_drop_between_subranks()
    {
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 100, masterPoint: 0);

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: false);

        Assert.That(r.Rank, Is.EqualTo(1));
        Assert.That(r.AfterBattlePoint, Is.EqualTo(50));
        Assert.That(r.BattlePoint, Is.EqualTo(-50));
    }

    [Test]
    public async Task Loss_at_D_tier_floor_stays_at_D()
    {
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 1200, masterPoint: 0);

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: false);

        Assert.That(r.Rank, Is.EqualTo(5));
        Assert.That(r.AfterBattlePoint, Is.EqualTo(1200));
        Assert.That(r.BattlePoint, Is.EqualTo(0));
    }

    [Test]
    public async Task Loss_within_D_drops_from_D1_toward_D0_and_stops_at_floor()
    {
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 1230, masterPoint: 0);

        var r1 = await svc.GrantAsync(v, Format.Rotation, isWin: false);
        Assert.That(r1.AfterBattlePoint, Is.EqualTo(1200));
        Assert.That(r1.BattlePoint, Is.EqualTo(-30));
        Assert.That(r1.Rank, Is.EqualTo(5));

        var r2 = await svc.GrantAsync(v, Format.Rotation, isWin: false);
        Assert.That(r2.AfterBattlePoint, Is.EqualTo(1200));
        Assert.That(r2.BattlePoint, Is.EqualTo(0));
    }

    [Test]
    public async Task Win_crossing_50000_lands_at_Master_with_no_MP_change()
    {
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 49950, masterPoint: 0);

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: true);

        Assert.That(r.Rank, Is.EqualTo(25));
        Assert.That(r.AfterBattlePoint, Is.EqualTo(50050));
        Assert.That(r.BattlePoint, Is.EqualTo(100));
        Assert.That(r.AfterMasterPoint, Is.EqualTo(0));
        Assert.That(r.MasterPoint, Is.EqualTo(0));
        Assert.That(r.IsMasterRank, Is.True);
    }

    [Test]
    public async Task Win_at_Master_awards_MasterPoint_not_Point()
    {
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 50000, masterPoint: 0);

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: true);

        Assert.That(r.BattlePoint, Is.EqualTo(0));
        Assert.That(r.MasterPoint, Is.EqualTo(100));
        Assert.That(r.AfterMasterPoint, Is.EqualTo(100));
        Assert.That(r.Rank, Is.EqualTo(25));
    }

    [Test]
    public async Task MP_crossing_5000_promotes_to_GrandMaster_0()
    {
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 50000, masterPoint: 4900);

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: true);

        Assert.That(r.AfterMasterPoint, Is.EqualTo(5000));
        Assert.That(r.Rank, Is.EqualTo(26));
        Assert.That(r.IsGrandMasterRank, Is.True);
        Assert.That(r.IsMasterRank, Is.False);
    }

    [Test]
    public async Task Loss_at_GM0_floor_stays_at_5000_and_does_not_demote_to_Master()
    {
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 50000, masterPoint: 5000);

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: false);

        Assert.That(r.AfterMasterPoint, Is.EqualTo(5000));
        Assert.That(r.MasterPoint, Is.EqualTo(0));
        Assert.That(r.Rank, Is.EqualTo(26));
    }

    [Test]
    public async Task Loss_at_Master_with_MP_zero_leaves_Point_at_50000()
    {
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 50000, masterPoint: 0);

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: false);

        Assert.That(r.AfterBattlePoint, Is.EqualTo(50000));
        Assert.That(r.BattlePoint, Is.EqualTo(0));
        Assert.That(r.Rank, Is.EqualTo(25));
    }

    [Test]
    public async Task Rotation_and_Unlimited_progressions_are_separate()
    {
        var svc = NewService();
        var v = new Viewer { Id = 1, DisplayName = "v" };
        v.RankProgress.Add(new ViewerRankProgress { Format = Format.Rotation, Point = 500 });

        var r = await svc.GrantAsync(v, Format.Unlimited, isWin: true);

        Assert.That(r.AfterBattlePoint, Is.EqualTo(100));
        Assert.That(v.RankProgress.Count, Is.EqualTo(2));
        Assert.That(v.RankProgress.Single(x => x.Format == Format.Rotation).Point, Is.EqualTo(500));
        Assert.That(v.RankProgress.Single(x => x.Format == Format.Unlimited).Point, Is.EqualTo(100));
    }

    [Test]
    public void Crossover_format_throws()
    {
        var svc = NewService();
        var v = new Viewer { Id = 1, DisplayName = "v" };

        Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            await svc.GrantAsync(v, Format.Crossover, isWin: true));
    }

    [Test]
    public async Task GetAsync_returns_zero_when_no_row_exists()
    {
        var svc = NewService();
        var v = new Viewer { Id = 1, DisplayName = "v" };

        var r = await svc.GetAsync(v, Format.Rotation);

        Assert.That(r.Rank, Is.EqualTo(1));
        Assert.That(r.AfterBattlePoint, Is.EqualTo(0));
        Assert.That(r.AfterMasterPoint, Is.EqualTo(0));
        Assert.That(v.RankProgress, Is.Empty);
    }

    // ---- TierAdvanced ----

    [Test]
    public async Task TierAdvanced_is_false_when_win_stays_within_same_tier()
    {
        // Point 100 (Beginner 1, rank 2) + 100 win = 200 (Beginner 3, rank 4). Still beginner.
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 100, masterPoint: 0);

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: true);

        Assert.That(RankTier.Name(r.Rank), Is.EqualTo("beginner"));
        Assert.That(r.TierAdvanced, Is.False);
    }

    [Test]
    public async Task TierAdvanced_is_true_when_win_crosses_beginner_to_d()
    {
        // Point 1150 (Beginner 3, rank 4 — needs 1200 to leave beginner) + 100 = 1250 = D0 (rank 5).
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 1150, masterPoint: 0);

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: true);

        Assert.That(r.Rank, Is.EqualTo(5), "Expected promotion to rank 5 (D0)");
        Assert.That(r.TierAdvanced, Is.True);
    }

    [Test]
    public async Task TierAdvanced_is_false_on_loss_even_if_tier_string_changes()
    {
        // Loss can't advance a tier — even if it demoted, the achievement doesn't fire backward.
        // Point 1200 (D0, rank 5) - 50 loss = 1200 (floored). Still D0. Prove flag stays false.
        var svc = NewService();
        var v = NewViewer(Format.Rotation, point: 1200, masterPoint: 0);

        var r = await svc.GrantAsync(v, Format.Rotation, isWin: false);

        Assert.That(r.TierAdvanced, Is.False);
    }
}
