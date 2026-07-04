using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.BattleNode.Protocol;

/// <summary>
/// Serializes a <see cref="bool"/> as the wire's numeric 0/1. The client reads these flags via
/// <c>Convert.ToInt32</c> / <c>Convert.ToBoolean</c> (e.g. <c>isOfficial</c>, <c>isInvoke</c>) —
/// never as a JSON <c>true</c>/<c>false</c> token — so a real <c>bool</c> property must still emit
/// a number. Read accepts a JSON number (0 = false, non-zero = true) and, defensively, a
/// <c>true</c>/<c>false</c> token or a numeric string. Applied per-field via
/// <c>[JsonConverter(typeof(NumericBoolJsonConverter))]</c>; works on <c>bool?</c> too (System.Text.Json
/// wraps a <c>JsonConverter&lt;bool&gt;</c> for the nullable case).
/// </summary>
public sealed class NumericBoolJsonConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.Number => reader.GetInt64() != 0,
            JsonTokenType.True => true,
            JsonTokenType.False => false,
            JsonTokenType.String => long.TryParse(reader.GetString(), out var n) && n != 0,
            _ => throw new JsonException($"Cannot convert token {reader.TokenType} to a numeric bool"),
        };

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        => writer.WriteNumberValue(value ? 1 : 0);
}
