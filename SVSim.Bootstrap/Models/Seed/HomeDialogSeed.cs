using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.Bootstrap.Models.Seed;

public sealed class HomeDialogSeed
{
    [JsonPropertyName("id")]            public int Id { get; set; }
    [JsonPropertyName("title_text_id")] public string TitleTextId { get; set; } = "";
    [JsonPropertyName("image")]         public string Image { get; set; } = "";
    [JsonPropertyName("button_list")]   public JsonElement ButtonList { get; set; }
    [JsonPropertyName("begin_time")]    public string BeginTime { get; set; } = "";
    [JsonPropertyName("end_time")]      public string EndTime { get; set; } = "";
    [JsonPropertyName("type")]          public int? Type { get; set; }
    [JsonPropertyName("priority")]      public int Priority { get; set; }
}
