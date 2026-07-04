using System.Text.Json;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Common;

/// <summary>Serializes int as a JSON string ("123"), deserializes from either form. Several
/// /basic_puzzle/* fields use this on the wire (puzzle_master_id, total_count, reward_type, etc.).</summary>
public sealed class StringifiedIntConverter : JsonConverter<int>
{
    public override int Read(ref Utf8JsonReader r, Type _, JsonSerializerOptions __) =>
        r.TokenType switch
        {
            JsonTokenType.String when int.TryParse(r.GetString(), out var v) => v,
            JsonTokenType.Number => r.GetInt32(),
            _ => 0
        };
    public override void Write(Utf8JsonWriter w, int v, JsonSerializerOptions _) =>
        w.WriteStringValue(v.ToString());
}

/// <summary>Same for long. Reward ids fit in int but the client uses long internally.</summary>
public sealed class StringifiedLongConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader r, Type _, JsonSerializerOptions __) =>
        r.TokenType switch
        {
            JsonTokenType.String when long.TryParse(r.GetString(), out var v) => v,
            JsonTokenType.Number => r.GetInt64(),
            _ => 0
        };
    public override void Write(Utf8JsonWriter w, long v, JsonSerializerOptions _) =>
        w.WriteStringValue(v.ToString());
}
