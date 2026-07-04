using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Tutorial;

/// <summary>
/// <c>POST /tutorial/update</c> — client reports the step it is moving TO.
/// Client task: <c>Wizard/TutorialUpdateTask.cs</c>.
/// </summary>
[MessagePackObject]
public class TutorialUpdateRequest : BaseRequest
{
    /// <summary>The tutorial step the client is moving TO (0, 1, 11, 21, 31, 41, 100).</summary>
    [JsonPropertyName("tutorial_step")]
    [Key("tutorial_step")]
    public int TutorialStep { get; set; }

    /// <summary>0 = normal, 1 = user chose Skip Tutorial.</summary>
    [JsonPropertyName("is_skip")]
    [Key("is_skip")]
    public int IsSkip { get; set; }
}
