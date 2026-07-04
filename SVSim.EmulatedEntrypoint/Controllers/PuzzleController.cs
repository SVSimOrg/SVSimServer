using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Repositories.Globals;
using SVSim.Database.Repositories.Viewer;
using SVSim.Database.Services.Inventory;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.BasicPuzzle;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.BasicPuzzle;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.BasicPuzzle;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Controllers;

/// <summary>
/// /basic_puzzle/* — solo puzzle subsystem (the "Practice Match" puzzle catalog visible from
/// the home screen). Explicit [Route] override because the base SVSimController's [controller]
/// token would resolve to /puzzle.
/// </summary>
[Route("basic_puzzle")]
public class PuzzleController : SVSimController
{
    private readonly IPuzzleCatalogRepository _catalog;
    private readonly IPuzzleClearRepository _clears;
    private readonly PuzzleMissionEvaluator _evaluator;
    private readonly IInventoryService _inv;
    private readonly ILogger<PuzzleController> _logger;

    public PuzzleController(
        IPuzzleCatalogRepository catalog,
        IPuzzleClearRepository clears,
        PuzzleMissionEvaluator evaluator,
        IInventoryService inv,
        ILogger<PuzzleController> logger)
    {
        _catalog = catalog;
        _clears = clears;
        _evaluator = evaluator;
        _inv = inv;
        _logger = logger;
    }

    /// <summary>/basic_puzzle/info — full catalog of groups + per-viewer clear flags.</summary>
    [HttpPost("info")]
    public async Task<List<PuzzleGroupResponse>> Info(BaseRequest _)
    {
        if (!TryGetViewerId(out long viewerId)) viewerId = 0;

        var groups = await _catalog.GetAllGroupsWithPuzzles();
        var missions = await _catalog.GetAllMissionsOrdered();
        var clearedByGroup = await _clears.GetClearedPuzzleIdsByGroup(viewerId);

        return ProjectGroups(groups, missions, clearedByGroup);
    }

    /// <summary>/basic_puzzle/open_puzzle_dialog — per-group detail. Unknown puzzle_master_id
    /// returns 200 with an empty puzzle_quest array (matches client PuzzleQuestInfo fallback).</summary>
    [HttpPost("open_puzzle_dialog")]
    public async Task<OpenPuzzleDialogResponse> OpenPuzzleDialog(OpenPuzzleDialogRequest req)
    {
        if (!TryGetViewerId(out long viewerId)) viewerId = 0;
        var group = await _catalog.GetGroupWithPuzzles(req.PuzzleMasterId);
        if (group is null) return new OpenPuzzleDialogResponse();

        var cleared = await _clears.GetClearedPuzzleIds(viewerId);
        return new OpenPuzzleDialogResponse
        {
            PuzzleQuest = group.Puzzles
                .OrderBy(p => p.Id)
                .Select(p => new PuzzleEntryResponse
                {
                    PuzzleId = p.Id,
                    PuzzleDifficulty = p.PuzzleDifficulty,
                    IsCleared = cleared.Contains(p.Id),
                    IsAdditional = p.IsAdditional,
                    IsPlayable = p.IsPlayable,
                    ReleaseConditionTextId = p.ReleaseConditionTextId,
                })
                .ToList(),
            PuzzleQuestCharaId = group.PuzzleCharaId,
            PuzzleDifficultyNameList = JsonSerializer.Deserialize<Dictionary<string, string>>(group.DifficultyNameListJson) ?? new(),
            IsDisplayBadge = false,
            IsDisplayPuzzleNew = false,
        };
    }

    /// <summary>/basic_puzzle/start — server is essentially a no-op. Wire data is the literal empty array `[]`.</summary>
    [HttpPost("start")]
    public Task<object[]> Start(StartRequest _) => Task.FromResult(Array.Empty<object>());

