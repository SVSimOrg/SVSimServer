using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.BuildDeck;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.BuildDeck;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.BuildDeck;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /build_deck/* — the in-game "Structure Deck" prebuilt-deck shop. Catalog +
/// purchase + per-product purchase counter refresh. See
/// docs/superpowers/specs/2026-05-26-prebuilt-decks-design.md.
/// </summary>
[Route("build_deck")]
public class BuildDeckController : SVSimController
{
    private readonly IBuildDeckRepository _repo;
    private readonly IInventoryService _inv;

    public BuildDeckController(
        IBuildDeckRepository repo,
        IInventoryService inv)
    {
        _repo = repo;
        _inv = inv;
    }

    // The wire shape for /build_deck/info has `data` as a bare collection of series, not a
    // DTO with a `series_list` field. The client (BuildDeckPurchaseInfoTask.Parse) iterates
    // `data` directly via numeric indexer:
    //   for (int i = 0; i < data.Count; i++) data[i]["series_id"].ToInt();
    // So `data` must be either an array OR an object whose values are series. Wrapping in
    // `{series_list: [...]}` breaks the iteration: `data.Count` is 1 and `data[0]` is the
    // inner array, so `data[0]["series_id"]` throws "Instance of JsonData is not a dictionary".
    // We return a bare array — simpler than the dict-keyed-by-order_id shape prod emits, and
    // LitJson's numeric indexer iterates both shapes identically.
    [HttpPost("info")]
    public async Task<ActionResult<List<BuildDeckSeriesDto>>> Info(BuildDeckInfoRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var series = await _repo.GetEnabledCatalog(request.AddSeriesId);
        var purchases = await _repo.GetPurchasesForViewer(viewerId);

        return series.Select(s => ToSeriesDto(s, purchases)).ToList();
    }

    private static BuildDeckSeriesDto ToSeriesDto(
        BuildDeckSeriesEntry s,
        IReadOnlyDictionary<int, ViewerBuildDeckProductPurchase> purchases)
    {
        int totalSeriesPurchases = s.Products
            .Sum(p => purchases.TryGetValue(p.Id, out var v) ? v.PurchaseCount : 0);

        return new BuildDeckSeriesDto
        {
            SeriesId = s.Id,
            OrderId = s.OrderIndex,
            IsNew = s.IsNew,
            Products = s.Products
                .OrderBy(p => p.Id)
                .Select(p => ToProductDto(p, purchases))
                .ToList(),
            SeriesRewards = GroupSeriesRewards(s.SeriesRewards, totalSeriesPurchases),
        };
    }

    private static BuildDeckProductDto ToProductDto(
        BuildDeckProductEntry p,
        IReadOnlyDictionary<int, ViewerBuildDeckProductPurchase> purchases)
    {
        int current = purchases.TryGetValue(p.Id, out var v) ? v.PurchaseCount : 0;
        bool isFirstPrice = current == 0;
        int? priceCrystal = SelectPrice(isFirstPrice, p.IntroPriceCrystal, p.RegularPriceCrystal);
        int? priceRupy    = SelectPrice(isFirstPrice, p.IntroPriceRupy,    p.RegularPriceRupy);

        return new BuildDeckProductDto
        {
            ProductId = p.Id,
            ProductName = p.ProductNameKey,
            LeaderId = p.LeaderId,
            DeckCode = p.DeckCode,
            FeaturedCardId = p.FeaturedCardId,
            PurchaseNumMax = p.PurchaseNumMax,
            PurchaseNumCurrent = current,
            IsFirstPrice = isFirstPrice,
            PriceCrystal = priceCrystal,
            PriceRupy = priceRupy,
            Rewards = p.Rewards
                .OrderBy(r => r.RewardIndex)
                .Select(r => new BuildDeckProductRewardDto
                {
                    RewardType = (int)r.RewardType,
                    RewardDetailId = r.RewardDetailId,
                    RewardNumber = r.RewardNumber,
                    MessageId = r.MessageId,
                }).ToList(),
        };
    }

    private static int? SelectPrice(bool isFirstPrice, int? intro, int? regular)
    {
        if (isFirstPrice) return intro ?? regular;   // fall back when only one tier known
        return regular ?? intro;
    }

    private static List<BuildDeckSeriesRewardTierDto> GroupSeriesRewards(
        IReadOnlyList<BuildDeckSeriesRewardEntry> rows,
        int totalSeriesPurchases)
    {
        return rows
            .GroupBy(r => r.TierIndex)
            .OrderBy(g => g.Key)
            .Select(g => new BuildDeckSeriesRewardTierDto
            {
                IsGet = totalSeriesPurchases >= g.Key,
                RewardList = g.OrderBy(r => r.ItemIndex).Select(r => new BuildDeckProductRewardDto
                {
                    RewardType = (int)r.RewardType,
                    RewardDetailId = r.RewardDetailId,
                    RewardNumber = r.RewardNumber,
                    MessageId = r.MessageId,
                }).ToList(),
            }).ToList();
    }

    [HttpPost("buy")]
    public async Task<ActionResult<BuildDeckBuyResponse>> Buy(BuildDeckBuyRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var product = await _repo.GetProduct(request.ProductId);
        if (product is null) return NotFound(new { error = "unknown_product" });

        if (!product.IsEnabled || product.Series is not { IsEnabled: true })
            return BadRequest(new { error = "product_not_available" });

        if (request.SalesType is 3)
            return StatusCode(StatusCodes.Status501NotImplemented, new { error = "ticket_currency_path_not_implemented" });
        if (request.SalesType is < 0 or > 3)
            return BadRequest(new { error = "invalid_sales_type" });

        var purchases = await _repo.GetPurchasesForViewer(viewerId);
        int currentCount = purchases.TryGetValue(product.Id, out var pp) ? pp.PurchaseCount : 0;
        if (currentCount >= product.PurchaseNumMax)
            return BadRequest(new { error = "purchase_limit_reached" });

        bool isFirstPrice = currentCount == 0;
        int? priceCrystal = SelectPrice(isFirstPrice, product.IntroPriceCrystal, product.RegularPriceCrystal);
        int? priceRupy    = SelectPrice(isFirstPrice, product.IntroPriceRupy,    product.RegularPriceRupy);

        // Currency validation
        switch (request.SalesType)
        {
            case 0: // free
                if (!(product.IntroPriceCrystal == 0 && product.IntroPriceRupy == 0))
                    return BadRequest(new { error = "price_not_available_for_currency" });
                break;
            case 1: // crystal
                if (priceCrystal is null)
                    return BadRequest(new { error = "price_not_available_for_currency" });
                break;
            case 2: // rupy
                if (priceRupy is null)
                    return BadRequest(new { error = "price_not_available_for_currency" });
                break;
        }

        // Open the inventory transaction — loads canonical graph + BuildDeckPurchases.
        await using var tx = await _inv.BeginAsync(viewerId, HttpContext.RequestAborted, cfg =>
        {
            cfg.Source = GrantSource.BuildDeckBuy;
            cfg.WithInclude(v => v.BuildDeckPurchases);
        });
        var viewer = tx.Viewer;

        // Debit currency
        if (request.SalesType == 1)
        {
            var r = await tx.TrySpendAsync(SpendCurrency.Crystal, priceCrystal!.Value);
            if (!r.Success) return BadRequest(new { error = "insufficient_crystals" });
        }
        else if (request.SalesType == 2)
        {
            var r = await tx.TrySpendAsync(SpendCurrency.Rupee, priceRupy!.Value);
            if (!r.Success) return BadRequest(new { error = "insufficient_rupees" });
        }
        // sales_type == 0 (free): no debit

        // Compute series purchase total BEFORE this buy
        int prevSeriesCount = product.Series!.Products
            .Sum(p => purchases.TryGetValue(p.Id, out var v) ? v.PurchaseCount : 0);
        int newSeriesCount = prevSeriesCount + 1;

        // Increment purchase counter on tx.Viewer (tx loaded BuildDeckPurchases via WithInclude).
        var purchaseRow = viewer.BuildDeckPurchases.FirstOrDefault(p => p.ProductId == product.Id);
        if (purchaseRow is null)
            viewer.BuildDeckPurchases.Add(new ViewerBuildDeckProductPurchase { ProductId = product.Id, PurchaseCount = 1 });
        else
            purchaseRow.PurchaseCount += 1;

        // Grant deck cards (grouped by CardId)
        foreach (var grp in product.Cards.GroupBy(c => c.CardId))
            await tx.GrantAsync(UserGoodsType.Card, grp.Key, grp.Sum(c => c.Number));

        // Per-buy rewards
        foreach (var r in product.Rewards.OrderBy(r => r.RewardIndex))
            await tx.GrantAsync(r.RewardType, r.RewardDetailId, r.RewardNumber);

        // Series-reward tier crossings
        var crossedTiers = product.Series.SeriesRewards
            .Where(r => r.TierIndex > prevSeriesCount && r.TierIndex <= newSeriesCount)
            .GroupBy(r => r.TierIndex)
            .OrderBy(g => g.Key)
            .ToList();

        var seriesRewards = new List<BuildDeckProductRewardDto>();
        foreach (var tier in crossedTiers)
        {
            foreach (var item in tier.OrderBy(r => r.ItemIndex))
            {
                await tx.GrantAsync(item.RewardType, item.RewardDetailId, item.RewardNumber);
                seriesRewards.Add(new BuildDeckProductRewardDto
                {
                    RewardType = (int)item.RewardType,
                    RewardDetailId = item.RewardDetailId,
                    RewardNumber = item.RewardNumber,
                    MessageId = item.MessageId,
                });
            }
        }

        var result = await tx.CommitAsync(HttpContext.RequestAborted);

        return new BuildDeckBuyResponse
        {
            RewardList = result.RewardList.ToRewardList(),
            SeriesRewards = seriesRewards,
        };
    }

    [HttpPost("get_purchase_count")]
    public async Task<ActionResult<BuildDeckGetPurchaseCountResponse>> GetPurchaseCount(
        BuildDeckGetPurchaseCountRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var product = await _repo.GetProduct(request.ProductId);
        if (product is null) return NotFound(new { error = "unknown_product" });

        var purchases = await _repo.GetPurchasesForViewer(viewerId);
        int current = purchases.TryGetValue(request.ProductId, out var p) ? p.PurchaseCount : 0;

        return new BuildDeckGetPurchaseCountResponse
        {
            PurchaseNumCurrent = current,
            PurchaseNumMax = product.PurchaseNumMax,
        };
    }
}
