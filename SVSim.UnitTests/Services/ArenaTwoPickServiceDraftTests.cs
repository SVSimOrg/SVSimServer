using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

public class ArenaTwoPickServiceDraftTests
{
    private sealed class FakePool : IArenaTwoPickCardPoolService
    {
        public List<CandidatePair> GeneratePickSetsForTurn(int classId, int turn, long startingPairId, IRandom rng) => new()
        {
            new() { Id = startingPairId,     Turn = turn, SetNum = 1,
                    CardId1 = 1000 + turn * 10 + 1, CardId2 = 1000 + turn * 10 + 2, IsSelected = false },
            new() { Id = startingPairId + 1, Turn = turn, SetNum = 2,
                    CardId1 = 2000 + turn * 10 + 1, CardId2 = 2000 + turn * 10 + 2, IsSelected = false },
        };
        public List<CandidatePair> GeneratePickSetsForTurn(int classId, int turn, long startingPairId, IRandom rng, IReadOnlyList<int>? poolCardSetIds)
            => GeneratePickSetsForTurn(classId, turn, startingPairId, rng);
    }

    private static async Task<(IArenaTwoPickService, IArenaTwoPickRunRepository, long viewerId)> SetupWithActiveRunAsync(int classChosen = 0)
    {
        var factory = new SVSimTestFactory();
        var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await db.Database.EnsureCreatedAsync();

        var viewer = new SVSim.Database.Models.Viewer { Id = 7, DisplayName = "v", Currency = new ViewerCurrency() };
        db.Viewers.Add(viewer);
        await db.SaveChangesAsync();

        var runs = new ArenaTwoPickRunRepository(db);
        await runs.UpsertAsync(new ViewerArenaTwoPickRun
        {
            ViewerId = 7, EntryId = 4242,
            CandidateClassIdsJson = "[1,7,8]",
            ClassId = classChosen, MaxBattleCount = 7,
            SelectTurn = classChosen == 0 ? 0 : 1,
            PendingPickSetsJson = classChosen == 0
                ? "[]"
                : JsonSerializer.Serialize(new List<CandidatePair>
                {
                    new() { Id = 1, Turn = 1, SetNum = 1, CardId1 = 11, CardId2 = 12 },
                    new() { Id = 2, Turn = 1, SetNum = 2, CardId1 = 21, CardId2 = 22 },
                }),
            NextCandidateId = classChosen == 0 ? 1 : 3,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });

        var svc = new ArenaTwoPickService(
            runs,
            scope.ServiceProvider.GetRequiredService<IArenaTwoPickRewardRepository>(),
            new FakePool(),
            scope.ServiceProvider.GetRequiredService<IGameConfigService>(),
            scope.ServiceProvider.GetRequiredService<IViewerRepository>(),
            scope.ServiceProvider.GetRequiredService<IInventoryService>(),
            scope.ServiceProvider.GetRequiredService<SVSim.Database.Services.BattleXp.IBattleXpService>(),
            new SystemRandom(seed: 1),
            db);

        return (svc, runs, 7);
    }

    [Test]
    public async Task ChooseClassAsync_persists_class_and_emits_first_pick_sets()
    {
        var (svc, runs, vid) = await SetupWithActiveRunAsync();
        var dto = await svc.ChooseClassAsync(vid, classId: 7);

        Assert.That(dto.ClassInfo.SelectedClassId, Is.EqualTo(7));
        Assert.That(dto.DeckInfo.SelectTurn, Is.EqualTo(1));
        Assert.That(dto.DeckInfo.IsSelectCompleted, Is.False);
        Assert.That(dto.CandidateCardList.Count, Is.EqualTo(2));
        Assert.That(dto.CandidateCardList[0].Id, Is.EqualTo(1));
        Assert.That(dto.CandidateCardList[1].Id, Is.EqualTo(2));

        var row = await runs.GetByViewerIdAsync(vid);
        Assert.That(row!.ClassId, Is.EqualTo(7));
        Assert.That(row.NextCandidateId, Is.EqualTo(3));
    }

    [Test]
    public async Task ChooseClassAsync_rejects_class_not_offered()
    {
        var (svc, _, vid) = await SetupWithActiveRunAsync();
        var ex = Assert.ThrowsAsync<ArenaTwoPickException>(() => svc.ChooseClassAsync(vid, classId: 4));
        Assert.That(ex!.ErrorCode, Is.EqualTo("arena_two_pick_class_not_offered"));
    }

    [Test]
    public async Task ChooseClassAsync_rejects_when_class_already_chosen()
    {
        var (svc, _, vid) = await SetupWithActiveRunAsync(classChosen: 1);
        var ex = Assert.ThrowsAsync<ArenaTwoPickException>(() => svc.ChooseClassAsync(vid, classId: 1));
        Assert.That(ex!.ErrorCode, Is.EqualTo("arena_two_pick_invalid_state"));
    }

    [Test]
    public async Task ChooseCardAsync_appends_two_cards_and_advances_turn()
    {
        var (svc, runs, vid) = await SetupWithActiveRunAsync(classChosen: 1);
        var dto = await svc.ChooseCardAsync(vid, selectedId: 2);

        Assert.That(dto.DeckInfo.SelectedCardIds, Is.EqualTo(new List<long> { 21, 22 }));
        Assert.That(dto.DeckInfo.SelectTurn, Is.EqualTo(2));
        Assert.That(dto.CandidateCardList!.Count, Is.EqualTo(2));

        var row = await runs.GetByViewerIdAsync(vid);
        Assert.That(row!.NextCandidateId, Is.EqualTo(5));
    }

    [Test]
    public async Task ChooseCardAsync_at_turn_15_completes_the_draft_and_omits_pick_list()
    {
        var (svc, runs, vid) = await SetupWithActiveRunAsync(classChosen: 1);
        // Fast-forward to turn 15 by writing the row directly.
        var row = await runs.GetByViewerIdAsync(vid);
        row!.SelectTurn = 15;
        var pending = new List<CandidatePair>
        {
            new() { Id = 100, Turn = 15, SetNum = 1, CardId1 = 71, CardId2 = 72 },
            new() { Id = 101, Turn = 15, SetNum = 2, CardId1 = 81, CardId2 = 82 },
        };
        row.PendingPickSetsJson = JsonSerializer.Serialize(pending);
        row.SelectedCardIdsJson = JsonSerializer.Serialize(Enumerable.Range(1, 28).Select(i => (long)i).ToList());
        await runs.UpsertAsync(row);

        var dto = await svc.ChooseCardAsync(vid, selectedId: 100);
        Assert.That(dto.DeckInfo.IsSelectCompleted, Is.True);
        Assert.That(dto.DeckInfo.SelectedCardIds.Count, Is.EqualTo(30));
        Assert.That(dto.CandidateCardList, Is.Null, "omitted on completion per spec");
    }

    [Test]
    public async Task ChooseCardAsync_rejects_invalid_selected_id()
    {
        var (svc, _, vid) = await SetupWithActiveRunAsync(classChosen: 1);
        var ex = Assert.ThrowsAsync<ArenaTwoPickException>(() => svc.ChooseCardAsync(vid, selectedId: 999));
        Assert.That(ex!.ErrorCode, Is.EqualTo("arena_two_pick_invalid_selection"));
    }
}
