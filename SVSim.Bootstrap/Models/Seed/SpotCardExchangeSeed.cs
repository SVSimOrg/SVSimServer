using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class SpotCardExchangeSeed
{
    [JsonPropertyName("card_id")] public long CardId { get; set; }
    [JsonPropertyName("class")] public int ClassId { get; set; }
    [JsonPropertyName("exchange_point")] public int ExchangePoint { get; set; }
    [JsonPropertyName("ts_rotation_id")] public long TsRotationId { get; set; }
    [JsonPropertyName("is_pre_release")] public bool IsPreRelease { get; set; }
}
