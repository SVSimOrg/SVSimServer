using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// The state of a user's tutorial progress.
/// </summary>
[MessagePackObject]
public class UserTutorial
{
    /// <summary>
    /// The current tutorial step they are on.
    /// </summary>
    [JsonPropertyName("tutorial_step")]
    [Key("tutorial_step")]
    public int TutorialStep { get; set; }
}