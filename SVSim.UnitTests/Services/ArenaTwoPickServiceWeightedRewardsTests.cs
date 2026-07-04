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

public class ArenaTwoPickServiceWeightedRewardsTests
{
    private const long TicketItemId = 80001;

    private sealed class FakePool : IArenaTwoPickCardPoolService
    {
        public List<CandidatePair> GeneratePickSetsForTurn(int classId, int turn, long startingPairId, IRandom rng) => new();
        public List<CandidatePair> GeneratePickSetsForTurn(int classId, int turn, long startingPairId, IRandom rng, IReadOnlyList<int>? poolCardSetIds) => new();
    }

    /// <summary>
    /// A fake IRandom that returns a fixed value from Next(). Used to control weighted picks.
    /// </summary>
    private sealed class FakeRandom : IRandom
    {
        private readonly Queue<int> _ints;
        public FakeRandom(params int[] ints) { _ints = new Queue<int>(ints); }
        public double NextDouble() => 0.0;
        public int Next(int maxExclusive) => _ints.Count > 0 ? _ints.Dequeue() : 0;
    }

    /// <summary>
    /// Sets up a fresh in-memory DB with a viewer (id=7, 50 rupies, 5 tickets) and a run at
    /// the given winCount/lossCount. Does NOT seed catalog rows — callers add their own.
    /// </summary>
    private static async Task<(SVSimDbContext db, ArenaTwoPickService svc, long viewerId)>
        SetupAsync(int winCount, int lossCount, IRandom rng)
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

        var classEntry = await db.Classes.FirstOrDefaultAsync(c => c.Id == 1);
        if (classEntry is null)
        {
            classEntry = new ClassEntry { Id = 1, Name = "Class1" };
            db.Classes.Add(classEntry);
        }
        viewer.Classes.Add(new ViewerClassData { Class = classEntry, Level = 1, Exp = 0 });
        db.Viewers.Add(viewer);
        await db.SaveChangesAsync();

        var runs = new ArenaTwoPickRunRepository(db);
        var pickList = Enumerable.Range(0, 30).Select(i => (long)(100000 + i)).ToList();
        await runs.UpsertAsync(new ViewerArenaTwoPickRun
        {
            ViewerId = 7, EntryId = 4242,
            CandidateClassIdsJson = "[1,7,8]",
            ClassId = 1, LeaderSkinId = 1, MaxBattleCount = 5,
            SelectTurn = 15, IsSelectCompleted = true,
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
            rng,
            db);

        return (db, svc, 7L);
    }

    [Test]
    public async Task WeightedPicker_picks_high_weight_row_when_rng_lands_in_its_range()
    {
        // Two rows in the same group: weight=1 (Rupy 100) and weight=9 (Rupy 999).
        // Total weight = 10. Roll = 5 → falls in the second row's bucket [1, 10).
        // The service also uses rng.Next for MaxBattleCount resolution (GetMaxWinCountAsync
        // returns rows count which is 2 for a single WinCount = 3, but MaxBattles = MAX(WinCount)
        // which comes from the DB, not rng). The FakeRandom need only provide the weighted-pick roll.
        var rng = new FakeRandom(5);
        var (db, svc, vid) = await SetupAsync(winCount: 3, lossCount: 2, rng);
        await using var _ = db;

        // Seed catalog: two rows, same group, same WinCount=3.
        db.ArenaTwoPickRewards.Add(new ArenaTwoPickReward
        {
            WinCount = 3, RewardGroup = 1, Weight = 1, RewardType = (UserGoodsType)9, RewardId = 0, RewardNum = 100
        });
        db.ArenaTwoPickRewards.Add(new ArenaTwoPickReward
        {
            WinCount = 3, RewardGroup = 1, Weight = 9, RewardType = (UserGoodsType)9, RewardId = 0, RewardNum = 999
        });
        await db.SaveChangesAsync();

        var dto = await svc.RetireAsync(vid);

        Assert.That(dto.Rewards.Count, Is.EqualTo(1));
        Assert.That(dto.Rewards[0].RewardCount, Is.EqualTo(999),
            "roll=5 should land in the weight-9 row's bucket [1,10)");
    }

