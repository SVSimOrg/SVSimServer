using SVSim.Database.Models;

namespace SVSim.Database.Repositories.Mission;

public interface IMissionCatalogRepository
{
    Task<List<MissionCatalogEntry>> GetByLotTypeAsync(int lotType, CancellationToken ct);
    Task<List<MissionCatalogEntry>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken ct);
    Task<MissionCatalogEntry?> GetByIdAsync(int id, CancellationToken ct);

    Task<List<MissionCatalogEntry>> GetByEventTypesAsync(IReadOnlyCollection<string> eventTypes, CancellationToken ct);
    Task<List<AchievementCatalogEntry>> GetAchievementsByEventTypesAsync(IReadOnlyCollection<string> eventTypes, CancellationToken ct);

    /// <summary>All distinct achievement_type values present in the catalog. Used by /load/index materialization.</summary>
    Task<List<int>> GetAllAchievementTypesAsync(CancellationToken ct);

    /// <summary>MIN(Level) per achievement_type — the "starting tier" for new viewers when the
    /// catalog doesn't contain a level-1 row. With our captured-data-is-catalog model, a fresh
    /// viewer starts at whatever the lowest captured tier is for that type.</summary>
    Task<IReadOnlyDictionary<int, int>> GetMinLevelByAchievementTypeAsync(CancellationToken ct);

    /// <summary>MAX(Level) per achievement_type — cached. Used to compute wire max_level.</summary>
    Task<IReadOnlyDictionary<int, int>> GetMaxLevelByAchievementTypeAsync(CancellationToken ct);

    /// <summary>Catalog row at (type, level), or null if no such tier has been captured.</summary>
    Task<AchievementCatalogEntry?> GetAchievementAsync(int achievementType, int level, CancellationToken ct);

    Task<List<BattlePassMonthlyMissionEntry>> GetMonthlyMissionsAsync(int year, int month, CancellationToken ct);
}
