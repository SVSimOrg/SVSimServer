using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using SVSim.BattleNode.Protocol;

namespace SVSim.UnitTests.BattleNode.Protocol;

[TestFixture]
public class NumericBoolJsonConverterTests
{
    private sealed record Probe(
        [property: JsonPropertyName("flag")]
        [property: JsonConverter(typeof(NumericBoolJsonConverter))]
        bool Flag,
        [property: JsonPropertyName("opt")]
        [property: JsonConverter(typeof(NumericBoolJsonConverter))]
        bool? Opt = null);

    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    [Test]
    public void Writes_true_as_numeric_1_and_false_as_numeric_0()
    {
        var node = JsonSerializer.SerializeToElement(new Probe(Flag: true), Options);
        Assert.That(node.GetProperty("flag").ValueKind, Is.EqualTo(JsonValueKind.Number));
        Assert.That(node.GetProperty("flag").GetInt32(), Is.EqualTo(1));

        var falseNode = JsonSerializer.SerializeToElement(new Probe(Flag: false), Options);
        Assert.That(falseNode.GetProperty("flag").GetInt32(), Is.EqualTo(0));
    }

    [Test]
    public void Reads_numeric_0_and_1_back_to_bool()
    {
        Assert.That(JsonSerializer.Deserialize<Probe>("{\"flag\":1}", Options)!.Flag, Is.True);
        Assert.That(JsonSerializer.Deserialize<Probe>("{\"flag\":0}", Options)!.Flag, Is.False);
    }

    [Test]
    public void Nullable_true_emits_1_and_null_is_omitted()
    {
        var present = JsonSerializer.SerializeToElement(new Probe(Flag: false, Opt: true), Options);
        Assert.That(present.GetProperty("opt").GetInt32(), Is.EqualTo(1));

        var absent = JsonSerializer.SerializeToElement(new Probe(Flag: false, Opt: null), Options);
        Assert.That(absent.TryGetProperty("opt", out _), Is.False, "null bool? must be omitted, not emitted");
    }
}
