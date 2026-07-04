using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One entry from /mypage/index data.room_type_in_session.special_deck_format_list. A list of deck-format
/// codes that have a current "special" window (e.g. format "5" valid until 2030-06-26). Id is a synthetic
/// ordinal — the wire has no explicit identifier, and ImportRoomTypeInSession follows the same clear-and-
/// rewrite pattern as ImportBanners.
/// </summary>
public class SpecialDeckFormatEntry : BaseEntity<int>
{
    /// <summary>Wire is string per prod's PHP convention even though it looks numeric (e.g. "5").</summary>
    public string DeckFormat { get; set; } = string.Empty;

    public DateTime EndTime { get; set; }
}
