using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Profile;

[MessagePackObject]
public class ProfileUpdateOfficialMarkDisplayRequest : BaseRequest
{
    [JsonPropertyName("is_official_mark_displayed")]
    [Key("is_official_mark_displayed")]
    public int IsOfficialMarkDisplayed { get; set; }
}