    /// <summary>/basic_puzzle/mission — catalog + per-viewer progress on each mission.
    /// Special-Round missions always surface with total_count=0 (Phase 1 deferral).</summary>
    [HttpPost("mission")]
    public async Task<List<PuzzleMissionResponse>> Mission(BaseRequest _)
    {
        if (!TryGetViewerId(out long viewerId)) viewerId = 0;

        var missions = await _catalog.GetAllMissionsOrdered();
        var clearedByGroup = await _clears.GetClearedPuzzleIdsByGroup(viewerId);
        var statuses = _evaluator.Evaluate(missions, clearedByGroup);

        return statuses.Select(s => new PuzzleMissionResponse
        {
            MissionName = s.Mission.MissionName,
            RequireNumber = s.Mission.RequireNumber,
            CampaignCommenceTime = s.Mission.CampaignCommenceTime,
            RewardList = new List<PuzzleMissionRewardResponse>
            {
                new() {
                    RewardType = (int)s.Mission.RewardType,
                    RewardDetailId = s.Mission.RewardDetailId,
                    RewardNumber = s.Mission.RewardNumber,
                },
            },
            OrderId = s.Mission.OrderId,
            TotalCount = s.TotalCount,
            IsAchieved = s.IsAchieved,
        }).ToList();
    }

    /// <summary>
    /// /basic_puzzle/finish — record a puzzle attempt outcome. Wins persist a ViewerPuzzleClear
    /// row and may grant a mission reward; losses are fully stateless (the client only sends
    /// is_win=false on user-initiated retire, not on in-battle resets).
    ///
    /// CONCURRENCY: this controller does not serialize concurrent finishes for the same viewer.
    /// The ViewerPuzzleClear PK protects per-row idempotency but two simultaneous finishes for
    /// different puzzles in the same group could both observe "this is the last clear" and
    /// double-grant the mission reward. The same race exists across many viewer-mutating
    /// endpoints in this codebase — address with a holistic audit, not a puzzle-specific fix.
    /// </summary>
    [HttpPost("finish")]
    public async Task<FinishResponse> Finish(FinishRequest req)
    {
        if (!TryGetViewerId(out long viewerId)) viewerId = 0;

        var response = new FinishResponse();
        var groups = await _catalog.GetAllGroupsWithPuzzles();
        var missions = await _catalog.GetAllMissionsOrdered();

        if (!req.IsWin)
        {
            // Loss: no DB writes. Loss-specific wire quirks: win_count is the NUMBER 0
            // (not string "1"), and mission_start_data is empty.
            response.WinCount = 0;
            response.AchievedInfo.MissionStartData = new();
            response.PuzzleList = ProjectGroups(groups, missions, await _clears.GetClearedPuzzleIdsByGroup(viewerId));
            return response;
        }

        // ---- Win path ----
        var beforeByGroup = await _clears.GetClearedPuzzleIdsByGroup(viewerId);
        await _clears.UpsertClearAsync(viewerId, req.PuzzleId, req.RetryCount);

        // Recompute clearedByGroup by adding the freshly cleared puzzle to its group.
        var puzzleLocation = groups
            .SelectMany(g => g.Puzzles.Select(p => (GroupId: g.Id, PuzzleId: p.Id)))
            .FirstOrDefault(x => x.PuzzleId == req.PuzzleId);
        var afterByGroup = beforeByGroup.ToDictionary(k => k.Key, v => new HashSet<int>(v.Value));
        if (puzzleLocation.PuzzleId != 0)
        {
            if (!afterByGroup.TryGetValue(puzzleLocation.GroupId, out var groupSet))
            {
                groupSet = new HashSet<int>();
                afterByGroup[puzzleLocation.GroupId] = groupSet;
            }
            groupSet.Add(req.PuzzleId);
        }

        var fresh = _evaluator.FreshlyCompleted(missions, beforeByGroup, afterByGroup);
        var freshlyAchievedIds = new HashSet<int>(fresh.Select(s => s.Mission.Id));

        if (fresh.Count > 0)
        {
            await using var tx = await _inv.BeginAsync(viewerId, configure: cfg => cfg.Source = GrantSource.PuzzleReward);

            foreach (var status in fresh)
            {
                IReadOnlyList<SVSim.Database.Services.GrantedReward> granted;
                try
                {
                    granted = await tx.GrantAsync(
                        status.Mission.RewardType,
                        status.Mission.RewardDetailId,
                        status.Mission.RewardNumber);
                }
                catch (NotSupportedException ex)
                {
                    _logger.LogWarning(ex,
                        "PuzzleController: skipping unsupported reward_type={Type} detail={Detail} num={Num} for mission={MissionId}",
                        status.Mission.RewardType, status.Mission.RewardDetailId, status.Mission.RewardNumber, status.Mission.Id);
                    continue;
                }

                response.AchievedInfo.AchievedMissionList.Add(new PuzzleAchievedMissionEntry
                {
                    AchievedMessage = status.Mission.AchievedMessage,
                });
                response.AchievedInfo.AchievedMissionRewardList.Add(new PuzzleAchievedMissionReward
                {
                    MissionRewardType = (int)status.Mission.RewardType,
                    MissionRewardDetailId = status.Mission.RewardDetailId,
                    MissionRewardNumber = status.Mission.RewardNumber,
                });
                foreach (var g in granted)
                {
                    response.RewardList.Add(new TreasureRewardResponse
                    {
                        RewardType = (int)g.RewardType,
                        RewardId = g.RewardId,
                        RewardNum = g.RewardNum,
                    });
                }
            }

            await tx.CommitAsync();
        }

        response.WinCount = "1";
        response.AchievedInfo.MissionStartData = BuildMissionStartData(missions, afterByGroup, freshlyAchievedIds);
        response.PuzzleList = ProjectGroups(groups, missions, afterByGroup);
        return response;
    }

