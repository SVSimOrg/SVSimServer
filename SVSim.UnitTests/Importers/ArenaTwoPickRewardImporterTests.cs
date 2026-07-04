using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Importers;

public class ArenaTwoPickRewardImporterTests
{
    private static async Task<SVSimDbContext> CreateContextAsync()
    {
        var factory = new SVSimTestFactory();
        var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await db.Database.EnsureCreatedAsync();
        return db;
    }

    [Test]
    public async Task Import_loads_all_12_rows_from_seed_file()
    {
        await using var db = await CreateContextAsync();
        var importer = new ArenaTwoPickRewardImporter();

        await importer.ImportAsync(db, FindSeedDir());

        // 6 WinCount tiers (0..5) × 2 reward rows each = 12. Classic SV TK2 caps at 5 battles.
        var rows = await db.ArenaTwoPickRewards.OrderBy(r => r.WinCount).ThenBy(r => r.RewardType).ToListAsync();
        Assert.That(rows.Count, Is.EqualTo(12));
        Assert.That(rows.Max(r => r.WinCount), Is.EqualTo(5));

        var w0 = rows.Where(r => r.WinCount == 0).ToList();
        Assert.That(w0.Count, Is.EqualTo(2));
        Assert.That(w0.Single(r => r.RewardType == (UserGoodsType)4).RewardNum, Is.EqualTo(1));
        Assert.That(w0.Single(r => r.RewardType == (UserGoodsType)9).RewardNum, Is.EqualTo(100));

        var w5 = rows.Where(r => r.WinCount == 5).ToList();
        Assert.That(w5.Single(r => r.RewardType == (UserGoodsType)4).RewardNum, Is.EqualTo(1));
        Assert.That(w5.Single(r => r.RewardType == (UserGoodsType)9).RewardNum, Is.EqualTo(1000));

        // New columns: all rows should have weight=1 and win5 should span 2 distinct groups.
        Assert.That(w5.All(r => r.Weight == 1), "all win5 rows should have Weight=1");
        Assert.That(w5.Select(r => r.RewardGroup).Distinct().Count(), Is.EqualTo(2),
            "win5 rewards split across 2 distinct groups (ticket + rupy)");
    }

    [Test]
    public async Task Import_is_idempotent_on_re_run()
    {
        await using var db = await CreateContextAsync();
        var importer = new ArenaTwoPickRewardImporter();

        await importer.ImportAsync(db, FindSeedDir());
        await importer.ImportAsync(db, FindSeedDir());

        var count = await db.ArenaTwoPickRewards.CountAsync();
        Assert.That(count, Is.EqualTo(12), "second import should upsert, not duplicate");
    }

    private static string FindSeedDir()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "Data", "seeds");
        if (!Directory.Exists(dir))
            throw new DirectoryNotFoundException($"seeds dir not found at {dir} — verify csproj copy");
        return dir;
    }
}
