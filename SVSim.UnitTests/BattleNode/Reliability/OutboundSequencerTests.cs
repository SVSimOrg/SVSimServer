using NUnit.Framework;
using SVSim.BattleNode.Protocol;
using SVSim.BattleNode.Reliability;

namespace SVSim.UnitTests.BattleNode.Reliability;

[TestFixture]
public class OutboundSequencerTests
{
    private static MsgEnvelope MakeEnvelope(NetworkBattleUri uri) =>
        new(uri, ViewerId: 1, Uuid: "u", Bid: null, RetryAttempt: 0, Cat: EmitCategory.Battle,
            PubSeq: null, PlaySeq: null, Body: new RawBody(new Dictionary<string, object?>()));

    [Test]
    public void AssignAndArchive_FirstCall_ReturnsEnvelopeWithPlaySeq1()
    {
        var seq = new OutboundSequencer();
        var assigned = seq.AssignAndArchive(MakeEnvelope(NetworkBattleUri.BattleStart));

        Assert.That(assigned.PlaySeq, Is.EqualTo(1));
    }

    [Test]
    public void AssignAndArchive_SubsequentCalls_ReturnContiguousSequence()
    {
        var seq = new OutboundSequencer();
        Assert.That(seq.AssignAndArchive(MakeEnvelope(NetworkBattleUri.Matched)).PlaySeq, Is.EqualTo(1));
        Assert.That(seq.AssignAndArchive(MakeEnvelope(NetworkBattleUri.BattleStart)).PlaySeq, Is.EqualTo(2));
        Assert.That(seq.AssignAndArchive(MakeEnvelope(NetworkBattleUri.Deal)).PlaySeq, Is.EqualTo(3));
    }

    [Test]
    public void NoStockControlPush_DoesNotAssignPlaySeqOrArchive()
    {
        var seq = new OutboundSequencer();
        var env = seq.WrapNoStock(MakeEnvelope(NetworkBattleUri.BattleFinish));

        Assert.That(env.PlaySeq, Is.Null);
        Assert.That(seq.Archive, Is.Empty);
    }

    [Test]
    public void Archive_ContainsArchivedEnvelopesKeyedByPlaySeq()
    {
        var seq = new OutboundSequencer();
        seq.AssignAndArchive(MakeEnvelope(NetworkBattleUri.Matched));
        seq.AssignAndArchive(MakeEnvelope(NetworkBattleUri.BattleStart));

        Assert.That(seq.Archive.Keys, Is.EquivalentTo(new[] { 1L, 2L }));
        Assert.That(seq.Archive[1L].Uri, Is.EqualTo(NetworkBattleUri.Matched));
    }

    [Test]
    public void Clear_empties_archive()
    {
        var seq = new OutboundSequencer();
        seq.AssignAndArchive(MakeEnvelope(NetworkBattleUri.Matched));
        seq.AssignAndArchive(MakeEnvelope(NetworkBattleUri.BattleStart));

        seq.Clear();

        Assert.That(seq.Archive, Is.Empty);
    }

    [Test]
    public void Clear_does_not_reset_next_seq()
    {
        // A post-Clear emit is a bug per the design (terminate has already fired),
        // but the impl must keep the seq stream monotonic if it does happen — no
        // silent re-use of playSeq=1 against a different envelope.
        var seq = new OutboundSequencer();
        seq.AssignAndArchive(MakeEnvelope(NetworkBattleUri.Matched));     // playSeq=1
        seq.AssignAndArchive(MakeEnvelope(NetworkBattleUri.BattleStart)); // playSeq=2

        seq.Clear();
        var post = seq.AssignAndArchive(MakeEnvelope(NetworkBattleUri.Deal));

        Assert.That(post.PlaySeq, Is.EqualTo(3), "_next must continue, not reset, after Clear.");
    }

    [Test]
    public void Clear_on_empty_sequencer_is_noop()
    {
        var seq = new OutboundSequencer();

        Assert.DoesNotThrow(() => seq.Clear());
        Assert.That(seq.Archive, Is.Empty);
    }
}
