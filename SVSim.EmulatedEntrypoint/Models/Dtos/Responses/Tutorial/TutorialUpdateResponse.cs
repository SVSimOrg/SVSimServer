using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Tutorial;

/// <summary>
/// Server echoes the new step. Capture confirms exact value mirror — no validation,
/// no munging. <c>tutorial_replay_step</c> is in the spec as optional but the live capture
/// never includes it; omit unless we observe a need.
/// </summary>
[MessagePackObject]
public class TutorialUpdateResponse
{
    [JsonPropertyName("tutorial_step")]
    [Key("tutorial_step")]
    public int TutorialStep { get; set; }
}
