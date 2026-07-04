using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Security;
using SVSim.EmulatedEntrypoint.Security.SteamSessionAuthentication;

namespace SVSim.EmulatedEntrypoint.Controllers
{
    /// <summary>
    /// A base controller for SVSim with helpers for getting some values.
    /// </summary>
    [Route("[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = SteamAuthenticationConstants.SchemeName)]
    public abstract class SVSimController : ControllerBase
    {
        /// <summary>
        /// Reads the authenticated viewer's internal id from the ViewerId claim populated by
        /// <c>SteamSessionAuthenticationHandler</c>. Returns false (and viewerId = 0) when the
        /// claim is missing or unparseable — handler should respond with Unauthorized().
        /// </summary>
        protected bool TryGetViewerId(out long viewerId)
        {
            viewerId = 0;
            var claim = User.Claims.FirstOrDefault(c => c.Type == ShadowverseClaimTypes.ViewerIdClaim)?.Value;
            return claim is not null && long.TryParse(claim, out viewerId);
        }
    }
}
