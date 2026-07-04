using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.UnitTests.BattleNode.Protocol.Bodies;

[TestFixture]
public class BattleStartBodyTests
{
    [Test]
    public void Serializes_TopLevelFields_WithCorrectWireKeys()
    {
        var body = new BattleStartBody(
            TurnState: TurnState.First, BattleModeId: BattleModes.TakeTwo,
            SelfInfo: new BattleStartSelfInfo("10", "6270", "1", "1", "card_master_node_10015"),
            OppoInfo: new BattleStartOppoInfo("1", "0", 0, "0", "8", "8", "card_master_node_10015"));

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;

        Assert.That(node["turnState"]!.GetValue<int>(), Is.EqualTo(0));
        Assert.That(node["battleType"]!.GetValue<int>(), Is.EqualTo(11));
        Assert.That(node["resultCode"]!.GetValue<int>(), Is.EqualTo(1));   // default
    }

    [Test]
    public void SelfInfo_BattlePoint_IsString_OnTheWire()
    {
        var body = new BattleStartBody(0, 11,
            new BattleStartSelfInfo("10", "6270", "1", "1", "cm"),
            new BattleStartOppoInfo("1", "0", 0, "0", "8", "8", "cm"));

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;
        var selfInfo = (JsonObject)node["selfInfo"]!;

        // Wire shape: self.battlePoint is a string, oppo.battlePoint is an int. Preserved verbatim.
        Assert.That(selfInfo["battlePoint"]!.GetValue<string>(), Is.EqualTo("6270"));
    }

    [Test]
    public void OppoInfo_BattlePoint_IsInt_OnTheWire()
    {
        var body = new BattleStartBody(0, 11,
            new BattleStartSelfInfo("10", "6270", "1", "1", "cm"),
            new BattleStartOppoInfo("1", "0", 0, "0", "8", "8", "cm"));

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;
        var oppoInfo = (JsonObject)node["oppoInfo"]!;

        Assert.That(oppoInfo["battlePoint"]!.GetValue<int>(), Is.EqualTo(0));
        Assert.That(oppoInfo["isMasterRank"]!.GetValue<string>(), Is.EqualTo("0"));
        Assert.That(oppoInfo["masterPoint"]!.GetValue<string>(), Is.EqualTo("0"));
    }
}
