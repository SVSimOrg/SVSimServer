using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Models;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of /mypage/index-derived globals from per-table seed files.
/// Banners and SpecialDeckFormats use CLEAR-AND-REWRITE semantics (no stable wire ID, capture is authoritative).
/// Colosseum and SealedSeason are singletons (Id=1). MasterPointRankingPeriod upserts by wire id.
/// </summary>
public class MyPageGlobalsImporter
{
    public async Task<int> ImportBannersAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<BannerSeed>(Path.Combine(seedDir, "banners.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[MyPageGlobalsImporter] No banner seed rows; skipping.");
            return 0;
        }

        // Clear-and-rewrite: banners have no stable wire ID, the capture is authoritative.
        var existing = await context.Banners.ToListAsync();
        context.Banners.RemoveRange(existing);

        foreach (var s in seed)
        {
            context.Banners.Add(new BannerEntry
            {
                Id = s.Id,
                ImageName = s.ImageName,
                Click = s.Click,
                Status = s.Status,
                ChangeTime = s.ChangeTime,
                RemainingTime = s.RemainingTime,
                ImagePaths = s.ImagePaths.ValueKind == JsonValueKind.Undefined
                    ? "[]"
                    : JsonSerializer.Serialize(s.ImagePaths),
            });
        }
        await context.SaveChangesAsync();
        Console.WriteLine($"[MyPageGlobalsImporter] Banners: -{existing.Count}/+{seed.Count}");
        return seed.Count;
    }

    public async Task<int> ImportHomeDialogsAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<HomeDialogSeed>(Path.Combine(seedDir, "home-dialogs.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[MyPageGlobalsImporter] No home-dialog seed rows; skipping.");
            return 0;
        }

        // Clear-and-rewrite — same semantics as banners. Seed file is authoritative.
        var existing = await context.HomeDialogEntries.ToListAsync();
        context.HomeDialogEntries.RemoveRange(existing);

        foreach (var s in seed)
        {
            context.HomeDialogEntries.Add(new HomeDialogEntry
            {
                Id = s.Id,
                TitleTextId = s.TitleTextId,
                Image = s.Image,
                ButtonListJson = s.ButtonList.ValueKind == JsonValueKind.Undefined
                    ? "[]"
                    : JsonSerializer.Serialize(s.ButtonList),
                BeginTime = ImporterBase.ParseWireDateTime(s.BeginTime),
                EndTime = ImporterBase.ParseWireDateTime(s.EndTime),
                Type = s.Type,
                Priority = s.Priority,
            });
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[MyPageGlobalsImporter] HomeDialogs: -{existing.Count}/+{seed.Count}");
        return seed.Count;
    }

    public async Task<int> ImportSealedAsync(SVSimDbContext context, string seedDir)
    {
        var s = SeedLoader.LoadObject<SealedSeasonSeed>(Path.Combine(seedDir, "sealed-season.json"));
        if (s is null)
        {
            Console.WriteLine("[MyPageGlobalsImporter] No sealed-season seed; skipping.");
            return 0;
        }

        var existing = await context.SealedSeasons.FirstOrDefaultAsync(e => e.Id == 1);
        var entry = existing ?? new SealedConfig { Id = 1 };

        entry.Enable = s.Enable;
        entry.CrystalCost = s.CrystalCost;
        entry.RupyCost = s.RupyCost;
        entry.TicketCost = s.TicketCost;
        entry.DeckUsingNumMin = s.DeckUsingNumMin;
        entry.ScheduleId = s.ScheduleId;
        entry.IsJoin = s.IsJoin;
        entry.IsDeckCodeMaintenance = s.IsDeckCodeMaintenance;
        entry.PackInfo = s.PackInfo.ValueKind == JsonValueKind.Undefined
            ? "[]"
            : JsonSerializer.Serialize(s.PackInfo);
        entry.SalesPeriodInfo = s.SalesPeriodInfo.ValueKind == JsonValueKind.Undefined
            ? "{}"
            : JsonSerializer.Serialize(s.SalesPeriodInfo);

        if (existing is null) context.SealedSeasons.Add(entry);
        await context.SaveChangesAsync();
        Console.WriteLine($"[MyPageGlobalsImporter] SealedSeason: {(existing is null ? "+1" : "~1")}");
        return 1;
    }

    public async Task<int> ImportMasterPointRankingPeriodAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<MasterPointRankingPeriodSeed>(
            Path.Combine(seedDir, "master-point-ranking-periods.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[MyPageGlobalsImporter] No master-point-ranking-period seed rows; skipping.");
            return 0;
        }

        var existing = await context.MasterPointRankingPeriods.ToDictionaryAsync(e => e.Id);
        int created = 0, updated = 0;

        foreach (var s in seed)
        {
            if (s.Id == 0) continue;

            var entry = existing.TryGetValue(s.Id, out var ex)
                ? ex : new MasterPointRankingPeriodEntry { Id = s.Id };

            entry.PeriodNum = s.PeriodNum;
            entry.NecessaryScore = s.NecessaryScore;
            entry.BeginTime = ImporterBase.ParseWireDateTime(s.BeginTime);
            entry.EndTime = ImporterBase.ParseWireDateTime(s.EndTime);

            if (ex is null)
            {
                context.MasterPointRankingPeriods.Add(entry);
                existing[s.Id] = entry;
                created++;
            }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[MyPageGlobalsImporter] MasterPointRankingPeriod: +{created}/~{updated}");
        return created + updated;
    }

    public async Task<int> ImportSpecialDeckFormatsAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<SpecialDeckFormatSeed>(
            Path.Combine(seedDir, "special-deck-formats.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[MyPageGlobalsImporter] No special-deck-format seed rows; skipping.");
            return 0;
        }

        // Clear-and-rewrite: same semantics as banners — no stable wire ID, capture is authoritative.
        var existing = await context.SpecialDeckFormats.ToListAsync();
        context.SpecialDeckFormats.RemoveRange(existing);

        foreach (var s in seed)
        {
            context.SpecialDeckFormats.Add(new SpecialDeckFormatEntry
            {
                Id = s.Id,
                DeckFormat = s.DeckFormat,
                EndTime = ImporterBase.ParseWireDateTime(s.EndTime),
            });
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[MyPageGlobalsImporter] SpecialDeckFormats: -{existing.Count}/+{seed.Count}");
        return seed.Count;
    }
}
