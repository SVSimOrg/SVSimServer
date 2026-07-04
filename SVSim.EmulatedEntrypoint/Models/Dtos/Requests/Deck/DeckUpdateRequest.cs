using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Deck;

[MessagePackObject]
public class DeckUpdateRequest : BaseRequest
{
    [JsonPropertyName("deck_no")]
    [Key("deck_no")] public int DeckNo { get; set; }
    [JsonPropertyName("class_id")]
    [Key("class_id")] public int ClassId { get; set; }
    [JsonPropertyName("leader_skin_id")]
    [Key("leader_skin_id")] public int LeaderSkinId { get; set; }
    [JsonPropertyName("is_random_leader_skin")]
    [Key("is_random_leader_skin")] public bool IsRandomLeaderSkin { get; set; }
    [JsonPropertyName("leader_skin_id_list")]
    [Key("leader_skin_id_list")] public List<int>? LeaderSkinIdList { get; set; }
    [JsonPropertyName("sleeve_id")]
    [Key("sleeve_id")] public long SleeveId { get; set; }
    [JsonPropertyName("deck_name")]
    [Key("deck_name")] public string? DeckName { get; set; }

    /// <summary>0 = save the deck, 1 = delete this deck slot.</summary>
    [JsonPropertyName("is_delete")]
    [Key("is_delete")] public int IsDelete { get; set; }

    [JsonPropertyName("card_id_array")]
    [Key("card_id_array")] public List<long>? CardIdArray { get; set; }
    [JsonPropertyName("deck_format")]
    [Key("deck_format")] public int DeckFormat { get; set; }

    /// <summary>MyRotation rule-set id (only when deck_format = MyRotation).</summary>
    [JsonPropertyName("rotation_id")]
    [Key("rotation_id")] public string? RotationId { get; set; }

    /// <summary>Crossover sub-class id (only when deck_format = Crossover).</summary>
    [JsonPropertyName("sub_class_id")]
    [Key("sub_class_id")] public int? SubClassId { get; set; }
}
