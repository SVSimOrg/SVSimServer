using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Mission;

[MessagePackObject]
public class MissionChangeReceiveSettingRequest : BaseRequest
{
    [Key("mission_receive_type")]
    [JsonPropertyName("mission_receive_type")]
    public int MissionReceiveType { get; set; }
}
