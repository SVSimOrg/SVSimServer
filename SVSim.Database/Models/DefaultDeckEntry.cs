using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Starter / "use default" deck definition from /deck/info data.default_deck_list.
/// CardIdArray is the wire's int[] of 40 card_id values; stored as jsonb to keep it array-shaped.
/// </summary>
public class DefaultDeckEntry : BaseEntity<int>
{
    public int DeckNo { get => Id; set => Id = value; }

    public int ClassId { get; set; }

    public long SleeveId { get; set; }

    public int LeaderSkinId { get; set; }

    public string DeckName { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string CardIdArray { get; set; } = "[]";
}
