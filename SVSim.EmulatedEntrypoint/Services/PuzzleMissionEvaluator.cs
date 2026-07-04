using SVSim.Database.Models;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Pure service — maps the puzzle mission catalog against a viewer's cleared-puzzle set and
/// produces per-mission (total_count, is_achieved) statuses. Used by both /basic_puzzle/mission
/// (snapshot) and /basic_puzzle/finish (post-clear delta detection).
/// </summary>
public sealed class PuzzleMissionEvaluator
{
    public sealed record MissionStatus(PuzzleMissionEntry Mission, int TotalCount, bool IsAchieved);

    public IReadOnlyList<MissionStatus> Evaluate(
        IEnumerable<PuzzleMissionEntry> catalog,
        IReadOnlyDictionary<int, HashSet<int>> clearedByGroup)
    {
        var result = new List<MissionStatus>();
        foreach (var mission in catalog)
        {
            int count = ComputeTotalCount(mission, clearedByGroup);
            result.Add(new MissionStatus(mission, count, count >= mission.RequireNumber));
        }
        return result;
    }

    /// <summary>Returns ONLY the missions whose status flipped from not-achieved to achieved
    /// between before and after. Other missions (already-achieved, still-incomplete) are omitted.</summary>
    public IReadOnlyList<MissionStatus> FreshlyCompleted(
        IEnumerable<PuzzleMissionEntry> catalog,
        IReadOnlyDictionary<int, HashSet<int>> clearedByGroupBefore,
        IReadOnlyDictionary<int, HashSet<int>> clearedByGroupAfter)
    {
        var result = new List<MissionStatus>();
        foreach (var mission in catalog)
        {
            int before = ComputeTotalCount(mission, clearedByGroupBefore);
            int after  = ComputeTotalCount(mission, clearedByGroupAfter);
            bool wasAchieved = before >= mission.RequireNumber;
            bool isAchieved  = after  >= mission.RequireNumber;
            if (!wasAchieved && isAchieved)
                result.Add(new MissionStatus(mission, after, true));
        }
        return result;
    }

    private static int ComputeTotalCount(PuzzleMissionEntry mission, IReadOnlyDictionary<int, HashSet<int>> clearedByGroup)
    {
        if (mission.TargetPuzzleGroupId is not int groupId) return 0;
        if (!clearedByGroup.TryGetValue(groupId, out var cleared)) return 0;
        return Math.Min(cleared.Count, mission.RequireNumber);
    }
}
