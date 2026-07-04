using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Repositories.Viewer;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

public class ToolController : SVSimController
{
    private readonly ILogger<ToolController> _logger;
    private readonly IViewerRepository _viewerRepository;
    private readonly ShadowverseSessionService _sessionService;

    public ToolController(
        ILogger<ToolController> logger,
        IViewerRepository viewerRepository,
        ShadowverseSessionService sessionService)
    {
        _logger = logger;
        _viewerRepository = viewerRepository;
        _sessionService = sessionService;
    }

    /// <summary>
    /// <c>POST /tool/signup</c> — the client's first request on a fresh boot. Creates (or returns
    /// the existing) Viewer keyed on the request's UDID. The interesting outputs (viewer_id,
    /// short_udid, udid) all flow back via <c>data_headers</c>, populated by the translation
    /// middleware after this action returns — we just need to stash the viewer on HttpContext so
    /// the middleware picks it up the same way the auth handler does for logged-in endpoints.
    ///
    /// Spec: <c>docs/api-spec/endpoints/pre-login/tool-signup.md</c>.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("signup")]
    public async Task<SignupResponse> Signup([FromBody] SignupRequest request)
    {
        Guid? maybeUdid = HttpContext.GetUdid();
        if (maybeUdid is not Guid udid || udid == Guid.Empty)
        {
            throw new InvalidOperationException(
                "Cannot register viewer: request has no resolvable UDID (missing UDID/SID headers, or " +
                "SessionidMappingMiddleware couldn't decode the UDID header).");
        }

        var viewer = await _viewerRepository.GetViewerByUdid(udid)
                     ?? await _viewerRepository.RegisterAnonymousViewer(udid);

        HttpContext.SetViewer(viewer);

        // Pre-store the SID the client will compute and use for its very next request. After
        // signup the client switches to SID-only headers (no UDID), so without this mapping the
        // translation middleware can't decrypt the next body. Formula mirrors the decompiled
        // Cute/Certification.SessionId getter — see ShadowverseSessionService.ComputeClientSessionId.
        _sessionService.StoreSessionForViewer(viewer.Id, udid);

        _logger.LogInformation("Signup resolved for udid={Udid} → viewer_id={ViewerId}, short_udid={ShortUdid}.",
            udid, viewer.Id, viewer.ShortUdid);

        return new SignupResponse();
    }
}
