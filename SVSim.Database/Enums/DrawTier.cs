namespace SVSim.Database.Enums;

/// <summary>
/// Per-draw page tier the slot rolls into. Distinct from card-master <see cref="Rarity"/>:
/// for the four base values they line up, but <c>Special</c> covers the per-pack
/// "Leader Card" / "Limited-Time Leader" tiers — its cards are typically Rarity.Legendary
/// with the IsLeader printing flag set.
/// </summary>
public enum DrawTier
{
    Bronze = 0,
    Silver = 1,
    Gold = 2,
    Legendary = 3,
    Special = 4,
}
