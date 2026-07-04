using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol.Bodies;

/// <summary>Server-pushed opponent-turn-open frame (relayed to the non-active player).
/// Same wire shape as <see cref="JudgeBody"/> — kept distinct because they back different
/// frames/URIs.</summary>
public sealed record OpponentTurnStartBody(
    [property: JsonPropertyName("spin")] int Spin,
    [property: JsonPropertyName("resultCode")] int ResultCode = (int)ReceiveNodeResultCode.Success) : IMsgBody;
