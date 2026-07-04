using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Tutorial;

/// <summary>
/// <c>POST /tutorial/update_action</c> — fire-and-forget sub-step tracking.
/// Client task: <c>Wizard/TutorialUpdateActionTask.cs</c>. SkipAllNetworkChecks is on,
/// so any return value (including failures) is silently ignored.
/// </summary>
[MessagePackObject]
public class TutorialUpdateActionRequest : BaseRequest
{
    [JsonPropertyName("tutorial_step")]
    [Key("tutorial_step")]
    public int TutorialStep { get; set; }

    [JsonPropertyName("tutorial_action_number")]
    [Key("tutorial_action_number")]
    public int TutorialActionNumber { get; set; }
}
