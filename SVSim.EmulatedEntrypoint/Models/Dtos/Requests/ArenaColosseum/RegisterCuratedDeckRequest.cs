using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaColosseum;

/// <summary>
/// Shared request shape for the three curated-deck register URLs (HOF / WindFall / Avatar).
/// Same JSON-encoded-string gotcha as <see cref="ArenaColosseumRegisterDeckRequest"/> —
/// <c>deck_no_list</c> is a wire string like <c>"[1001,1002]"</c>. No <c>is_published</c>
/// here (constructed-only flag per spec).
/// </summary>
[MessagePackObject]
public sealed class RegisterCuratedDeckRequest : BaseRequest
{
    [JsonPropertyName("deck_no_list")] [Key("deck_no_list")]
    public string DeckNoList { get; set; } = "[]";
}
