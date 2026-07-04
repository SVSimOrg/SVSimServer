using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Viewer;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Security.SteamSessionAuthentication;

public class SteamSessionAuthenticationHandler : AuthenticationHandler<SteamAuthenticationHandlerOptions>
{
    private readonly SteamSessionService _sessionService;
    private readonly IViewerRepository _viewerRepository;
    public SteamSessionAuthenticationHandler(IOptionsMonitor<SteamAuthenticationHandlerOptions> options, ILoggerFactory logger, UrlEncoder encoder, SteamSessionService sessionService, IViewerRepository viewerRepository) : base(options, logger, encoder)
    {
        _sessionService = sessionService;
        _viewerRepository = viewerRepository;
    }

    protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string path = Request.Path;
        // WebSocket upgrades carry no body — Request.Body.Seek throws NotSupportedException
        // on Kestrel's HttpRequestStream. The battle node has its own per-connection auth
        // (encrypted viewerId header validated against the matched battle id), so the
        // Steam handler has nothing to do here. Returning NoResult lets the request proceed
        // unauthenticated to the WS endpoint.
        // Header-based detection: Context.WebSockets.IsWebSocketRequest needs UseWebSockets()
        // to have already run, but UseBattleNode (which calls UseWebSockets) is registered
        // AFTER UseAuthentication in Program.cs. Reading the raw Upgrade header works
        // regardless of middleware order.
        if (string.Equals(Request.Headers["Upgrade"].ToString(), "websocket", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        // Read the auth tuple from HttpContext.Items, populated by ShadowverseTranslationMiddleware
        // off the raw decrypted msgpack dict BEFORE the action's typed DTO deserialize. This
        // decouples auth from DTO shape — see AuthFields and the design spec at
        // docs/superpowers/specs/2026-06-02-baseRequest-auth-footgun-improvement.md. The prior
        // approach re-parsed Request.Body as JSON into a BaseRequest; any action whose DTO didn't
        // inherit BaseRequest silently 401'd because the msgpack→DTO→JSON pivot dropped the fields.
        if (Context.Items[AuthFields.ContextKey] is not AuthFields auth)
        {
            Logger.LogWarning(
                "Auth: no AuthFields in HttpContext.Items on {Path}. The translation middleware " +
                "either didn't run (non-Unity UA?) or the body wasn't a msgpack map.",
                path);
            return AuthenticateResult.Fail("Invalid request body.");
        }

        if (string.IsNullOrEmpty(auth.SteamSessionTicket))
        {
            Logger.LogWarning(
                "Auth: request body missing steam_session_ticket on {Path} (hasViewerId={HasViewerId}, steamId={SteamId}).",
                path, !string.IsNullOrEmpty(auth.ViewerId), auth.SteamId);
            return AuthenticateResult.Fail("Invalid request body.");
        }

        // Check steam session validity
        bool sessionIsValid = _sessionService.IsTicketValidForUser(auth.SteamSessionTicket, auth.SteamId);
        if (!sessionIsValid)
        {
            Logger.LogWarning(
                "Auth: Steam ticket rejected on {Path} for steamId={SteamId} (ticketLen={TicketLen}). " +
                "See SteamSessionService logs above for the underlying Steam reason (BeginAuthSession failure, duplicate, etc.).",
                path, auth.SteamId, auth.SteamSessionTicket.Length);
            return AuthenticateResult.Fail("Invalid ticket.");
        }

        Viewer? viewer =
            await _viewerRepository.GetViewerBySocialConnection(SocialAccountType.Steam, auth.SteamId);

        if (viewer is null)
        {
            // Find-or-link: first authenticated request after /tool/signup. The client signed up
            // anonymously and has no Steam social row yet; if the UDID resolves to a viewer, attach
            // Steam to it now so subsequent requests hit the fast SteamId path. The unique index
            // on SocialAccountConnection (AccountType, AccountId) — declared in OnModelCreating —
            // is the second-layer dedup backstop: if two concurrent first-touches both pass the
            // .Any(...) check in LinkSteamToViewer, the second SaveChanges throws cleanly instead
            // of silently duplicating connections.
            Guid? udid = Context.GetUdid();
            if (udid is Guid u && u != Guid.Empty)
            {
                viewer = await _viewerRepository.GetViewerByUdid(u);
                if (viewer is not null)
                {
                    await _viewerRepository.LinkSteamToViewer(viewer.Id, auth.SteamId);
                    // Re-read with socials so transition_account_data downstream sees the new link.
                    viewer = await _viewerRepository.GetViewerWithSocials(viewer.Id) ?? viewer;
                    Logger.LogInformation(
                        "Auth: linked steamId={SteamId} to UDID-keyed viewer_id={ViewerId} on {Path} (first-Steam-touch).",
                        auth.SteamId, viewer.Id, path);
                }
            }

            if (viewer is null)
            {
                Logger.LogWarning(
                    "Auth: no viewer linked to steamId={SteamId} on {Path}, and no UDID-keyed viewer to link to. " +
                    "Client must call /tool/signup before authenticated endpoints.",
                    auth.SteamId, path);
                return AuthenticateResult.Fail("User not found.");
            }
        }
        
        // Add viewer to context
        Context.SetViewer(viewer);

        // Build identity
        ClaimsIdentity identity = new ClaimsIdentity(SteamAuthenticationConstants.SchemeName);
        identity.AddClaim(new Claim(ClaimTypes.Name, viewer.DisplayName));
        identity.AddClaim(new Claim(ShadowverseClaimTypes.ShortUdidClaim, viewer.ShortUdid.ToString()));
        identity.AddClaim(new Claim(ShadowverseClaimTypes.ViewerIdClaim, viewer.Id.ToString()));
        identity.AddClaim(new Claim(SteamAuthenticationConstants.SteamIdClaim, auth.SteamId.ToString()));
        
        // Build and return final ticket
        AuthenticationTicket ticket =
            new AuthenticationTicket(new ClaimsPrincipal(identity), SteamAuthenticationConstants.SchemeName);
        return AuthenticateResult.Success(ticket);
    }
}