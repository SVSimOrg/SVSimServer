using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos.ItemAcquireHistory;

namespace SVSim.EmulatedEntrypoint.Controllers;

[Route("item_acquire_history")]
public sealed class ItemAcquireHistoryController : SVSimController
{
    private const string WireDateFormat = "yyyy-MM-dd HH:mm:ss";

    private readonly SVSimDbContext _db;

    public ItemAcquireHistoryController(SVSimDbContext db) => _db = db;

    [HttpPost("info")]
    public async Task<ActionResult<ItemAcquireHistoryInfoResponse>> Info(
        [FromBody] ItemAcquireHistoryInfoRequest _,
        CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();

        var rows = await _db.ViewerAcquireHistory
            .Where(h => h.ViewerId == viewerId)
            .OrderByDescending(h => h.AcquireTime)
            .ThenByDescending(h => h.Id)
            .Take(InventoryHistoryConfig.RetentionRowsPerViewer)
            .AsNoTracking()
            .ToListAsync(ct);

        return new ItemAcquireHistoryInfoResponse
        {
            Histories = rows.Select(h => new ItemAcquireHistoryEntryDto
            {
                RewardType = h.RewardType.ToString(),
                RewardDetailId = h.RewardDetailId.ToString(),
                RewardCount = h.RewardCount.ToString(),
                AcquireType = h.AcquireType.ToString(),
                AcquireTime = h.AcquireTime.ToString(WireDateFormat, CultureInfo.InvariantCulture),
                Message = h.Message,
            }).ToList(),
        };
    }
}
