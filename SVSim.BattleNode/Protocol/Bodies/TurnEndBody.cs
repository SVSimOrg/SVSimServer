using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol.Bodies;

public sealed record TurnEndBody(
    [property: JsonPropertyName("turnState")]
    [property: JsonConverter(typeof(JsonNumberEnumConverter<TurnState>))] TurnState TurnState,
    [property: JsonPropertyName("resultCode")] int ResultCode = (int)ReceiveNodeResultCode.Success) : IMsgBody;
