using SVSim.Database.Models;

namespace SVSim.Database.Repositories.PackDrawTables;

/// <summary>
/// All draw data for a single pack: per-pack config + slot rates + per-card weights.
/// Loaded as one unit by <see cref="IPackDrawTableRepository.GetAsync"/>.
/// </summary>
public sealed class PackDrawTable
{
    public required PackDrawConfigEntry Config { get; init; }
    public required IReadOnlyList<PackDrawSlotRateEntry> SlotRates { get; init; }
    public required IReadOnlyList<PackDrawCardWeightEntry> CardWeights { get; init; }
}
