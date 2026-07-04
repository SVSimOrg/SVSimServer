using SVSim.Database.Enums;

namespace SVSim.Database.Services;

/// <summary>
/// Registry of every string that can appear as <c>ViewerEventCounter.EventKey</c> or
/// mission/achievement <c>EventType</c>. Two goals:
///
/// <list type="number">
///   <item>Give emitters (controllers) a single named entry point per event family so no
///     controller inlines the string form.</item>
///   <item>Give the catalog seed importers a validation set — every <c>event_type</c> in a
///     seed row must start with a prefix registered here, else the importer throws at bootstrap.
///     Prevents silent drift between catalog data and emitter code (e.g. a mission that
///     references <c>practice_wln</c> and never advances).</item>
/// </list>
///
/// Format convention: colon-hierarchical, most-general first. Callers emit multiple levels
/// so a single event increments every level of counter the catalog might reference.
/// </summary>
public static class MissionEventKeys
{
    // ---- Top-level catalog prefixes (12) — 1:1 with seed JSON event_type strings ----

    public const string PracticeWin        = "practice_win";
    public const string RankedWin          = "ranked_win";
    public const string RankedOrArenaWin   = "ranked_or_arena_win";
    public const string DailyMatchWin      = "daily_match_win";
    public const string StoryChapterFinish = "story_chapter_finish";
    public const string ClassLevelUp       = "class_level_up";
    public const string RankAchieved       = "rank_achieved";
    public const string ChallengePlay      = "challenge_play";
    public const string ChallengeWin       = "challenge_win";
    public const string ChallengeFullClear = "challenge_full_clear";
    public const string PlayFollowers      = "play_followers";
    public const string PrivateMatchDistinctOpponent = "private_match_distinct_opponent";

    // ---- Item purchase (per-catalog-entry) ----

    public const string ItemPurchasePrefix = "item_purchase";
    public static string ItemPurchase(int catalogId) => $"{ItemPurchasePrefix}:{catalogId}";

    // ---- Practice hierarchical builders ----

    public static class Practice
    {
        /// <summary>
        /// Wire <c>difficulty</c> → tier name used in catalog rows. Values 4/6/7 correspond to
        /// Elite / Elite 2 / Elite 3 respectively (verified via
        /// practicetext.json + practice-opponents.json + practice_ai_setting.csv cross-reference).
        /// Other CSV difficulty values (0, 2, 3, 5, 101-109) have no achievement catalog rows —
        /// null return means "emit only the top-level counter."
        /// </summary>
        public static string? TierName(int wireDifficulty) => wireDifficulty switch
        {
            4 => "elite",
            6 => "elite2",
            7 => "elite3",
            _ => null,
        };

        /// <summary>
        /// Wire <c>enemy_class_id</c> → leader name used in catalog rows. Class ordering
        /// matches the <c>CardClass</c> enum (1=Forestcraft/Arisa, ..., 8=Portalcraft/Yuwan).
        /// </summary>
        public static string? LeaderName(int enemyClassId) => enemyClassId switch
        {
            1 => "arisa",
            2 => "erika",
            3 => "isabelle",
            4 => "rowen",
            5 => "luna",
            6 => "urias",
            7 => "eris",
            8 => "yuwan",
            _ => null,
        };

        /// <summary>
        /// Emits <c>practice_win</c> plus any hierarchical variants whose parts resolve. If the
        /// wire values don't resolve to a known tier or leader, that level is skipped — the
        /// lifetime counter still advances.
        /// </summary>
        public static IReadOnlyList<string> WinAll(int wireDifficulty, int enemyClassId)
        {
            var tier = TierName(wireDifficulty);
            var leader = LeaderName(enemyClassId);
            var list = new List<string>(3) { PracticeWin };
            if (tier is not null)
                list.Add($"{PracticeWin}:{tier}");
            if (tier is not null && leader is not null)
                list.Add($"{PracticeWin}:{tier}:{leader}");
            return list;
        }
    }

    // ---- Story hierarchical builders ----

    public static class Story
    {
        /// <summary>
        /// Emits <c>story_chapter_finish</c> plus <c>:{family}</c> plus <c>:{family}:{storyId}</c>.
        /// <paramref name="family"/> is the low-cardinality family label (<c>main</c>,
        /// <c>limited</c>, <c>event</c>, ...) resolved from <c>StoryApiType</c> upstream.
        /// </summary>
        public static IReadOnlyList<string> ChapterFinishAll(string family, long storyId) => new[]
        {
            StoryChapterFinish,
            $"{StoryChapterFinish}:{family}",
            $"{StoryChapterFinish}:{family}:{storyId}",
        };
    }

    // ---- Class-name mapping (shared by Ranked and ClassLevel families) ----

