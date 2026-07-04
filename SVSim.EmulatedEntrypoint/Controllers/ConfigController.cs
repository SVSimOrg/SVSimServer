using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /config/* — viewer-scoped preference toggles. Persists to ViewerInfo; reads back through
/// /load/index + /mypage/index user_config.
/// </summary>
public class ConfigController : SVSimController
{
    private readonly SVSimDbContext _db;

    public ConfigController(SVSimDbContext db) => _db = db;

    [HttpPost("update_foil_preferred")]
    public async Task<ActionResult<EmptyResponse>> UpdateFoilPreferred(
        ConfigUpdateFoilPreferredRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var viewer = await _db.Viewers.FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return Unauthorized();
        viewer.Info.IsFoilPreferred = request.IsFoilPreferred != 0;
        await _db.SaveChangesAsync(ct);
        return new EmptyResponse();
    }

    [HttpPost("update_prize_preferred")]
    public async Task<ActionResult<EmptyResponse>> UpdatePrizePreferred(
        ConfigUpdatePrizePreferredRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var viewer = await _db.Viewers.FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return Unauthorized();
        viewer.Info.IsPrizePreferred = request.IsPrizePreferred != 0;
        await _db.SaveChangesAsync(ct);
        return new EmptyResponse();
    }

    [HttpPost("update")]
    public async Task<ActionResult<EmptyResponse>> Update(
        ConfigUpdateRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var viewer = await _db.Viewers.FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return Unauthorized();
        viewer.Info.IsSkipGachaEffect = request.IsSkipGachaEffect != 0;
        await _db.SaveChangesAsync(ct);
        return new EmptyResponse();
    }

    [HttpPost("update_challenge_config")]
    public async Task<ActionResult<EmptyResponse>> UpdateChallengeConfig(
        ConfigUpdateChallengeConfigRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var viewer = await _db.Viewers.FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return Unauthorized();
        viewer.Info.UseChallengeTwoPickPremiumCard = request.UseChallengeTwoPickPremiumCard != 0;
        viewer.Info.ChallengeTwoPickSleeveId = request.ChallengeTwoPickSleeveId;
        await _db.SaveChangesAsync(ct);
        return new EmptyResponse();
    }
}
