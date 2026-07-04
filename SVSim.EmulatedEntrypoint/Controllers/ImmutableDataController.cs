using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ImmutableData;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.ImmutableData;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// Family <c>/immutable_data/*</c>. v1 hosts the single endpoint <c>card_master</c>;
/// future siblings (e.g. mission masters, asset bundle hash tables) would land here.
/// </summary>
[Route("immutable_data")]
public class ImmutableDataController : SVSimController
{
    private readonly ICardMasterPayloadProvider _provider;
    private readonly IGameConfigService _config;

    public ImmutableDataController(ICardMasterPayloadProvider provider, IGameConfigService config)
    {
        _provider = provider;
        _config = config;
    }

    /// <summary>
    /// Returns the base64+gzip+json+csv card-master payload. Tier 1 serves a static prod
    /// snapshot regardless of the request's <c>card_master_hash</c>; freshness gating is
    /// handled on <c>/load/index</c> instead.
    /// </summary>
    [HttpPost("card_master")]
    public ActionResult<CardMasterResponse> CardMaster([FromBody] CardMasterRequest req)
    {
        if (!_provider.IsAvailable)
        {
            // 500 not 503: blob missing is operator error (config / build), not transient — no retry will help.
            return StatusCode(500, "card-master payload not available");
        }
        if (!_config.Get<CardMasterConfig>().EnableServing)
        {
            // 503: explicit operator kill switch — temporarily disabled, distinct from 500 (misconfig).
            return StatusCode(503, "card-master serving disabled");
        }
        return Ok(new CardMasterResponse { CardMaster = _provider.Base64Blob });
    }
}
