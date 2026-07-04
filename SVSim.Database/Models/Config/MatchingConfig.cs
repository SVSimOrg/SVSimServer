namespace SVSim.Database.Models.Config;

/// <summary>
/// Tunables for the in-process pair-up matching service. Today: just the AI-fallback
/// threshold for rank-battle modes. The full matching-queue API is a separate spec;
/// this config section lives alongside the placeholder.
/// </summary>
[ConfigSection("Matching")]
public class MatchingConfig
{
    /// <summary>
    /// How long (seconds) a viewer must have been parked in a PvpFirstThenAiFallback
    /// queue before their next /do_matching poll resolves to an AI battle.
    /// Defaults to 15 — matches the prod 4s pre-AIBattleStart pause plus a comfortable
    /// polling cycle.
    /// </summary>
    public int RankBattleAiFallbackThresholdSeconds { get; set; } = 15;

    public static MatchingConfig ShippedDefaults() => new();
}
