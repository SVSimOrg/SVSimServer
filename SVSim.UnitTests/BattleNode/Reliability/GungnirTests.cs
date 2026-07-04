using NUnit.Framework;
using SVSim.BattleNode.Reliability;

namespace SVSim.UnitTests.BattleNode.Reliability;

[TestFixture]
public class GungnirTests
{
    [Test]
    public void BuildAliveEmit_CarriesCurrentSeqFromTracker()
    {
        var tracker = new InboundTracker();
        tracker.Observe(7);

        var body = Gungnir.BuildAliveEmitBody(tracker);

        Assert.That(body["currentSeq"], Is.EqualTo(7L));
        Assert.That(body.ContainsKey("actionSeq"), Is.False);  // omitted in v1
    }
}
