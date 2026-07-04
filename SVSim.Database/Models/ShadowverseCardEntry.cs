using System.ComponentModel.DataAnnotations;
using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

public class ShadowverseCardEntry : BaseEntity<long>
{
    /// <summary>
    /// The internal name of this card (not the localized display name).
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Attack stat (atk on the wire).
    /// </summary>
    public int? Attack { get; set; }

    /// <summary>
    /// Life / defense stat (life on the wire).
    /// </summary>
    public int? Defense { get; set; }

    /// <summary>
    /// Play cost (cost on the wire).
    /// </summary>
    public int? PrimaryResourceCost { get; set; }

    /// <summary>
    /// The rarity of this card.
    /// </summary>
    public Rarity Rarity { get; set; }

    /// <summary>
    /// True for foil/animated card rows (cards.json `is_foil=1`). Foils live in the same
    /// CardSet as their non-foil twin (twin's card_id = this.Id - 1). Excluded from pack
    /// draw pools by DbCardPoolProvider; reached via the per-card animated-upgrade roll
    /// in PackOpenService.
    /// </summary>
    public bool IsFoil { get; set; }

    #region Owned

    /// <summary>
    /// Info about this card in the collection, if it can be collected.
    /// </summary>
    public CardCollectionInfo? CollectionInfo { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// The class this card belongs to, or null for neutral cards.
    /// </summary>
    public ClassEntry? Class { get; set; }

    #endregion
}
