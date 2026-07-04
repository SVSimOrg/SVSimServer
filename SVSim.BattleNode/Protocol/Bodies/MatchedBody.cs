using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol.Bodies;

public sealed record MatchedBody(
    [property: JsonPropertyName("selfInfo")] MatchedSelfInfo SelfInfo,
    [property: JsonPropertyName("oppoInfo")] MatchedOppoInfo OppoInfo,
    [property: JsonPropertyName("selfDeck")] IReadOnlyList<DeckCardRef> SelfDeck,
    [property: JsonPropertyName("resultCode")] int ResultCode = (int)ReceiveNodeResultCode.Success) : IMsgBody;

// Note: `country_code` is deliberately snake_case among camelCase siblings — that's what prod
// sends on this frame (verified against the TK2 capture). Do NOT "normalize" it to countryCode.
public sealed record MatchedSelfInfo(
    [property: JsonPropertyName("country_code")] string CountryCode,
    [property: JsonPropertyName("userName")] string UserName,
    [property: JsonPropertyName("sleeveId")] string SleeveId,
    [property: JsonPropertyName("emblemId")] string EmblemId,
    [property: JsonPropertyName("degreeId")] string DegreeId,
    [property: JsonPropertyName("fieldId")] int FieldId,
    [property: JsonPropertyName("isOfficial")]
    [property: JsonConverter(typeof(NumericBoolJsonConverter))] bool IsOfficial,
    [property: JsonPropertyName("oppoId")] int OppoId,
    [property: JsonPropertyName("seed")] int Seed);

public sealed record MatchedOppoInfo(
    [property: JsonPropertyName("country_code")] string CountryCode,
    [property: JsonPropertyName("userName")] string UserName,
    [property: JsonPropertyName("sleeveId")] string SleeveId,
    [property: JsonPropertyName("emblemId")] string EmblemId,
    [property: JsonPropertyName("degreeId")] string DegreeId,
    [property: JsonPropertyName("fieldId")] int FieldId,
    [property: JsonPropertyName("isOfficial")]
    [property: JsonConverter(typeof(NumericBoolJsonConverter))] bool IsOfficial,
    [property: JsonPropertyName("oppoId")] int OppoId,
    [property: JsonPropertyName("seed")] int Seed,
    [property: JsonPropertyName("oppoDeckCount")] int OppoDeckCount);

public sealed record DeckCardRef(
    [property: JsonPropertyName("idx")] int Idx,
    [property: JsonPropertyName("cardId")] long CardId);
