using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Models.Dtos.Campaign;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /campaign/* — promotional surfaces. Currently just <c>regist_serial_code</c>.
/// </summary>
[Route("campaign")]
public sealed class CampaignController : SVSimController
{
    private const int FailureResultCode = 4202;

    private readonly SVSimDbContext _db;

    public CampaignController(SVSimDbContext db) => _db = db;

    [HttpPost("regist_serial_code")]
    public async Task<IActionResult> RegisterSerialCode(
        [FromBody] RegisterSerialCodeRequest request,
        CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();

        var now = DateTime.UtcNow;

        var code = await _db.SerialCodes
            .Include(c => c.Rewards)
            .FirstOrDefaultAsync(c => c.Code == request.SerialCode, ct);

        if (code is null) return Fail();
        if (!code.IsEnabled) return Fail();
        if (code.StartAt is { } start && start > now) return Fail();
        if (code.EndAt is { } end && end < now) return Fail();

        bool alreadyRedeemed = await _db.ViewerSerialCodeRedemptions
            .AnyAsync(r => r.ViewerId == viewerId && r.SerialCodeId == code.Id, ct);
        if (alreadyRedeemed) return Fail();

        if (code.Rewards.Any(r => !GiftRewardTypes.IsSupported(r.RewardType))) return Fail();

        try
        {
            _db.ViewerSerialCodeRedemptions.Add(new ViewerSerialCodeRedemption
            {
                ViewerId = viewerId,
                SerialCodeId = code.Id,
                RedeemedAt = now,
            });

            foreach (var reward in code.Rewards.OrderBy(r => r.Slot))
            {
                _db.ViewerPresents.Add(new ViewerPresent
                {
                    ViewerId = viewerId,
                    PresentId = Guid.NewGuid().ToString("N").Substring(0, 16),
                    Status = PresentStatus.Unclaimed,
                    RewardType = reward.RewardType,
                    RewardDetailId = reward.RewardDetailId,
                    RewardCount = reward.RewardCount,
                    Message = code.Message,
                    CreatedAt = now,
                    Source = $"serial_code:{code.Id}",
                });
            }

            await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Race: two concurrent redeems for the same (viewer, code). The composite PK
            // on ViewerSerialCodeRedemption rejects the second one; treat as already-redeemed.
            return Fail();
        }

        return Ok(new RegisterSerialCodeResponse { IsComplete = true });
    }

    private IActionResult Fail() => Ok(new { result_code = FailureResultCode });

}
