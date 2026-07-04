namespace SVSim.Database.Models.Config;

/// <summary>
/// Default cosmetic loadout ids for a newly-registered viewer. Untyped longs in the jsonb tree
/// (FK validation would live in a future config-editing UI — see TODO(config-validation)).
/// </summary>
[ConfigSection("DefaultLoadout")]
public class DefaultLoadoutConfig
{
    public int DegreeId { get; set; } = 300003;
    public int EmblemId { get; set; } = 100000000;
    public int MyPageBackgroundId { get; set; } = 100000000;
    public int SleeveId { get; set; } = 3000011;

    public static DefaultLoadoutConfig ShippedDefaults() => new();
}
