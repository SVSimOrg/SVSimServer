namespace SVSim.Database.Models.Config;

/// <summary>
/// Reset boundaries for time-windowed game state (daily missions, daily-single packs,
/// login bonuses). All timestamps in the codebase are UTC; this config only shifts
/// where the day boundary falls.
/// </summary>
[ConfigSection("GameCalendar")]
public class GameCalendarConfig
{
    /// <summary>
    /// UTC hour (0-23) at which the daily reset occurs. Default 0 = midnight UTC.
    /// Prod parity: 17 (= 02:00 JST). Changing the value at runtime is safe for the
    /// ResetReady check but retroactively misaligns any DB rows keyed by DayKey /
    /// WeekKey / MonthKey — treat as a startup constant in production.
    /// </summary>
    public int DailyResetUtcHour { get; set; } = 0;

    public static GameCalendarConfig ShippedDefaults() => new();
}
