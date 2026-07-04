using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Account;

[MessagePackObject]
public class AccountUpdateRegionCodeRequest : BaseRequest
{
    [JsonPropertyName("initialize_flag")]
    [Key("initialize_flag")]
    public int InitializeFlag { get; set; }
}
