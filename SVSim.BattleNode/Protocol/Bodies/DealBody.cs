using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol.Bodies;

public sealed record DealBody(
    [property: JsonPropertyName("self")] IReadOnlyList<PosIdx> Self,
    [property: JsonPropertyName("oppo")] IReadOnlyList<PosIdx> Oppo,
    [property: JsonPropertyName("resultCode")] int ResultCode = (int)ReceiveNodeResultCode.Success) : IMsgBody;
