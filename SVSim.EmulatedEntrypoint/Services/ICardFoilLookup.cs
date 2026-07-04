using SVSim.Database.Models;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Looks up the foil printing of a card. The card-master convention is foil_id = base_id + 1
/// with the IsFoil flag set; leader-card / alt-art printings typically have no foil twin
/// (TryGetFoilTwin returns null and the sampler silently keeps the base).
/// </summary>
public interface ICardFoilLookup
{
    ShadowverseCardEntry? TryGetFoilTwin(long baseCardId);
}
