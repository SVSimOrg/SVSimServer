namespace SVSim.Database.Models.Config;

/// <summary>
/// Global "freeplay" toggle. When <see cref="Enabled"/>, every viewer is treated (in logic,
/// never in the DB) as owning all cards (<see cref="CardCopies"/> each), all cosmetics, and
/// <see cref="CurrencyAmount"/> of Crystal/Rupee/Red-Ether. See
/// docs/superpowers/specs/2026-05-29-freeplay-mode-design.md.
/// </summary>
[ConfigSection("Freeplay")]
public class FreeplayConfig
{
    public bool Enabled { get; set; } = false;
    public ulong CurrencyAmount { get; set; } = 99999;
    public int CardCopies { get; set; } = 3;

    public static FreeplayConfig ShippedDefaults() => new();
}
