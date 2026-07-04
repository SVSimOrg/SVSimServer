using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Extensions;

public static class HttpContextExtensions
{
    private const string ViewerItemName = "SVSimViewer";

    public static Viewer? GetViewer(this HttpContext context)
    {
        if (context.Items.TryGetValue(ViewerItemName, out object? viewer))
        {
            return viewer as Viewer;
        }

        return null;
    }

    public static Viewer SetViewer(this HttpContext context, Viewer viewer)
    {
        context.Items[ViewerItemName] = viewer;
        return viewer;
    }

    /// <summary>
    /// Resolves the client's UDID for this request by looking up the SID header in the
    /// in-memory SID→UDID dict that <see cref="Middlewares.SessionidMappingMiddleware"/>
    /// populates from the UDID header. Returns null when the SID isn't mapped (e.g. the
    /// request didn't carry a UDID header at all, or carried an undecodable one).
    /// </summary>
    public static Guid? GetUdid(this HttpContext context)
    {
        string? sid = context.Request.Headers[NetworkConstants.SessionIdHeaderName];
        if (sid is null) return null;
        var sessionService = context.RequestServices.GetService<ShadowverseSessionService>();
        return sessionService?.GetUdidFromSessionId(sid);
    }
}
