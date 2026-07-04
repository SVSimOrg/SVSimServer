using System.ComponentModel.DataAnnotations.Schema;

namespace SVSim.Database.Models;

/// <summary>
/// Curated Hall-of-Fame deck pool for Arena Colosseum. Identical schema to
/// <see cref="ColosseumWindFallDeck"/> and <see cref="ColosseumAvatarDeck"/> — separate
/// tables instead of an enum-discriminated shared one because the operational lifecycle
/// (per-pool importer, per-pool register endpoint, per-pool query) is independent.
/// </summary>
public class ColosseumHofDeck : IColosseumCuratedDeck
{
    public long Id { get; set; }
    public int DeckNo { get; set; }
    public int ClassId { get; set; }

    [Column(TypeName = "jsonb")]
    public string CardListJson { get; set; } = "[]";

    public long SleeveId { get; set; }
    public long LeaderSkinId { get; set; }
    public int DisplayOrder { get; set; }
}
