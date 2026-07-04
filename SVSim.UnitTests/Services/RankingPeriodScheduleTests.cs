using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.UnitTests.Services;

public class RankingPeriodScheduleTests
{
    // 2026-06-09 17:00 UTC = 2026-06-10 02:00 JST — clearly in JST's June 2026 month.
    private static readonly DateTime CaptureNowUtc =
        new(2026, 6, 9, 17, 0, 0, DateTimeKind.Utc);

    [Test]
    public void RankMatch_at_capture_time_has_current_period_id_122()
    {
        var list = RankingPeriodSchedule.GenerateFor(
            RankingPeriodSchedule.Family.RankMatch, CaptureNowUtc);

        Assert.That(list, Is.Not.Empty);
        Assert.That(list[0].Id, Is.EqualTo("122"));
        Assert.That(list[0].BeginTime, Is.EqualTo("2026-06-01 02:00:00"));
        Assert.That(list[0].EndTime, Is.EqualTo("2026-07-01 01:59:59"));
        Assert.That(list.Count, Is.EqualTo(122));
    }

    [Test]
    public void MasterPoint_at_capture_time_has_current_period_id_120()
    {
        var list = RankingPeriodSchedule.GenerateFor(
            RankingPeriodSchedule.Family.MasterPoint, CaptureNowUtc);
        Assert.That(list[0].Id, Is.EqualTo("120"));
        Assert.That(list.Count, Is.EqualTo(120));
    }

    [Test]
    public void TwoPick_at_capture_time_has_current_period_id_119()
    {
        var list = RankingPeriodSchedule.GenerateFor(
            RankingPeriodSchedule.Family.TwoPick, CaptureNowUtc);
        Assert.That(list[0].Id, Is.EqualTo("119"));
        Assert.That(list.Count, Is.EqualTo(119));
    }

    [Test]
    public void Sealed_at_capture_time_has_current_period_id_62()
    {
        var list = RankingPeriodSchedule.GenerateFor(
            RankingPeriodSchedule.Family.Sealed, CaptureNowUtc);
        Assert.That(list[0].Id, Is.EqualTo("62"));
        Assert.That(list.Count, Is.EqualTo(62));
    }

    [Test]
    public void Schedule_is_descending_by_id()
    {
        var list = RankingPeriodSchedule.GenerateFor(
            RankingPeriodSchedule.Family.RankMatch, CaptureNowUtc);
        for (int i = 1; i < list.Count; i++)
        {
            Assert.That(int.Parse(list[i - 1].Id), Is.GreaterThan(int.Parse(list[i].Id)),
                $"position {i - 1} ({list[i - 1].Id}) should be > position {i} ({list[i].Id})");
        }
    }

    [Test]
    public void TryFindById_returns_entry_when_id_in_range()
    {
        var entry = RankingPeriodSchedule.TryFindById(
            RankingPeriodSchedule.Family.RankMatch, periodId: 122, CaptureNowUtc);
        Assert.That(entry, Is.Not.Null);
        Assert.That(entry!.BeginTime, Is.EqualTo("2026-06-01 02:00:00"));
    }

    [Test]
    public void TryFindById_returns_null_for_unknown_id()
    {
        var entry = RankingPeriodSchedule.TryFindById(
            RankingPeriodSchedule.Family.RankMatch, periodId: 9999, CaptureNowUtc);
        Assert.That(entry, Is.Null);
    }
}
