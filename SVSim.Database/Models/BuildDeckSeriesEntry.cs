using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One prebuilt-deck series ("Structure Deck Set 7", "Trial 19", etc.). PK = wire series_id.
/// IsEnabled gates whether /build_deck/info renders this series — disabled rows are placeholder
/// stubs created from the client CSV until we capture a /info response that enriches them.
/// </summary>
public class BuildDeckSeriesEntry : BaseEntity<int>
{
    public int OrderIndex { get; set; }   // wire order_id; controls display order
    public string NameKey { get; set; } = string.Empty;      // BDSSN_*
    public string IntroKey { get; set; } = string.Empty;     // BDSI_*
    public string TitlePath { get; set; } = string.Empty;
    public string DrumrollPath { get; set; } = string.Empty;
    public bool IsNew { get; set; }
    public bool IsEnabled { get; set; }

    public List<BuildDeckSeriesRewardEntry> SeriesRewards { get; set; } = new();
    public List<BuildDeckProductEntry> Products { get; set; } = new();
}
