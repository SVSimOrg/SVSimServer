using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Repositories.Viewer;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Profile;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Profile;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /profile/* — viewer-scoped profile read endpoint. Surfaces total rank-match wins
/// and the per-class roster (level, exp, leader-skin selection).
/// </summary>
[Route("profile")]
public sealed class ProfileController : SVSimController
{
    private readonly IViewerRepository _viewerRepository;
    private readonly SVSimDbContext _db;

    public ProfileController(IViewerRepository viewerRepository, SVSimDbContext db)
    {
        _viewerRepository = viewerRepository;
        _db = db;
    }

    [HttpPost("index")]
    public async Task<ActionResult<ProfileIndexResponse>> Index(
        [FromBody] ProfileIndexRequest _,
        CancellationToken ct)
    {
        var shortUdidClaim = User.Claims.FirstOrDefault(c => c.Type == ShadowverseClaimTypes.ShortUdidClaim)?.Value;
        if (shortUdidClaim is null || !long.TryParse(shortUdidClaim, out long shortUdid))
            return Unauthorized();

        var viewer = await _viewerRepository.GetViewerByShortUdid(shortUdid);
        if (viewer is null) return NotFound();

        var skinsByClass = viewer.LeaderSkins
            .Where(s => s.ClassId.HasValue)
            .GroupBy(s => s.ClassId!.Value)
            .ToDictionary(g => g.Key, g => (IReadOnlyCollection<int>)g.Select(s => s.Id).ToList());

        var classes = viewer.Classes
            .Select(vc => new UserClass(
                vc,
                skinsByClass.GetValueOrDefault(vc.Class.Id, Array.Empty<int>())))
            .ToList();

        return new ProfileIndexResponse
        {
            // TODO: when rank-match results are tracked, compute from viewer's rank history.
            UserRankMatchTotalWin = 0,
            UserClassList = classes,
        };
    }

    [HttpPost("update_official_mark_display")]
    public async Task<ActionResult<EmptyResponse>> UpdateOfficialMarkDisplay(
        ProfileUpdateOfficialMarkDisplayRequest request,
        CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var viewer = await _db.Viewers.FirstAsync(v => v.Id == viewerId, ct);
        viewer.Info.IsOfficialMarkDisplayed = request.IsOfficialMarkDisplayed != 0;
        await _db.SaveChangesAsync(ct);

        return new EmptyResponse();
    }
}
