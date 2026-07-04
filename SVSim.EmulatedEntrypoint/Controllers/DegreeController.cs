using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Degree;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Degree;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /degree/* — viewer-scoped degree (title) management. Read, mark-favorite, set-currently-displayed.
/// Mirrors /sleeve/* and /emblem/*; favorite handler is no-op like the others until a viewer-favorites
/// schema lands (would also serve /sleeve/favorite + /emblem/favorite via the shared Wizard/FavoriteTask.cs Kind enum).
/// </summary>
[Route("degree")]
public class DegreeController : SVSimController
{
    private readonly SVSimDbContext _db;

    public DegreeController(SVSimDbContext db) => _db = db;

    [HttpPost("degree_list")]
    public async Task<ActionResult<DegreeListResponse>> DegreeList(DegreeListRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var viewer = await _db.Viewers
            .Include(v => v.Degrees)
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return Unauthorized();

        return new DegreeListResponse
        {
            UserDegreeList = viewer.Degrees
                .Select(d => new DegreeListEntry { DegreeId = d.Id })
                .OrderBy(e => e.DegreeId)
                .ToList(),
        };
    }

    [HttpPost("favorite")]
    public ActionResult<EmptyResponse> Favorite([FromBody] DegreeFavoriteRequest _)
    {
        if (!TryGetViewerId(out long __)) return Unauthorized();
        // Accept-and-ack. Persisting per-viewer cosmetic favorites is deferred until
        // a viewer-favorites schema lands (would also serve /sleeve/favorite + /emblem/favorite
        // via the shared Wizard/FavoriteTask.cs Kind enum).
        return new EmptyResponse();
    }

    [HttpPost("update_degree")]
    public async Task<ActionResult<EmptyResponse>> Update(DegreeUpdateRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var viewer = await _db.Viewers
            .Include(v => v.Degrees)
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return Unauthorized();

        var owned = viewer.Degrees.FirstOrDefault(d => d.Id == request.DegreeId);
        if (owned is null)
            return BadRequest(new { error = "degree_not_owned" });

        viewer.Info.SelectedDegree = owned;
        await _db.SaveChangesAsync(ct);
        return new EmptyResponse();
    }
}
