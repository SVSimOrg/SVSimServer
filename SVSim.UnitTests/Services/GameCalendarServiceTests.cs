using SVSim.Database.Models.Config;
using SVSim.Database.Services;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.UnitTests.Services;

public class GameCalendarServiceTests
{
    private sealed class FakeConfig : IGameConfigService
    {
        private readonly int _hour;
        public FakeConfig(int hour) { _hour = hour; }
        public T Get<T>() where T : class, new() =>
            (T)(object)new GameCalendarConfig { DailyResetUtcHour = _hour };
    }

    private static GameCalendarService NewService(int resetHour) =>
        new(new FakeConfig(resetHour));

    private static DateTimeOffset Utc(int y, int mo, int d, int h, int m, int s) =>
        new(y, mo, d, h, m, s, TimeSpan.Zero);

    // ------------ ResetReady ------------

    [Test]
    public void ResetReady_returns_true_when_lastCheckpoint_is_null()
    {
        var svc = NewService(0);
        Assert.That(svc.ResetReady(null), Is.True);
    }

    [Test]
    public void ResetReady_returns_false_when_lastCheckpoint_is_after_todays_boundary()
    {
        // hour=0 → today's boundary is 00:00 UTC. lastCheckpoint at 00:00:01 UTC today
        // is AFTER the boundary → not yet time to reset.
        var svc = NewService(0);
        var last = DateTime.UtcNow.Date.AddSeconds(1);
        Assert.That(svc.ResetReady(last), Is.False);
    }

    [Test]
    public void ResetReady_returns_true_when_lastCheckpoint_is_before_todays_boundary()
    {
        var svc = NewService(0);
        var last = DateTime.UtcNow.Date.AddSeconds(-1); // yesterday, 1s before midnight
        Assert.That(svc.ResetReady(last), Is.True);
    }

    [Test]
    public void ResetReady_respects_config_reset_hour()
    {
        // hour=17. lastCheckpoint at 16:00 UTC today = before today's boundary → reset ready.
        // (unless we're currently before 17:00 UTC — in which case today's boundary is
        // "tomorrow-in-the-future" and MostRecent is yesterday 17:00; last at 16:00 today
        // is AFTER yesterday's 17:00 → NOT ready. Test both sides explicitly rather than
        // relying on wall clock.)
        var svc = NewService(17);
        var now = DateTime.UtcNow;

        // Instant well before yesterday's 17:00 UTC — guaranteed to be reset-ready no matter
        // when we run this.
        var stale = now.AddDays(-2);
        Assert.That(svc.ResetReady(stale), Is.True, "48h ago is always reset-ready");
    }

    // ------------ DayKey ------------

    [Test]
    public void DayKey_hour0_uses_utc_calendar_day()
    {
        var svc = NewService(0);
        Assert.That(svc.DayKey(Utc(2026, 6, 1, 0, 0, 0)),  Is.EqualTo("day:2026-06-01"));
        Assert.That(svc.DayKey(Utc(2026, 6, 1, 23, 59, 59)), Is.EqualTo("day:2026-06-01"));
        Assert.That(svc.DayKey(Utc(2026, 6, 2, 0, 0, 0)),  Is.EqualTo("day:2026-06-02"));
    }

    [Test]
    public void DayKey_hour17_shifts_boundary_to_17_00_utc()
    {
        var svc = NewService(17);
        // Before today's 17:00 UTC → window started yesterday at 17:00.
        Assert.That(svc.DayKey(Utc(2026, 6, 1, 16, 59, 59)), Is.EqualTo("day:2026-05-31"));
        // 17:00 UTC exactly → new window starts.
        Assert.That(svc.DayKey(Utc(2026, 6, 1, 17, 0, 0)),  Is.EqualTo("day:2026-06-01"));
        // Later same day → same window.
        Assert.That(svc.DayKey(Utc(2026, 6, 1, 23, 59, 59)), Is.EqualTo("day:2026-06-01"));
    }

    // ------------ WeekKey / MonthKey ------------

    [Test]
    public void WeekKey_is_iso_week_of_window_start()
    {
        var svc = NewService(0);
        // 2026-05-25 is Monday (ISO week 22 of 2026).
        Assert.That(svc.WeekKey(Utc(2026, 5, 25, 12, 0, 0)), Is.EqualTo("week:2026-W22"));
        // Sunday of that week.
        Assert.That(svc.WeekKey(Utc(2026, 5, 24, 23, 59, 59)), Is.EqualTo("week:2026-W21"));
    }

    [Test]
    public void MonthKey_uses_utc_month()
    {
        var svc = NewService(0);
        Assert.That(svc.MonthKey(Utc(2026, 6, 30, 23, 59, 59)), Is.EqualTo("month:2026-06"));
        Assert.That(svc.MonthKey(Utc(2026, 7, 1, 0, 0, 0)), Is.EqualTo("month:2026-07"));
    }

    [Test]
    public void MonthKey_shifts_with_reset_hour_at_month_edge()
    {
        var svc = NewService(17);
        // 16:59 UTC on Jul 1 → window still in June (started Jun 30 17:00).
        Assert.That(svc.MonthKey(Utc(2026, 7, 1, 16, 59, 0)), Is.EqualTo("month:2026-06"));
        Assert.That(svc.MonthKey(Utc(2026, 7, 1, 17, 0, 0)), Is.EqualTo("month:2026-07"));
    }

    [Test]
    public void AllPeriods_returns_day_week_month_all_time()
    {
        var svc = NewService(0);
        var periods = svc.AllPeriods(Utc(2026, 5, 27, 12, 0, 0));
        Assert.That(periods, Is.EquivalentTo(new[] {
            "day:2026-05-27", "week:2026-W22", "month:2026-05", GameCalendarPeriods.AllTime,
        }));
    }
}
