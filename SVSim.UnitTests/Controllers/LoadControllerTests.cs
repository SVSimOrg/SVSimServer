using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// Coverage for <c>/load/index</c>. The endpoint hits the heaviest <c>.Include</c> chain in the
/// app (<c>ViewerRepository.GetViewerByShortUdid</c>) and serializes the wide
/// <c>IndexResponse</c> shape — first end-to-end exercise of either against a real EF provider.
/// Shape assertions are split per test so a single regression pinpoints one named expectation.
/// </summary>
public class LoadControllerTests
{
    private const string IndexRequestJson =
        """{"viewer_id":"0","steam_id":0,"steam_session_ticket":"","carrier":"steam","card_master_hash":""}""";

    private static string SeedDir => Path.Combine(AppContext.BaseDirectory, "Data", "seeds");

    /// <summary>
    /// Wire keys (from <c>[Key("...")]</c> / mirrored <c>[JsonPropertyName]</c>) for fields the
    /// client reads UNCONDITIONALLY in <c>LoadDetail.ConvertJsonData</c> (no <c>Keys.Contains</c>
    /// or <c>TryGetValue</c> guard). Missing any of these crashes the client.
    ///
    /// Fields that ARE guarded by the client get a separate, dedicated assertion (or no assertion)
    /// — they're allowed to be omitted. Examples: <c>arena_info</c>, <c>daily_login_bonus</c>,
    /// <c>battle_pass_level_info</c>, <c>pre_release_info</c>, <c>my_rotation_info</c>,
    /// <c>avatar_info</c>, <c>item_expire_date</c> are all optional per
    /// <c>docs/api-spec/endpoints/post-login/load-index.md</c>.
    /// </summary>
    private static readonly string[] RequiredIndexKeys =
    {
        "user_tutorial", "user_info", "user_crystal_count", "user_item_list",
        "user_deck_rotation", "user_deck_unlimited", "user_deck_my_rotation",
        "user_card_list", "user_class_list", "user_sleeve_list", "user_emblem_list",
        "user_degree_list", "user_leader_skin_list", "user_mypage_list",
        "user_rank", "user_rank_match_list", "challenge_config",
        "red_ether_overwrite_list", "maintenance_card_list", "rank_info",
        "class_exp", "loading_exclusion_card_list", "default_setting",
        "unlimited_restricted_base_card_id_list", "rotation_card_set_id_list",
        "reprinted_base_card_ids", "spot_cards", "feature_maintenance_list",
        "special_crystal_info", "open_battle_field_id_list", "loot_box_regulation",
        "gathering_info", "user_config", "deck_format", "card_set_id_for_resource_dl_view"
    };

