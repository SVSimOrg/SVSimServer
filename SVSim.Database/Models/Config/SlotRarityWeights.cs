namespace SVSim.Database.Models.Config;

/// <summary>
/// Per-rarity weights for a single pack slot. Sum should be ≤ 1.0;
/// remainder absorbs into Bronze via the PickRarity catch-all band.
/// <para>
/// <see cref="Slot"/> is the 1-based slot index as a string (e.g. "8") and is used as the
/// lookup key in <see cref="PackRateConfig.PerSlot"/>. It is null/empty for the global
/// <see cref="PackRateConfig.Default"/> entry, which has no slot affiliation.
/// </para>
/// </summary>
public class SlotRarityWeights
{
    /// <summary>1-based slot index (as a string) for entries in PerSlot. Null/empty for the Default entry.</summary>
    public string? Slot { get; set; }
    public double Bronze { get; set; }
    public double Silver { get; set; }
    public double Gold { get; set; }
    public double Legendary { get; set; }
}
