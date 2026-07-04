using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Tutorial;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Tutorial;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// Tutorial step bookkeeping. The tutorial itself runs entirely client-side
/// (StoryTutorial*BattleMgr per class); the server only persists step transitions.
/// </summary>
public class TutorialController : SVSimController
{
    private readonly SVSimDbContext _db;

    public TutorialController(SVSimDbContext db)
    {
        _db = db;
    }

    [HttpPost("update_action")]
    public IActionResult UpdateAction([FromBody] TutorialUpdateActionRequest request)
    {
        // Fire-and-forget. Client uses SkipAllNetworkChecks; response body is ignored.
        // We still emit an empty object so the translation middleware has a `data` payload to wrap.
        return new JsonResult(new { });
    }

    [HttpPost("update")]
    public async Task<ActionResult<TutorialUpdateResponse>> Update([FromBody] TutorialUpdateRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        var viewer = await _db.Viewers
            .Include(v => v.MissionData)
            .FirstAsync(v => v.Id == viewerId);

        // Preserve max — never regress. Mirrors GiftController.TutorialGiftReceive's 31→41 guard.
        // Without this, a stale or replayed request with tutorial_step=0 (or any value below the
        // viewer's current state) crashes the client on next /load/index: NextSceneSwitcher routes
        // step==0 to AreaSelect section 0, which has no chapter data → LINQ Single() failure.
        // Response keeps echoing request.TutorialStep so the client's own transition confirmation
        // still works; the client owns the step-it-thinks-it's-moving-to concept and we don't
        // want to surface a divergent value mid-flow.
        viewer.MissionData.TutorialState = Math.Max(viewer.MissionData.TutorialState, request.TutorialStep);
        await _db.SaveChangesAsync();

        return new TutorialUpdateResponse { TutorialStep = request.TutorialStep };
    }
}
