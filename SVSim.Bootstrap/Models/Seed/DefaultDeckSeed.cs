using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class DefaultDeckSeed
{
    [JsonPropertyName("id")] public int Id { get; set; }
    [JsonPropertyName("class_id")] public int ClassId { get; set; }
    [JsonPropertyName("sleeve_id")] public long SleeveId { get; set; }
    [JsonPropertyName("leader_skin_id")] public int LeaderSkinId { get; set; }
    [JsonPropertyName("deck_name")] public string DeckName { get; set; } = "";
    [JsonPropertyName("card_id_array")] public List<long> CardIdArray { get; set; } = new();
}
