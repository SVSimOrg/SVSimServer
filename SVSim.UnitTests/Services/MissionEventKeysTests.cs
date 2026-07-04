using SVSim.Database.Enums;
using SVSim.Database.Services;

namespace SVSim.UnitTests.Services;

public class MissionEventKeysTests
{
    // ---- Practice.WinAll ----

    [Test]
    public void Practice_WinAll_emits_all_three_levels_for_known_tier_and_leader()
    {
        // Wire difficulty 4 = "elite", enemy_class_id 1 = "arisa" (Forestcraft).
        var keys = MissionEventKeys.Practice.WinAll(wireDifficulty: 4, enemyClassId: 1);
        Assert.That(keys, Is.EqualTo(new[]
        {
            "practice_win",
            "practice_win:elite",
            "practice_win:elite:arisa",
        }));
    }

    [Test]
    public void Practice_WinAll_maps_all_three_elite_tiers()
    {
        Assert.That(MissionEventKeys.Practice.WinAll(4, 2), Contains.Item("practice_win:elite:erika"));
        Assert.That(MissionEventKeys.Practice.WinAll(6, 2), Contains.Item("practice_win:elite2:erika"));
        Assert.That(MissionEventKeys.Practice.WinAll(7, 2), Contains.Item("practice_win:elite3:erika"));
    }

    [Test]
    public void Practice_WinAll_covers_all_eight_leaders()
    {
        Assert.Multiple(() =>
        {
            Assert.That(MissionEventKeys.Practice.WinAll(4, 1), Contains.Item("practice_win:elite:arisa"));
            Assert.That(MissionEventKeys.Practice.WinAll(4, 2), Contains.Item("practice_win:elite:erika"));
            Assert.That(MissionEventKeys.Practice.WinAll(4, 3), Contains.Item("practice_win:elite:isabelle"));
            Assert.That(MissionEventKeys.Practice.WinAll(4, 4), Contains.Item("practice_win:elite:rowen"));
            Assert.That(MissionEventKeys.Practice.WinAll(4, 5), Contains.Item("practice_win:elite:luna"));
            Assert.That(MissionEventKeys.Practice.WinAll(4, 6), Contains.Item("practice_win:elite:urias"));
            Assert.That(MissionEventKeys.Practice.WinAll(4, 7), Contains.Item("practice_win:elite:eris"));
            Assert.That(MissionEventKeys.Practice.WinAll(4, 8), Contains.Item("practice_win:elite:yuwan"));
        });
    }

    [Test]
    public void Practice_WinAll_falls_back_to_top_level_for_non_elite_difficulty()
    {
        // Wire difficulty 2 = "Advanced" (or similar) — not in the elite tier registry.
        // Emit only the top-level counter; hierarchical levels drop off.
        var keys = MissionEventKeys.Practice.WinAll(wireDifficulty: 2, enemyClassId: 1);
        Assert.That(keys, Is.EqualTo(new[] { "practice_win" }));
    }

    [Test]
    public void Practice_WinAll_drops_leader_level_for_unknown_class()
    {
        var keys = MissionEventKeys.Practice.WinAll(wireDifficulty: 4, enemyClassId: 99);
        Assert.That(keys, Is.EqualTo(new[] { "practice_win", "practice_win:elite" }));
    }

    // ---- Story.ChapterFinishAll ----

    [Test]
    public void Story_ChapterFinishAll_emits_all_three_levels()
    {
        var keys = MissionEventKeys.Story.ChapterFinishAll("main", 42);
        Assert.That(keys, Is.EqualTo(new[]
        {
            "story_chapter_finish",
            "story_chapter_finish:main",
            "story_chapter_finish:main:42",
        }));
    }

    // ---- ItemPurchase ----

    [Test]
    public void ItemPurchase_returns_prefixed_id_string()
    {
        Assert.That(MissionEventKeys.ItemPurchase(501), Is.EqualTo("item_purchase:501"));
    }

    // ---- IsRegistered ----

