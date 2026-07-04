namespace SVSim.Database.Models.Config;

/// <summary>
/// Story-family placeholder config section. Class XP per clear moved to
/// <see cref="BattleXpConfig.StoryXpPerClear"/> (BattleXpMode.Story) as part of the
/// unified per-mode XP surface. Kept as an empty section so future story-specific
/// knobs (dialogue speed, auto-skip, etc.) have a home.
/// </summary>
[ConfigSection("Story")]
public class StoryConfig
{
    public static StoryConfig ShippedDefaults() => new();
}
