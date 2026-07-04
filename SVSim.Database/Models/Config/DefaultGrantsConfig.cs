namespace SVSim.Database.Models.Config;

/// <summary>Per-viewer-registration default currency grants.</summary>
[ConfigSection("DefaultGrants")]
public class DefaultGrantsConfig
{
    public ulong Crystals { get; set; } = 0;
    public ulong Rupees { get; set; } = 0;
    public ulong Ether { get; set; } = 0;

    public static DefaultGrantsConfig ShippedDefaults() => new();
}
