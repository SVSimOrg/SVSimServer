using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.MyPage;

[MessagePackObject]
public class MyPageFinishBattleRequest : BaseRequest
{
    // Wire key is uppercase "SDTRB" — verbatim from
    // PlayerPrefsWrapper.SELF_DISCONNECT_OPEN_STATUS_TO_REPLACE_LOG.
    [JsonPropertyName("SDTRB")]
    [Key("SDTRB")]
    public int Sdtrb { get; set; }
}
