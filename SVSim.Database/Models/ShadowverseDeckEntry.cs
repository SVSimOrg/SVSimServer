using System.ComponentModel.DataAnnotations;
using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

public class ShadowverseDeckEntry : BaseEntity<Guid>
{
    /// <summary>
    /// Internal deck name.
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Cards in this deck.
    /// </summary>
    public List<DeckCard> Cards { get; set; } = new List<DeckCard>();

    public int Number { get; set; }
    public Format Format { get; set; }
    public bool RandomLeaderSkin { get; set; }

    /// <summary>
    /// MyRotation period id (key into <see cref="MyRotationSettingEntry"/>). Required when
    /// <see cref="Format"/> is <see cref="Format.MyRotation"/> so the client can resolve the
    /// deck's pack range; null for every other format. If null on a MyRotation deck, clicking
    /// the deck NREs inside DeckData.CreateMyRotationClassName (info.LastPackText on null).
    /// </summary>
    public string? MyRotationId { get; set; }

    #region Navigation Properties

    public ClassEntry Class { get; set; } = new ClassEntry();

    public SleeveEntry Sleeve { get; set; } = new SleeveEntry();

    public LeaderSkinEntry LeaderSkin { get; set; } = new LeaderSkinEntry();

    #endregion
}
