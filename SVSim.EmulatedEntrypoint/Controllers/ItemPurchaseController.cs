using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Services;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ItemPurchase;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ItemPurchase;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /item_purchase/* — the generic item shop where viewers spend item-or-currency to acquire
/// other items (e.g. RedEther → Seer's Globe, Orb Shards → Seer's Globe). Per-viewer monthly
/// or lifetime quota tracked via <see cref="ViewerEventCounter"/>.
/// </summary>
[Route("item_purchase")]
public class ItemPurchaseController : SVSimController
{
    private readonly SVSimDbContext _db;
    private readonly IInventoryService _inv;
    private readonly TimeProvider _time;
    private readonly IGameCalendarService _calendar;

    public ItemPurchaseController(SVSimDbContext db, IInventoryService inv, TimeProvider time, IGameCalendarService calendar)
    {
        _db = db;
        _inv = inv;
        _time = time;
        _calendar = calendar;
    }

    [HttpPost("info")]
    public async Task<ActionResult<ItemPurchaseInfoResponse>> Info(BaseRequest _)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var catalog = await _db.ItemPurchaseCatalog
            .Where(c => c.IsEnabled)
            .OrderBy(c => c.Id)
            .ToListAsync();

        var now = _time.GetUtcNow();
        var monthKey = _calendar.MonthKey(now);
        var keys = catalog.Select(c => CounterKey(c.Id)).ToList();
        var counters = await _db.ViewerEventCounters
            .Where(c => c.ViewerId == viewerId && keys.Contains(c.EventKey))
            .ToListAsync();

        var info = new List<ItemPurchaseEntryDto>(catalog.Count);
        foreach (var c in catalog)
        {
            int count = CounterCount(counters, c, monthKey);
            info.Add(new ItemPurchaseEntryDto
            {
                PurchaseId = c.Id,
                RequireItemType = c.RequireItemType,
                RequireItemId = c.RequireItemId,
                RequireItemNum = c.RequireItemNum,
                PurchaseItemType = c.PurchaseItemType,
                PurchaseItemId = c.PurchaseItemId,
                PurchaseItemNum = c.PurchaseItemNum,
                PurchaseName = c.PurchaseName,
                IsMonthlyReset = c.IsMonthlyReset ? 1 : 0,
                Rest = Math.Max(0, c.PurchaseLimit - count),
            });
        }

        // user_card_pack_ticket_list: every item with Type == 2 paired with the viewer's count
        // (zero counts included — the client unconditionally calls UpdateItemNum per entry).
        var ticketItems = await _db.Items
            .Where(i => i.Type == 2)
            .OrderByDescending(i => i.Id)
            .ToListAsync();
        var ownedByItemId = (await _db.Viewers
            .Where(v => v.Id == viewerId)
            .SelectMany(v => v.Items)
            .Select(oi => new { oi.Item.Id, oi.Count })
            .ToListAsync())
            .ToDictionary(x => x.Id, x => x.Count);

        var ticketList = ticketItems.Select(i => new UserCardPackTicketDto
        {
            ItemId = i.Id,
            Number = ownedByItemId.TryGetValue(i.Id, out var cnt) ? cnt : 0,
        }).ToList();

        return new ItemPurchaseInfoResponse
        {
            ItemPurchaseInfo = info,
            UserCardPackTicketList = ticketList,
        };
    }

    [HttpPost("purchase")]
    public async Task<ActionResult<ItemPurchasePurchaseResponse>> Purchase(ItemPurchasePurchaseRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var entry = await _db.ItemPurchaseCatalog.FindAsync(request.PurchaseId);
        if (entry is null || !entry.IsEnabled)
            return BadRequest(new { error = "unknown_purchase" });

        var now = _time.GetUtcNow();
        var period = entry.IsMonthlyReset ? _calendar.MonthKey(now) : GameCalendarPeriods.AllTime;
        var key = CounterKey(entry.Id);

        var counter = await _db.ViewerEventCounters
            .FirstOrDefaultAsync(c => c.ViewerId == viewerId && c.EventKey == key && c.Period == period);
        int currentCount = counter?.Count ?? 0;
        int rest = entry.PurchaseLimit - currentCount;
        if (rest <= 0)
            return BadRequest(new { error = "sold_out" });

        await using var tx = await _inv.BeginAsync(viewerId, HttpContext.RequestAborted, cfg => cfg.Source = GrantSource.ItemPurchase);

        // Debit the require side via the tx.
        var debit = await tx.TryDebitAsync(
            (UserGoodsType)entry.RequireItemType, entry.RequireItemId, entry.RequireItemNum);
        if (!debit.Success) return BadRequest(new { error = MapDebitError(entry.RequireItemType) });

        // Grant the purchase side.
        await tx.GrantAsync((UserGoodsType)entry.PurchaseItemType, entry.PurchaseItemId, entry.PurchaseItemNum);

        // Increment the per-period counter (tracked via _db, outside the inventory tx).
        if (counter is null)
        {
            _db.ViewerEventCounters.Add(new ViewerEventCounter
            {
                ViewerId = viewerId,
                EventKey = key,
                Period = period,
                Count = 1,
            });
        }
        else
        {
            counter.Count++;
        }
        await _db.SaveChangesAsync();

        var result = await tx.CommitAsync(HttpContext.RequestAborted);

        return new ItemPurchasePurchaseResponse
        {
            RewardList = result.RewardList.ToRewardList(),
        };
    }

    private static string MapDebitError(int requireType) => requireType switch
    {
        (int)UserGoodsType.RedEther => "insufficient_red_ether",
        (int)UserGoodsType.Crystal => "insufficient_crystals",
        (int)UserGoodsType.Rupy => "insufficient_rupees",
        (int)UserGoodsType.Item => "insufficient_item",
        _ => "debit_type_not_supported",
    };

    private static string CounterKey(int purchaseId) => MissionEventKeys.ItemPurchase(purchaseId);

    private static int CounterCount(List<ViewerEventCounter> counters, ItemPurchaseCatalogEntry entry, string monthKey)
    {
        var period = entry.IsMonthlyReset ? monthKey : GameCalendarPeriods.AllTime;
        return counters.FirstOrDefault(c => c.EventKey == CounterKey(entry.Id) && c.Period == period)?.Count ?? 0;
    }
}
