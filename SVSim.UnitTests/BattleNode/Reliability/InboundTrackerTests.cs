using NUnit.Framework;
using SVSim.BattleNode.Reliability;

namespace SVSim.UnitTests.BattleNode.Reliability;

[TestFixture]
public class InboundTrackerTests
{
    [Test]
    public void Observe_FirstSeenPubSeq_ReturnsDispatchTrue()
    {
        var t = new InboundTracker();
        Assert.That(t.Observe(pubSeq: 1), Is.True);
    }

    [Test]
    public void Observe_SamePubSeqTwice_SecondReturnsFalse()
    {
        var t = new InboundTracker();
        t.Observe(1);
        Assert.That(t.Observe(1), Is.False);
    }

    [Test]
    public void Observe_DifferentPubSeqs_BothDispatch()
    {
        var t = new InboundTracker();
        Assert.That(t.Observe(1), Is.True);
        Assert.That(t.Observe(2), Is.True);
    }

    [Test]
    public void HighWaterMark_TracksHighestObserved()
    {
        var t = new InboundTracker();
        t.Observe(3);
        t.Observe(1);
        t.Observe(5);
        Assert.That(t.HighWaterMark, Is.EqualTo(5));
    }

    [Test]
    public void Observe_returns_false_for_pubSeq_below_high_water_minus_window()
    {
        // Drive watermark past WindowSize, then re-observe seq=1.
        // The watermark guard rejects it without consulting ring membership.
        var t = new InboundTracker();
        for (long i = 1; i <= InboundTracker.WindowSize + 10; i++) t.Observe(i);

        Assert.That(t.Observe(pubSeq: 1), Is.False);
    }

    [Test]
    public void Evicted_seq_stays_rejected_by_watermark()
    {
        // Load-bearing invariant: even after seq=1 is evicted from the ring,
        // re-observing it must still return false because the watermark guard
        // catches `1 <= 257 - 256`. Otherwise window eviction would admit
        // an old seq as novel.
        var t = new InboundTracker();
        for (long i = 1; i <= InboundTracker.WindowSize; i++) t.Observe(i); // ring: {1..256}
        t.Observe(InboundTracker.WindowSize + 1);                            // evicts 1; watermark=257

        Assert.That(t.Observe(pubSeq: 1), Is.False);
    }

    [Test]
    public void Within_window_dedup_still_fires_after_eviction()
    {
        var t = new InboundTracker();
        for (long i = 1; i <= InboundTracker.WindowSize; i++) t.Observe(i); // ring: {1..256}, watermark=256
        t.Observe(pubSeq: 300);                                              // ring: {45..256, 300}, watermark=300

        // 200 is in the ring → dedup catches it.
        Assert.That(t.Observe(pubSeq: 200), Is.False, "Within-window dedup still works after eviction.");

        // 44 was evicted, but is below HighWaterMark - WindowSize = 44 → stale guard.
        Assert.That(t.Observe(pubSeq: 44), Is.False, "Below-window guard rejects evicted seqs.");
    }

    [Test]
    public void Memory_bound_stays_constant_after_many_observes()
    {
        // The dedup structures must not grow without bound. Internals are accessible
        // via the existing InternalsVisibleTo("SVSim.UnitTests"). Use reflection so
        // the test doesn't force the field-access modifier choice in production.
        var t = new InboundTracker();
        for (long i = 1; i <= 10_000; i++) t.Observe(i);

        var seenField = typeof(InboundTracker).GetField("_seen",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var orderField = typeof(InboundTracker).GetField("_order",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.That(seenField, Is.Not.Null);
        Assert.That(orderField, Is.Not.Null);

        var seen = (System.Collections.Generic.IReadOnlyCollection<long>)seenField!.GetValue(t)!;
        var order = (System.Collections.Generic.IReadOnlyCollection<long>)orderField!.GetValue(t)!;
        Assert.That(seen.Count, Is.LessThanOrEqualTo(InboundTracker.WindowSize),
            $"_seen must stay <= WindowSize ({InboundTracker.WindowSize}).");
        Assert.That(order.Count, Is.LessThanOrEqualTo(InboundTracker.WindowSize),
            $"_order must stay <= WindowSize ({InboundTracker.WindowSize}).");
    }

    [Test]
    public void HighWaterMark_is_not_moved_backward_by_eviction()
    {
        var t = new InboundTracker();
        for (long i = 1; i <= InboundTracker.WindowSize; i++) t.Observe(i);
        var beforeEviction = t.HighWaterMark;
        t.Observe(InboundTracker.WindowSize + 1); // eviction fires here

        Assert.That(t.HighWaterMark, Is.GreaterThanOrEqualTo(beforeEviction));
        Assert.That(t.HighWaterMark, Is.EqualTo(InboundTracker.WindowSize + 1));
    }
}
