using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaTwoPick;

[MessagePackObject]
public class DoMatchingRequest : BaseRequest
{
    [JsonPropertyName("card_master_hash")] [Key("card_master_hash")] public string? CardMasterHash { get; set; }
    [JsonPropertyName("deck_no")] [Key("deck_no")] public long DeckNo { get; set; }
    [JsonPropertyName("need_init")] [Key("need_init")] public int NeedInit { get; set; }
    [JsonPropertyName("log")] [Key("log")] public int Log { get; set; }
    [JsonPropertyName("excluded_field_id_list")] [Key("excluded_field_id_list")] public List<long> ExcludedFieldIdList { get; set; } = new();
    [JsonPropertyName("use_stage_select")] [Key("use_stage_select")] public int UseStageSelect { get; set; }
    [JsonPropertyName("is_default_skin")] [Key("is_default_skin")] public int IsDefaultSkin { get; set; }
}
