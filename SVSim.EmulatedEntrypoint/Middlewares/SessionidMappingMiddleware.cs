using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Security;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Middlewares;

/// <summary>
/// Maps an incoming request's session id to a udid if both are present.
/// </summary>
public class SessionidMappingMiddleware : IMiddleware
{
    private readonly ShadowverseSessionService _shadowverseSessionService;
    private readonly ILogger<SessionidMappingMiddleware> _logger;

    public SessionidMappingMiddleware(
        ShadowverseSessionService shadowverseSessionService,
        ILogger<SessionidMappingMiddleware> logger)
    {
        _shadowverseSessionService = shadowverseSessionService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // NOTE: the bool names below were historically inverted (hasSessionId held UDID and
        // vice versa). Variable names corrected in-place; behavior unchanged.
        bool hasUdid = context.Request.Headers.TryGetValue(NetworkConstants.UdidHeaderName, out var udid);
        bool hasSid = context.Request.Headers.TryGetValue(NetworkConstants.SessionIdHeaderName, out var sid);

        if (hasUdid && hasSid)
        {
            string? sidValue = sid.FirstOrDefault();
            string? encodedUdid = udid.FirstOrDefault();
            try
            {
                string? decoded = Encryption.Decode(encodedUdid);
                Guid parsedUdid = Guid.Parse(decoded);
                _shadowverseSessionService.StoreUdidForSessionId(sidValue, parsedUdid);
                _logger.LogDebug(
                    "Stored SID→UDID mapping for {Path} (sid={Sid}, udid={Udid}).",
                    context.Request.Path, sidValue, parsedUdid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to decode/parse UDID header for {Path} (sid={Sid}, encodedUdidLen={EncodedUdidLen}). " +
                    "Downstream translation will fall back to Guid.Empty and almost certainly fail msgpack decrypt.",
                    context.Request.Path, sidValue, encodedUdid?.Length ?? 0);
            }
        }
        else if (hasSid && !hasUdid)
        {
            // Normal post-signup pattern: once /tool/signup completes, the client switches
            // to SID-only headers (see CheckController.Signup comment). The translation
            // middleware resolves UDID via the SID→UDID cache we stored on the signup
            // request, so decryption succeeds. Debug-level so the ~50 post-signup requests
            // per session don't drown out real warnings.
            _logger.LogDebug(
                "SID-only headers for {Path} (post-signup pattern) — SID={Sid}",
                context.Request.Path, sid.FirstOrDefault());
        }
        else if (hasUdid && !hasSid)
        {
            // UDID without SID is anomalous — no SID means the translation middleware has
            // no cache key to look up and will fall back to Guid.Empty, surfacing as a
            // generic msgpack/decrypt error.
            _logger.LogWarning(
                "UDID header present without SID for {Path}. Translation will fall back to " +
                "Guid.Empty as the encryption key.",
                context.Request.Path);
        }

        await next.Invoke(context);
    }
}