    [Test]
    public void IsRegistered_accepts_bare_top_level_prefix()
    {
        Assert.That(MissionEventKeys.IsRegistered("practice_win"), Is.True);
        Assert.That(MissionEventKeys.IsRegistered("ranked_or_arena_win"), Is.True);
        Assert.That(MissionEventKeys.IsRegistered("challenge_full_clear"), Is.True);
    }

    [Test]
    public void IsRegistered_accepts_hierarchical_extensions()
    {
        Assert.That(MissionEventKeys.IsRegistered("practice_win:elite:arisa"), Is.True);
        Assert.That(MissionEventKeys.IsRegistered("class_level_up:forestcraft"), Is.True);
        Assert.That(MissionEventKeys.IsRegistered("item_purchase:501"), Is.True);
        Assert.That(MissionEventKeys.IsRegistered("rank_achieved:master"), Is.True);
    }

    [Test]
    public void IsRegistered_rejects_typos_and_unknown_prefixes()
    {
        Assert.That(MissionEventKeys.IsRegistered("practice_wln"), Is.False);          // typo
        Assert.That(MissionEventKeys.IsRegistered("practice_win_extra"), Is.False);    // suffix without colon
        Assert.That(MissionEventKeys.IsRegistered("battle_win_total"), Is.False);      // test-only fixture
        Assert.That(MissionEventKeys.IsRegistered("orphan_event"), Is.False);
        Assert.That(MissionEventKeys.IsRegistered(""), Is.False);
    }

    // ---- Seed drift sanity check ----

    // ---- Class name mapping ----

