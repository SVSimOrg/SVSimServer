using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol.Bodies;

/// <summary>Server-pushed Judge frame (turn-handover gate; reflected to the sender in PvP).
/// Same wire shape as <see cref="OpponentTurnStartBody"/> — kept distinct because they back
/// different frames/URIs.</summary>
public sealed record JudgeBody(
    [property: JsonPropertyName("spin")] int Spin,
    [property: JsonPropertyName("resultCode")] int ResultCode = (int)ReceiveNodeResultCode.Success) : IMsgBody;
