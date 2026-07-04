using Microsoft.EntityFrameworkCore;
using SVSim.Bootstrap.Models.Seed;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using static SVSim.Bootstrap.Importers.ImporterBase;

namespace SVSim.Bootstrap.Importers;

/// <summary>
/// Idempotent upsert of /pack/info catalog from <c>seeds/packs.json</c>. Owned collections
/// (ChildGachas, Banners) are replaced wholesale per pack (clear-then-rehydrate) -- diffing owned
/// collections by composite keys is more code than it's worth for catalog updates, and this
/// matches the wholesale-replace semantics of the previous in-line ImportPacks implementation.
/// Rows missing from the seed are LEFT INTACT.
/// </summary>
public class PackImporter
{
    public async Task<int> ImportAsync(SVSimDbContext context, string seedDir)
    {
        var seed = SeedLoader.LoadList<PackSeed>(Path.Combine(seedDir, "packs.json"));
        if (seed.Count == 0)
        {
            Console.WriteLine("[PackImporter] No seed rows; skipping.");
            return 0;
        }

        var existing = await context.Packs
            .Include(p => p.ChildGachas)
            .Include(p => p.Banners)
            .ToDictionaryAsync(p => p.Id);

        int created = 0, updated = 0;
        foreach (var s in seed)
        {
            if (s.ParentGachaId == 0) continue;

            var pack = existing.TryGetValue(s.ParentGachaId, out var ex)
                ? ex : new PackConfigEntry { Id = s.ParentGachaId };

            pack.BasePackId = s.BasePackId;
            pack.GachaType = s.GachaType;
            pack.PackCategory = (PackCategory)s.PackCategory;
            pack.PosterType = s.PosterType;
            pack.CommenceDate = ParseWireDateTime(s.CommenceDate);
            pack.CompleteDate = ParseWireDateTime(s.CompleteDate);
            pack.SleeveId = s.SleeveId;
            pack.SpecialSleeveId = s.SpecialSleeveId;
            pack.OverrideDrawEffectPackId = s.OverrideDrawEffectPackId;
            pack.OverrideUiEffectPackId = s.OverrideUiEffectPackId;
            pack.GachaDetail = s.GachaDetail;
            pack.IsHide = s.IsHide;
            pack.IsNew = s.IsNew;
            pack.IsPreRelease = s.IsPreRelease;
            pack.OpenCountLimit = s.OpenCountLimit;
            pack.SalesPeriodTime = string.IsNullOrEmpty(s.SalesPeriodTime)
                ? null
                : ParseWireDateTime(s.SalesPeriodTime);
            pack.GachaPointConfig = s.GachaPoint is null ? null : new PackGachaPointConfig
            {
                ExchangeablePoint = s.GachaPoint.ExchangeablePoint,
                IncreaseGachaPoint = s.GachaPoint.IncreaseGachaPoint,
            };
            pack.IsEnabled = s.IsEnabled;

            // Owned collections -- clear and rehydrate (matches the previous wholesale-replace semantics).
            pack.ChildGachas.Clear();
            foreach (var c in s.ChildGachas)
            {
                pack.ChildGachas.Add(new PackChildGachaEntry
                {
                    GachaId = c.GachaId,
                    TypeDetail = (CardPackType)c.TypeDetail,
                    Cost = c.Cost,
                    CardCount = c.CardCount,
                    ItemId = c.ItemId,
                    IsDailySingle = c.IsDailySingle,
                    OverrideIncreaseGachaPoint = c.OverrideIncreaseGachaPoint,
                    PurchaseLimitCount = c.PurchaseLimitCount,
                    DailyFreeGachaCount = c.DailyFreeGachaCount,
                    FreeGachaCampaignId = c.FreeGachaCampaignId,
                    CampaignName = c.CampaignName,
                });
            }

            pack.Banners.Clear();
            foreach (var b in s.Banners)
            {
                pack.Banners.Add(new PackBannerEntry
                {
                    BannerName = b.BannerName,
                    DialogTitle = b.DialogTitle,
                });
            }

            if (ex is null)
            {
                context.Packs.Add(pack);
                existing[s.ParentGachaId] = pack;
                created++;
            }
            else updated++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[PackImporter] capture: +{created}/~{updated}");

        // Second pass: synthesized stubs from pack-stubs.json. Skip any pack_id that already
        // exists from the live-capture pass (capture wins on conflict).
        var stubs = SeedLoader.LoadList<PackSeed>(Path.Combine(seedDir, "pack-stubs.json"));
        int stubsAdded = 0;
        foreach (var s in stubs)
        {
            if (s.ParentGachaId == 0) continue;
            if (existing.ContainsKey(s.ParentGachaId)) continue;

            var pack = new PackConfigEntry
            {
                Id = s.ParentGachaId,
                BasePackId = s.BasePackId,
                GachaType = s.GachaType,
                PackCategory = (PackCategory)s.PackCategory,
                PosterType = s.PosterType,
                CommenceDate = ParseWireDateTime(s.CommenceDate),
                CompleteDate = ParseWireDateTime(s.CompleteDate),
                SleeveId = s.SleeveId,
                SpecialSleeveId = s.SpecialSleeveId,
                OverrideDrawEffectPackId = s.OverrideDrawEffectPackId,
                OverrideUiEffectPackId = s.OverrideUiEffectPackId,
                GachaDetail = s.GachaDetail,
                IsHide = s.IsHide,
                IsNew = s.IsNew,
                IsPreRelease = s.IsPreRelease,
                OpenCountLimit = s.OpenCountLimit,
                SalesPeriodTime = string.IsNullOrEmpty(s.SalesPeriodTime) ? null : ParseWireDateTime(s.SalesPeriodTime),
                GachaPointConfig = s.GachaPoint is null ? null : new PackGachaPointConfig
                {
                    ExchangeablePoint = s.GachaPoint.ExchangeablePoint,
                    IncreaseGachaPoint = s.GachaPoint.IncreaseGachaPoint,
                },
                IsEnabled = s.IsEnabled,
            };
            foreach (var c in s.ChildGachas)
            {
                pack.ChildGachas.Add(new PackChildGachaEntry
                {
                    GachaId = c.GachaId,
                    TypeDetail = (CardPackType)c.TypeDetail,
                    Cost = c.Cost,
                    CardCount = c.CardCount,
                    ItemId = c.ItemId,
                    IsDailySingle = c.IsDailySingle,
                    OverrideIncreaseGachaPoint = c.OverrideIncreaseGachaPoint,
                    PurchaseLimitCount = c.PurchaseLimitCount,
                    DailyFreeGachaCount = c.DailyFreeGachaCount,
                    FreeGachaCampaignId = c.FreeGachaCampaignId,
                    CampaignName = c.CampaignName,
                });
            }
            foreach (var b in s.Banners)
            {
                pack.Banners.Add(new PackBannerEntry
                {
                    BannerName = b.BannerName,
                    DialogTitle = b.DialogTitle,
                });
            }
            context.Packs.Add(pack);
            existing[s.ParentGachaId] = pack;
            stubsAdded++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"[PackImporter] stubs: +{stubsAdded}");

        return created + updated + stubsAdded;
    }
}
