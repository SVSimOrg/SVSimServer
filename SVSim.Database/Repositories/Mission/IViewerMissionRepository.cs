using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Mission;

public interface IViewerMissionRepository
{
    Task<List<ViewerMission>> GetMissionsAsync(long viewerId, CancellationToken ct);
    Task<ViewerMission?> GetMissionByIdAsync(long viewerId, long missionId, CancellationToken ct);

    Task<List<ViewerAchievement>> GetAchievementsAsync(long viewerId, CancellationToken ct);
    Task<ViewerAchievement?> GetAchievementAsync(long viewerId, int achievementType, CancellationToken ct);

    /// <summary>Reads counter rows for (viewerId, eventKey IN list, period IN list). Empty inputs return [].</summary>
    Task<List<ViewerEventCounter>> GetCountersAsync(
        long viewerId,
        IReadOnlyCollection<string> eventKeys,
        IReadOnlyCollection<string> periods,
        CancellationToken ct);

    /// <summary>Single-row counter read. Returns 0 if no row exists.</summary>
    Task<int> GetCounterAsync(long viewerId, string eventKey, string period, CancellationToken ct);

    /// <summary>Add a viewer mission row (in-memory; caller saves).</summary>
    void AddMission(ViewerMission row);

    /// <summary>Remove a viewer mission row (in-memory; caller saves).</summary>
    void RemoveMission(ViewerMission row);

    /// <summary>Add a viewer achievement row (in-memory; caller saves).</summary>
    void AddAchievement(ViewerAchievement row);

    /// <summary>Upsert a counter delta (in-memory; caller saves). Creates the row if missing.</summary>
    Task UpsertCounterAsync(long viewerId, string eventKey, string period, int delta, CancellationToken ct);
}
