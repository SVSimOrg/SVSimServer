using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// gacha_point block inside /pack/info entries. Prod ships strings for pack_id/increase_gacha_point;
/// mirror exactly per project_wire_key_serialization.
/// </summary>
[MessagePackObject]
public class PackGachaPointDto
{
    [JsonPropertyName("pack_id")]
    [Key("pack_id")]
    public string PackId { get; set; } = "0";

    [JsonPropertyName("gacha_point")]
    [Key("gacha_point")]
    public int GachaPoint { get; set; }

    [JsonPropertyName("increase_gacha_point")]
    [Key("increase_gacha_point")]
    public string IncreaseGachaPoint { get; set; } = "0";

    [JsonPropertyName("exchangeable_gacha_point")]
    [Key("exchangeable_gacha_point")]
    public int ExchangeableGachaPoint { get; set; }

    [JsonPropertyName("is_exchangeable_gacha_point")]
    [Key("is_exchangeable_gacha_point")]
    public bool IsExchangeableGachaPoint { get; set; }
}
