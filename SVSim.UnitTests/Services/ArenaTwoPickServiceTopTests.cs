using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Viewer;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

public class ArenaTwoPickServiceTopTests
{
    private static async Task<(SVSimDbContext, IArenaTwoPickRunRepository)> SetupAsync()
    {
        var factory = new SVSimTestFactory();
        var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await db.Database.EnsureCreatedAsync();
        return (db, new ArenaTwoPickRunRepository(db));
    }

    [Test]
    public async Task GetTopAsync_returns_null_entry_info_when_no_run()
    {
        var (db, runRepo) = await SetupAsync();
        await using var _ = db;
        var svc = BuildService(db, runRepo);

        var dto = await svc.GetTopAsync(viewerId: 99);
        Assert.That(dto.EntryInfo, Is.Null);
        Assert.That(dto.ClassInfo, Is.Null);
        Assert.That(dto.DeckInfo, Is.Null);
    }

    [Test]
    public async Task GetTopAsync_after_entry_omits_class_info_and_deck_info()
    {
        var (db, runRepo) = await SetupAsync();
        await using var _ = db;
        await runRepo.UpsertAsync(new ViewerArenaTwoPickRun
        {
            ViewerId = 99, EntryId = 1234, RewardScheduleId = 1, ChallengeId = 1,
            MaxBattleCount = 7, ClassId = 0,
            CandidateClassIdsJson = "[1,7,8]",
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        var svc = BuildService(db, runRepo);

        var dto = await svc.GetTopAsync(viewerId: 99);
        Assert.That(dto.EntryInfo, Is.Not.Null);
        Assert.That(dto.EntryInfo!.Id, Is.EqualTo(1234));
        Assert.That(dto.ClassInfo, Is.Null);
        Assert.That(dto.DeckInfo, Is.Null);
        Assert.That(dto.BattleResults!.WinCount, Is.EqualTo(0));
    }

    [Test]
    public async Task GetTopAsync_during_card_select_emits_class_and_deck_info()
    {
        var (db, runRepo) = await SetupAsync();
        await using var _ = db;
        await runRepo.UpsertAsync(new ViewerArenaTwoPickRun
        {
            ViewerId = 99, EntryId = 1234, MaxBattleCount = 7, ClassId = 1, LeaderSkinId = 1,
            CandidateClassIdsJson = "[1,7,8]",
            SelectTurn = 5, IsSelectCompleted = false,
            SelectedCardIdsJson = "[100111010,100121010,100131010,100141010,100114010,100124010,100134010,100144010]",
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        });
        var svc = BuildService(db, runRepo);

        var dto = await svc.GetTopAsync(viewerId: 99);
        Assert.That(dto.ClassInfo, Is.Not.Null);
        Assert.That(dto.ClassInfo!.ClassId1, Is.EqualTo(1));
        Assert.That(dto.DeckInfo, Is.Not.Null);
        Assert.That(dto.DeckInfo!.SelectTurn, Is.EqualTo(5));
        Assert.That(dto.DeckInfo.SelectedCardIds.Count, Is.EqualTo(8));
    }

    private static IArenaTwoPickService BuildService(SVSimDbContext db, IArenaTwoPickRunRepository runRepo)
    {
        // GetTopAsync only uses _runs — every other dep can be null! because the test path
        // never touches them.
        return new ArenaTwoPickService(runRepo, null!, null!, null!, null!, null!, null!, null!, db);
    }
}
