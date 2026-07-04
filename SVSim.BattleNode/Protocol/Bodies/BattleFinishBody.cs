using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol.Bodies;

public sealed record BattleFinishBody(
    [property: JsonPropertyName("result")]
    [property: JsonConverter(typeof(JsonNumberEnumConverter<BattleResult>))]
    BattleResult Result,
    [property: JsonPropertyName("resultCode")] int ResultCode = (int)ReceiveNodeResultCode.Success) : IMsgBody;
