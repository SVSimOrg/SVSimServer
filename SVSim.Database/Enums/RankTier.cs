namespace SVSim.Database.Enums;

/// <summary>
/// Rank-tier bucket names used by mission/achievement catalog rows keyed as
/// <c>rank_achieved:{tier}</c>. Boundaries pinned from ranks.csv:
/// <list type="bullet">
///   <item>1-4 = Beginner 0-3</item>
///   <item>5-8 = D0-D3</item>
///   <item>9-12 = C0-C3</item>
///   <item>13-16 = B0-B3</item>
///   <item>17-20 = A0-A3</item>
///   <item>21-24 = AA0-AA3</item>
///   <item>25 = Master</item>
///   <item>26-29 = Grand Master (G026-G029)</item>
/// </list>
/// The current prod catalog has no <c>rank_achieved:grand_master</c> rows, but the
/// emit is still faithful — a grand-master promotion advances the top-level
/// <c>rank_achieved</c> counter and the tier-qualified one (which no catalog row
/// reads yet, but stays consistent if one lands later).
/// </summary>
public static class RankTier
{
    /// <summary>Wire rank_id → catalog-facing tier name. Null iff <paramref name="rankId"/> is out of range.</summary>
    public static string? Name(int rankId) => rankId switch
    {
        >= 1  and <= 4  => "beginner",
        >= 5  and <= 8  => "d",
        >= 9  and <= 12 => "c",
        >= 13 and <= 16 => "b",
        >= 17 and <= 20 => "a",
        >= 21 and <= 24 => "aa",
        25 => "master",
        >= 26 and <= 29 => "grand_master",
        _ => null,
    };
}
