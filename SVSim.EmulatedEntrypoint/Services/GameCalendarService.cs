using System.Globalization;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Reset-boundary primitive. All timestamps are UTC. The boundary hour is a config
/// value (<see cref="GameCalendarConfig.DailyResetUtcHour"/>); everything else — daily
/// reset checks, bucket keys for <c>ViewerEventCounters</c> — derives from it.
/// Because bucket keys and <see cref="ResetReady"/> share the same
/// <see cref="MostRecentBoundary"/> math, they can never disagree.
/// </summary>
public interface IGameCalendarService
{
    /// <summary>
    /// True if a reset boundary has passed since <paramref name="lastCheckpointUtc"/>.
    /// Null → true (never checked in).
    /// </summary>
    bool ResetReady(DateTime? lastCheckpointUtc);

    /// <summary>Bucket key for the current daily reset window: <c>"day:yyyy-MM-dd"</c>.</summary>
    string DayKey(DateTimeOffset when);

    /// <summary>Bucket key for the ISO week containing the current daily window's start.</summary>
    string WeekKey(DateTimeOffset when);

    /// <summary>Bucket key for the calendar month containing the current daily window's start.</summary>
    string MonthKey(DateTimeOffset when);

    /// <summary>Returns [day, week, month, all-time] for the given instant.</summary>
    IReadOnlyList<string> AllPeriods(DateTimeOffset when);
}

public static class GameCalendarPeriods
{
    /// <summary>The lifetime bucket string — same constant regardless of reset boundary config.</summary>
    public const string AllTime = "all-time";
}

public class GameCalendarService : IGameCalendarService
{
    private readonly IGameConfigService _config;

    public GameCalendarService(IGameConfigService config)
    {
        _config = config;
    }

    public bool ResetReady(DateTime? lastCheckpointUtc)
    {
        if (lastCheckpointUtc is null) return true;
        var now = DateTime.UtcNow;
        var boundary = MostRecentBoundary(now);
        return DateTime.SpecifyKind(lastCheckpointUtc.Value, DateTimeKind.Utc) < boundary;
    }

    public string DayKey(DateTimeOffset when)
    {
        var d = WindowStartDate(when);
        return $"day:{d:yyyy-MM-dd}";
    }

    public string WeekKey(DateTimeOffset when)
    {
        var d = WindowStartDate(when);
        var iso = ISOWeek.GetWeekOfYear(d);
        var year = ISOWeek.GetYear(d);
        return $"week:{year:D4}-W{iso:D2}";
    }

    public string MonthKey(DateTimeOffset when)
    {
        var d = WindowStartDate(when);
        return $"month:{d:yyyy-MM}";
    }

    public IReadOnlyList<string> AllPeriods(DateTimeOffset when) => new[]
    {
        DayKey(when), WeekKey(when), MonthKey(when), GameCalendarPeriods.AllTime,
    };

    private DateTime MostRecentBoundary(DateTime nowUtc)
    {
        int hour = _config.Get<GameCalendarConfig>().DailyResetUtcHour;
        var todayBoundary = nowUtc.Date.AddHours(hour);
        return nowUtc >= todayBoundary ? todayBoundary : todayBoundary.AddDays(-1);
    }

    private DateTime WindowStartDate(DateTimeOffset when) =>
        MostRecentBoundary(when.UtcDateTime);
}
