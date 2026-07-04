using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.UnitTests.BattleNode.Protocol.Bodies;

[TestFixture]
public class SmallBodiesTests
{
    [Test]
    public void OpponentTurnStartBody_SerializesSpin_AndDefaultsResultCodeToOne()
    {
        var body = new OpponentTurnStartBody(Spin: 100);

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;

        Assert.That(node["spin"]!.GetValue<int>(), Is.EqualTo(100));
        Assert.That(node["resultCode"]!.GetValue<int>(), Is.EqualTo(1));
    }

    [Test]
    public void ResultCodeOnlyBody_SerializesJustResultCode()
    {
        var body = new ResultCodeOnlyBody();

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;

        Assert.That(node.Count, Is.EqualTo(1));
        Assert.That(node["resultCode"]!.GetValue<int>(), Is.EqualTo(1));
    }

    [Test]
    public void BattleFinishBody_SerializesResultAndResultCode_AsNumericWireValues()
    {
        // The wire field is the int RESULT_CODE (LifeWin=101); BattleResult uses
        // JsonNumberEnumConverter to override the default JsonStringEnumConverter (which
        // would emit "LifeWin" instead).
        var body = new BattleFinishBody(Result: BattleResult.LifeWin);

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;

        Assert.That(node["result"]!.GetValue<int>(), Is.EqualTo(101));
        Assert.That(node["resultCode"]!.GetValue<int>(), Is.EqualTo(1));
    }

    [Test]
    public void AlivePushBody_SerializesScsAndOcs_AndDoesNotIncludeResultCode()
    {
        var body = new AlivePushBody(Scs: "ONLINE", Ocs: "ONLINE");

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;

        Assert.That(node["scs"]!.GetValue<string>(), Is.EqualTo("ONLINE"));
        Assert.That(node["ocs"]!.GetValue<string>(), Is.EqualTo("ONLINE"));
        Assert.That(node.ContainsKey("resultCode"), Is.False);
    }
}
