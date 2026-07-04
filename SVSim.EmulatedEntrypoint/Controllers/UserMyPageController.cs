using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Models.Dtos.UserMyPage;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /user_mypage/* — viewer-scoped MyPage configuration writes. Separate from the
/// <c>/mypage/*</c> family because the wire URL family is distinct.
/// </summary>
[Route("user_mypage")]
public sealed class UserMyPageController : SVSimController
{
    private readonly SVSimDbContext _db;

    public UserMyPageController(SVSimDbContext db) => _db = db;

    [HttpPost("update")]
    public async Task<ActionResult<UserMyPageUpdateResponse>> Update(
        [FromBody] UserMyPageUpdateRequest request,
        CancellationToken ct)
    {
        if (!TryGetViewerId(out var viewerId)) return Unauthorized();

        var viewer = await _db.Viewers
            .Include(v => v.MyPageBgRotation)
            .FirstOrDefaultAsync(v => v.Id == viewerId, ct);
        if (viewer is null) return NotFound();

        viewer.MyPageBgSelectType = request.SelectType;
        viewer.MyPageBgId = ParseIdOrZero(request.MyPageId);

        // Clear() on a loaded OwnsMany marks every tracked entry as Deleted; SaveChangesAsync
        // issues DELETEs for all old slots before inserting the new ones.
        viewer.MyPageBgRotation.Clear();
        for (int slot = 0; slot < request.MyPageIdList.Count; slot++)
        {
            viewer.MyPageBgRotation.Add(new MyPageBgRotationEntry
            {
                Slot = slot,
                BgId = ParseIdOrZero(request.MyPageIdList[slot]),
            });
        }

        await _db.SaveChangesAsync(ct);
        return new UserMyPageUpdateResponse();
    }

    private static int ParseIdOrZero(string s) =>
        int.TryParse(s, out var n) ? n : 0;
}
