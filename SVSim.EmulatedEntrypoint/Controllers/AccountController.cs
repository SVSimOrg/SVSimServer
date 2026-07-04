using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Account;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /account/* — viewer profile mutations that aren't tied to a specific subsystem.
/// </summary>
public class AccountController : SVSimController
{
    /// <summary>
    /// Conservative server-side cap on viewer display names. The client's UserNameInput
    /// enforces its own limit at the keyboard; this is the backstop against direct API
    /// abuse (10-MB names ballooning every subsequent /load/index, etc.). Names are
    /// typically &lt;=20 chars in prod traffic.
    /// </summary>
    private const int MaxDisplayNameLength = 24;

    private readonly SVSimDbContext _db;

    public AccountController(SVSimDbContext db)
    {
        _db = db;
    }

    [HttpPost("update_name")]
    public async Task<IActionResult> UpdateName([FromBody] AccountUpdateNameRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        // Defensive null check: the DTO defaults to string.Empty but a JSON body with
        // an explicit `"name": null` deserialises through msgpack→JSON→STJ to null, and
        // assigning null to viewer.DisplayName (non-nullable in the entity) would NRE.
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "name_empty" });
        if (request.Name.Length > MaxDisplayNameLength)
            return BadRequest(new { error = "name_too_long" });

        var viewer = await _db.Viewers.FirstAsync(v => v.Id == viewerId);
        viewer.DisplayName = request.Name;
        await _db.SaveChangesAsync();

        // Prod returns `data: []` — empty array, not empty object. Use an empty array literal
        // so the translation middleware emits the right msgpack shape.
        return Ok(Array.Empty<object>());
    }

    [HttpPost("update_region_code")]
    public ActionResult<EmptyResponse> UpdateRegionCode([FromBody] AccountUpdateRegionCodeRequest _)
    {
        if (!TryGetViewerId(out long __)) return Unauthorized();
        // No-op: actual region value is sent in the REGION_CODE HTTP header, which we
        // don't currently consume server-side. The body carries only initialize_flag,
        // which is a first-set-vs-update signal we have no use for yet.
        return new EmptyResponse();
    }

    [HttpPost("update_birth")]
    public async Task<ActionResult<EmptyResponse>> UpdateBirth([FromBody] AccountUpdateBirthRequest request)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        // Wire format is "yyyy-MM-dd" (see /load/index UserInfo.Birthday round-trip).
        // Parse strict; client only ever submits via its date-picker dialog.
        if (!DateTime.TryParseExact(request.Birth, "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                out var birth))
            return BadRequest(new { error = "birth_invalid" });

        var viewer = await _db.Viewers.FirstAsync(v => v.Id == viewerId);
        viewer.Info.BirthDate = birth;
        await _db.SaveChangesAsync();
        // data_headers.servertime drives the client's BirthDayUpdateServerTime — the standard
        // envelope already emits it, so an empty data payload is sufficient.
        return new EmptyResponse();
    }
}
