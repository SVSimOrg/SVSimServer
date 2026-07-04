using System.Text.Json;
using SVSim.Database.Enums;
using SVSim.EmulatedEntrypoint.Extensions;

namespace SVSim.UnitTests.Extensions;

/// <summary>
/// Pins the wire deck_format mapping against the client's <c>Data.FormatConvertApi</c>
/// (Shadowverse_Code/Assembly-CSharp/Wizard/Data.cs:580). Each row in <see cref="WireMapping"/>
/// is "internal Format ↔ wire int" and must match the client decompile exactly — see
/// docs/api-spec/common/types.ts.md for the table.
/// </summary>
public class FormatExtensionsTests
{
    /// <summary>(internal Format, wire deck_format int)</summary>
    public static IEnumerable<(Format Format, int Wire)> WireMapping => new (Format, int)[]
    {
        (Format.Rotation,    1),
        (Format.Unlimited,   2),
        (Format.PreRotation, 3),
        (Format.Crossover,   4),
        (Format.MyRotation,  5),
        (Format.TwoPick,     10),
        (Format.Sealed,      20),
        (Format.Hof,         31),
        (Format.Windfall,    33),
        (Format.Avatar,      39),
        (Format.All,         0),
    };

    [TestCaseSource(nameof(WireMapping))]
    public void ToApi_matches_client_FormatConvertApi((Format Format, int Wire) row)
    {
        Assert.That(row.Format.ToApi(), Is.EqualTo(row.Wire));
    }

    [TestCaseSource(nameof(WireMapping))]
    public void FromApi_inverts_ToApi((Format Format, int Wire) row)
    {
        Assert.That(FormatExtensions.FromApi(row.Wire), Is.EqualTo(row.Format));
    }

    [Test]
    public void ToApi_Max_aliases_onto_Rotation_per_client_FormatConvertApi()
    {
        // Format.Max is a client-side sentinel ("invalid/unknown"); the client's
        // FormatConvertApi sends it as wire 1 (same slot as Rotation). Pinned so we can spot
        // the day the client decompile changes this.
        Assert.That(Format.Max.ToApi(), Is.EqualTo(1));
    }

    [Test]
    public void ToApi_throws_on_unmapped_format()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ((Format)9999).ToApi());
    }

    [TestCase(7)]
    [TestCase(99)]
    [TestCase(-1)]
    public void FromApi_throws_on_unknown_wire_code(int wire)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => FormatExtensions.FromApi(wire));
    }

    // ---- JSON converter ----

    private static JsonSerializerOptions ConverterOptions()
    {
        var opts = new JsonSerializerOptions();
        opts.Converters.Add(new FormatJsonConverter());
        return opts;
    }

    [TestCaseSource(nameof(WireMapping))]
    public void JsonConverter_writes_wire_code((Format Format, int Wire) row)
    {
        string json = JsonSerializer.Serialize(row.Format, ConverterOptions());
        Assert.That(json, Is.EqualTo(row.Wire.ToString()));
    }

    [TestCaseSource(nameof(WireMapping))]
    public void JsonConverter_reads_wire_code((Format Format, int Wire) row)
    {
        Format result = JsonSerializer.Deserialize<Format>(row.Wire.ToString(), ConverterOptions());
        Assert.That(result, Is.EqualTo(row.Format));
    }

    [Test]
    public void JsonConverter_rejects_non_numeric_input()
    {
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Format>("\"rotation\"", ConverterOptions()));
    }
}
