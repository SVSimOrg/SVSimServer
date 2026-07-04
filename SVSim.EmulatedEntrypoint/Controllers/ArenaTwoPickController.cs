using Microsoft.AspNetCore.Mvc;
using SVSim.Database.Services;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.ArenaTwoPick;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

[Route("arena_two_pick")]
public class ArenaTwoPickController : SVSimController
{
    private readonly IArenaTwoPickService _svc;
    private readonly IMissionProgressService _missionProgress;

    public ArenaTwoPickController(IArenaTwoPickService svc, IMissionProgressService missionProgress)
    {
        _svc = svc;
        _missionProgress = missionProgress;
    }

    [HttpPost("top")]
    public async Task<IActionResult> Top([FromBody] TopRequest _)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        return Ok(await _svc.GetTopAsync(vid));
    }

    [HttpPost("entry")]
    public async Task<IActionResult> Entry([FromBody] EntryRequest req)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        return await GuardAsync(() => _svc.EntryAsync(vid, req.ConsumeItemType));
    }

    [HttpPost("class_choose")]
    public async Task<IActionResult> ClassChoose([FromBody] ClassChooseRequest req)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        return await GuardAsync(() => _svc.ChooseClassAsync(vid, req.ClassId));
    }

    [HttpPost("card_choose")]
    public async Task<IActionResult> CardChoose([FromBody] CardChooseRequest req)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        return await GuardAsync(() => _svc.ChooseCardAsync(vid, req.SelectedId));
    }

    [HttpPost("retire")]
    public async Task<IActionResult> Retire([FromBody] RetireRequest _)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        return await GuardAsync(() => _svc.RetireAsync(vid));
    }

    [HttpPost("finish")]
    public async Task<IActionResult> Finish([FromBody] FinishRequest _, CancellationToken ct = default)
    {
        if (!TryGetViewerId(out var vid)) return Unauthorized();
        return await GuardAsync(async () =>
        {
            var outcome = await _svc.FinishAsync(vid);
            if (outcome.WasFullClear)
            {
                await _missionProgress.RecordEventAsync(
                    vid, MissionEventKeys.Challenge.FullClearAll(), ct: ct);
            }
            return outcome.Response;
        });
    }

    private async Task<IActionResult> GuardAsync<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (ArenaTwoPickException ex) { return BadRequest(new { error_code = ex.ErrorCode }); }
    }
}
