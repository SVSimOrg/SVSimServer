using System.Collections.Concurrent;
using Microsoft.Extensions.Logging.Abstractions;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.UnitTests.Services;

/// <summary>
/// Coverage for the end-then-begin lifecycle that fixes the "401 on second client launch"
/// bug (audit: game-start-steam-ticket-401-on-client-restart-2026-05-23). The Facepunch
/// SDK can't be mocked directly (static + process-global state), so these tests run against
/// the <see cref="ISteamServer"/> wrapper via <see cref="FakeSteamServer"/>, which models the
/// part of Steam's behavior we actually care about: a second BeginAuthSession for a steamId
/// that already has an open session is rejected unless EndSession is called first.
/// </summary>
public class SteamSessionServiceTests
{
    private const ulong AliceSteamId = 76_561_198_000_000_001UL;
    private const ulong BobSteamId   = 76_561_198_000_000_002UL;
    private const string TicketA = "deadbeef";
    private const string TicketB = "cafef00d";

    [Test]
    public void IsTicketValidForUser_first_call_invokes_BeginAuthSession()
    {
        var steam = new FakeSteamServer();
        using var svc = new SteamSessionService(steam, NullLogger<SteamSessionService>.Instance);

        Assert.That(svc.IsTicketValidForUser(TicketA, AliceSteamId), Is.True);
        Assert.That(steam.BeginCallCount, Is.EqualTo(1));
        Assert.That(steam.EndCallCount, Is.EqualTo(0));
    }

    [Test]
    public void IsTicketValidForUser_cache_hit_skips_steam_call()
    {
        var steam = new FakeSteamServer();
        using var svc = new SteamSessionService(steam, NullLogger<SteamSessionService>.Instance);

        Assert.That(svc.IsTicketValidForUser(TicketA, AliceSteamId), Is.True);
        Assert.That(svc.IsTicketValidForUser(TicketA, AliceSteamId), Is.True);
        Assert.That(svc.IsTicketValidForUser(TicketA, AliceSteamId), Is.True);

        Assert.That(steam.BeginCallCount, Is.EqualTo(1), "Same ticket bytes should not re-call BeginAuthSession.");
    }

    [Test]
    public void IsTicketValidForUser_cache_hit_with_wrong_steamId_returns_false()
    {
        var steam = new FakeSteamServer();
        using var svc = new SteamSessionService(steam, NullLogger<SteamSessionService>.Instance);

        Assert.That(svc.IsTicketValidForUser(TicketA, AliceSteamId), Is.True);
        // Same ticket bytes presented for a different steamId — must reject without re-asking Steam.
        Assert.That(svc.IsTicketValidForUser(TicketA, BobSteamId), Is.False);
        Assert.That(steam.BeginCallCount, Is.EqualTo(1));
    }

    [Test]
    public void IsTicketValidForUser_new_ticket_same_steamId_ends_prior_session_and_begins_new()
    {
        // This is the audit-doc regression: client restart → new ticket bytes → same steamId.
        // Without the end-then-begin, the second BeginAuthSession returns DuplicateRequest
        // (false in Facepunch 2.3.3) and the user gets 401 until the server restarts.
        var steam = new FakeSteamServer();
        using var svc = new SteamSessionService(steam, NullLogger<SteamSessionService>.Instance);

        Assert.That(svc.IsTicketValidForUser(TicketA, AliceSteamId), Is.True, "first launch must succeed");
        Assert.That(svc.IsTicketValidForUser(TicketB, AliceSteamId), Is.True, "second launch must succeed (this is the bug)");

        Assert.That(steam.BeginCallCount, Is.EqualTo(2));
        Assert.That(steam.EndCallCount, Is.EqualTo(1), "EndSession must be called between the two Begin calls");
        Assert.That(steam.EndedSteamIds.Single(), Is.EqualTo(AliceSteamId));
    }

    [Test]
    public void IsTicketValidForUser_does_not_end_session_for_different_steamId()
    {
        // Two different users authenticating shouldn't trigger any EndSession — sessions are
        // per-steamId, so Bob's login has no bearing on Alice's lease.
        var steam = new FakeSteamServer();
        using var svc = new SteamSessionService(steam, NullLogger<SteamSessionService>.Instance);

        Assert.That(svc.IsTicketValidForUser(TicketA, AliceSteamId), Is.True);
        Assert.That(svc.IsTicketValidForUser(TicketB, BobSteamId), Is.True);

        Assert.That(steam.EndCallCount, Is.EqualTo(0));
    }

    [Test]
    public void IsTicketValidForUser_empty_ticket_returns_false_without_calling_steam()
    {
        var steam = new FakeSteamServer();
        using var svc = new SteamSessionService(steam, NullLogger<SteamSessionService>.Instance);

        Assert.That(svc.IsTicketValidForUser("", AliceSteamId), Is.False);
        Assert.That(svc.IsTicketValidForUser(null!, AliceSteamId), Is.False);
        Assert.That(steam.InitializeCallCount, Is.EqualTo(0));
        Assert.That(steam.BeginCallCount, Is.EqualTo(0));
    }

