using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class BattlePassRewardImporterTests
{
    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    private static async Task SeedSeason23(SVSimDbContext db)
    {
        db.BattlePassSeasons.Add(new SVSim.Database.Models.BattlePassSeasonEntry
        {
            Id = 23, Name = "Season 23", MaxLevel = 100,
            StartDate = DateTimeOffset.Parse("2026-04-01T02:00:00+09:00"),
            EndDate = DateTimeOffset.Parse("2026-07-01T01:59:59+09:00"),
            CanPurchase = true, PriceCrystal = 980, Description = "",
        });
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Imports_normal_and_premium_tracks()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await SeedSeason23(db);

        await new BattlePassRewardImporter().ImportAsync(db, SeedDir);

        int normal = await db.BattlePassRewards.CountAsync(r => r.SeasonId == 23 && r.Track == BattlePassTrack.Normal);
        int premium = await db.BattlePassRewards.CountAsync(r => r.SeasonId == 23 && r.Track == BattlePassTrack.Premium);
        Assert.That(normal, Is.EqualTo(44), "captured normal track count for Season 23");
        Assert.That(premium, Is.EqualTo(99), "captured premium track count for Season 23");
    }

    [Test]
    public async Task Is_idempotent_on_rerun()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await SeedSeason23(db);

        await new BattlePassRewardImporter().ImportAsync(db, SeedDir);
        int before = await db.BattlePassRewards.CountAsync();
        await new BattlePassRewardImporter().ImportAsync(db, SeedDir);
        int after = await db.BattlePassRewards.CountAsync();

        Assert.That(after, Is.EqualTo(before));
    }

    [Test]
    public async Task Authoritatively_removes_orphan_rows_for_seeded_season()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await SeedSeason23(db);
        db.BattlePassRewards.Add(new SVSim.Database.Models.BattlePassRewardEntry
        {
            Id = 230_099,  // MakeId(23, Normal=0, 99) == 23*10_000 + 0*1_000 + 99
            SeasonId = 23, Track = BattlePassTrack.Normal, Level = 99, RewardType = (UserGoodsType)9,
            RewardDetailId = 0, RewardNumber = 9999, IsAppealExclusion = false,
        });
        await db.SaveChangesAsync();

        await new BattlePassRewardImporter().ImportAsync(db, SeedDir);

        bool orphanPresent = await db.BattlePassRewards.AnyAsync(
            r => r.SeasonId == 23 && r.Track == BattlePassTrack.Normal && r.Level == 99);
        Assert.That(orphanPresent, Is.False, "orphan reward not in seed must be deleted");
    }

    [Test]
    public async Task Leaves_other_seasons_untouched()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await SeedSeason23(db);
        db.BattlePassSeasons.Add(new SVSim.Database.Models.BattlePassSeasonEntry
        {
            Id = 22, Name = "Season 22", MaxLevel = 100,
            StartDate = DateTimeOffset.Parse("2026-01-01T02:00:00+09:00"),
            EndDate = DateTimeOffset.Parse("2026-04-01T01:59:59+09:00"),
            CanPurchase = false, PriceCrystal = 980, Description = "",
        });
        db.BattlePassRewards.Add(new SVSim.Database.Models.BattlePassRewardEntry
        {
            SeasonId = 22, Track = BattlePassTrack.Normal, Level = 1, RewardType = (UserGoodsType)9,
            RewardDetailId = 0, RewardNumber = 100, IsAppealExclusion = false,
        });
        await db.SaveChangesAsync();

        await new BattlePassRewardImporter().ImportAsync(db, SeedDir);

        bool s22Preserved = await db.BattlePassRewards.AnyAsync(r => r.SeasonId == 22 && r.Level == 1);
        Assert.That(s22Preserved, Is.True, "rewards for unseeded seasons must be left intact");
    }
}
