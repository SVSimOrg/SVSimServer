using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.UnitTests.Services;

/// <summary>Covers the always-accept contract and no-throw stubs of the Dev-only ISteamServer bypass.</summary>
[TestFixture]
public class DevAlwaysValidSteamServerTests
{
    [Test]
    public void BeginAuthSession_accepts_any_ticket_for_any_steamId()
    {
        var sut = new DevAlwaysValidSteamServer(NullLogger<DevAlwaysValidSteamServer>.Instance);

        Assert.That(sut.BeginAuthSession(new byte[] { 0xDE, 0xAD }, 900001UL), Is.True);
        Assert.That(sut.BeginAuthSession(System.Array.Empty<byte>(), 0UL), Is.True);
    }

    [Test]
    public void Lifecycle_methods_do_not_throw()
    {
        var sut = new DevAlwaysValidSteamServer(NullLogger<DevAlwaysValidSteamServer>.Instance);
        Assert.DoesNotThrow(() => { sut.Initialize(453480); sut.EndSession(900001UL); sut.Shutdown(); });
    }
}
