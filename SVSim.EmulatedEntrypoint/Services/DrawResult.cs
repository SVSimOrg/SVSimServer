using SVSim.Database.Enums;

namespace SVSim.EmulatedEntrypoint.Services;

public record DrawnCard(long CardId, Rarity Rarity);

public record DrawResult(IReadOnlyList<DrawnCard> Cards);
