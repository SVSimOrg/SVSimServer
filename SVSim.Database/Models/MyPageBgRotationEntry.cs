using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// One row per (viewer, slot) in the viewer's saved MyPage BG rotation pool. The client posts
/// the full pool on every <c>/user_mypage/update</c> regardless of mode, so the server overwrites
/// it atomically each time. Slot is the 0-based position; order is preserved for the
/// <c>/mypage/index</c> echo.
/// </summary>
[Owned]
public class MyPageBgRotationEntry
{
    public int Slot { get; set; }
    public int BgId { get; set; }
}
