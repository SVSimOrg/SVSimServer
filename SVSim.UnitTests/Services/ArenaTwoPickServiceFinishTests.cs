using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Globals;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

public class ArenaTwoPickServiceFinishTests
{
    private const long TicketItemId = 80001;

    private sealed class FakePool : IArenaTwoPickCardPoolService
    {
        public List<CandidatePair> GeneratePickSetsForTurn(int classId, int turn, long startingPairId, IRandom rng) => new();
        public List<CandidatePair> GeneratePickSetsForTurn(int classId, int turn, long startingPairId, IRandom rng, IReadOnlyList<int>? poolCardSetIds) => new();
    }

    private static async Task<(SVSimDbContext db, IArenaTwoPickService svc, long viewerId)> SetupWithRunAsync(
        int winCount, int lossCount, bool isSelectCompleted = true)
    {
        var factory = new SVSimTestFactory();
        var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await db.Database.EnsureCreatedAsync();

        var ticketItem = new ItemEntry { Id = (int)TicketItemId, Name = "TK2 Ticket" };
        db.Items.Add(ticketItem);

        var viewer = new SVSim.Database.Models.Viewer
        {
            Id = 7, DisplayName = "v",
            Currency = new ViewerCurrency { Rupees = 50 },
        };
        viewer.Items.Add(new OwnedItemEntry { Item = ticketItem, Count = 5 });

        // Seed a ViewerClassData for class 1 so battle/finish XP path works.
        var classEntry = await db.Classes.FirstOrDefaultAsync(c => c.Id == 1);
        if (classEntry is null)
        {
            classEntry = new ClassEntry { Id = 1, Name = "Class1" };
            db.Classes.Add(classEntry);
        }
        viewer.Classes.Add(new ViewerClassData { Class = classEntry, Level = 1, Exp = 0 });
        db.Viewers.Add(viewer);
        await db.SaveChangesAsync();

        await new ArenaTwoPickRewardImporter()
            .ImportAsync(db, Path.Combine(AppContext.BaseDirectory, "Data", "seeds"));

        var runs = new ArenaTwoPickRunRepository(db);
        var pickList = Enumerable.Range(0, isSelectCompleted ? 30 : 4).Select(i => (long)(100000 + i)).ToList();
        await runs.UpsertAsync(new ViewerArenaTwoPickRun
        {
            ViewerId = 7, EntryId = 4242,
            CandidateClassIdsJson = "[1,7,8]",
            ClassId = 1, LeaderSkinId = 1, MaxBattleCount = 7,
            SelectTurn = isSelectCompleted ? 15 : 2,
            IsSelectCompleted = isSelectCompleted,
            SelectedCardIdsJson = JsonSerializer.Serialize(pickList),
            PendingPickSetsJson = "[]",
            WinCount = winCount, LossCount = lossCount,
            ResultListJson = JsonSerializer.Serialize(
                Enumerable.Repeat(true, winCount).Concat(Enumerable.Repeat(false, lossCount)).ToList()),
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });

        var svc = new ArenaTwoPickService(
            runs,
            new ArenaTwoPickRewardRepository(db),
            new FakePool(),
            scope.ServiceProvider.GetRequiredService<IGameConfigService>(),
            scope.ServiceProvider.GetRequiredService<IViewerRepository>(),
            scope.ServiceProvider.GetRequiredService<IInventoryService>(),
            scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.BattleXp.IBattleXpService>(),
            new SystemRandom(seed: 1),
            db);

        return (db, svc, 7L);
    }

    [Test]
    public async Task RetireAsync_at_3_wins_grants_1_ticket_700_rupies_and_deletes_run()
    {
        var (db, svc, vid) = await SetupWithRunAsync(winCount: 3, lossCount: 1);
        await using var _ = db;

        var dto = await svc.RetireAsync(vid);

        Assert.That(dto.Rewards.Count, Is.EqualTo(2));
        Assert.That(dto.Rewards.Single(r => r.RewardType == 4).RewardCount, Is.EqualTo(1));
        Assert.That(dto.Rewards.Single(r => r.RewardType == 9).RewardCount, Is.EqualTo(700));

        Assert.That(dto.RewardList.Single(r => r.RewardType == 4).RewardNum, Is.EqualTo(6), "5 + 1");
        Assert.That(dto.RewardList.Single(r => r.RewardType == 9).RewardNum, Is.EqualTo(750), "50 + 700");

        var rowAfter = await db.ViewerArenaTwoPickRuns.FirstOrDefaultAsync(r => r.ViewerId == vid);
        Assert.That(rowAfter, Is.Null);
    }

