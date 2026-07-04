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
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.LeaderSkin;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.LeaderSkin;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /leader_skin/* — the leader-skin shop family.
/// <list type="bullet">
///   <item><c>/set</c>: per-class equipped-skin preference (the fallback when a deck has
///   <c>leader_skin_id == 0</c>). Per-deck overrides go through /deck/update_leader_skin.</item>
///   <item><c>/products</c>: shop catalog (dict-keyed by series_id).</item>
///   <item><c>/buy</c>: single-skin purchase. Currency dispatch crystal/rupy/ticket(501).</item>
///   <item><c>/buy_set</c>: whole-series purchase at set discount.</item>
///   <item><c>/buy_set_item</c>: claim series-completion bonus (idempotent via
///   <see cref="ViewerLeaderSkinSetClaim"/>).</item>
///   <item><c>/ids</c>: flat list of owned skin ids for badge refresh.</item>
/// </list>
/// </summary>
[Route("leader_skin")]
public class LeaderSkinController : SVSimController
{
    private readonly SVSimDbContext _db;
    private readonly IInventoryService _inv;
    private readonly TimeProvider _time;
    private readonly ICollectionRepository _collection;

    public LeaderSkinController(SVSimDbContext db, IInventoryService inv, TimeProvider time, ICollectionRepository collection)
    {
        _db = db;
        _inv = inv;
        _time = time;
        _collection = collection;
    }

    [HttpPost("set")]
    public async Task<ActionResult<LeaderSkinSetResponse>> Set(LeaderSkinSetRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        if (request.IsRandomLeaderSkin)
        {
            return StatusCode(StatusCodes.Status501NotImplemented,
                new { error = "random_leader_skin_not_implemented" });
        }

        var viewer = await _db.Viewers
            .AsSplitQuery()
            .Include(v => v.Classes).ThenInclude(c => c.Class)
            .Include(v => v.Classes).ThenInclude(c => c.LeaderSkin)
            .Include(v => v.LeaderSkins)
            .FirstOrDefaultAsync(v => v.Id == viewerId);
        if (viewer is null) return Unauthorized();

        var classData = viewer.Classes.FirstOrDefault(c => c.Class.Id == request.ClassId);
        if (classData is null) return BadRequest(new { error = "unknown_class" });

        var skin = await _db.LeaderSkins.FindAsync(request.LeaderSkinId);
        if (skin is null) return BadRequest(new { error = "unknown_skin" });
        if (skin.ClassId != request.ClassId) return BadRequest(new { error = "skin_class_mismatch" });
        var cosmeticsForSet = await _inv.EffectiveCosmeticsAsync(viewer);
        if (!cosmeticsForSet.OwnedLeaderSkinIds.Contains(skin.Id))
            return BadRequest(new { error = "skin_not_owned" });

        classData.LeaderSkin = skin;
        await _db.SaveChangesAsync();

        return new LeaderSkinSetResponse
        {
            IsRandomLeaderSkin = false,
            LeaderSkinId = skin.Id,
            LeaderSkinIdList = new(),
        };
    }

    [HttpPost("ids")]
    public async Task<ActionResult<LeaderSkinIdsResponse>> Ids(BaseRequest _)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var viewer = await _db.Viewers
            .Include(v => v.LeaderSkins)
            .FirstOrDefaultAsync(v => v.Id == viewerId);
        if (viewer is null) return Unauthorized();

        var cosmetics = await _inv.EffectiveCosmeticsAsync(viewer);
        var ids = cosmetics.OwnedLeaderSkinIds.OrderBy(id => id).ToList();
        return new LeaderSkinIdsResponse { UserLeaderSkinIds = ids };
    }

    [HttpPost("products")]
    public async Task<ActionResult<Dictionary<string, SkinSeriesDto>>> Products(BaseRequest _)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var viewerForProducts = await _db.Viewers
            .Include(v => v.LeaderSkins)
            .FirstOrDefaultAsync(v => v.Id == viewerId);
        if (viewerForProducts is null) return Unauthorized();

        var cosmeticsForProducts = await _inv.EffectiveCosmeticsAsync(viewerForProducts);
        var ownedSkinIds = cosmeticsForProducts.OwnedLeaderSkinIds;

        var claimedSeries = (await _db.ViewerLeaderSkinSetClaims
            .Where(c => c.ViewerId == viewerId)
            .Select(c => c.SeriesId)
            .ToListAsync()).ToHashSet();

        var series = await _db.LeaderSkinShopSeries
            .AsSplitQuery()
            .Where(s => s.IsEnabled)
            .Include(s => s.SetCompletionRewards)
            .Include(s => s.Products.Where(p => p.IsEnabled)).ThenInclude(p => p.Rewards)
            .OrderBy(s => s.Id)
            .ToListAsync();

        var result = new Dictionary<string, SkinSeriesDto>();
        foreach (var s in series)
        {
            var products = s.Products.OrderBy(p => p.Id).Select(p => ToProductDto(p, ownedSkinIds)).ToList();
            bool seriesCompleted = products.Count > 0 && products.All(p => p.IsPurchased);
            int rewardStatus = ComputeRewardStatus(s, seriesCompleted, claimedSeries.Contains(s.Id));

            result[s.Id.ToString()] = new SkinSeriesDto
            {
                SeriesId = s.Id,
                IsCompleted = seriesCompleted,
                IsNew = s.IsNew,
                SetSalesStatus = s.SetSalesStatus,
                Rewards = new SkinSeriesRewardsDto
                {
                    Status = rewardStatus,
                    Items = s.SetCompletionRewards.OrderBy(r => r.OrderIndex).Select(r => new SkinSeriesRewardItemDto
                    {
                        RewardType = (int)r.RewardType,
                        RewardDetailId = r.RewardDetailId,
                        RewardNumber = r.RewardNumber,
                    }).ToList(),
                },
                SetPrices = new SkinSeriesSetPricesDto
                {
                    SetPriceCrystal = s.SetPriceCrystal,
                    SetPriceRupy = s.SetPriceRupy,
                    SetPriceTicket = s.SetPriceTicket,
                    TicketId = s.SetPriceTicketId,
                },
                Products = products,
            };
        }

        return result;
    }

    [HttpPost("buy")]
    public async Task<ActionResult<LeaderSkinBuyResponse>> Buy(LeaderSkinBuyRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        if (request.SalesType is 3)
            return StatusCode(StatusCodes.Status501NotImplemented,
                new { error = "ticket_currency_path_not_implemented" });
        if (request.SalesType is < 0 or > 3)
            return BadRequest(new { error = "invalid_sales_type" });

        var product = await _db.LeaderSkinShopProducts
            .Include(p => p.Rewards)
            .Include(p => p.Series)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId);
        if (product is null) return NotFound(new { error = "unknown_product" });
        if (!product.IsEnabled || product.Series is not { IsEnabled: true })
            return BadRequest(new { error = "product_not_available" });

        await using var tx = await _inv.BeginAsync(viewerId, HttpContext.RequestAborted, cfg => cfg.Source = GrantSource.LeaderSkinBuy);

        // Already-purchased = viewer owns the leader_skin this product grants.
        if (tx.OwnsCosmetic(CosmeticType.Skin, product.LeaderSkinId))
            return BadRequest(new { error = "already_purchased" });

        // Debit currency
        switch (request.SalesType)
        {
            case 0 when product.SinglePriceCrystal == 0 && product.SinglePriceRupy == 0:
                break; // free
            case 0:
                return BadRequest(new { error = "price_not_available_for_currency" });
            case 1:
                if (product.SinglePriceCrystal is null) return BadRequest(new { error = "price_not_available_for_currency" });
                { var r = await tx.TrySpendAsync(SpendCurrency.Crystal, product.SinglePriceCrystal.Value); if (!r.Success) return BadRequest(new { error = "insufficient_crystals" }); }
                break;
            case 2:
                if (product.SinglePriceRupy is null) return BadRequest(new { error = "price_not_available_for_currency" });
                { var r = await tx.TrySpendAsync(SpendCurrency.Rupee, product.SinglePriceRupy.Value); if (!r.Success) return BadRequest(new { error = "insufficient_rupees" }); }
                break;
            default:
                return BadRequest(new { error = "invalid_sales_type" });
        }

        foreach (var r in product.Rewards.OrderBy(r => r.OrderIndex))
            await tx.GrantAsync(r.RewardType, r.RewardDetailId, r.RewardNumber);

        var result = await tx.CommitAsync(HttpContext.RequestAborted);
        return new LeaderSkinBuyResponse
        {
            RewardList = result.RewardList.ToRewardList(),
        };
    }

    [HttpPost("buy_set")]
    public async Task<ActionResult<LeaderSkinBuyResponse>> BuySet(LeaderSkinBuySetRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        if (request.SalesType is 3)
            return StatusCode(StatusCodes.Status501NotImplemented,
                new { error = "ticket_currency_path_not_implemented" });
        if (request.SalesType is < 0 or > 3)
            return BadRequest(new { error = "invalid_sales_type" });

        var series = await _db.LeaderSkinShopSeries
            .Include(s => s.Products.Where(p => p.IsEnabled)).ThenInclude(p => p.Rewards)
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId);
        if (series is null) return NotFound(new { error = "unknown_series" });
        if (!series.IsEnabled || series.SetSalesStatus == 0)
            return BadRequest(new { error = "set_sale_not_active" });

        await using var tx = await _inv.BeginAsync(viewerId, HttpContext.RequestAborted, cfg => cfg.Source = GrantSource.LeaderSkinBuy);

        if (tx.IsFreeplay)
            return BadRequest(new { error = "already_purchased" });

        // Debit set price
        switch (request.SalesType)
        {
            case 0 when series.SetPriceCrystal == 0 && series.SetPriceRupy == 0:
                break; // free
            case 0:
                return BadRequest(new { error = "price_not_available_for_currency" });
            case 1:
                if (series.SetPriceCrystal is null) return BadRequest(new { error = "price_not_available_for_currency" });
                { var r = await tx.TrySpendAsync(SpendCurrency.Crystal, series.SetPriceCrystal.Value); if (!r.Success) return BadRequest(new { error = "insufficient_crystals" }); }
                break;
            case 2:
                if (series.SetPriceRupy is null) return BadRequest(new { error = "price_not_available_for_currency" });
                { var r = await tx.TrySpendAsync(SpendCurrency.Rupee, series.SetPriceRupy.Value); if (!r.Success) return BadRequest(new { error = "insufficient_rupees" }); }
                break;
            default:
                return BadRequest(new { error = "invalid_sales_type" });
        }

        // Grant every product's rewards; tx.GrantAsync is idempotent on already-owned cosmetics.
        foreach (var p in series.Products.OrderBy(p => p.Id))
        {
            foreach (var r in p.Rewards.OrderBy(r => r.OrderIndex))
                await tx.GrantAsync(r.RewardType, r.RewardDetailId, r.RewardNumber);
        }

        var result = await tx.CommitAsync(HttpContext.RequestAborted);
        return new LeaderSkinBuyResponse
        {
            RewardList = result.RewardList.ToRewardList(),
        };
    }

    [HttpPost("buy_set_item")]
    public async Task<ActionResult<LeaderSkinBuyResponse>> BuySetItem(LeaderSkinBuySetItemRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var series = await _db.LeaderSkinShopSeries
            .AsSplitQuery()
            .Include(s => s.SetCompletionRewards)
            .Include(s => s.Products.Where(p => p.IsEnabled))
            .FirstOrDefaultAsync(s => s.Id == request.SeriesId);
        if (series is null) return NotFound(new { error = "unknown_series" });

        // Check claim hasn't been made already (idempotent — returns empty reward_list rather
        // than 400 so the client doesn't error if it retries).
        var existingClaim = await _db.ViewerLeaderSkinSetClaims
            .FirstOrDefaultAsync(c => c.ViewerId == viewerId && c.SeriesId == series.Id);
        if (existingClaim is not null)
            return new LeaderSkinBuyResponse { RewardList = new() };

        await using var tx = await _inv.BeginAsync(viewerId, HttpContext.RequestAborted, cfg => cfg.Source = GrantSource.LeaderSkinBuy);

        // Must own every skin in the series to claim the bonus.
        bool ownsAll = series.Products.Count > 0 && series.Products.All(p => tx.OwnsCosmetic(CosmeticType.Skin, p.LeaderSkinId));
        if (!ownsAll)
            return BadRequest(new { error = "series_not_completed" });

        foreach (var r in series.SetCompletionRewards.OrderBy(r => r.OrderIndex))
            await tx.GrantAsync(r.RewardType, r.RewardDetailId, r.RewardNumber);

        _db.ViewerLeaderSkinSetClaims.Add(new ViewerLeaderSkinSetClaim
        {
            ViewerId = viewerId,
            SeriesId = series.Id,
            ClaimedAt = _time.GetUtcNow().UtcDateTime,
        });

        var result = await tx.CommitAsync(HttpContext.RequestAborted);
        return new LeaderSkinBuyResponse
        {
            RewardList = result.RewardList.ToRewardList(),
        };
    }

    /// <summary>
    /// Computes the per-viewer <c>rewards.status</c> for a series:
    ///   0=none — set_sales_status==0 OR no bonus items configured (matches prod, which ships
    ///   status=0 for series where items[] is empty even when set_sales_status==1)
    ///   1=not_got — bonus exists, series completed by viewer, bonus unclaimed
    ///   2=got — viewer claimed the bonus
    ///   1 (effectively "available later") when set sale active with bonus and viewer hasn't
    ///   completed the series.
    /// The 1/2 distinction matches the client enum (RewardStatus.not_got vs .got).
    /// <para>
    /// Important: emitting status=1 when items[] is empty triggers the client's
    /// <c>is_completed &amp;&amp; not_got</c> branch in SkinPurchaseInfoTask.CreateSetSaleInfo,
    /// which marks the set sale as FREE and renders a useless "claim" button for a
    /// nonexistent bonus. Always return 0 when there's nothing to claim.
    /// </para>
    /// </summary>
    private static int ComputeRewardStatus(LeaderSkinShopSeriesEntry series, bool seriesCompleted, bool claimed)
    {
        if (series.SetSalesStatus == 0) return 0;
        if (series.SetCompletionRewards.Count == 0) return 0;
        if (claimed) return 2;
        if (seriesCompleted) return 1;
        return 1;
    }

    private static SkinProductDto ToProductDto(LeaderSkinShopProductEntry p, IReadOnlySet<int> ownedSkinIds)
    {
        bool isPurchased = ownedSkinIds.Contains(p.LeaderSkinId);
        return new SkinProductDto
        {
            ProductId = p.Id,
            LeaderSkinId = p.LeaderSkinId,
            ProductName = p.ProductNameKey,
            Introduction = p.IntroductionKey,
            CvName = p.CvNameKey,
            IsPurchased = isPurchased,
            Sale = new SkinProductSaleDto
            {
                SinglePriceCrystal = p.SinglePriceCrystal,
                SinglePriceRupy = p.SinglePriceRupy,
                SinglePriceTicket = p.SinglePriceTicket,
                TicketNumber = p.TicketNumber,
                ItemId = p.TicketItemId,
            },
            Rewards = p.Rewards.OrderBy(r => r.OrderIndex).Select(r => new SkinProductRewardDto
            {
                RewardType = (int)r.RewardType,
                RewardDetailId = r.RewardDetailId,
                RewardNumber = r.RewardNumber,
                IsOwned = IsRewardOwned(r, ownedSkinIds),
            }).ToList(),
        };
    }

    /// <summary>
    /// A bundled reward shows as "owned" when the viewer already has the cosmetic. For now we
    /// only flag the Skin reward (type==10) against the viewer's skin collection — the cascaded
    /// emblem/sleeve typically come with the skin, so the heuristic is "skin owned → all three
    /// bundle items are de-facto owned." Refine later if a capture shows independent state.
    /// </summary>
    private static bool IsRewardOwned(LeaderSkinShopProductRewardEntry r, IReadOnlySet<int> ownedSkinIds)
    {
        // Skin reward: direct check.
        if (r.RewardType == UserGoodsType.Skin)
            return ownedSkinIds.Contains((int)r.RewardDetailId);
        // Other types: we don't have the full cosmetic-owned graph in scope here. The product's
        // sibling Skin reward tells us whether the bundle was purchased; piggy-back on that by
        // letting the caller pre-compute IsPurchased. Conservative default: not owned.
        return false;
    }

}
