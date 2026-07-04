using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Security.SteamSessionAuthentication;

namespace SVSim.UnitTests.Infrastructure;

/// <summary>
/// Replaces <see cref="SteamSessionAuthenticationHandler"/> in tests. Reads the viewer id from
/// the <c>X-Test-Viewer-Id</c> header, looks the viewer up, and builds the same claim set the
/// real handler would. Registered under the same scheme name so controller <c>[Authorize]</c>
/// attributes resolve without modification.
/// </summary>
internal class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string ViewerIdHeader = "X-Test-Viewer-Id";

    private readonly SVSimDbContext _dbContext;

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        SVSimDbContext dbContext)
        : base(options, logger, encoder)
    {
        _dbContext = dbContext;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ViewerIdHeader, out var raw))
        {
            return AuthenticateResult.NoResult();
        }

        if (!long.TryParse(raw.ToString(), out long viewerId))
        {
            return AuthenticateResult.Fail($"{ViewerIdHeader} is not a valid long.");
        }

        Viewer? viewer = await _dbContext.Viewers
            .AsNoTracking()
            .Include(v => v.SocialAccountConnections)
            .FirstOrDefaultAsync(v => v.Id == viewerId);

        if (viewer is null)
        {
            return AuthenticateResult.Fail($"No viewer with id {viewerId} — test forgot to seed.");
        }

        Context.SetViewer(viewer);

        var identity = new ClaimsIdentity(SteamAuthenticationConstants.SchemeName);
        identity.AddClaim(new Claim(ClaimTypes.Name, viewer.DisplayName));
        identity.AddClaim(new Claim(ShadowverseClaimTypes.ShortUdidClaim, viewer.ShortUdid.ToString()));
        identity.AddClaim(new Claim(ShadowverseClaimTypes.ViewerIdClaim, viewer.Id.ToString()));

        var steamConnection = viewer.SocialAccountConnections.FirstOrDefault();
        if (steamConnection is not null)
        {
            identity.AddClaim(new Claim(SteamAuthenticationConstants.SteamIdClaim, steamConnection.AccountId.ToString()));
        }

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SteamAuthenticationConstants.SchemeName);
        return AuthenticateResult.Success(ticket);
    }
}