    [Test]
    public async Task FinishAsync_rejects_when_run_not_complete()
    {
        var (db, svc, vid) = await SetupWithRunAsync(winCount: 3, lossCount: 1);
        await using var _ = db;
        var ex = Assert.ThrowsAsync<ArenaTwoPickException>(() => svc.FinishAsync(vid));
        Assert.That(ex!.ErrorCode, Is.EqualTo("arena_two_pick_run_not_complete"));
    }

    [Test]
    public async Task FinishAsync_at_5_total_battles_with_0_wins_grants_loss_rewards_and_deletes_run()
    {
        // Classic Take Two: run ends after 5 total battles played, regardless of W/L split.
        // 0W 5L = floor-tier reward (1 ticket + 100 rupies).
        var (db, svc, vid) = await SetupWithRunAsync(winCount: 0, lossCount: 5);
        await using var _ = db;

        var outcome = await svc.FinishAsync(vid);
        Assert.That(outcome.Response.Rewards.Single(r => r.RewardType == 9).RewardCount, Is.EqualTo(100));
        Assert.That(outcome.WasFullClear, Is.False, "0W 5L is not a full clear");
        Assert.That(await db.ViewerArenaTwoPickRuns.AnyAsync(), Is.False);
    }

    [Test]
    public async Task FinishAsync_signals_full_clear_when_run_ended_with_5_wins()
    {
        // Full clear = 5W 0L (max battles = 5). Controller consumes this to fire the
        // challenge_full_clear mission event.
        var (db, svc, vid) = await SetupWithRunAsync(winCount: 5, lossCount: 0);
        await using var _ = db;

        var outcome = await svc.FinishAsync(vid);
        Assert.That(outcome.WasFullClear, Is.True);
    }

    [Test]
    public async Task RecordBattleResultAsync_win_increments_win_and_grants_xp_and_spot_points()
    {
        var (db, svc, vid) = await SetupWithRunAsync(winCount: 1, lossCount: 0);
        await using var _ = db;

        var result = await svc.RecordBattleResultAsync(vid, isWin: true);

        Assert.That(result.BattleResult, Is.EqualTo(1));
        Assert.That(result.GetClassExperience, Is.EqualTo(200),
            "Default BattleXpConfig.XpPerWin");
        Assert.That(result.AddSpotPoint, Is.EqualTo(10));
        var run = await db.ViewerArenaTwoPickRuns.FirstAsync(r => r.ViewerId == vid);
        Assert.That(run.WinCount, Is.EqualTo(2));
        Assert.That(JsonSerializer.Deserialize<List<bool>>(run.ResultListJson)!.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task RecordBattleResultAsync_loss_grants_loss_xp()
    {
        var (db, svc, vid) = await SetupWithRunAsync(winCount: 0, lossCount: 0);
        await using var _ = db;

        var result = await svc.RecordBattleResultAsync(vid, isWin: false);

        Assert.That(result.BattleResult, Is.EqualTo(0));
        Assert.That(result.GetClassExperience, Is.EqualTo(50),
            "Default BattleXpConfig.XpPerLoss");
        Assert.That(result.ClassExperience, Is.EqualTo(0),
            "Fresh viewer with Exp=0, +50 loss XP exactly meets curve[1]=50 → L2, Exp=0.");
        Assert.That(result.ClassLevel, Is.EqualTo(2));
    }

    [Test]
    public async Task RecordBattleResultAsync_increments_loss_without_terminating()
    {
        // No 2-loss cap (that's a Worlds Beyond rule). Run termination is purely battles-played
        // based and handled at Finish/Retire time, not in RecordBattleResult.
        var (db, svc, vid) = await SetupWithRunAsync(winCount: 0, lossCount: 1);
        await using var _ = db;

        await svc.RecordBattleResultAsync(vid, isWin: false);
        var run = await db.ViewerArenaTwoPickRuns.FirstAsync();
        Assert.That(run.LossCount, Is.EqualTo(2));
        // Run still alive — IsSelectCompleted only flips when the 30-card draft finishes.
        Assert.That(run.IsSelectCompleted, Is.True, "the test setup pre-sets isSelectCompleted=true");
    }
}
