using NUnit.Framework;
using SVSim.BattleNode.Bridge;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Protocol.Bodies;
using SVSim.BattleNode.Sessions;
using SVSim.BattleNode.Sessions.Participants;

namespace SVSim.UnitTests.BattleNode.Sessions.Participants;

[TestFixture]
public class NoOpBotParticipantTests
{
    [Test]
    public void PushAsync_swallows_without_firing_FrameEmitted()
    {
        var p = new NoOpBotParticipant();
        var fired = 0;
        p.FrameEmitted += (_, _) => { fired++; return Task.CompletedTask; };

        var env = new MsgEnvelope(
            NetworkBattleUri.TurnEnd, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0,
            Cat: EmitCategory.Battle, PubSeq: null, PlaySeq: null,
            Body: new ResultCodeOnlyBody());

        Assert.DoesNotThrowAsync(() => p.PushAsync(env, Stock.Normal, CancellationToken.None));
        Assert.That(fired, Is.EqualTo(0));
    }

    [Test]
    public async Task RunAsync_returns_immediately()
    {
        var p = new NoOpBotParticipant();
        await p.RunAsync(CancellationToken.None);
        // If we got here, it returned.
        Assert.Pass();
    }

    [Test]
    public void ViewerId_is_FakeOpponent()
    {
        var p = new NoOpBotParticipant();
        Assert.That(p.ViewerId, Is.EqualTo(SVSim.BattleNode.Lifecycle.ServerBattleFrames.FakeOpponentViewerId));
    }
}
