namespace SVSim.Database.Models;

/// <summary>
/// Per-viewer "how many times has this happened" counter. Composite PK
/// (ViewerId, EventKey, Period). Period strings: "all-time", "month:YYYY-MM",
/// "week:YYYY-W##", "day:YYYY-MM-DD" — all UTC, with the reset-window boundary
/// set by <c>GameCalendarConfig.DailyResetUtcHour</c>.
/// Single source of truth for total_count / done_number on every wire shape.
/// </summary>
public class ViewerEventCounter
{
    public long ViewerId { get; set; }
    public string EventKey { get; set; } = "";
    public string Period { get; set; } = "";
    public int Count { get; set; }
}
