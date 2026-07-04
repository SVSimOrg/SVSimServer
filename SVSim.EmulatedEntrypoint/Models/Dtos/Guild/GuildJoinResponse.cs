using MessagePack;
using System.Text.Json.Serialization;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Guild;

/// <summary>
/// Response for POST /guild/join.
/// Returns the user's new guild_status; client calls /guild/info next when status is JOINING.
/// </summary>
[MessagePackObject]
public class GuildJoinResponse
{
    /// <summary>eGUILD_STATUS the user is now in. Stringified.</summary>
    [JsonPropertyName("guild_status"), Key("guild_status"), JsonConverter(typeof(StringifiedIntConverter)),
     JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    public int GuildStatus { get; set; }
}
