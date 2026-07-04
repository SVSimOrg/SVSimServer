using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol.Bodies;

public sealed record PosIdx(
    [property: JsonPropertyName("pos")] int Pos,
    [property: JsonPropertyName("idx")] int Idx);