    public static class Class
    {
        /// <summary>
        /// Wire <c>class_id</c> 1-8 → catalog-facing craft name. Ordering matches the
        /// <see cref="SVSim.BattleNode.Bridge.CardClass"/> enum (1=Forestcraft ... 8=Portalcraft).
        /// Duplicated as a string switch rather than reused from the enum's <c>ToString()</c>
        /// because the catalog uses lowercase — an accidental case change would silently break
        /// counter alignment.
        /// </summary>
        public static string? Name(int classId) => classId switch
        {
            1 => "forestcraft",
            2 => "swordcraft",
            3 => "runecraft",
            4 => "dragoncraft",
            5 => "shadowcraft",
            6 => "bloodcraft",
            7 => "havencraft",
            8 => "portalcraft",
            _ => null,
        };
    }

    // ---- Ranked / Free / Challenge hierarchical builders ----

    public static class Ranked
    {
        /// <summary>
        /// Rank-battle win: emits <c>ranked_win</c>, the class-qualified variant, and the two
        /// aggregate keys (<c>ranked_or_arena_win</c>, <c>daily_match_win</c>) that every
        /// ranked/arena win advances.
        /// </summary>
        public static IReadOnlyList<string> WinAll(int classId)
        {
            var list = new List<string>(4)
            {
                RankedWin,
                RankedOrArenaWin,
                DailyMatchWin,
            };
            if (Class.Name(classId) is { } name)
                list.Add($"{RankedWin}:{name}");
            return list;
        }
    }

    public static class Free
    {
        /// <summary>
        /// Unranked/free-battle win: only the two aggregates. There is no <c>free_win</c>
        /// catalog prefix — free battles just count toward "any match" mission lines.
        /// </summary>
        public static IReadOnlyList<string> WinAll() => new[] { RankedOrArenaWin, DailyMatchWin };
    }

    public static class Challenge
    {
        /// <summary>Any TK2 match finish (win OR loss). Advances <c>challenge_play</c>.</summary>
        public static IReadOnlyList<string> MatchPlayAll() => new[] { ChallengePlay };

        /// <summary>
        /// TK2 match win: fires both <c>challenge_win</c> and <c>challenge_play</c> (a win is
        /// also a play) plus the two aggregates. Keeping <c>challenge_play</c> in this list
        /// makes the invariant "always increment play, additionally increment win" obvious.
        /// </summary>
        public static IReadOnlyList<string> MatchWinAll() => new[]
        {
            ChallengeWin, ChallengePlay, RankedOrArenaWin, DailyMatchWin,
        };

        /// <summary>TK2 run ended with all 5 wins — advances <c>challenge_full_clear</c>.</summary>
        public static IReadOnlyList<string> FullClearAll() => new[] { ChallengeFullClear };
    }

    // ---- ClassLevel / Rank hierarchical builders ----

    public static class ClassLevel
    {
        /// <summary>
        /// Class went up at least one level in this battle. Callsite must gate on
        /// <c>BattleXpGrantResult.LeveledUp</c> — this method doesn't check.
        /// </summary>
        public static IReadOnlyList<string> UpAll(int classId)
        {
            var list = new List<string>(2) { ClassLevelUp };
            if (Class.Name(classId) is { } name)
                list.Add($"{ClassLevelUp}:{name}");
            return list;
        }
    }

    public static class Rank
    {
        /// <summary>
        /// Viewer's rank crossed into a new tier. Callsite must gate on
        /// <c>RankProgressResult.TierAdvanced</c>. Tier name comes from
        /// <see cref="RankTier.Name(int)"/>.
        /// </summary>
        public static IReadOnlyList<string> AchievedAll(int rankId)
        {
            var list = new List<string>(2) { RankAchieved };
            if (RankTier.Name(rankId) is { } name)
                list.Add($"{RankAchieved}:{name}");
            return list;
        }
    }

    // ---- Seed-import validation ----

    private static readonly IReadOnlySet<string> _registeredPrefixes = new HashSet<string>
    {
        PracticeWin, RankedWin, RankedOrArenaWin, DailyMatchWin, StoryChapterFinish,
        ClassLevelUp, RankAchieved, ChallengePlay, ChallengeWin, ChallengeFullClear,
        PlayFollowers, PrivateMatchDistinctOpponent,
        ItemPurchasePrefix,
    };

    /// <summary>
    /// True iff <paramref name="eventType"/> is a registered top-level prefix or a hierarchical
    /// extension of one (<c>prefix</c> alone, or <c>prefix:qualifier</c>). Called by the
    /// achievement/mission catalog importers to catch drift between seed data and code.
    /// </summary>
    public static bool IsRegistered(string eventType)
    {
        foreach (var prefix in _registeredPrefixes)
        {
            if (eventType == prefix) return true;
            if (eventType.Length > prefix.Length + 1
                && eventType[prefix.Length] == ':'
                && eventType.AsSpan(0, prefix.Length).SequenceEqual(prefix))
                return true;
        }
        return false;
    }

    /// <summary>Exposed for test assertions; do NOT mutate.</summary>
    public static IReadOnlySet<string> RegisteredPrefixes => _registeredPrefixes;
}