    private List<MissionStartEntry> BuildMissionStartData(
        IEnumerable<SVSim.Database.Models.PuzzleMissionEntry> missions,
        IReadOnlyDictionary<int, HashSet<int>> clearedByGroup,
        ISet<int> freshlyAchieved)
    {
        var statuses = _evaluator.Evaluate(missions, clearedByGroup);
        return statuses
            .Where(s => !s.IsAchieved && !freshlyAchieved.Contains(s.Mission.Id))
            .Select(s => new MissionStartEntry
            {
                MissionName = s.Mission.MissionName,
                StartTime = s.Mission.CampaignCommenceTime,
                LotType = "3",   // puzzle-group-clear; Phase 1 only emits puzzle missions
            })
            .ToList();
    }

    /// <summary>Shared projection used by /info and /finish.puzzle_list. Applies per-viewer clear
    /// flags, computes is_all_cleared, and toggles is_mission_target based on mission progress.</summary>
    internal List<PuzzleGroupResponse> ProjectGroups(
        IEnumerable<SVSim.Database.Models.PuzzleGroupEntry> groups,
        IEnumerable<SVSim.Database.Models.PuzzleMissionEntry> missions,
        IReadOnlyDictionary<int, HashSet<int>> clearedByGroup)
    {
        var statuses = _evaluator.Evaluate(missions, clearedByGroup);
        var achievedGroupIds = statuses
            .Where(s => s.IsAchieved && s.Mission.TargetPuzzleGroupId is int)
            .Select(s => s.Mission.TargetPuzzleGroupId!.Value)
            .ToHashSet();
        var mappedGroupIds = missions
            .Where(m => m.TargetPuzzleGroupId is int)
            .Select(m => m.TargetPuzzleGroupId!.Value)
            .ToHashSet();

        var result = new List<PuzzleGroupResponse>();
        foreach (var g in groups)
        {
            var cleared = clearedByGroup.TryGetValue(g.Id, out var c) ? c : new HashSet<int>();
            var puzzleEntries = g.Puzzles
                .OrderBy(p => p.Id)
                .Select(p => new PuzzleEntryResponse
                {
                    PuzzleId = p.Id,
                    PuzzleDifficulty = p.PuzzleDifficulty,
                    IsCleared = cleared.Contains(p.Id),
                    IsAdditional = p.IsAdditional,
                    IsPlayable = p.IsPlayable,
                    ReleaseConditionTextId = p.ReleaseConditionTextId,
                })
                .ToList();

            bool isAllCleared = puzzleEntries.All(p => p.IsCleared) && puzzleEntries.Count > 0;
            bool isMissionTarget = mappedGroupIds.Contains(g.Id) && !achievedGroupIds.Contains(g.Id);

            result.Add(new PuzzleGroupResponse
            {
                PuzzleMasterId = g.Id,
                PuzzleData = puzzleEntries,
                PuzzleCharaId = g.PuzzleCharaId,
                PuzzleDifficultyNameList = JsonSerializer.Deserialize<Dictionary<string, string>>(g.DifficultyNameListJson) ?? new(),
                IsAllCleared = isAllCleared,
                CharaId = g.CharaId,
                SortType = g.SortType,
                BasicTitleTextId = g.BasicTitleTextId,
                IsMissionTarget = isMissionTarget,
            });
        }
        // Captured order in prod is descending by puzzle_master_id; mirror that for the wire.
        return result.OrderByDescending(r => r.PuzzleMasterId).ToList();
    }
}
