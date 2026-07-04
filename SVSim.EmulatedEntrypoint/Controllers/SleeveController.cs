using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Collectibles;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Sleeve;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Sleeve;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /sleeve/* — the sleeve shop. Catalog + single-product purchase. No series-completion bonus
/// (sleeves are sold individually; the leader-skin shop is the family with set-buys).
/// </summary>
[Route("sleeve")]
public class SleeveController : SVSimController
{
    private readonly SVSimDbContext _db;
    private readonly IInventoryService _inv;
    private readonly ICollectionRepository _collection;

    public SleeveController(SVSimDbContext db, IInventoryService inv, ICollectionRepository collection)
    {
        _db = db;
        _inv = inv;
        _collection = collection;
    }

    [HttpPost("info")]
    public async Task<ActionResult<SleeveInfoResponse>> Info(BaseRequest _)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        // is_purchased_product is "viewer owns at least one sleeve granted by this product".
        // Loading the viewer's sleeve-id set once and checking each product against it avoids
        // an N+1 over products.
        var viewerForInfo = await _db.Viewers
            .Include(v => v.Sleeves)
            .FirstOrDefaultAsync(v => v.Id == viewerId);
        if (viewerForInfo is null) return Unauthorized();

        var cosmeticsForInfo = await _inv.EffectiveCosmeticsAsync(viewerForInfo);
        var ownedSleeveIds = cosmeticsForInfo.SleeveIds.Select(id => (long)id).ToHashSet();

        var series = await _db.SleeveShopSeries
            .AsSplitQuery()
            .Where(s => s.IsEnabled)
            .Include(s => s.Products.Where(p => p.IsEnabled)).ThenInclude(p => p.Rewards)
            .OrderBy(s => s.Id)
            .ToListAsync();

        var sleeveList = new Dictionary<string, SleeveSeriesDto>();
        foreach (var s in series)
        {
            var products = new Dictionary<string, SleeveProductDto>();
            foreach (var p in s.Products.OrderBy(p => p.Id))
            {
                products[p.Id.ToString()] = new SleeveProductDto
                {
                    ProductId = p.Id,
                    Name = p.NameKey,
                    PriceCrystal = p.PriceCrystal,
                    PriceRupy = p.PriceRupy,
                    IsPurchasedProduct = IsProductPurchased(p, ownedSleeveIds),
                    Rewards = p.Rewards.OrderBy(r => r.OrderIndex).Select(r => new SleeveProductRewardDto
                    {
                        RewardType = (int)r.RewardType,
                        RewardDetailId = r.RewardDetailId,
                        RewardNumber = r.RewardNumber,
                    }).ToList(),
                };
            }

            sleeveList[s.Id.ToString()] = new SleeveSeriesDto
            {
                SeriesId = s.Id,
                IsNew = s.IsNew,
                ProductInfo = products,
            };
        }

        return new SleeveInfoResponse { SleeveList = sleeveList };
    }

    [HttpPost("buy")]
    public async Task<ActionResult<SleeveBuyResponse>> Buy(SleeveBuyRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        if (request.SalesType is 3)
            return StatusCode(StatusCodes.Status501NotImplemented,
                new { error = "ticket_currency_path_not_implemented" });
        if (request.SalesType is < 0 or > 3)
            return BadRequest(new { error = "invalid_sales_type" });

        var product = await _db.SleeveShopProducts
            .Include(p => p.Rewards)
            .Include(p => p.Series)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);
        if (product is null) return NotFound(new { error = "unknown_product" });

        if (!product.IsEnabled || product.Series is not { IsEnabled: true })
            return BadRequest(new { error = "product_not_available" });

        // Defence-in-depth: client also sends series_id; reject mismatches so a misencoded
        // request can't accidentally bypass per-series state we'll later add (e.g. series-new flag).
        if (product.SeriesId != request.SeriesId)
            return BadRequest(new { error = "series_product_mismatch" });

        await using var tx = await _inv.BeginAsync(viewerId, HttpContext.RequestAborted, cfg => cfg.Source = GrantSource.SleeveBuy);

        if (tx.IsFreeplay)
            return BadRequest(new { error = "already_purchased" });

        if (IsProductPurchased(product, tx.Viewer.Sleeves.Select(s => (long)s.Id).ToHashSet()))
            return BadRequest(new { error = "already_purchased" });

        // Pricing: capture-confirmed shape is single-price-per-currency (no intro/regular tiers
        // like BuildDeck). At least one of crystal/rupy must match the chosen sales_type;
        // sales_type==0 means "free", which requires both prices == 0.
        switch (request.SalesType)
        {
            case 0: // free
                if (!(product.PriceCrystal == 0 && product.PriceRupy == 0))
                    return BadRequest(new { error = "price_not_available_for_currency" });
                break;
            case 1: // crystal
                if (product.PriceCrystal is null)
                    return BadRequest(new { error = "price_not_available_for_currency" });
                { var r = await tx.TrySpendAsync(SpendCurrency.Crystal, product.PriceCrystal.Value); if (!r.Success) return BadRequest(new { error = "insufficient_crystals" }); }
                break;
            case 2: // rupy
                if (product.PriceRupy is null)
                    return BadRequest(new { error = "price_not_available_for_currency" });
                { var r = await tx.TrySpendAsync(SpendCurrency.Rupee, product.PriceRupy.Value); if (!r.Success) return BadRequest(new { error = "insufficient_rupees" }); }
                break;
        }

        // Grant each catalog reward through the central dispatcher.
        foreach (var r in product.Rewards.OrderBy(r => r.OrderIndex))
            await tx.GrantAsync(r.RewardType, r.RewardDetailId, r.RewardNumber);

        var result = await tx.CommitAsync(HttpContext.RequestAborted);

        return new SleeveBuyResponse
        {
            RewardList = result.RewardList.ToRewardList(),
        };
    }

    [HttpPost("sleeve_list")]
    public async Task<ActionResult<List<SleeveListEntry>>> SleeveList(BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var viewer = await _db.Viewers
            .Include(v => v.Sleeves)
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return Unauthorized();

        return viewer.Sleeves
            .Select(s => new SleeveListEntry { SleeveId = s.Id })
            .OrderBy(e => e.SleeveId)
            .ToList();
    }

    [HttpPost("favorite")]
    public ActionResult<EmptyResponse> Favorite([FromBody] SleeveFavoriteRequest _)
    {
        if (!TryGetViewerId(out long __)) return Unauthorized();
        // Accept-and-ack. Persisting per-viewer cosmetic favorites is deferred until
        // a viewer-favorites schema lands (would also serve /emblem/favorite + /degree/favorite
        // via the shared Wizard/FavoriteTask.cs Kind enum). Same pattern as ConfigController.
        return new EmptyResponse();
    }

    /// <summary>
    /// A product is "purchased" once the viewer owns at least one of its sleeve-typed reward
    /// grants. Emblem/other grants aren't load-bearing for this check — a viewer who somehow
    /// ended up with the emblem but not the sleeve (e.g. partial gift) should still be allowed
    /// to buy the product to pick up the sleeve.
    /// </summary>
    private static bool IsProductPurchased(SleeveShopProductEntry product, HashSet<long> ownedSleeveIds)
    {
        foreach (var r in product.Rewards)
        {
            if (r.RewardType == UserGoodsType.Sleeve && ownedSleeveIds.Contains(r.RewardDetailId))
                return true;
        }
        return false;
    }

}
