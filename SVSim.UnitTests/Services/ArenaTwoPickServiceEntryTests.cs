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

public class ArenaTwoPickServiceEntryTests
{
    private const long TicketItemId = 1;

    /// <summary>Minimal stub — EntryAsync never calls pool methods.</summary>
    private sealed class NullCardPoolService : IArenaTwoPickCardPoolService
    {
        public List<CandidatePair> GeneratePickSetsForTurn(int classId, int turn, long startingPairId, IRandom rng)
            => throw new NotSupportedException("pool not used in EntryAsync");
        public List<CandidatePair> GeneratePickSetsForTurn(int classId, int turn, long startingPairId, IRandom rng, IReadOnlyList<int>? poolCardSetIds)
            => throw new NotSupportedException("pool not used in EntryAsync");
    }

    private static async Task<(SVSimDbContext db, IArenaTwoPickService svc, long viewerId)> SetupAsync(
        int ticketCount, bool freeplay = false, ulong crystals = 0, ulong rupees = 0)
    {
        var factory = new SVSimTestFactory(freeplayEnabled: freeplay);
        var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await db.Database.EnsureCreatedAsync();

        var ticketItem = new ItemEntry { Id = (int)TicketItemId, Name = "TK2 Ticket" };
        db.Items.Add(ticketItem);
        var viewer = new SVSim.Database.Models.Viewer
        {
            Id = 99, DisplayName = "X",
            Currency = new ViewerCurrency { Crystals = crystals, Rupees = rupees },
        };
        viewer.Items.Add(new OwnedItemEntry { Item = ticketItem, Count = ticketCount });
        db.Viewers.Add(viewer);
        await db.SaveChangesAsync();

        var config = scope.ServiceProvider.GetRequiredService<IGameConfigService>();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        // Seed reward catalog so GetMaxWinCountAsync returns 7.
        await new ArenaTwoPickRewardImporter()
            .ImportAsync(db, Path.Combine(AppContext.BaseDirectory, "Data", "seeds"));

        var svc = new ArenaTwoPickService(
            new ArenaTwoPickRunRepository(db),
            new ArenaTwoPickRewardRepository(db),
            new NullCardPoolService(),
            config,
            scope.ServiceProvider.GetRequiredService<IViewerRepository>(),
            inv,
            scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.BattleXp.IBattleXpService>(),
            new SystemRandom(seed: 1234),
            db);

        return (db, svc, viewer.Id);
    }

    [Test]
    public async Task EntryAsync_with_ticket_debits_one_and_creates_run_in_class_select_state()
    {
        var (db, svc, viewerId) = await SetupAsync(ticketCount: 5);
        await using var _ = db;

        var dto = await svc.EntryAsync(viewerId, consumeItemType: 3);

        Assert.That(dto.EntryInfo.Id, Is.GreaterThan(0));
        Assert.That(dto.EntryInfo.MaxBattleCount, Is.EqualTo(5));
        Assert.That(dto.CandidateClassIds.Count, Is.EqualTo(3));
        Assert.That(dto.RewardList.Count, Is.EqualTo(1));
        Assert.That(dto.RewardList[0].RewardType, Is.EqualTo(4));
        Assert.That(dto.RewardList[0].RewardId, Is.EqualTo(TicketItemId));
        Assert.That(dto.RewardList[0].RewardNum, Is.EqualTo(4), "post-state ticket count");

        var run = await db.ViewerArenaTwoPickRuns.FirstAsync(r => r.ViewerId == viewerId);
        Assert.That(run.ClassId, Is.EqualTo(0));
        Assert.That(run.MaxBattleCount, Is.EqualTo(5));

        // Re-read viewer to verify ticket was debited.
        var updated = await db.Viewers.Include(v => v.Items).ThenInclude(i => i.Item).FirstAsync(v => v.Id == viewerId);
        var item = updated.Items.First(i => i.Item.Id == (int)TicketItemId);
        Assert.That(item.Count, Is.EqualTo(4));
    }

    [Test]
    public async Task EntryAsync_without_ticket_throws_insufficient_ticket()
    {
        var (db, svc, viewerId) = await SetupAsync(ticketCount: 0);
        await using var _ = db;

        var ex = Assert.ThrowsAsync<ArenaTwoPickException>(() => svc.EntryAsync(viewerId, 3));
        Assert.That(ex!.ErrorCode, Is.EqualTo("insufficient_ticket"));
        Assert.That(await db.ViewerArenaTwoPickRuns.AnyAsync(), Is.False);
    }

