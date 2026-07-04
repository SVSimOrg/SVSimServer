using SVSim.Database.Models;

namespace SVSim.Database.Repositories.BattlePass;

public interface IBattlePassRepository
{
    /// <summary>
    /// Active season for the given moment (StartDate &lt;= when &lt; EndDate). Returns null
    /// if none. If multiple match (overlap), the most recently started wins.
    /// </summary>
    Task<BattlePassSeasonEntry?> GetActiveSeasonAsync(DateTimeOffset when, CancellationToken ct);

    /// <summary>
    /// Season by id (no time-window filter). Used by /battle_pass/buy to validate request.season_id.
    /// </summary>
    Task<BattlePassSeasonEntry?> GetSeasonAsync(int seasonId, CancellationToken ct);

    /// <summary>
    /// All rewards for a season, both tracks. Sorted by (Track, Level) for deterministic wire order.
    /// </summary>
    Task<List<BattlePassRewardEntry>> GetSeasonRewardsAsync(int seasonId, CancellationToken ct);

    /// <summary>
    /// Global level curve. Cached after first load.
    /// </summary>
    Task<IReadOnlyList<BattlePassLevelEntry>> GetLevelCurveAsync(CancellationToken ct);
}
