using System.Text.Json;
using System.Text.Json.Serialization;
using SVSim.Database.Enums;

namespace SVSim.EmulatedEntrypoint.Extensions;

/// <summary>
/// Bridges the server's internal <see cref="Format"/> enum (a verbatim copy of the client's
/// Wizard.Format) and the wire <c>deck_format</c> integer the client speaks. The two are not
/// interchangeable: the wire value is what flows over the network and what the client's
/// LoadDetail._userRank dictionary is keyed by AFTER routing through Data.ParseApiFormat;
/// the internal value is what the server uses for switches, dictionary keys, and database
/// columns.
///
/// Mapping mirrors the client's <c>Wizard.Data.FormatConvertApi</c>
/// (Shadowverse_Code/Assembly-CSharp/Wizard/Data.cs:580); the inverse mirrors
/// <c>Data.ParseApiFormat</c> (Data.cs:635). See
/// docs/api-spec/common/types.ts.md for the table and rationale.
/// </summary>
public static class FormatExtensions
{
    /// <summary>Internal <see cref="Format"/> &#x2192; wire <c>deck_format</c> integer.</summary>
    public static int ToApi(this Format format) => format switch
    {
        Format.Rotation    => 1,
        Format.Unlimited   => 2,
        Format.Max         => 1, // client sentinel; aliases onto Rotation, same as FormatConvertApi.
        Format.PreRotation => 3,
        Format.Sealed      => 20,
        Format.MyRotation  => 5,
        Format.TwoPick     => 10,
        Format.Hof         => 31,
        Format.Windfall    => 33,
        Format.Avatar      => 39,
        Format.All         => 0,
        Format.Crossover   => 4,
        _ => throw new ArgumentOutOfRangeException(nameof(format), format,
            $"No wire deck_format mapping for {format} ({(int)format}). " +
            "Update FormatExtensions.ToApi and Data.cs:580 if a new format was added.")
    };

    /// <summary>Wire <c>deck_format</c> integer &#x2192; internal <see cref="Format"/>.</summary>
    public static Format FromApi(int apiValue) => apiValue switch
    {
        0  => Format.All,         // Client emits 0 only for "all formats" meta-queries.
        1  => Format.Rotation,
        2  => Format.Unlimited,
        3  => Format.PreRotation,
        4  => Format.Crossover,
        5  => Format.MyRotation,
        10 => Format.TwoPick,
        20 => Format.Sealed,
        31 => Format.Hof,
        33 => Format.Windfall,
        39 => Format.Avatar,
        _ => throw new ArgumentOutOfRangeException(nameof(apiValue), apiValue,
            $"Unknown wire deck_format {apiValue}. The client's ParseApiFormat would warn and " +
            "fall back to Format.Max; we throw so the calling controller surfaces the bad input.")
    };
}

/// <summary>
/// System.Text.Json converter that emits / accepts <see cref="Format"/> as the wire
/// <c>deck_format</c> integer rather than the underlying enum value. Wired up in Program.cs
/// via AddJsonOptions; applies to every response DTO property typed <see cref="Format"/>.
///
/// IMPORTANT: this only runs on the System.Text.Json serialization path (response writer +
/// model binder). MessagePack-CSharp deserialization of request DTOs does NOT honor STJ
/// converters &#x2014; keep request DTO format fields typed as <c>int</c> and call
/// <see cref="FormatExtensions.FromApi"/> in the controller.
/// </summary>
public sealed class FormatJsonConverter : JsonConverter<Format>
{
    public override Format Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number || !reader.TryGetInt32(out int wire))
        {
            throw new JsonException(
                $"Expected deck_format as a JSON number, got {reader.TokenType}.");
        }
        return FormatExtensions.FromApi(wire);
    }

    public override void Write(Utf8JsonWriter writer, Format value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.ToApi());
    }
}
