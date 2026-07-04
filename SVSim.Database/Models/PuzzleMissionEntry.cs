using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One row per basic_puzzle mission (e.g. "Clear all Round 1 puzzles"). Static catalog
/// seeded by SVSim.Bootstrap from seeds/puzzle-missions.json. The wire has no
/// stable id; importer assigns 1-based by capture order via the inherited <see cref="BaseEntity{TKey}.Id"/>.
/// See docs/api-spec/endpoints/post-login/basic-puzzle/mission.md.
/// </summary>
public class PuzzleMissionEntry : BaseEntity<int>
{
    /// <summary>Pre-localized name on the wire. "Clear all Round 1 puzzles".</summary>
    public string MissionName { get; set; } = string.Empty;

    /// <summary>Pre-localized achievement banner ("Cleared all Round 1 puzzles"). Derived by importer.</summary>
    public string AchievedMessage { get; set; } = string.Empty;

    public int RequireNumber { get; set; }
    public long CampaignCommenceTime { get; set; }
    public int OrderId { get; set; }

    // Reward (single-entry per mission)
    public UserGoodsType RewardType { get; set; }
    public long RewardDetailId { get; set; }
    public int RewardNumber { get; set; }

    /// <summary>
    /// Maps Round-N missions to their target group (300+N). NULL for Special-Round missions
    /// (deferred per Phase 1; they always surface as total_count=0).
    /// </summary>
    public int? TargetPuzzleGroupId { get; set; }
}
