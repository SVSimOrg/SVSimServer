namespace SVSim.Database.Models.Config;

/// <summary>
/// Time-varying season/rotation state, populated by RotationConfigImporter from seed files.
/// <see cref="RotationCardSetIds"/> drives <c>CardSet.IsInRotation</c> via RotationFlagUpdater.
/// </summary>
[ConfigSection("Rotation")]
public class RotationConfig
{
    public string TsRotationId { get; set; } = "";
    public bool IsBattlePassPeriod { get; set; }
    public bool IsBeginnerMission { get; set; }
    public int CardSetIdForResourceDlView { get; set; }
    public List<int> RotationCardSetIds { get; set; } = new();

    public static RotationConfig ShippedDefaults() => new();
}
