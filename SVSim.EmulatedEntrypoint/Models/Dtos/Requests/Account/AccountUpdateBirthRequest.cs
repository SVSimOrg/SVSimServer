using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Account;

[MessagePackObject]
public class AccountUpdateBirthRequest : BaseRequest
{
    [JsonPropertyName("birth")]
    [Key("birth")]
    public string Birth { get; set; } = string.Empty;
}
