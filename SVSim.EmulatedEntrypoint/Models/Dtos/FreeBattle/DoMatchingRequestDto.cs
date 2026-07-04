using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.FreeBattle;

/// <summary>
/// Standard DoMatchingParam shape for free battle (rotation/unlimited). Identical wire
/// shape to the rank-battle variant — see Models/Dtos/RankBattle/DoMatchingRequestDto.cs.
/// Mirrored (not reused) so each family owns its namespace; consolidate when a third
/// matched-PvP family lands.
///
/// Inherits viewer_id / steam_id / steam_session_ticket from <see cref="BaseRequest"/>
/// so the auth fields survive the translation-middleware msgpack → DTO → JSON round-trip.
/// </summary>
[MessagePackObject]
public sealed class DoMatchingRequestDto : BaseRequest
{
    [JsonPropertyName("deck_no")]
    [Key("deck_no")]
    public int DeckNo { get; set; }

    [JsonPropertyName("need_init")]
    [Key("need_init")]
    public int NeedInit { get; set; }

    [JsonPropertyName("card_master_hash")]
    [Key("card_master_hash")]
    public string? CardMasterHash { get; set; }

    [JsonPropertyName("log")]
    [Key("log")]
    public int Log { get; set; }

    [JsonPropertyName("use_stage_select")]
    [Key("use_stage_select")]
    public int UseStageSelect { get; set; }

    [JsonPropertyName("excluded_field_id_list")]
    [Key("excluded_field_id_list")]
    public List<long> ExcludedFieldIdList { get; set; } = new();

    [JsonPropertyName("is_default_skin")]
    [Key("is_default_skin")]
    public int IsDefaultSkin { get; set; }
}
