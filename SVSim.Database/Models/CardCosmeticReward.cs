using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// Association: when a viewer acquires <see cref="CardId"/>, they should also receive
/// the cosmetic identified by (<see cref="Type"/>, <see cref="CosmeticId"/>) if they don't
/// already own it.
///
/// Always recorded on the NON-FOIL row of a card. Foil twins (card_id + 1) inherit at
/// lookup time — see CardAcquisitionService for the foil-resolution rule.
///
/// Composite PK on (CardId, Type, CosmeticId): naturally enforces "no duplicates" AND
/// satisfies EF's deterministic-PK requirement for HasData seeding.
/// </summary>
public class CardCosmeticReward
{
    public long CardId { get; set; }
    public CosmeticType Type { get; set; }
    public long CosmeticId { get; set; }
    public int Quantity { get; set; } = 1;

    public ShadowverseCardEntry Card { get; set; } = null!;
}
