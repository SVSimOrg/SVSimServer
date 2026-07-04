namespace SVSim.Database.Models.Config;

/// <summary>
/// Tunables for pack-opening RNG. Property initialisers reproduce the original Shadowverse
/// Classic main-slot rates exactly. Collection-shaped defaults (slot-8 PerSlot entry) live in
/// <see cref="ShippedDefaults"/>, not in the initialiser — see PerSlot docstring.
/// </summary>
[ConfigSection("PackRates")]
[Obsolete("PackRateConfig is no longer consulted by PackOpenService — per-pack rates come from PackDrawTable. Retire once v1 stabilizes.")]
public class PackRateConfig
{
    /// <summary>
    /// Per-card-slot probability of upgrading any drawn card to its foil/animated twin.
    /// Applied AFTER rarity selection — independent of rarity, slot position, and pack category.
    /// Default 0.08 (8%). Cards without a foil twin in master data keep the non-foil silently.
    /// </summary>
    public double AnimatedRate { get; set; } = 0.08;

    /// <summary>
    /// Global default rarity weights, used for any slot that has no entry in
    /// <see cref="PerSlot"/>. Defaults match SV Classic main-slot. Weights sum to 0.9994;
    /// the 0.06% slack absorbs into Bronze via the PickRarity catch-all band.
    /// </summary>
    public SlotRarityWeights Default { get; set; } = new()
    {
        Bronze = 0.6744, Silver = 0.25, Gold = 0.06, Legendary = 0.015,
    };

    /// <summary>
    /// Per-slot overrides keyed by 1-based slot index (stored as a list for json compatibility —
    /// Dictionary&lt;string,T&gt; of complex owned types is not supported). Look up by
    /// <see cref="SlotRarityWeights.Slot"/>. A missing slot falls back to <see cref="Default"/>.
    /// Each entry is a FULL OVERRIDE, not a delta — if you change <see cref="Default"/>, existing
    /// PerSlot entries do NOT auto-recompute.
    /// <para>
    /// MUST default to empty. The original EF Core 8 <c>OwnsMany</c>+<c>ToJson</c> path APPENDED
    /// jsonb rows onto whatever collection the parent's parameterless ctor produced — a non-empty
    /// initialiser here meant every config load doubled-up and the original seed silently won the
    /// <c>FirstOrDefault</c> lookup in <c>PackOpenService.ResolveWeights</c>. The EF path is gone
    /// now (config goes through <c>IGameConfigService</c> + STJ), but the rule stays: collection
    /// defaults live in <see cref="ShippedDefaults"/>, not in property initialisers.
    /// </para>
    /// </summary>
    public List<SlotRarityWeights> PerSlot { get; set; } = [];

    /// <summary>
    /// Canonical SV Classic shipped defaults — what an operator gets if neither the DB nor
    /// appsettings.json supplies a PackRates section. Source of truth for the fresh-install seeder
    /// and the <c>IGameConfigService</c> inline-default tier.
    /// </summary>
    public static PackRateConfig ShippedDefaults() => new()
    {
        PerSlot =
        {
            new SlotRarityWeights { Slot = "8", Bronze = 0, Silver = 0.7692, Gold = 0.1846, Legendary = 0.0462 },
        },
    };
}
