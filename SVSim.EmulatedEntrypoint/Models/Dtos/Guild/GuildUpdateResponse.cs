using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Guild;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// Response for POST /guild/update.
/// GuildUpdateTask.Parse() reads data["guild"] directly as GuildDetailInfo — flat, no "detail" wrapper.
/// </summary>
[MessagePackObject]
public class GuildUpdateResponse
{
    [JsonPropertyName("guild"), Key("guild")]
    public GuildDetailDto Guild { get; set; } = new();
}

/// <summary>
/// Response for POST /guild/update_emblem.
/// GuildEmblemUpdateTask.Parse() reads data["guild"]["detail"] — requires the nested wrapper.
/// </summary>
[MessagePackObject]
public class GuildUpdateEmblemResponse
{
    [JsonPropertyName("guild"), Key("guild")]
    public GuildDetailSubTree Guild { get; set; } = new();
}

[MessagePackObject]
public class GuildDetailSubTree
{
    [JsonPropertyName("detail"), Key("detail")]
    public GuildDetailDto Detail { get; set; } = new();
}
