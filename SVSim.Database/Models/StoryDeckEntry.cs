using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// Presentation metadata for a story-mode prebuilt/trial deck, as surfaced under
/// main_story/get_deck_list's build_deck_list / trial_deck_list. PK (DeckNo) equals the deck's
/// wire deck_no, which also equals BuildDeckProductEntry.Id — the 40-card list is read from that
/// product (single source of truth), NOT stored here. Sourced from
/// data_dumps/captures/traffic_prod_trial_decks.ndjson via seeds/story-decks.json.
/// </summary>
public class StoryDeckEntry : BaseEntity<int>
{
    public int DeckNo { get => Id; set => Id = value; }   // == BuildDeckProductEntry.Id

    public StoryDeckKind Kind { get; set; }
    public int ClassId { get; set; }
    public string DeckName { get; set; } = string.Empty;
    public int SleeveId { get; set; }
    public int LeaderSkinId { get; set; }
    public int IsRecommend { get; set; }
    public int OrderNum { get; set; }
    public int EntryNo { get; set; }

    /// <summary>Trial decks carry a deck_format on the wire; build decks do not (null).</summary>
    public int? DeckFormat { get; set; }
}
