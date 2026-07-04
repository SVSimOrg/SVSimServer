using Microsoft.AspNetCore.Mvc;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /download_time/* — asset-download timing telemetry. The client fires
/// <c>POST /download_time/start</c> right before kicking off an Akamai asset bundle
/// download (<c>Wizard/DownloadStartTask.cs</c>) and <c>POST /download_time/end</c> when
/// it completes (<c>Wizard/DownloadFinishTask.cs</c>). Both are pure telemetry from our
/// perspective — we don't track download timings — but the client surfaces an HTTP error
/// dialog if either 404s, so we ack with empty <c>data: {}</c> bodies.
///
/// <para>Explicit <see cref="RouteAttribute"/> because the base controller token would
/// resolve to <c>/downloadtime</c>, missing the underscore.</para>
/// </summary>
[Route("download_time")]
public class DownloadTimeController : SVSimController
{
    /// <summary>
    /// Spec: <c>docs/api-spec/endpoints/post-login/download_time-start.md</c>. The client's
    /// <c>DownloadStartTask.Parse</c> reads an optional <c>image_type</c> string
    /// (<c>"card"</c> → CardDetail loading-screen art, <c>"still"</c> → StoryDetail, anything
    /// else → default). We omit it; the client falls through to the default art.
    /// </summary>
    [HttpPost("start")]
    public IActionResult Start([FromBody] BaseRequest request) => Ok(new { });

    /// <summary>
    /// Spec: <c>docs/api-spec/endpoints/post-login/download_time-end.md</c>. The client's
    /// <c>DownloadFinishTask</c> doesn't override <c>Parse</c> at all — only <c>result_code</c>
    /// matters. Empty data is the documented minimum-viable response.
    /// </summary>
    [HttpPost("end")]
    public IActionResult End([FromBody] BaseRequest request) => Ok(new { });
}
