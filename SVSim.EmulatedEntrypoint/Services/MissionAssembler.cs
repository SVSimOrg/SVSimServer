using SVSim.Database.Models;
using SVSim.Database.Repositories.Mission;
using SVSim.EmulatedEntrypoint.Models.Dtos.Common.Mission;

namespace SVSim.EmulatedEntrypoint.Services;

public sealed class MissionAssembler : IMissionAssembler
{
    private readonly IMissionCatalogRepository _catalog;
    private readonly IViewerMissionRepository _viewerRepo;
    private readonly TimeProvider _time;
    private readonly IGameCalendarService _calendar;

    public MissionAssembler(
        IMissionCatalogRepository catalog,
        IViewerMissionRepository viewerRepo,
        TimeProvider time,
        IGameCalendarService calendar)
    {
        _catalog = catalog;
        _viewerRepo = viewerRepo;
        _time = time;
        _calendar = calendar;
    }

    public async Task<MissionInfoDataDto> BuildAsync(Viewer viewer, CancellationToken ct = default)
    {
        var now = _time.GetUtcNow();
        var dto = new MissionInfoDataDto();

        // Read fresh state — don't trust navigation properties.
        var viewerMissions = await _viewerRepo.GetMissionsAsync(viewer.Id, ct);
        var viewerAchievements = await _viewerRepo.GetAchievementsAsync(viewer.Id, ct);

        var missionCatalogIds = viewerMissions.Select(m => m.MissionCatalogId).Distinct().ToList();
        var missionCatalog = missionCatalogIds.Count == 0
            ? new List<MissionCatalogEntry>()
            : await _catalog.GetByIdsAsync(missionCatalogIds, ct);
        var missionById = missionCatalog.ToDictionary(c => c.Id);

        var maxLevelByType = await _catalog.GetMaxLevelByAchievementTypeAsync(ct);

        // Gather all event keys we'll need to read counters for, in one batch.
        var counterEventKeys = new HashSet<string>();
        foreach (var m in viewerMissions)
        {
            if (missionById.TryGetValue(m.MissionCatalogId, out var cat) && cat.EventType is not null)
                counterEventKeys.Add(cat.EventType);
        }
        // Achievements need catalog rows at viewer's current Level to find their EventType.
        var achievementCatalogByKey = new Dictionary<(int, int), AchievementCatalogEntry>();
        foreach (var a in viewerAchievements)
        {
            var c = await _catalog.GetAchievementAsync(a.AchievementType, a.Level, ct);
            if (c is null) continue;
            achievementCatalogByKey[(a.AchievementType, a.Level)] = c;
            if (c.EventType is not null) counterEventKeys.Add(c.EventType);
        }

        // BP monthly missions for the current calendar month (UTC).
        var monthlyMissions = await _catalog.GetMonthlyMissionsAsync(now.Year, now.Month, ct);
        foreach (var mm in monthlyMissions)
        {
            if (mm.EventType is not null) counterEventKeys.Add(mm.EventType);
        }

        var periods = _calendar.AllPeriods(now);
        var counters = counterEventKeys.Count == 0
            ? new List<ViewerEventCounter>()
            : await _viewerRepo.GetCountersAsync(viewer.Id, counterEventKeys.ToList(), periods, ct);
        var counterLookup = counters.ToDictionary(c => (c.EventKey, c.Period), c => c.Count);

        int GetCounter(string eventKey, string period) =>
            counterLookup.TryGetValue((eventKey, period), out var v) ? v : 0;

        // user_mission_list
        foreach (var m in viewerMissions.OrderBy(m => m.Slot))
        {
            if (!missionById.TryGetValue(m.MissionCatalogId, out var cat)) continue;
            int total = 0;
            if (cat.EventType is not null)
            {
                string period = cat.LotType == 6 ? _calendar.DayKey(now) : _calendar.WeekKey(now);
                total = GetCounter(cat.EventType, period);
            }
            dto.UserMissionList.Add(new UserMissionDto
            {
                Id = m.Id,
                MissionId = cat.Id,
                TotalCount = total,
                MissionStatus = m.MissionStatus,
                DisplayOrder = 0,
                MissionName = cat.Name,
                LotType = cat.LotType.ToString(),
                BattlePassPoint = cat.BattlePassPoint.ToString(),
                RequireNumber = cat.RequireNumber,
                RewardType = (int)cat.RewardType,
                RewardDetailId = cat.RewardDetailId,
                RewardNumber = cat.RewardNumber,
                DefaultFlag = cat.DefaultFlag,
                StartTime = cat.StartTime,
                EndTime = cat.EndTime,
            });
        }

        // user_achievement_list — one row per ViewerAchievement, looking up catalog at viewer's current Level.
        foreach (var a in viewerAchievements.OrderBy(a => a.AchievementType))
        {
            if (!achievementCatalogByKey.TryGetValue((a.AchievementType, a.Level), out var catalog)) continue;
            int total = catalog.EventType is null ? 0 : GetCounter(catalog.EventType, GameCalendarPeriods.AllTime);
            int maxLevel = maxLevelByType.TryGetValue(a.AchievementType, out var ml) ? ml : a.Level;
            dto.UserAchievementList.Add(new UserAchievementDto
            {
                AchievementType = a.AchievementType,
                AchievementStatus = a.AchievementStatus,
                Level = a.Level,
                NowAchievedLevel = a.NowAchievedLevel,
                ResultAnnounceSawLevel = a.ResultAnnounceSawLevel,
                TotalCount = total,
                AchievementName = catalog.Name,
                RequireNumber = catalog.RequireNumber,
                RewardType = (int)catalog.RewardType,
                RewardDetailId = catalog.RewardDetailId,
                RewardNumber = catalog.RewardNumber,
                MaxLevel = maxLevel,
                OrderNum = catalog.OrderNum,
                Ios = "",
                Android = "",
            });
        }

        // Mission-change gating: viewer.MissionData.MissionChangeTime is when retire becomes available again.
        bool canChange = viewer.MissionData.MissionChangeTime <= now.UtcDateTime;
        dto.IsChangeMission = canChange;
        dto.CanChangeMissionTime = canChange ? null
            : new DateTimeOffset(viewer.MissionData.MissionChangeTime, TimeSpan.Zero).ToUnixTimeSeconds();

        // Receive-type cooldown: v1 has none, always true/null.
        dto.IsChangeReceiveType = true;
        dto.CanChangeReceiveTypeTime = null;

        dto.MissionReceiveType = viewer.MissionData.MissionReceiveType.ToString();

        // BP monthly missions block — omit when no rows for current month.
        if (monthlyMissions.Count > 0)
        {
            var startUtc = new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var endUtc = startUtc.AddMonths(1).AddSeconds(-1);
            dto.BattlePassMonthlyMission = new BPMonthlyMissionsDto
            {
                StartDate = startUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                EndDate = endUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                MissionList = monthlyMissions.Select(mm =>
                {
                    int done = mm.EventType is null ? 0
                        : GetCounter(mm.EventType, _calendar.MonthKey(now));
                    var entry = new BPMonthlyMissionDto
                    {
                        Name = mm.Name,
                        IsCleared = done >= mm.RequireNumber,
                        RequireNumber = mm.RequireNumber,
                        DoneNumber = done,
                        BattlePassPoint = mm.BattlePassPoint,
                    };
                    if (mm.RewardType is not null)
                    {
                        entry.RewardInfo = new BPMonthlyMissionRewardInfoDto
                        {
                            RewardType = ((int)mm.RewardType.Value).ToString(),
                            RewardDetailId = (mm.RewardDetailId ?? 0).ToString(),
                            RewardNumber = (mm.RewardNumber ?? 0).ToString(),
                        };
                    }
                    return entry;
                }).ToList(),
            };
        }

        return dto;
    }
}
