using Microsoft.Extensions.Logging;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Development-only <see cref="ISteamServer"/> that accepts every ticket without contacting
/// Steam. Selected in <c>Program.cs</c> when <c>Auth:BypassSteamTicket</c> is true, so clients
/// with a synthetic (non-Steam) identity — e.g. a second instance on the same machine for the
/// two-client PvP smoke — can authenticate. NEVER select this outside local dev: it turns the
/// Steam ticket gate into a no-op for the whole process.
/// </summary>
public sealed class DevAlwaysValidSteamServer : ISteamServer
{
    private readonly ILogger<DevAlwaysValidSteamServer> _logger;

    public DevAlwaysValidSteamServer(ILogger<DevAlwaysValidSteamServer> logger) => _logger = logger;

    public void Initialize(int appId) { }

    public bool BeginAuthSession(byte[] ticket, ulong steamId)
    {
        _logger.LogWarning(
            "DEV Steam bypass: accepting ticket for steamId {SteamId} WITHOUT validation (ticketLen={Len}).",
            steamId, ticket.Length);
        return true;
    }

    public void EndSession(ulong steamId) { }

    public void Shutdown() { }
}
