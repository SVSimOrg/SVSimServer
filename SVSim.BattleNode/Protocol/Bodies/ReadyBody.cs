using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol.Bodies;

public sealed record ReadyBody(
    [property: JsonPropertyName("self")] IReadOnlyList<PosIdx> Self,
    [property: JsonPropertyName("oppo")] IReadOnlyList<PosIdx> Oppo,
    [property: JsonPropertyName("idxChangeSeed")] int IdxChangeSeed,
    [property: JsonPropertyName("spin")] int Spin,
    [property: JsonPropertyName("resultCode")] int ResultCode = (int)ReceiveNodeResultCode.Success) : IMsgBody;
