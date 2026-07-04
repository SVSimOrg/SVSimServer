using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaColosseum;

/// <summary>
/// <c>POST /arena_colosseum/register_deck</c> — submit deck slot(s) for a constructed-format
/// entry. Same wire gotcha as <c>arena_competition/register_deck</c> et al — <see cref="DeckNoList"/>
/// is a JSON-encoded STRING like <c>"[3,4,5]"</c>, not an array. The server parses it.
/// </summary>
[MessagePackObject(keyAsPropertyName: false)]
public class ArenaColosseumRegisterDeckRequest : BaseRequest
{
    /// <summary>JSON-encoded list of deck slot numbers. Client does <c>JsonMapper.ToJson(List&lt;int&gt;)</c>.</summary>
    [JsonPropertyName("deck_no_list")] [Key("deck_no_list")]
    public string DeckNoList { get; set; } = "[]";

    /// <summary>Server-stored visibility flag — does not affect bracket play.</summary>
    [JsonPropertyName("is_published")] [Key("is_published")]
    public bool IsPublished { get; set; }
}