    [Test]
    public async Task EntryAsync_in_freeplay_skips_debit_and_emits_unchanged_count()
    {
        var (db, svc, viewerId) = await SetupAsync(ticketCount: 0, freeplay: true);
        await using var _ = db;

        var dto = await svc.EntryAsync(viewerId, 3);

        Assert.That(dto.RewardList[0].RewardNum, Is.EqualTo(0), "unchanged in freeplay");
        var run = await db.ViewerArenaTwoPickRuns.FirstAsync();
        Assert.That(run, Is.Not.Null);
    }

    [Test]
    public async Task EntryAsync_while_run_active_throws_already_in_progress()
    {
        var (db, svc, viewerId) = await SetupAsync(ticketCount: 5);
        await using var _ = db;
        await svc.EntryAsync(viewerId, 3);

        var ex = Assert.ThrowsAsync<ArenaTwoPickException>(() => svc.EntryAsync(viewerId, 3));
        Assert.That(ex!.ErrorCode, Is.EqualTo("arena_two_pick_already_in_progress"));
    }

    [Test]
    public async Task EntryAsync_with_crystals_debits_150_and_emits_reward_list_with_post_state_crystal_balance()
    {
        var (db, svc, viewerId) = await SetupAsync(ticketCount: 0, crystals: 500);
        await using var _ = db;

        var dto = await svc.EntryAsync(viewerId, consumeItemType: 1);

        Assert.That(dto.RewardList.Count, Is.EqualTo(1));
        Assert.That(dto.RewardList[0].RewardType, Is.EqualTo((int)SVSim.Database.Enums.UserGoodsType.Crystal));
        Assert.That(dto.RewardList[0].RewardId, Is.EqualTo(0));
        Assert.That(dto.RewardList[0].RewardNum, Is.EqualTo(350), "post-state = 500 - 150");

        var updated = await db.Viewers.Include(v => v.Currency).FirstAsync(v => v.Id == viewerId);
        Assert.That((long)updated.Currency!.Crystals, Is.EqualTo(350));
    }

    [Test]
    public async Task EntryAsync_with_rupies_debits_150_and_emits_reward_list_with_post_state_rupy_balance()
    {
        var (db, svc, viewerId) = await SetupAsync(ticketCount: 0, rupees: 500);
        await using var _ = db;

        var dto = await svc.EntryAsync(viewerId, consumeItemType: 4);

        Assert.That(dto.RewardList.Count, Is.EqualTo(1));
        Assert.That(dto.RewardList[0].RewardType, Is.EqualTo((int)SVSim.Database.Enums.UserGoodsType.Rupy));
        Assert.That(dto.RewardList[0].RewardId, Is.EqualTo(0));
        Assert.That(dto.RewardList[0].RewardNum, Is.EqualTo(350), "post-state = 500 - 150");

        var updated = await db.Viewers.Include(v => v.Currency).FirstAsync(v => v.Id == viewerId);
        Assert.That((long)updated.Currency!.Rupees, Is.EqualTo(350));
    }

    [Test]
    public async Task EntryAsync_free_entry_emits_empty_reward_list_and_creates_run()
    {
        var (db, svc, viewerId) = await SetupAsync(ticketCount: 0);
        await using var _ = db;

        var dto = await svc.EntryAsync(viewerId, consumeItemType: 5);

        Assert.That(dto.RewardList, Is.Empty, "free entry emits no fee entry");
        var run = await db.ViewerArenaTwoPickRuns.FirstAsync(r => r.ViewerId == viewerId);
        Assert.That(run, Is.Not.Null);
    }

    [Test]
    public async Task EntryAsync_with_invalid_consume_item_type_throws()
    {
        var (db, svc, viewerId) = await SetupAsync(ticketCount: 5);
        await using var _ = db;

        var ex = Assert.ThrowsAsync<ArenaTwoPickException>(() => svc.EntryAsync(viewerId, consumeItemType: 99));
        Assert.That(ex!.ErrorCode, Is.EqualTo("invalid_consume_item_type"));
        Assert.That(await db.ViewerArenaTwoPickRuns.AnyAsync(), Is.False);
    }
}