    private static async Task<JsonElement> PostIndexAndReadBody(SVSimTestFactory factory, long viewerId)
    {
        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/load/index",
            new StringContent(IndexRequestJson, Encoding.UTF8, "application/json"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

        var doc = JsonDocument.Parse(body);
        return doc.RootElement.Clone();
    }

    [Test]
    public async Task Index_with_minimal_viewer_returns_200()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var response = await client.PostAsync("/load/index",
            new StringContent(IndexRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task Index_with_no_auth_header_returns_401()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsync("/load/index",
            new StringContent(IndexRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Index_returns_all_required_keys()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        var root = await PostIndexAndReadBody(factory, viewerId);

        var missing = RequiredIndexKeys.Where(k => !root.TryGetProperty(k, out _)).ToList();
        Assert.That(missing, Is.Empty,
            $"Required IndexResponse keys missing: {string.Join(", ", missing)}");
    }

    [Test]
    public async Task Index_rank_info_is_array_not_dict()
    {
        // Guards the dict-vs-array regression that ate a previous release. Client iterates
        // user_rank by index; a dict would silently deserialize as zero entries.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        var root = await PostIndexAndReadBody(factory, viewerId);

        Assert.That(root.GetProperty("user_rank").ValueKind, Is.EqualTo(JsonValueKind.Array));
    }

    [Test]
    public async Task Index_user_rank_has_five_entries()
    {
        // Hard-coded format list in LoadController.RankFormats — five entries, one per
        // deck_format discriminator. Client indexes by format value; mismatched count
        // would point the wrong format at the wrong rank slot.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        var root = await PostIndexAndReadBody(factory, viewerId);

        Assert.That(root.GetProperty("user_rank").GetArrayLength(), Is.EqualTo(5));
    }

    [Test]
    public async Task Index_user_rank_deck_formats_are_wire_codes_not_internal_enum()
    {
        // Regression for the /load/index KeyNotFoundException crash (2026-05-23):
        // server was emitting (int)Format directly, so deck_format 0 (Format.Rotation
        // internal) reached the client, ParseApiFormat mapped wire-0 to Format.Max, and
        // LoadDetail._userRank[2] threw. Wire codes per Data.FormatConvertApi:
        //   Rotation→1, Unlimited→2, Crossover→4, MyRotation→5, Avatar→39.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        var root = await PostIndexAndReadBody(factory, viewerId);

        var deckFormats = root.GetProperty("user_rank").EnumerateArray()
            .Select(e => e.GetProperty("deck_format").GetInt32())
            .ToList();
        Assert.That(deckFormats, Is.EquivalentTo(new[] { 1, 2, 5, 39, 4 }),
            "user_rank entries must carry wire deck_format codes, not internal Format ints.");

        // The top-level deck_format default is also a wire code (Rotation = wire 1).
        Assert.That(root.GetProperty("deck_format").GetInt32(), Is.EqualTo(1));
    }

    [Test]
    public async Task Index_rotation_card_set_id_list_has_at_least_two_entries()
    {
        // LoadDetail.cs:184 unconditionally indexes [1] and [Count-1] — fewer than two
        // entries crashes the client at the home screen.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        var root = await PostIndexAndReadBody(factory, viewerId);

        Assert.That(root.GetProperty("rotation_card_set_id_list").GetArrayLength(),
            Is.GreaterThanOrEqualTo(2));
    }

    [Test]
    public async Task Index_omits_arena_info_when_empty()
    {
        // ArenaData(JsonData) ctor reads data[0] inside the Keys.Contains("arena_info")
        // branch (LoadDetail.cs:261 → ArenaData.cs:48) — an empty array crashes the client
        // with ArgumentOutOfRangeException. Field must be absent when there's no arena.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        var root = await PostIndexAndReadBody(factory, viewerId);

        Assert.That(root.TryGetProperty("arena_info", out _), Is.False,
            "arena_info must be omitted when empty; the client crashes on []. " +
            "If you re-add it, populate at least one entry with a valid format_info.");
    }

    [Test]
    public async Task Index_user_card_list_excludes_zero_count_entries()
    {
        // Documents the divergence from prod (see load-index.md §user_card_list policy).
        // Our server emits only owned cards (Count > 0) plus basics; prod returns a
        // larger curated set that includes some 0-count "ever-touched" rows we don't
        // model. The client falls back to 0 for absent ids (DataMgr.cs:1182), so this
        // is semantically safe — but if anything ever starts emitting Count=0 rows again
        // (e.g. someone re-introduces a left-join against the full card catalog), this
        // test pins the policy.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        var root = await PostIndexAndReadBody(factory, viewerId);

        var userCards = root.GetProperty("user_card_list");
        Assert.That(userCards.ValueKind, Is.EqualTo(JsonValueKind.Array));
        var zeroCount = userCards.EnumerateArray()
            .Where(c => c.GetProperty("number").GetInt32() == 0)
            .ToList();
        Assert.That(zeroCount, Is.Empty,
            "user_card_list must not contain Count=0 entries; we ship only the owned-only " +
            "subset (plus basics with count=3). See load-index.md §user_card_list policy.");
    }

    [Test]
    public async Task Index_when_viewer_has_no_decks_returns_empty_format_lists()
    {
        // A freshly-registered viewer has no decks of any format. The three per-format deck
        // containers must still be present and empty so the client's iteration is well-formed.
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        var root = await PostIndexAndReadBody(factory, viewerId);

        foreach (var key in new[] { "user_deck_rotation", "user_deck_unlimited", "user_deck_my_rotation" })
        {
            var container = root.GetProperty(key);
            Assert.That(container.ValueKind, Is.EqualTo(JsonValueKind.Object),
                $"{key} should be the UserFormatDeckInfo object wrapper, not a raw array.");
            var inner = container.GetProperty("user_deck_list");
            Assert.That(inner.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(inner.GetArrayLength(), Is.EqualTo(0),
                $"{key}.user_deck_list must be an empty array for a deckless viewer, not null.");
        }
    }

    [Test]
    public async Task Index_surfaces_seeded_globals_after_bootstrap()
    {
        // Verifies the end-to-end seed → repo → controller wiring for the load-index globals.
        // Counts and spot-checked values come from the 2026-05-23 capture; if a recapture lands
        // with different cardinalities, update the assertions alongside.
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        long viewerId = await factory.SeedViewerAsync();

        var root = await PostIndexAndReadBody(factory, viewerId);

        // SpotCards: dict[card_id_str] → cost, 239 entries
        var spotCards = root.GetProperty("spot_cards");
        Assert.That(spotCards.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(spotCards.EnumerateObject().Count(), Is.EqualTo(239), "spot_cards entry count");

        // ReprintedCards: flat number[], 54 entries
        var reprinted = root.GetProperty("reprinted_base_card_ids");
        Assert.That(reprinted.GetArrayLength(), Is.EqualTo(54), "reprinted_base_card_ids length");

        // UnlimitedBanList: dict[card_id_str] → restriction value, 3 entries; 107813030 = hard ban
        var bans = root.GetProperty("unlimited_restricted_base_card_id_list");
        Assert.That(bans.EnumerateObject().Count(), Is.EqualTo(3));
        Assert.That(bans.GetProperty("107813030").GetInt32(), Is.EqualTo(1));

        // LoadingExclusion: 176 ids
        Assert.That(root.GetProperty("loading_exclusion_card_list").GetArrayLength(), Is.EqualTo(176));

        // GameConfiguration-sourced scalars
        Assert.That(root.GetProperty("is_battle_pass_period").GetBoolean(), Is.True,
            "is_battle_pass_period is bool on the wire (matches prod 2026-05-23)");
        Assert.That(root.GetProperty("card_set_id_for_resource_dl_view").GetInt32(), Is.EqualTo(1));

        // challenge_config sourced from GameConfiguration cols
        var challenge = root.GetProperty("challenge_config");
        Assert.That(challenge.GetProperty("use_challenge_two_pick_premium_card").GetInt32(), Is.EqualTo(0));
        Assert.That(challenge.GetProperty("challenge_two_pick_sleeve_id").GetInt32(), Is.EqualTo(3000011));

        // arena_info: single element with format_info populated
        Assert.That(root.TryGetProperty("arena_info", out var arenaInfo), Is.True,
            "arena_info present once an ArenaSeasonConfig row is seeded");
        Assert.That(arenaInfo.GetArrayLength(), Is.EqualTo(1));
        var fi = arenaInfo[0].GetProperty("format_info");
        Assert.That(fi.GetProperty("card_pool_name").GetString(), Does.Contain("Take Two"));

        // my_rotation_info: setting dict has 27 entries
        var mri = root.GetProperty("my_rotation_info");
        Assert.That(mri.GetProperty("setting").EnumerateObject().Count(), Is.EqualTo(27));
        Assert.That(mri.GetProperty("abilities").EnumerateObject().Count(), Is.EqualTo(6));

        // my_rotation_info.schedules drives the client's "Custom Rotation" button visibility
        // (Wizard/MyRotationAllInfo.cs:45 — IsMyRotationEnable). RotationConfigImporter sources the
        // window from the prod capture; default-initialised DateTime.MinValue values would hide
        // the button. Assert the captured 2024→2030 free_battle window round-trips through the
        // MyRotationScheduleConfig section.
        var fb = mri.GetProperty("schedules").GetProperty("free_battle");
        Assert.That(DateTime.Parse(fb.GetProperty("begin_time").GetString()!),
            Is.EqualTo(new DateTime(2024, 5, 1, 20, 0, 0, DateTimeKind.Utc)));
        Assert.That(DateTime.Parse(fb.GetProperty("end_time").GetString()!),
            Is.EqualTo(new DateTime(2030, 6, 26, 19, 59, 59, DateTimeKind.Utc)));

        // avatar_info: abilities dict has 24 entries; schedules is empty list
        var ai = root.GetProperty("avatar_info");
        Assert.That(ai.GetProperty("abilities").EnumerateObject().Count(), Is.EqualTo(24));
        Assert.That(ai.GetProperty("schedules").ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(ai.GetProperty("schedules").GetArrayLength(), Is.EqualTo(0));

        // pre_release_info: present (singleton seeded, even with stale dates per audit)
        Assert.That(root.TryGetProperty("pre_release_info", out var pri), Is.True);
        Assert.That(pri.GetProperty("id").GetString(), Is.EqualTo("1"));

        // rotation_card_set_id_list: now comes from the real CardSets table — six entries after
        // RotationFlagUpdater flags IsInRotation on the rotation_card_set_ids seeded list. But
        // CardImport isn't run in tests, so the table is empty and we fall back to StubRotationSets
        // (3 entries). That's still ≥ 2 so the client won't crash.
        Assert.That(root.GetProperty("rotation_card_set_id_list").GetArrayLength(),
            Is.GreaterThanOrEqualTo(2));

        // daily_login_bonus IS emitted for a fresh viewer (LastLoginBonusClaimedAt is null →
        // IsDue returns true). The seeded globals test uses a fresh viewer, so it gets Day 1.
        Assert.That(root.TryGetProperty("daily_login_bonus", out var dlbGlobals), Is.True,
            "daily_login_bonus should be present for a fresh viewer (IsDue=true)");
        Assert.That(dlbGlobals.ValueKind, Is.EqualTo(JsonValueKind.Object));

        // battle_pass_level_info is present when levels are seeded — 100-entry dict keyed by level string.
        Assert.That(root.TryGetProperty("battle_pass_level_info", out var bpli), Is.True,
            "battle_pass_level_info must be present once BattlePassLevels rows are seeded");
        Assert.That(bpli.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(bpli.EnumerateObject().Count(), Is.EqualTo(100));
    }

    [Test]
    public async Task LoadIndex_GrantsMissingCosmeticsForOwnedCards()
    {
        // Verifies the C.2 wiring: /load/index invokes ICardAcquisitionService in backfill mode,
        // so a viewer who already owns a leader card but lacks its associated cosmetics gets
        // them granted on the next load. Mirrors the in-flight migration story for existing
        // accounts that pre-date the cosmetic-grant feature.
        using var factory = new SVSimTestFactory();
        var viewerId = await factory.SeedViewerAsync();

        // Seed viewer with leader card 704741010 (count=1), seed mapping → skin 407, seed
        // master skin row. Viewer does NOT yet own the skin.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await db.Viewers.Include(v => v.Cards).FirstAsync(v => v.Id == viewerId);
            var card = await db.Cards.FindAsync(704741010L);
            if (card is null)
            {
                card = new ShadowverseCardEntry { Id = 704741010L, Name = "TestLeader", Rarity = Rarity.Legendary, IsFoil = false };
                db.Cards.Add(card);
            }
            viewer.Cards.Add(new OwnedCardEntry { Card = card, Count = 1, IsProtected = false });
            db.CardCosmeticRewards.Add(new CardCosmeticReward { CardId = 704741010L, Type = CosmeticType.Skin, CosmeticId = 407L, Quantity = 1 });
            if (await db.LeaderSkins.FindAsync(407) is null)
                db.LeaderSkins.Add(new LeaderSkinEntry { Id = 407, Name = "TestSkin407" });
            await db.SaveChangesAsync();
        }

        // Call /load/index — backfill should fire as part of the action.
        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/load/index",
            new StringContent(IndexRequestJson, Encoding.UTF8, "application/json"));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());

        // Verify the response payload includes the backfilled cosmetic (not just the DB state).
        // This guards against a regression where the controller serves a stale viewer snapshot
        // (GetViewerByShortUdid uses .AsNoTracking() so the in-memory `viewer` reference does
        // not see writes the service makes on its own tracked instance — without a post-grant
        // re-fetch the first /load/index would report the skin as un-owned even though the DB
        // had been updated). user_leader_skin_list always carries all master skins; the per-entry
        // is_owned flag is the actual ownership signal.
        using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var skin407 = doc.RootElement.GetProperty("user_leader_skin_list").EnumerateArray()
            .FirstOrDefault(e => e.GetProperty("leader_skin_id").GetInt32() == 407);
        Assert.That(skin407.ValueKind, Is.Not.EqualTo(JsonValueKind.Undefined),
            "response payload should include leader skin 407 entry");
        Assert.That(skin407.GetProperty("is_owned").GetBoolean(), Is.True,
            "response payload should mark backfilled skin 407 as owned, not just DB state");

        // Verify skin 407 was actually granted by re-reading viewer state.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            var viewer = await db.Viewers.Include(v => v.LeaderSkins).FirstAsync(v => v.Id == viewerId);
            Assert.That(viewer.LeaderSkins.Any(s => s.Id == 407), Is.True,
                "skin 407 should have been backfilled by /load/index");
        }
    }

    [Test]
    public async Task Index_emits_daily_login_bonus_for_fresh_viewer()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        var root = await PostIndexAndReadBody(factory, viewerId);

        Assert.That(root.TryGetProperty("daily_login_bonus", out var dlb), Is.True);
        Assert.That(dlb.ValueKind, Is.EqualTo(JsonValueKind.Object));

        var normal = dlb.GetProperty("normal");
        Assert.That(normal.GetProperty("now_count").GetInt32(), Is.EqualTo(1));
        Assert.That(normal.GetProperty("name").GetString(), Is.EqualTo("Daily Bonus"));
        Assert.That(normal.GetProperty("campaign_id").ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(normal.GetProperty("img").ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(normal.GetProperty("reward").GetArrayLength(), Is.EqualTo(15));

        var firstReward = normal.GetProperty("reward")[0];
        Assert.That(firstReward.GetProperty("reward_type").GetString(), Is.EqualTo("9"));
        Assert.That(firstReward.GetProperty("reward_number").GetString(), Is.EqualTo("20"));

        Assert.That(dlb.GetProperty("campaign").ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(dlb.GetProperty("campaign").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task Index_omits_daily_login_bonus_on_second_call_same_day()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();

        await PostIndexAndReadBody(factory, viewerId);
        var root = await PostIndexAndReadBody(factory, viewerId);

        var present = root.TryGetProperty("daily_login_bonus", out var dlb)
                      && dlb.ValueKind != JsonValueKind.Null;
        Assert.That(present, Is.False, "Second-same-day /load/index must not re-emit the bonus");
    }

    [Test]
    public async Task LoadIndex_emits_battle_pass_level_info_with_100_entries_when_period_active()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            await new SVSim.Bootstrap.Importers.BattlePassImporter().ImportAsync(db, SeedDir);
        }

        using var client = factory.CreateAuthenticatedClient(viewerId);
        var response = await client.PostAsync("/load/index",
            new StringContent(IndexRequestJson, System.Text.Encoding.UTF8, "application/json"));
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK), body);

        using var doc = System.Text.Json.JsonDocument.Parse(body);
        var levels = doc.RootElement.GetProperty("battle_pass_level_info");
        Assert.That(levels.ValueKind, Is.EqualTo(System.Text.Json.JsonValueKind.Object));
        Assert.That(levels.GetProperty("1").GetProperty("level").GetString(), Is.EqualTo("1"));
        Assert.That(levels.GetProperty("1").GetProperty("required_point").GetString(), Is.EqualTo("0"));
        Assert.That(levels.GetProperty("100").GetProperty("required_point").GetString(), Is.EqualTo("49500"));
    }
}
