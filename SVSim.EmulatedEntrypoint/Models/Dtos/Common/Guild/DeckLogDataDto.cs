using MessagePack;
using SVSim.Database.Enums;
using SVSim.EmulatedEntrypoint.Extensions;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

/// <summary>
/// Wire shape for a single entry in the deck_log bucket array, and for the inline
/// `deck` object on a DECK-type chat message. Matches ChatMessageInfo.DeckLogData
/// shape (Wizard/ChatMessageInfo.cs:DeckLogData).
///
/// Keys are a strict subset of what DeckData.Initialize() consumes:
///   deck_format, message_id, delete_permission_exists, deck_no, deck_name,
///   class_id, card_id_array, sleeve_id, leader_skin_id.
/// </summary>
[MessagePackObject]
public class DeckLogDataDto
{
    /// <summary>API-side Format integer (via FormatJsonConverter).</summary>
    [JsonPropertyName("deck_format"), Key("deck_format")]
    [JsonConverter(typeof(FormatJsonConverter))]
    public Format DeckFormat { get; set; }

    /// <summary>Per-guild monotonic message_id of the DECK chat message that shared this deck.</summary>
    [JsonPropertyName("message_id"), Key("message_id")]
    [JsonConverter(typeof(StringifiedIntConverter))]
    public int MessageId { get; set; }

    /// <summary>Whether the viewing user may call /guild_chat/delete_deck for this entry.</summary>
    [JsonPropertyName("delete_permission_exists"), Key("delete_permission_exists")]
    public bool DeletePermissionExists { get; set; }

    /// <summary>Deck slot number (within the format's personal deck list).</summary>
    [JsonPropertyName("deck_no"), Key("deck_no")]
    [JsonConverter(typeof(StringifiedIntConverter))]
    public int DeckNo { get; set; }

    /// <summary>Deck name as it was at share-time.</summary>
    [JsonPropertyName("deck_name"), Key("deck_name")]
    public string DeckName { get; set; } = string.Empty;

    /// <summary>Class/craft id.</summary>
    [JsonPropertyName("class_id"), Key("class_id")]
    [JsonConverter(typeof(StringifiedIntConverter))]
    public int ClassId { get; set; }

    /// <summary>
    /// Flat card list — card ids repeated by count, as DeckData.ParseCardIdList expects.
    /// </summary>
    [JsonPropertyName("card_id_array"), Key("card_id_array"),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public List<long> CardIdArray { get; set; } = new();

    /// <summary>Sleeve id at share-time.</summary>
    [JsonPropertyName("sleeve_id"), Key("sleeve_id")]
    [JsonConverter(typeof(StringifiedLongConverter))]
    public long SleeveId { get; set; }

    /// <summary>Leader skin id at share-time.</summary>
    [JsonPropertyName("leader_skin_id"), Key("leader_skin_id")]
    [JsonConverter(typeof(StringifiedIntConverter))]
    public int LeaderSkinId { get; set; }
}
