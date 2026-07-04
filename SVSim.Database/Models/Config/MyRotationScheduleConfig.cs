namespace SVSim.Database.Models.Config;

/// <summary>
/// Window-based schedule for the Custom Rotation (a.k.a. MyRotation) feature. Two parallel windows:
/// <c>Gathering</c> (deck-building period) and <c>FreeBattle</c> (active play period). The client
/// gates the format-selector button on these windows — see Wizard/MyRotationAllInfo.cs:45
/// (<c>IsMyRotationEnable =&gt; IsWithinPeriod(FreeMatchPeriod)</c>) and Wizard/DeckListUI.cs:92.
/// Mapped to the wire-shape <c>SpecialRotationSchedule</c> at the controller seam.
/// <para>
/// Shipped defaults reproduce the 2026-05-23 prod capture so a fresh install ships with the
/// feature enabled. RotationConfigImporter overwrites the DB section from any newer seed.
/// </para>
/// </summary>
[ConfigSection("MyRotationSchedule")]
public class MyRotationScheduleConfig
{
    public ScheduleWindow Gathering { get; set; } = new()
    {
        Begin = new DateTime(2024, 5, 1, 20, 0, 0, DateTimeKind.Utc),
        End = new DateTime(2030, 6, 26, 19, 59, 59, DateTimeKind.Utc),
    };

    public ScheduleWindow FreeBattle { get; set; } = new()
    {
        Begin = new DateTime(2024, 5, 1, 20, 0, 0, DateTimeKind.Utc),
        End = new DateTime(2030, 6, 26, 19, 59, 59, DateTimeKind.Utc),
    };

    public static MyRotationScheduleConfig ShippedDefaults() => new();
}

public class ScheduleWindow
{
    public DateTime Begin { get; set; }
    public DateTime End { get; set; }
}
