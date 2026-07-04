using NUnit.Framework;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.UnitTests.Services;

public class ShadowverseSessionServiceTests
{
    /// <summary>
    /// Fixture captured live from a fresh signup against this server. The client computed this
    /// exact SID locally and sent it on the next /check/game_start request. Pinning the formula
    /// here means any future refactor of <see cref="ShadowverseSessionService.ComputeClientSessionId"/>
    /// that drifts from <c>Cute/Cryptographer.MakeMd5(viewerId + udid)</c> will fail this test
    /// before the user discovers it as a decrypt failure on game_start.
    /// </summary>
    [Test]
    public void ComputeClientSessionId_matches_captured_fixture()
    {
        var svc = new ShadowverseSessionService();
        const long viewerId = 1;
        var udid = new System.Guid("62747917-93bc-454c-abb4-ef423b3c9317");

        string sid = svc.ComputeClientSessionId(viewerId, udid);

        Assert.That(sid, Is.EqualTo("dc4aac79d35fe15dfb6262e0071bb03c"));
    }

    [Test]
    public void StoreSessionForViewer_makes_sid_resolvable_to_udid()
    {
        var svc = new ShadowverseSessionService();
        const long viewerId = 1;
        var udid = new System.Guid("62747917-93bc-454c-abb4-ef423b3c9317");

        svc.StoreSessionForViewer(viewerId, udid);

        Assert.That(svc.GetUdidFromSessionId("dc4aac79d35fe15dfb6262e0071bb03c"), Is.EqualTo(udid));
    }

    [Test]
    public void StoreUdidForSessionId_evicts_oldest_when_cap_exceeded()
    {
        // Cap=3, insert 5 distinct SIDs; the two earliest must be evicted.
        var svc = new ShadowverseSessionService(maxEntries: 3);
        var udid = new System.Guid("62747917-93bc-454c-abb4-ef423b3c9317");

        svc.StoreUdidForSessionId("sid-1", udid);
        svc.StoreUdidForSessionId("sid-2", udid);
        svc.StoreUdidForSessionId("sid-3", udid);
        svc.StoreUdidForSessionId("sid-4", udid);
        svc.StoreUdidForSessionId("sid-5", udid);

        Assert.That(svc.GetUdidFromSessionId("sid-1"), Is.Null, "Oldest entry must be evicted.");
        Assert.That(svc.GetUdidFromSessionId("sid-2"), Is.Null, "Second-oldest entry must be evicted.");
        Assert.That(svc.GetUdidFromSessionId("sid-3"), Is.EqualTo(udid));
        Assert.That(svc.GetUdidFromSessionId("sid-4"), Is.EqualTo(udid));
        Assert.That(svc.GetUdidFromSessionId("sid-5"), Is.EqualTo(udid));
    }

    [Test]
    public void StoreUdidForSessionId_re_storing_same_sid_does_not_grow_queue()
    {
        // Cap=2. Store sid-A, then re-store sid-A many times, then store sid-B and sid-C.
        // The re-stores must NOT count toward the cap — sid-A should still resolve after
        // sid-B and sid-C land, because only two distinct SIDs are tracked.
        var svc = new ShadowverseSessionService(maxEntries: 2);
        var udid = new System.Guid("62747917-93bc-454c-abb4-ef423b3c9317");

        svc.StoreUdidForSessionId("sid-A", udid);
        for (int i = 0; i < 20; i++) svc.StoreUdidForSessionId("sid-A", udid);
        svc.StoreUdidForSessionId("sid-B", udid);

        Assert.That(svc.GetUdidFromSessionId("sid-A"), Is.EqualTo(udid), "sid-A must still resolve after re-stores.");
        Assert.That(svc.GetUdidFromSessionId("sid-B"), Is.EqualTo(udid));

        // sid-C pushes us over the cap → sid-A (oldest) evicted.
        svc.StoreUdidForSessionId("sid-C", udid);
        Assert.That(svc.GetUdidFromSessionId("sid-A"), Is.Null);
        Assert.That(svc.GetUdidFromSessionId("sid-B"), Is.EqualTo(udid));
        Assert.That(svc.GetUdidFromSessionId("sid-C"), Is.EqualTo(udid));
    }

    [Test]
    public void Constructor_rejects_non_positive_cap()
    {
        Assert.Throws<System.ArgumentOutOfRangeException>(() => new ShadowverseSessionService(maxEntries: 0));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => new ShadowverseSessionService(maxEntries: -1));
    }
}
