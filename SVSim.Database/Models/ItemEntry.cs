using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Item master row. Mirrors the client's <c>item_master.csv</c> + <c>itemtext.json</c>
/// (under <c>data_dumps/client-assets/</c>): <see cref="Type"/> matches the client-side
/// item_type enum (1 = challenge ticket, 2 = card-pack ticket, 3 = premium orb,
/// 4 = colosseum ticket, 5 = orb piece, 6 = skin/event ticket, 7 = other);
/// <see cref="ThumbnailPath"/> is the client-resolved sprite key.
/// </summary>
public class ItemEntry : BaseEntity<int>
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Client-side item_type enum (1-7). Drives shop categorisation, e.g.
    /// <c>user_card_pack_ticket_list</c> in /item_purchase/info filters on Type == 2.</summary>
    public int Type { get; set; }

    /// <summary>Sprite key, e.g. <c>"ticket_10032"</c>. Empty when unknown.</summary>
    public string ThumbnailPath { get; set; } = string.Empty;
}