    [Test]
    public void Class_Name_covers_all_eight_classes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(MissionEventKeys.Class.Name(1), Is.EqualTo("forestcraft"));
            Assert.That(MissionEventKeys.Class.Name(2), Is.EqualTo("swordcraft"));
            Assert.That(MissionEventKeys.Class.Name(3), Is.EqualTo("runecraft"));
            Assert.That(MissionEventKeys.Class.Name(4), Is.EqualTo("dragoncraft"));
            Assert.That(MissionEventKeys.Class.Name(5), Is.EqualTo("shadowcraft"));
            Assert.That(MissionEventKeys.Class.Name(6), Is.EqualTo("bloodcraft"));
            Assert.That(MissionEventKeys.Class.Name(7), Is.EqualTo("havencraft"));
            Assert.That(MissionEventKeys.Class.Name(8), Is.EqualTo("portalcraft"));
            Assert.That(MissionEventKeys.Class.Name(0), Is.Null);
            Assert.That(MissionEventKeys.Class.Name(9), Is.Null);
        });
    }

    // ---- Ranked / Free / Challenge ----

    [Test]
    public void Ranked_WinAll_emits_family_class_and_aggregates()
    {
        var keys = MissionEventKeys.Ranked.WinAll(classId: 1);
        Assert.That(keys, Is.EquivalentTo(new[]
        {
            "ranked_win",
            "ranked_win:forestcraft",
            "ranked_or_arena_win",
            "daily_match_win",
        }));
    }

    [Test]
    public void Ranked_WinAll_drops_class_variant_for_unknown_class()
    {
        var keys = MissionEventKeys.Ranked.WinAll(classId: 99);
        Assert.That(keys, Is.EquivalentTo(new[]
        {
            "ranked_win", "ranked_or_arena_win", "daily_match_win",
        }));
    }

    [Test]
    public void Free_WinAll_emits_only_aggregates()
    {
        Assert.That(MissionEventKeys.Free.WinAll(),
            Is.EquivalentTo(new[] { "ranked_or_arena_win", "daily_match_win" }));
    }

    [Test]
    public void Challenge_MatchPlayAll_top_level_only()
    {
        Assert.That(MissionEventKeys.Challenge.MatchPlayAll(),
            Is.EquivalentTo(new[] { "challenge_play" }));
    }

    [Test]
    public void Challenge_MatchWinAll_includes_play_win_and_aggregates()
    {
        Assert.That(MissionEventKeys.Challenge.MatchWinAll(), Is.EquivalentTo(new[]
        {
            "challenge_win", "challenge_play", "ranked_or_arena_win", "daily_match_win",
        }));
    }

    [Test]
    public void Challenge_FullClearAll_top_level_only()
    {
        Assert.That(MissionEventKeys.Challenge.FullClearAll(),
            Is.EquivalentTo(new[] { "challenge_full_clear" }));
    }

    // ---- ClassLevel ----

    [Test]
    public void ClassLevel_UpAll_emits_family_and_class_variant()
    {
        var keys = MissionEventKeys.ClassLevel.UpAll(classId: 3);
        Assert.That(keys, Is.EquivalentTo(new[] { "class_level_up", "class_level_up:runecraft" }));
    }

    [Test]
    public void ClassLevel_UpAll_drops_class_variant_for_unknown_class()
    {
        Assert.That(MissionEventKeys.ClassLevel.UpAll(classId: 0),
            Is.EquivalentTo(new[] { "class_level_up" }));
    }

    // ---- Rank ----

    [Test]
    public void Rank_AchievedAll_maps_all_seven_catalog_tiers()
    {
        Assert.Multiple(() =>
        {
            Assert.That(MissionEventKeys.Rank.AchievedAll(1),  Contains.Item("rank_achieved:beginner"));
            Assert.That(MissionEventKeys.Rank.AchievedAll(5),  Contains.Item("rank_achieved:d"));
            Assert.That(MissionEventKeys.Rank.AchievedAll(9),  Contains.Item("rank_achieved:c"));
            Assert.That(MissionEventKeys.Rank.AchievedAll(13), Contains.Item("rank_achieved:b"));
            Assert.That(MissionEventKeys.Rank.AchievedAll(17), Contains.Item("rank_achieved:a"));
            Assert.That(MissionEventKeys.Rank.AchievedAll(21), Contains.Item("rank_achieved:aa"));
            Assert.That(MissionEventKeys.Rank.AchievedAll(25), Contains.Item("rank_achieved:master"));
            Assert.That(MissionEventKeys.Rank.AchievedAll(26), Contains.Item("rank_achieved:grand_master"));
        });
    }

    [Test]
    public void Rank_AchievedAll_boundary_edges()
    {
        // Rank 4 = last Beginner slot; rank 5 = first D slot.
        Assert.That(RankTier.Name(4), Is.EqualTo("beginner"));
        Assert.That(RankTier.Name(5), Is.EqualTo("d"));
        // Rank 24 = last AA; rank 25 = Master; rank 26 = first Grand Master.
        Assert.That(RankTier.Name(24), Is.EqualTo("aa"));
        Assert.That(RankTier.Name(25), Is.EqualTo("master"));
        Assert.That(RankTier.Name(26), Is.EqualTo("grand_master"));
        // Out of range
        Assert.That(RankTier.Name(0), Is.Null);
        Assert.That(RankTier.Name(30), Is.Null);
    }

    [Test]
    public void Rank_AchievedAll_drops_tier_variant_for_out_of_range_rank()
    {
        Assert.That(MissionEventKeys.Rank.AchievedAll(0),
            Is.EquivalentTo(new[] { "rank_achieved" }));
        Assert.That(MissionEventKeys.Rank.AchievedAll(30),
            Is.EquivalentTo(new[] { "rank_achieved" }));
    }

    [Test]
    public void All_seed_prefixes_are_registered()
    {
        // Every prefix that could appear in a seed row must be in the registry. Enumerating
        // explicitly rather than reading the seed JSON — the point is to catch someone removing
        // a prefix from the code without noticing the seed still references it.
        string[] knownSeedPrefixes = {
            "practice_win", "ranked_win", "ranked_or_arena_win", "daily_match_win",
            "story_chapter_finish", "class_level_up", "rank_achieved",
            "challenge_play", "challenge_win", "challenge_full_clear",
            "play_followers", "private_match_distinct_opponent",
        };
        foreach (var prefix in knownSeedPrefixes)
        {
            Assert.That(MissionEventKeys.IsRegistered(prefix), Is.True, $"prefix '{prefix}' not registered");
        }
    }
}
