using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.UnitTests.Services;

public class HomeDialogSessionTrackerTests
{
    [Test]
    public void TryReserve_returns_true_first_time_and_false_on_repeat_for_same_viewer_and_dialog()
    {
        var tracker = new HomeDialogSessionTracker();

        Assert.That(tracker.TryReserve(viewerShortUdid: 100, dialogId: 1), Is.True);
        Assert.That(tracker.TryReserve(viewerShortUdid: 100, dialogId: 1), Is.False);
    }

    [Test]
    public void TryReserve_is_independent_across_viewers()
    {
        var tracker = new HomeDialogSessionTracker();

        Assert.That(tracker.TryReserve(100, 1), Is.True);
        Assert.That(tracker.TryReserve(200, 1), Is.True, "viewer 200 must see the dialog even though viewer 100 already did");
    }

    [Test]
    public void TryReserve_is_independent_across_dialog_ids_for_one_viewer()
    {
        var tracker = new HomeDialogSessionTracker();

        Assert.That(tracker.TryReserve(100, 1), Is.True);
        Assert.That(tracker.TryReserve(100, 2), Is.True);
        Assert.That(tracker.TryReserve(100, 1), Is.False);
        Assert.That(tracker.TryReserve(100, 2), Is.False);
    }

    [Test]
    public void TryReserve_is_thread_safe_under_concurrent_calls_for_one_viewer()
    {
        var tracker = new HomeDialogSessionTracker();
        const int dialogId = 42;
        const int parallelism = 200;
        int trueCount = 0;

        Parallel.For(0, parallelism, _ =>
        {
            if (tracker.TryReserve(viewerShortUdid: 1, dialogId: dialogId))
                Interlocked.Increment(ref trueCount);
        });

        Assert.That(trueCount, Is.EqualTo(1),
            "Exactly one thread must win the reservation; the rest must observe false.");
    }
}
