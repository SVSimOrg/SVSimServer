using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Mission;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Mission;
using SVSim.EmulatedEntrypoint.Models.Dtos.Mission;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Mission;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /mission/* — daily/weekly mission slots + achievement claim flow. Wire shapes mirror
/// MissionInfoDetail.cs + Wizard/Mission*Task.cs.
/// </summary>
[Route("mission")]
public class MissionController : SVSimController
{
    private const int RetireCooldownSeconds = 75600; // 21h per capture
    private const int FailureResultCode = 2;

    private readonly SVSimDbContext _db;
    private readonly IViewerMissionStateService _state;
    private readonly IMissionAssembler _assembler;
    private readonly IMissionCatalogRepository _catalog;
    private readonly IViewerMissionRepository _viewerRepo;
    private readonly TimeProvider _time;

    public MissionController(
        SVSimDbContext db,
        IViewerMissionStateService state,
        IMissionAssembler assembler,
        IMissionCatalogRepository catalog,
        IViewerMissionRepository viewerRepo,
        TimeProvider time)
    {
        _db = db;
        _state = state;
        _assembler = assembler;
        _catalog = catalog;
        _viewerRepo = viewerRepo;
        _time = time;
    }

    [HttpPost("info")]
    public async Task<IActionResult> Info(BaseRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var viewer = await LoadViewer(viewerId, ct);

        await _state.EnsureCurrentAsync(viewer.Id, ct);
        await _db.SaveChangesAsync(ct);

        var dto = await _assembler.BuildAsync(viewer, ct);
        return Ok(dto);
    }

    [HttpPost("retire")]
    public async Task<IActionResult> Retire(MissionRetireRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var viewer = await LoadViewer(viewerId, ct);

        var missions = await _viewerRepo.GetMissionsAsync(viewerId, ct);
        var target = missions.FirstOrDefault(m => m.Id == request.Id);
        if (target is null)
        {
            return Ok(new { result_code = FailureResultCode });
        }

        var catalogRow = await _catalog.GetByIdAsync(target.MissionCatalogId, ct);
        if (catalogRow is null || catalogRow.LotType != 2)
        {
            return Ok(new { result_code = FailureResultCode });
        }

        var pool = await _catalog.GetByLotTypeAsync(2, ct);
        var assignedIds = missions
            .Where(m => m.Slot != target.Slot)
            .Select(m => m.MissionCatalogId).ToHashSet();
        var candidates = pool.Where(p => p.Id != target.MissionCatalogId && !assignedIds.Contains(p.Id)).ToList();
        if (candidates.Count == 0)
        {
            return Ok(new { result_code = FailureResultCode });
        }
        var pick = candidates[Random.Shared.Next(candidates.Count)];

        var now = _time.GetUtcNow();
        _viewerRepo.RemoveMission(target);
        _viewerRepo.AddMission(new ViewerMission
        {
            ViewerId = viewerId,
            MissionCatalogId = pick.Id,
            Slot = target.Slot,
            AssignedAt = now.ToUnixTimeSeconds(),
            MissionStatus = 1,
        });
        viewer.MissionData.MissionChangeTime = now.AddSeconds(RetireCooldownSeconds).UtcDateTime;
        await _db.SaveChangesAsync(ct);

        var dto = await _assembler.BuildAsync(viewer, ct);
        return Ok(dto);
    }

    [HttpPost("change_receive_setting")]
    public async Task<IActionResult> ChangeReceiveSetting(MissionChangeReceiveSettingRequest request, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();
        var viewer = await LoadViewer(viewerId, ct);

        viewer.MissionData.MissionReceiveType = request.MissionReceiveType;
        await _db.SaveChangesAsync(ct);

        var dto = await _assembler.BuildAsync(viewer, ct);
        return Ok(dto);
    }

    [HttpPost("buy_additional_right")]
    public async Task<IActionResult> BuyAdditionalRight(BaseRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        // The client-side task class is absent from the decompilation; the URL is
        // registered but no Parse() is documented. Spec marks the shape as INFERRED.
        // Safe stub: refresh MissionInfoDetail and let the client re-render — same
        // shape as /mission/info. No currency debit until we see a real wire call.
        var viewer = await LoadViewer(viewerId, ct);
        await _state.EnsureCurrentAsync(viewer.Id, ct);
        await _db.SaveChangesAsync(ct);

        var dto = await _assembler.BuildAsync(viewer, ct);
        return Ok(dto);
    }

    [HttpPost("receive_reward")]
    public async Task<IActionResult> ReceiveReward(MissionReceiveRewardRequest _, CancellationToken ct)
    {
        if (!TryGetViewerId(out long viewerId)) return Unauthorized();

        // Spec is INFERRED — no decomp task class. Safe stub: refresh MissionInfoDetail
        // so the client sees its current state. Real reward granting deferred until we
        // observe a wire call.
        var viewer = await LoadViewer(viewerId, ct);
        await _state.EnsureCurrentAsync(viewer.Id, ct);
        await _db.SaveChangesAsync(ct);

        var dto = await _assembler.BuildAsync(viewer, ct);
        return Ok(dto);
    }

    private Task<Viewer> LoadViewer(long viewerId, CancellationToken ct) =>
        _db.Viewers
            .Include(v => v.MissionData)
            .AsSplitQuery()
            .FirstAsync(v => v.Id == viewerId, ct);
}
