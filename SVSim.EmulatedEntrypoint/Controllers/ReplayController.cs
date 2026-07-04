using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Services.Replay;
using SVSim.EmulatedEntrypoint.Models.Dtos.Replay;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// Replay menu — recent-battles list + per-battle detail stub.
/// /replay/info  returns up to 50 rows newest-first from ViewerBattleHistories.
/// /replay/detail returns 400 (result_code=99) — local cache is the canonical
/// playback source; this endpoint is only hit on cache miss, and we don't store
/// replay payloads. The client (ReplayDialogContent.GoReplay) aborts the scene
/// transition cleanly on non-success.
/// </summary>
[Route("replay")]
public sealed class ReplayController : SVSimController
{
    private const string TimeFormat = "yyyy-MM-dd HH:mm:ss";

    private readonly IReplayHistoryReader _reader;

    public ReplayController(IReplayHistoryReader reader) => _reader = reader;

    [HttpPost("info")]
    public async Task<IActionResult> Info([FromBody] BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();

        var rows = await _reader.GetRecentAsync(vid, take: 50, ct);
        var resp = new ReplayInfoResponseDto
        {
            ReplayList = rows.Select(MapToWire).ToList(),
        };
        return Ok(resp);
    }

    [HttpPost("detail")]
    public IActionResult Detail([FromBody] ReplayDetailRequestDto req)
    {
        if (!TryGetViewerId(out _)) return Unauthorized();
        return BadRequest(new { result_code = 99 });
    }

    private static ReplayInfoItemDto MapToWire(ReplayHistoryEntry e) => new()
    {
        BattleType          = e.BattleType.ToString(CultureInfo.InvariantCulture),
        TwoPickType         = e.TwoPickType.ToString(CultureInfo.InvariantCulture),
        DeckFormat          = e.DeckFormat.ToString(CultureInfo.InvariantCulture),
        BattleId            = e.BattleId.ToString(CultureInfo.InvariantCulture),
        IsLimitTurn         = e.IsLimitTurn.ToString(CultureInfo.InvariantCulture),
        OpponentName        = e.OpponentName,
        ClassId             = e.SelfClassId.ToString(CultureInfo.InvariantCulture),
        OpponentClassId     = e.OpponentClassId.ToString(CultureInfo.InvariantCulture),
        SubClassId          = e.SelfSubClassId.ToString(CultureInfo.InvariantCulture),
        OpponentSubClassId  = e.OpponentSubClassId.ToString(CultureInfo.InvariantCulture),
        RotationId          = e.SelfRotationId,
        OpponentRotationId  = e.OpponentRotationId,
        OpponentCountryCode = e.OpponentCountryCode,
        CharaId             = e.SelfCharaId.ToString(CultureInfo.InvariantCulture),
        OpponentCharaId     = e.OpponentCharaId.ToString(CultureInfo.InvariantCulture),
        OpponentEmblemId    = e.OpponentEmblemId.ToString(CultureInfo.InvariantCulture),
        OpponentDegreeId    = e.OpponentDegreeId.ToString(CultureInfo.InvariantCulture),
        IsWin               = e.IsWin ? "1" : "0",
        BattleStartTime     = e.BattleStartTime.ToString(TimeFormat, CultureInfo.InvariantCulture),
        CreateTime          = e.CreateTime.ToString(TimeFormat, CultureInfo.InvariantCulture),
    };
}