    [Test]
    public async Task Weight_zero_rows_are_never_picked()
    {
        // One weight=0 row (should never be picked) and one weight=1 row.
        var (db, svc, vid) = await SetupAsync(winCount: 2, lossCount: 3, new SystemRandom(seed: 42));
        await using var _ = db;

        db.ArenaTwoPickRewards.Add(new ArenaTwoPickReward
        {
            WinCount = 2, RewardGroup = 1, Weight = 0, RewardType = (UserGoodsType)9, RewardId = 0, RewardNum = 9999
        });
        db.ArenaTwoPickRewards.Add(new ArenaTwoPickReward
        {
            WinCount = 2, RewardGroup = 1, Weight = 1, RewardType = (UserGoodsType)9, RewardId = 0, RewardNum = 500
        });
        await db.SaveChangesAsync();

        var dto = await svc.RetireAsync(vid);

        Assert.That(dto.Rewards.Count, Is.EqualTo(1));
        Assert.That(dto.Rewards[0].RewardCount, Is.EqualTo(500),
            "weight=0 row must never be picked");
    }

    [Test]
    public async Task RewardNum_zero_pick_emits_no_delta_and_no_grant()
    {
        // A single group whose only pickable row has RewardNum=0 → "nothing" outcome.
        var (db, svc, vid) = await SetupAsync(winCount: 1, lossCount: 4, new SystemRandom(seed: 1));
        await using var _ = db;

        db.ArenaTwoPickRewards.Add(new ArenaTwoPickReward
        {
            WinCount = 1, RewardGroup = 1, Weight = 1, RewardType = (UserGoodsType)9, RewardId = 0, RewardNum = 0
        });
        await db.SaveChangesAsync();

        var viewerBefore = await db.Viewers
            .Include(v => v.Currency)
            .FirstAsync(v => v.Id == vid);
        var rupiesBefore = viewerBefore.Currency!.Rupees;

        var dto = await svc.RetireAsync(vid);

        Assert.That(dto.Rewards.Count, Is.EqualTo(0), "RewardNum=0 row must not emit a delta");
        Assert.That(dto.RewardList.Count, Is.EqualTo(0), "RewardNum=0 row must not emit a post-state entry");

        var viewerAfter = await db.Viewers.Include(v => v.Currency).FirstAsync(v => v.Id == vid);
        Assert.That(viewerAfter.Currency!.Rupees, Is.EqualTo(rupiesBefore),
            "viewer balance must be unchanged when all picks are RewardNum=0");
    }

    [Test]
    public async Task Empty_WinCount_emits_empty_rewards_without_throwing()
    {
        // WinCount=99 has no rows in the catalog. Should return empty arrays cleanly.
        var (db, svc, vid) = await SetupAsync(winCount: 99, lossCount: 0, new SystemRandom(seed: 1));
        await using var _ = db;

        // Seed at least one row for a different WinCount so GetMaxWinCountAsync returns >0
        // (otherwise the service falls back to default MaxBattleCount=5, but battlesPlayed=99
        // still satisfies the >=5 check so Retire works regardless).
        db.ArenaTwoPickRewards.Add(new ArenaTwoPickReward
        {
            WinCount = 0, RewardGroup = 1, Weight = 1, RewardType = (UserGoodsType)9, RewardId = 0, RewardNum = 50
        });
        await db.SaveChangesAsync();

        Assert.DoesNotThrowAsync(async () =>
        {
            var dto = await svc.RetireAsync(vid);
            Assert.That(dto.Rewards.Count, Is.EqualTo(0));
            Assert.That(dto.RewardList.Count, Is.EqualTo(0));
        });
    }
}
