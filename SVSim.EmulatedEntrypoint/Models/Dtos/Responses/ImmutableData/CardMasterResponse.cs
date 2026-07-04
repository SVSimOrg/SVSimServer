using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ImmutableData;

/// <summary>
/// Wire response shape. Client decodes <c>card_master</c> via:
/// <c>Convert.FromBase64String</c> → <c>GZipStream.UncompressBuffer</c> → UTF-8 decode →
/// <c>JsonMapper.ToObject</c>, yielding a dict <c>{ "1": "&lt;csv&gt;", "2"?: "&lt;csv&gt;" }</c>.
/// </summary>
[MessagePackObject]
public class CardMasterResponse
{
    [JsonPropertyName("card_master")]
    [Key("card_master")]
    public string CardMaster { get; set; } = "";
}
