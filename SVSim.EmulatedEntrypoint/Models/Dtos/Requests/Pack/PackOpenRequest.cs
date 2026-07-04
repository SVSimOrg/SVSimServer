using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Pack;

/// <summary>
/// Inbound /pack/open body. Accepts ALL three client-side overloads in one DTO — fields
/// for Starter (<c>class_id</c>) and Skin (<c>target_card_id</c>) are nullable so we can
/// reject those overloads in the controller without a custom binder.
/// See <c>Wizard/PackOpenTask.cs</c> for the three SetParameter variants.
/// </summary>
[MessagePackObject]
public class PackOpenRequest : BaseRequest
{
    [JsonPropertyName("parent_gacha_id")]
    [Key("parent_gacha_id")]
    public int ParentGachaId { get; set; }

    [JsonPropertyName("gacha_id")]
    [Key("gacha_id")]
    public int GachaId { get; set; }

    [JsonPropertyName("gacha_type")]
    [Key("gacha_type")]
    public int GachaType { get; set; }

    [JsonPropertyName("pack_number")]
    [Key("pack_number")]
    public int PackNumber { get; set; }

    [JsonPropertyName("exclude_card_ids")]
    [Key("exclude_card_ids")]
    public long[] ExcludeCardIds { get; set; } = Array.Empty<long>();

    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public int? ClassId { get; set; }

    [JsonPropertyName("target_card_id")]
    [Key("target_card_id")]
    public long? TargetCardId { get; set; }
}
