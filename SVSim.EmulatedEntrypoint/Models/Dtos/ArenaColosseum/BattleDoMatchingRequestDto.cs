using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

/// <summary>
/// <c>POST /colosseum_battle/do_matching</c> + <c>POST /colosseum_rank_battle/do_matching</c>.
/// Standard <c>DoMatchingParam</c> wire shape — same fields as rank/free-battle's variant.
/// Per do-matching.md, post-promotion the client forces <c>need_init = 0</c>, but the
/// server tolerates either value (URL is the routing signal).
/// </summary>
[MessagePackObject]
public sealed class ColosseumDoMatchingRequestDto : BaseRequest
{
    [JsonPropertyName("need_init")] [Key("need_init")]
    public int NeedInit { get; set; }

    [JsonPropertyName("card_master_hash")] [Key("card_master_hash")]
    public string? CardMasterHash { get; set; }

    [JsonPropertyName("log")] [Key("log")]
    public int Log { get; set; }

    [JsonPropertyName("use_stage_select")] [Key("use_stage_select")]
    public int UseStageSelect { get; set; }

    [JsonPropertyName("excluded_field_id_list")] [Key("excluded_field_id_list")]
    public List<long> ExcludedFieldIdList { get; set; } = new();

    [JsonPropertyName("is_default_skin")] [Key("is_default_skin")]
    public int IsDefaultSkin { get; set; }
}
