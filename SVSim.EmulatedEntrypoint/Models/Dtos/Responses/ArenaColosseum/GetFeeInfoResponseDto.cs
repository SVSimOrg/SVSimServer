using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ArenaColosseum;

/// <summary>
/// <c>POST /arena_colosseum/get_fee_info</c> — pre-entry oracle. Most fields are optional;
/// presence drives the lobby state machine on the client side
/// (<c>Wizard/ColosseumEntryInfoTask.cs</c>). When no season is active, only
/// <see cref="ColosseumInfo"/> + <see cref="ColosseumStatus"/> are emitted (the former with
/// <c>is_colosseum_period:false</c>, the latter as <c>{}</c> via WhenWritingNull stripping).
/// </summary>
[MessagePackObject]
public class GetFeeInfoResponseDto
{
    [JsonPropertyName("colosseum_info")] [Key("colosseum_info")]
    public ColosseumLobbyInfo ColosseumInfo { get; set; } = new();

    [JsonPropertyName("colosseum_status")] [Key("colosseum_status")]
    public ColosseumOwnStatus ColosseumStatus { get; set; } = new();

    [JsonPropertyName("is_unfinished_entry_exists")] [Key("is_unfinished_entry_exists")]
    public bool? IsUnfinishedEntryExists { get; set; }

    [JsonPropertyName("is_allowed_free_entry")] [Key("is_allowed_free_entry")]
    public bool? IsAllowedFreeEntry { get; set; }

    [JsonPropertyName("fee_list")] [Key("fee_list")]
    public ColosseumFeeList? FeeList { get; set; }

    [JsonPropertyName("deck_format")] [Key("deck_format")]
    public int? DeckFormat { get; set; }

    [JsonPropertyName("is_able_to_join_round_3")] [Key("is_able_to_join_round_3")]
    public bool? IsAbleToJoinRound3 { get; set; }

    [JsonPropertyName("is_already_entry_final_round")] [Key("is_already_entry_final_round")]
    public bool? IsAlreadyEntryFinalRound { get; set; }

    [JsonPropertyName("is_deck_deleted")] [Key("is_deck_deleted")]
    public bool? IsDeckDeleted { get; set; }

    [JsonPropertyName("two_pick_status")] [Key("two_pick_status")]
    public int? TwoPickStatus { get; set; }
}