    [Test]
    public void IsTicketValidForUser_when_steam_rejects_does_not_cache()
    {
        var steam = new FakeSteamServer { RejectAllBegins = true };
        using var svc = new SteamSessionService(steam, NullLogger<SteamSessionService>.Instance);

        Assert.That(svc.IsTicketValidForUser(TicketA, AliceSteamId), Is.False);
        // After rejection, a retry with the same ticket should ask Steam again, not return false from cache.
        Assert.That(svc.IsTicketValidForUser(TicketA, AliceSteamId), Is.False);
        Assert.That(steam.BeginCallCount, Is.EqualTo(2), "rejected tickets must not be cached");
    }

    [Test]
    public void Dispose_ends_all_active_sessions_then_shuts_down()
    {
        var steam = new FakeSteamServer();
        var svc = new SteamSessionService(steam, NullLogger<SteamSessionService>.Instance);
        svc.IsTicketValidForUser(TicketA, AliceSteamId);
        svc.IsTicketValidForUser(TicketB, BobSteamId);

        svc.Dispose();

        Assert.That(steam.EndedSteamIds, Is.EquivalentTo(new[] { AliceSteamId, BobSteamId }));
        Assert.That(steam.ShutdownCallCount, Is.EqualTo(1));
    }

    [Test]
    public async Task Concurrent_calls_for_same_steamId_serialize_through_the_per_user_lock()
    {
        // Race: two threads call IsTicketValidForUser simultaneously with DIFFERENT tickets
        // for the same steamId. Without per-user serialization both Begins race past the
        // active-session check and one of them ends up with Steam in an inconsistent state
        // (DuplicateRequest, or worse, the wrong ticket persisted as "active"). The
        // post-condition we assert: BeginAuthSession was called twice with exactly one
        // EndSession between them — never two Begins back-to-back with no End.
        var steam = new FakeSteamServer { DelayBeginByMilliseconds = 25 };
        using var svc = new SteamSessionService(steam, NullLogger<SteamSessionService>.Instance);

        var t1 = Task.Run(() => svc.IsTicketValidForUser(TicketA, AliceSteamId));
        var t2 = Task.Run(() => svc.IsTicketValidForUser(TicketB, AliceSteamId));
        bool[] results = await Task.WhenAll(t1, t2);

        Assert.That(results, Is.All.True);
        Assert.That(steam.BeginCallCount, Is.EqualTo(2));
        Assert.That(steam.EndCallCount, Is.EqualTo(1));
        Assert.That(steam.OperationOrder, Is.EqualTo(new[] { "Begin", "End", "Begin" }),
            "Operations must be serialized per-steamId: Begin → (replace) → End → Begin.");
    }

    /// <summary>
    /// In-memory <see cref="ISteamServer"/> that models the part of Steam's behavior our
    /// production code defends against: BeginAuthSession returns false (Facepunch's
    /// collapsed representation of DuplicateRequest) when the steamId already has an open
    /// session.
    /// </summary>
    private sealed class FakeSteamServer : ISteamServer
    {
        private readonly object _gate = new();
        private readonly HashSet<ulong> _openSessions = new();
        private readonly List<string> _operationOrder = new();
        private readonly List<ulong> _endedSteamIds = new();

        public int InitializeCallCount { get; private set; }
        public int BeginCallCount { get; private set; }
        public int EndCallCount { get; private set; }
        public int ShutdownCallCount { get; private set; }
        public bool RejectAllBegins { get; set; }
        public int DelayBeginByMilliseconds { get; set; }

        public IReadOnlyList<ulong> EndedSteamIds => _endedSteamIds;
        public IReadOnlyList<string> OperationOrder => _operationOrder;

        public void Initialize(int appId)
        {
            lock (_gate) InitializeCallCount++;
        }

        public bool BeginAuthSession(byte[] ticket, ulong steamId)
        {
            if (DelayBeginByMilliseconds > 0) Thread.Sleep(DelayBeginByMilliseconds);
            lock (_gate)
            {
                BeginCallCount++;
                _operationOrder.Add("Begin");
                if (RejectAllBegins) return false;
                if (!_openSessions.Add(steamId))
                {
                    // Duplicate request — Steam's failure mode for "this steamId already
                    // has an open session and you didn't EndSession first".
                    return false;
                }
                return true;
            }
        }

        public void EndSession(ulong steamId)
        {
            lock (_gate)
            {
                EndCallCount++;
                _operationOrder.Add("End");
                _endedSteamIds.Add(steamId);
                _openSessions.Remove(steamId);
            }
        }

        public void Shutdown()
        {
            lock (_gate)
            {
                ShutdownCallCount++;
                _openSessions.Clear();
            }
        }
    }
}
