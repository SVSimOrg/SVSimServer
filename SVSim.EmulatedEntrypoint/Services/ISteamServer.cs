namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Thin wrapper around the static <c>Steamworks.SteamServer</c> API. Exists purely so
/// <see cref="SteamSessionService"/> can be unit-tested — Facepunch.Steamworks is a static
/// class with process-global state that can't be mocked or run twice in the same test host.
///
/// Only the operations <see cref="SteamSessionService"/> actually invokes are exposed. Add
/// methods here as needed rather than expanding the surface speculatively.
/// </summary>
public interface ISteamServer
{
    /// <summary>Initialize the underlying Steam game server with the given app id. Idempotent.</summary>
    void Initialize(int appId);

    /// <summary>
    /// Open an auth session for the given steam id with the given ticket bytes. Returns true
    /// when Steam accepts the ticket. Returns false on any rejection — most commonly the
    /// "duplicate request" case (the steam id already has an open session on this server),
    /// which is the failure mode <see cref="SteamSessionService"/> resolves by calling
    /// <see cref="EndSession"/> first.
    /// </summary>
    bool BeginAuthSession(byte[] ticket, ulong steamId);

    /// <summary>
    /// Close any active auth session for this steam id. Safe to call when no session exists
    /// (Steam SDK no-ops). Must be called before a second <see cref="BeginAuthSession"/> for
    /// the same steam id or Steam will reject the new call as a duplicate request.
    /// </summary>
    void EndSession(ulong steamId);

    /// <summary>Tear down the game server. Implicitly ends every open auth session.</summary>
    void Shutdown();
}
