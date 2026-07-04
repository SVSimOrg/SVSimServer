using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Emblem;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Emblem;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /emblem/* — viewer-scoped emblem (lobby/profile badge) management. Read, mark-favorite,
/// set-currently-displayed. Mirrors /sleeve/* and /degree/*.
/// </summary>
[Route("emblem")]
public class EmblemController : SVSimController
{
    private readonly SVSimDbContext _db;

    public EmblemController(SVSimDbContext db) => _db = db;

    [HttpPost("emblem_list")]
    public async Task<ActionResult<EmblemListResponse>> EmblemList(BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var viewer = await _db.Viewers
            .Include(v => v.Emblems)
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return Unauthorized();

        return new EmblemListResponse
        {
            UserEmblemList = viewer.Emblems
                .Select(e => new EmblemListEntry { EmblemId = e.Id })
                .OrderBy(x => x.EmblemId)
                .ToList(),
        };
    }

    [HttpPost("favorite")]
    public ActionResult<EmptyResponse> Favorite([FromBody] EmblemFavoriteRequest _)
    {
        if (!TryGetViewerId(out long __)) return Unauthorized();
        // Accept-and-ack. Persisting per-viewer cosmetic favorites is deferred until
        // a viewer-favorites schema lands (would also serve /sleeve/favorite + /degree/favorite
        // via the shared Wizard/FavoriteTask.cs Kind enum).
        return new EmptyResponse();
    }

    [HttpPost("update_emblem")]
    public async Task<ActionResult<EmptyResponse>> Update(EmblemUpdateRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var viewer = await _db.Viewers
            .Include(v => v.Emblems)
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return Unauthorized();

        // EmblemEntry.Id is int; cast the wire long once at the boundary.
        var owned = viewer.Emblems.FirstOrDefault(e => e.Id == (int)request.EmblemId);
        if (owned is null)
            return BadRequest(new { error = "emblem_not_owned" });

        viewer.Info.SelectedEmblem = owned;
        await _db.SaveChangesAsync(ct);
        return new EmptyResponse();
    }
}
