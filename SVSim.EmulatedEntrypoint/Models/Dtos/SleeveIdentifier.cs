using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Identifies a card sleeve.
/// </summary>
[MessagePackObject]
public class SleeveIdentifier
{
    /// <summary>
    /// The id of the sleeve.
    /// </summary>
    [JsonPropertyName("sleeve_id")]
    [Key("sleeve_id")]
    public long SleeveId { get; set; }
}