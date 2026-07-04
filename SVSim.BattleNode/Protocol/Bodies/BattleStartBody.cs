using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol.Bodies;

public sealed record BattleStartBody(
    [property: JsonPropertyName("turnState")]
    [property: JsonConverter(typeof(JsonNumberEnumConverter<TurnState>))] TurnState TurnState,
    // Wire key stays "battleType" (the client's contract); the CLR name is BattleModeId so the
    // project keeps one meaning of "BattleType" — the Sessions.BattleType enum (Pvp/Bot).
    [property: JsonPropertyName("battleType")] int BattleModeId,
    [property: JsonPropertyName("selfInfo")] BattleStartSelfInfo SelfInfo,
    [property: JsonPropertyName("oppoInfo")] BattleStartOppoInfo OppoInfo,
    [property: JsonPropertyName("resultCode")] int ResultCode = (int)ReceiveNodeResultCode.Success) : IMsgBody;

public sealed record BattleStartSelfInfo(
    [property: JsonPropertyName("rank")] string Rank,
    [property: JsonPropertyName("battlePoint")] string BattlePoint,
    [property: JsonPropertyName("classId")] string ClassId,
    [property: JsonPropertyName("charaId")] string CharaId,
    [property: JsonPropertyName("cardMasterName")] string CardMasterName);

// Note: BattlePoint is int on the wire here (not string as on self) — matches the
// captured prod frame at data_dumps/captures/battle-traffic_tk2_regular.ndjson.
// The string-self / int-oppo split is INTENTIONAL; do NOT unify the two for "consistency".
public sealed record BattleStartOppoInfo(
    [property: JsonPropertyName("rank")] string Rank,
    [property: JsonPropertyName("isMasterRank")] string IsMasterRank,
    [property: JsonPropertyName("battlePoint")] int BattlePoint,
    [property: JsonPropertyName("masterPoint")] string MasterPoint,
    [property: JsonPropertyName("classId")] string ClassId,
    [property: JsonPropertyName("charaId")] string CharaId,
    [property: JsonPropertyName("cardMasterName")] string CardMasterName);
