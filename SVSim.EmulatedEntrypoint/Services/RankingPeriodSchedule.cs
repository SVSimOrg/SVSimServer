using System.Globalization;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Pure deterministic monthly period schedule for /ranking/*. Each ranking family
/// (RankMatch, MasterPoint, TwoPick, Sealed) launched in a different month on the
/// live server; id=1 in each family is its launch month. id=current means "this
/// month in JST." The generator returns descending-by-id (newest first).
///
/// Anchor dates derived from prod capture 2026-06-09 17:00 UTC:
///   RankMatch current id = 122 → launch month = 2026-06 minus 121 months = 2016-05
///   MasterPoint current id = 120 → 2016-07
///   TwoPick current id = 119 → 2016-08
///   Sealed current id = 62 → 2021-05
///
/// See docs/superpowers/specs/2026-06-10-ranking-stubs-design.md for rationale.
/// </summary>
public static class RankingPeriodSchedule
{
    public enum Family { RankMatch, MasterPoint, TwoPick, Sealed }

    // (Year, Month) of each family's id=1 month, JST.
    private static readonly Dictionary<Family, (int Year, int Month)> FamilyAnchors = new()
    {
        [Family.RankMatch]   = (2016, 5),
        [Family.MasterPoint] = (2016, 7),
        [Family.TwoPick]     = (2016, 8),
        [Family.Sealed]      = (2021, 5),
    };

    private static readonly TimeZoneInfo Jst = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");

    public static IReadOnlyList<PeriodEntry> GenerateFor(Family family, DateTime nowUtc)
    {
        var nowJst = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, Jst);
        var anchor = FamilyAnchors[family];
        int currentId = MonthsBetweenInclusive(anchor.Year, anchor.Month, nowJst.Year, nowJst.Month);
        if (currentId < 1) return Array.Empty<PeriodEntry>();

        var result = new List<PeriodEntry>(currentId);
        for (int id = currentId; id >= 1; id--)
        {
            result.Add(BuildEntry(family, id, anchor));
        }
        return result;
    }

    public static PeriodEntry? TryFindById(Family family, int periodId, DateTime nowUtc)
    {
        if (periodId < 1) return null;
        var nowJst = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, Jst);
        var anchor = FamilyAnchors[family];
        int currentId = MonthsBetweenInclusive(anchor.Year, anchor.Month, nowJst.Year, nowJst.Month);
        if (periodId > currentId) return null;
        return BuildEntry(family, periodId, anchor);
    }

    private static PeriodEntry BuildEntry(Family family, int id, (int Year, int Month) anchor)
    {
        // id=1 is the anchor month; id=N is anchor month + (N-1) months.
        int totalMonths = (anchor.Year * 12 + (anchor.Month - 1)) + (id - 1);
        int year = totalMonths / 12;
        int month = (totalMonths % 12) + 1;

        var begin = new DateTime(year, month, 1, 2, 0, 0);
        // End = first day of next month at 02:00:00 minus 1 second = "YYYY-MM+1-01 01:59:59"
        var end = begin.AddMonths(1).AddSeconds(-1);

        int periodNum = family switch
        {
            // Captured offsets: RankMatch period_num = id - 1; MasterPoint period_num = id - 1.
            // (Capture frame 64 shows rank_match[0] = { id:122, period_num:121 } and
            //  master_point[0] = { id:120, period_num:119 }, two_pick[0] = { id:119, period_num:119 }.)
            Family.RankMatch   => id - 1,
            Family.MasterPoint => id - 1,
            _                  => id,
        };

        return new PeriodEntry(
            Id: id.ToString(CultureInfo.InvariantCulture),
            PeriodNum: periodNum.ToString(CultureInfo.InvariantCulture),
            BeginTime: begin.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            EndTime: end.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
    }

    private static int MonthsBetweenInclusive(int startYear, int startMonth, int endYear, int endMonth)
        => (endYear - startYear) * 12 + (endMonth - startMonth) + 1;
}

public sealed record PeriodEntry(
    string Id,
    string PeriodNum,
    string BeginTime,
    string EndTime);
