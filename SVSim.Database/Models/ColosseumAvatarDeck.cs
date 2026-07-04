using System.ComponentModel.DataAnnotations.Schema;

namespace SVSim.Database.Models;

/// <summary>
/// Curated Avatar (themed-character) deck for Arena Colosseum. See
/// <see cref="ColosseumHofDeck"/> for the rationale on the duplicated schema.
/// </summary>
public class ColosseumAvatarDeck : IColosseumCuratedDeck
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
