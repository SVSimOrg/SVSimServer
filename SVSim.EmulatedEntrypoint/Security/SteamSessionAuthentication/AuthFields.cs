namespace SVSim.EmulatedEntrypoint.Security.SteamSessionAuthentication;

/// <summary>
/// Auth tuple extracted from the decrypted msgpack request body BEFORE it gets pivoted into
/// the action's typed DTO. Stashed into <c>HttpContext.Items</c> under <see cref="ContextKey"/>
/// by <c>ShadowverseTranslationMiddleware</c> so <c>SteamSessionAuthenticationHandler</c> can
/// read the ticket without depending on the DTO modelling these fields.
///
/// History: see <c>docs/superpowers/specs/2026-06-02-baseRequest-auth-footgun-improvement.md</c>.
/// The pre-existing route required every authed DTO to inherit <c>BaseRequest</c> (otherwise
/// the msgpack→DTO→JSON pivot dropped the auth fields and the handler silently 401'd live).
/// Surfacing the fields via a separate channel decouples auth from DTO shape entirely.
/// </summary>
public sealed class AuthFields
{
    /// <summary>Items key under which the middleware stashes / the handler reads the auth tuple.</summary>
    public const string ContextKey = "SVSim.AuthFields";

    public string? ViewerId { get; init; }
    public ulong SteamId { get; init; }
    public string? SteamSessionTicket { get; init; }
}
