using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;
using SVSim.BattleNode.Protocol.Bodies;

namespace SVSim.UnitTests.BattleNode.Protocol.Bodies;

[TestFixture]
public class HandBodiesTests
{
    [Test]
    public void DealBody_SerializesSelfAndOppoArrays_WithPosIdxKeys()
    {
        var body = new DealBody(
            Self: new[] { new PosIdx(0, 1), new PosIdx(1, 2), new PosIdx(2, 3) },
            Oppo: new[] { new PosIdx(0, 1), new PosIdx(1, 2), new PosIdx(2, 3) });

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;
        var self = (JsonArray)node["self"]!;

        Assert.That(self.Count, Is.EqualTo(3));
        Assert.That(((JsonObject)self[0]!)["pos"]!.GetValue<int>(), Is.EqualTo(0));
        Assert.That(((JsonObject)self[0]!)["idx"]!.GetValue<int>(), Is.EqualTo(1));
        Assert.That(node["resultCode"]!.GetValue<int>(), Is.EqualTo(1));   // default
    }

    [Test]
    public void SwapResponseBody_OnlyContainsSelf_NotOppo()
    {
        var body = new SwapResponseBody(
            Self: new[] { new PosIdx(0, 1), new PosIdx(1, 4), new PosIdx(2, 3) });

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;

        Assert.That(node.ContainsKey("self"), Is.True);
        Assert.That(node.ContainsKey("oppo"), Is.False);
        Assert.That(((JsonObject)((JsonArray)node["self"]!)[1]!)["idx"]!.GetValue<int>(), Is.EqualTo(4));
    }

    [Test]
    public void ReadyBody_SerializesAllFields_IncludingIdxChangeSeedAndSpin()
    {
        var body = new ReadyBody(
            Self: new[] { new PosIdx(0, 1) },
            Oppo: new[] { new PosIdx(0, 1) },
            IdxChangeSeed: 771_335_280,
            Spin: 0);

        var node = (JsonObject)JsonSerializer.SerializeToNode(body)!;

        Assert.That(node["idxChangeSeed"]!.GetValue<int>(), Is.EqualTo(771_335_280));
        Assert.That(node["spin"]!.GetValue<int>(), Is.EqualTo(0));
        Assert.That(node["resultCode"]!.GetValue<int>(), Is.EqualTo(1));
    }
}
