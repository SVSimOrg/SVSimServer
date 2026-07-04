using System.Text.Json.Serialization;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Pack;

/// <summary>
/// Inbound /pack/set_rotation_starter_class body. Locks in the class choice for a
/// RotationStarterCardPack before the user can open it. One-shot per (viewer, pack)
/// per the spec; subsequent attempts return 400.
/// See <c>Wizard/PackSetRotationStarterClassTask.cs</c>.
/// </summary>
[MessagePackObject]
public class PackSetRotationStarterClassRequest : BaseRequest
{
    [JsonPropertyName("pack_id")]
    [Key("pack_id")]
    public int PackId { get; set; }

    [JsonPropertyName("class_id")]
    [Key("class_id")]
    public int ClassId { get; set; }
}
