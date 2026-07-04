using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Viewer;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Check;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses;

namespace SVSim.EmulatedEntrypoint.Controllers;

public class CheckController : SVSimController
{
    private readonly ILogger _logger;
    private readonly IViewerRepository _viewerRepository;

    public CheckController(ILogger<CheckController> logger, IViewerRepository viewerRepository)
    {
        _logger = logger;
        _viewerRepository = viewerRepository;
    }

    [AllowAnonymous]
    [HttpPost("special_title")]
    public Task<SpecialTitleCheckResponse> SpecialTitleCheck(SpecialTitleCheckRequest request)
    {
        return Task.FromResult(new SpecialTitleCheckResponse
        {
            TitleImageId = "0"
        });
    }

    [HttpPost("game_start")]
    public async Task<GameStartResponse> GameStart(GameStartRequest request)
    {
        Viewer viewer = HttpContext.GetViewer()
            ?? throw new InvalidOperationException("Auth handler must set viewer in context.");
        Viewer fullViewer = await _viewerRepository.GetViewerWithSocials(viewer.Id) ?? viewer;

        // Wipe-and-resignup reconciliation: /tool/signup is anonymous on the wire and can't see
        // the Steam ticket, so a freshly-wiped client lands a blank V_new keyed on its new UDID
        // while the Steam handler on this very request resolves to the original V_old. The client
        // has already written V_new.Id into Certification.ViewerId from the signup response; left
        // alone, it stays wrong forever (NormalTask.Parse never reads data_headers.viewer_id —
        // only SignUpTask / GameStartCheckTask.rewrite_viewer_id / the social-chain tasks do).
        // Detect the mismatch by re-looking-up the UDID-keyed viewer and emit rewrite_viewer_id
        // when it disagrees with the auth-resolved one.
        long? rewriteViewerId = null;
        Guid? udid = HttpContext.GetUdid();
        if (udid is Guid u && u != Guid.Empty)
        {
            Viewer? udidViewer = await _viewerRepository.GetViewerByUdid(u);
            if (udidViewer is not null && udidViewer.Id != fullViewer.Id)
            {
                rewriteViewerId = fullViewer.Id;
                // Reclaim the orphan: transfer the fresh UDID onto the Steam-resolved viewer
                // and delete the just-created blank anonymous one. Future GetViewerByUdid
                // calls then short-circuit to V_old without going through the Steam handler.
                await _viewerRepository.MergeAnonymousViewerInto(udidViewer.Id, fullViewer.Id);
            }
        }

        return new GameStartResponse
        {
            NowViewerId = fullViewer.Id,
            NowName = fullViewer.DisplayName,
            NowTutorialStep = fullViewer.MissionData.TutorialState.ToString(),
            IsSetTransitionPassword = true,
            RewriteViewerId = rewriteViewerId,
            // Stub rank map until per-format ranks are persisted (prod observed: "1"/"2"/"4"
            // keys mapping to RankName_010 / RankName_017). Empty dict here may be safe but
            // we don't yet know which client paths read this — match prod stub.
            NowRank = new Dictionary<string, string>
            {
                { "1", "RankName_010" },
                { "2", "RankName_010" },
                { "4", "RankName_017" }
            },
            TransitionAccountData = fullViewer.SocialAccountConnections
                .Select(sac => new TransitionAccountData
                {
                    SocialAccountId = sac.AccountId.ToString(),
                    SocialAccountType = ((int)sac.AccountType).ToString(),
                    ConnectedViewerId = fullViewer.Id.ToString()
                }).ToList(),
            TosState = 1,
            PolicyState = 1,
            KorAuthorityState = 0,
            TosId = 1,
            PolicyId = 1,
            KorAuthorityId = 0
        };
    }

    /// <summary>
    /// Card-master rotation-period integrity probe. Wire path is
    /// <c>check/check_time_slip_card_master_hash</c> but the client task is
    /// <c>CheckTimeSlipRotationPeriodTask</c> — a pure <c>BaseTask</c> with no
    /// <c>Parse()</c> override (Wizard/CheckTimeSlipRotationPeriodTask.cs). Fired from
    /// <c>DeckDecisionUI.cs:140</c> (Arena "View Deck" path) and the TK2 prep screen.
    /// Prod responds with <c>data: []</c> in every observed capture across
    /// traffic_prod_taketwo_selections.ndjson + traffic_prod_tradeables_capture.ndjson.
    /// </summary>
    [HttpPost("check_time_slip_card_master_hash")]
    public IActionResult CheckTimeSlipCardMasterHash([FromBody] CheckTimeSlipCardMasterHashRequest req)
    {
        return Ok(Array.Empty<object>());
    }
}
