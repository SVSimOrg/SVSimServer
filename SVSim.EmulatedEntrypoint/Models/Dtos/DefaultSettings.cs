using MessagePack;
using SVSim.Database.Models.Config;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class DefaultSettings
{
    [JsonPropertyName("default_emblem_id")]
    [Key("default_emblem_id")]
    public int DefaultEmblemId { get; set; }
    [JsonPropertyName("default_degree_id")]
    [Key("default_degree_id")]
    public int DefaultDegreeId { get; set; }
    [JsonPropertyName("default_mypage_id")]
    [Key("default_mypage_id")]
    public int DefaultMyPageBackground { get; set; }

    public DefaultSettings(DefaultLoadoutConfig loadout)
    {
        this.DefaultMyPageBackground = loadout.MyPageBackgroundId;
        this.DefaultDegreeId = loadout.DegreeId;
        this.DefaultEmblemId = loadout.EmblemId;
    }

    public DefaultSettings()
    {
    }
}
