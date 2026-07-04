using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Account;

[MessagePackObject]
public class AccountUpdateNameRequest : BaseRequest
{
    [JsonPropertyName("name")]
    [Key("name")]
    public string Name { get; set; } = string.Empty;
}
