using Steamworks;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Production <see cref="ISteamServer"/> backed by Facepunch.Steamworks 2.3.3.
/// Initialization is process-global; registering this as a singleton is required.
/// </summary>
public sealed class FacepunchSteamServer : ISteamServer
{
    private readonly object _initLock = new();
    private bool _initialized;

    public void Initialize(int appId)
    {
        if (_initialized) return;
        lock (_initLock)
        {
            if (_initialized) return;
            SteamServer.Init(appId, new SteamServerInit
            {
                GamePort = default,
                QueryPort = default,
            });
            _initialized = true;
        }
    }

    public bool BeginAuthSession(byte[] ticket, ulong steamId) =>
        SteamServer.BeginAuthSession(ticket, new SteamId { Value = steamId });

    public void EndSession(ulong steamId) =>
        SteamServer.EndSession(new SteamId { Value = steamId });

    public void Shutdown()
    {
        if (!_initialized) return;
        SteamServer.Shutdown();
        _initialized = false;
    }
}
