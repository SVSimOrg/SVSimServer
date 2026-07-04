using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common;

/// <summary>
/// Reads a JSON string OR number as a nullable string, tolerating prod's polymorphic id fields.
/// <c>rotation_id</c> on a /load/index <c>UserDeck</c> is a numeric string ("10008") for real
/// MyRotation decks but a bare number (<c>0</c>) for unset slots — and the global
/// <c>AllowReadingFromString</c> only covers the string→number direction, not number→string, so a
/// plain <c>string?</c> property 400s on the numeric form. Null stays null; numbers serialize via
/// invariant culture so a captured <c>0</c> round-trips to <c>"0"</c>.
/// </summary>
public sealed class FlexibleStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        reader.TokenType switch
        {
            JsonTokenType.Null => null,
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.Number when reader.TryGetInt64(out var n) => n.ToString(CultureInfo.InvariantCulture),
            JsonTokenType.Number => reader.GetDouble().ToString(CultureInfo.InvariantCulture),
            _ => throw new JsonException($"Unexpected token {reader.TokenType} for a string-or-number field.")
        };

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
