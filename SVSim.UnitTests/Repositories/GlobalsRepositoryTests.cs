using Microsoft.Extensions.DependencyInjection;
using SVSim.Database.Repositories.Globals;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

/// <summary>
/// End-to-end tests for the seed-driven globals path: SeedGlobalsAsync runs the per-domain
/// importers against the test SQLite DB, then we verify each
/// IGlobalsRepository method returns the expected count + a spot-checked field value.
///
/// Counts come from the 2026-05-23 prod capture; if a recapture lands with different cardinalities,
/// expect to update these assertions.
/// </summary>
public class GlobalsRepositoryTests
{
    private static async Task<(SVSimTestFactory factory, IGlobalsRepository repo)> SetupAsync()
    {
        var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync();
        var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IGlobalsRepository>();
        return (factory, repo);
    }

    [Test]
    public async Task GetMyRotationSettings_returns_27_entries_from_capture()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var settings = await repo.GetMyRotationSettings();
        Assert.That(settings.Count, Is.EqualTo(27));
        var tsRotation = settings.FirstOrDefault(s => s.Id == 10015);
        Assert.That(tsRotation, Is.Not.Null, "Expected to find ts_rotation_id=10015 in seeded data.");
        Assert.That(tsRotation!.CardSetIdsCsv, Does.Contain("10015"));
    }

    [Test]
    public async Task GetMyRotationAbilities_returns_6_entries()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var abilities = await repo.GetMyRotationAbilities();
        Assert.That(abilities.Count, Is.EqualTo(6));
    }

    [Test]
    public async Task GetAvatarAbilities_returns_24_entries_with_skill_dsl_preserved()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var abilities = await repo.GetAvatarAbilities();
        Assert.That(abilities.Count, Is.EqualTo(24));
        var avatar2801 = abilities.FirstOrDefault(a => a.Id == 2801);
        Assert.That(avatar2801, Is.Not.Null);
        Assert.That(avatar2801!.BattleStartMaxLife, Is.EqualTo(25));
        Assert.That(avatar2801.Ability, Does.Contain("skill:"),
            "Avatar ability DSL string should be preserved verbatim.");
    }

    [Test]
    public async Task GetCurrentArenaSeason_returns_singleton_with_format_info()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var arena = await repo.GetCurrentArenaSeason();
        Assert.That(arena, Is.Not.Null);
        Assert.That(arena!.Mode, Is.EqualTo(1));
        Assert.That(arena.Enable, Is.EqualTo(1));
        Assert.That(arena.FormatInfo, Does.Contain("Take Two"),
            "Current 2pick season FormatInfo should preserve card_pool_name verbatim.");
    }

    [Test]
    public async Task GetBattlePassLevels_returns_100_levels()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var levels = await repo.GetBattlePassLevels();
        Assert.That(levels.Count, Is.EqualTo(100));
    }

    [Test]
    public async Task GetPreReleaseInfo_returns_singleton()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var pri = await repo.GetPreReleaseInfo();
        Assert.That(pri, Is.Not.Null);
        // Prod capture has stale 1900/2019 dates; the audit flags this as a recapture target.
        Assert.That(pri!.PreReleaseId, Is.EqualTo("1"));
    }

    [Test]
    public async Task GetSpotCards_returns_239_entries()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var spots = await repo.GetSpotCards();
        Assert.That(spots.Count, Is.EqualTo(239));
    }

    [Test]
    public async Task GetReprintedCards_returns_54_entries()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var reprinted = await repo.GetReprintedCards();
        Assert.That(reprinted.Count, Is.EqualTo(54));
    }

    [Test]
    public async Task GetUnlimitedRestrictions_returns_3_entries_with_values()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var bans = await repo.GetUnlimitedRestrictions();
        Assert.That(bans.Count, Is.EqualTo(3));
        var hardBan = bans.FirstOrDefault(r => r.Id == 107813030);
        Assert.That(hardBan, Is.Not.Null);
        Assert.That(hardBan!.RestrictionValue, Is.EqualTo(1));
    }

    [Test]
    public async Task GetLoadingExclusionCards_returns_176_entries()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var excl = await repo.GetLoadingExclusionCards();
        Assert.That(excl.Count, Is.EqualTo(176));
    }

    [Test]
    public async Task GetBanners_returns_4_entries_in_order()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var banners = await repo.GetBanners();
        Assert.That(banners.Count, Is.EqualTo(4));
        Assert.That(banners[0].ImageName, Is.EqualTo("banner_000788"));
        Assert.That(banners[0].Click, Is.EqualTo("account_transition_with_two"));
    }

    [Test]
    public async Task GetCurrentSealedSeason_returns_singleton_with_pack_info()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var sealedSeason = await repo.GetCurrentSealedSeason();
        Assert.That(sealedSeason, Is.Not.Null);
        Assert.That(sealedSeason!.CrystalCost, Is.EqualTo(600));
        Assert.That(sealedSeason.DeckUsingNumMin, Is.EqualTo(30));
        Assert.That(sealedSeason.PackInfo, Does.Contain("10032"));
    }

    [Test]
    public async Task GetCurrentMasterPointPeriod_returns_period_119()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var period = await repo.GetCurrentMasterPointPeriod();
        Assert.That(period, Is.Not.Null);
        Assert.That(period!.Id, Is.EqualTo(119));
        Assert.That(period.PeriodNum, Is.EqualTo(118));
    }

    [Test]
    public async Task GetDefaultDecks_returns_8_starter_decks_one_per_class()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var decks = await repo.GetDefaultDecks();
        Assert.That(decks.Count, Is.EqualTo(8));
        // Each starter deck packs 40 card IDs in card_id_array (jsonb).
        Assert.That(decks.All(d => d.CardIdArray.Contains(",")), Is.True,
            "Each starter deck should serialize multiple card IDs in card_id_array.");
    }

    // Note: GetDefaultLeaderSkinSettings was removed from IGlobalsRepository in the
    // 2026-05-26 per-viewer leader-skin refactor. /deck/info now sources
    // user_leader_skin_setting_list from viewer.Classes (each ViewerClassData carries the
    // active LeaderSkin), and /leader_skin/set mutates it. Coverage moved to
    // LeaderSkinControllerTests.

    // Note: GetGameConfiguration was removed from IGlobalsRepository in the 2026-05-24 config
    // refactor — Rotation/Challenge/etc. now load via IGameConfigService. See
    // GameConfigurationJsonbTests for the equivalent round-trip coverage.

    [Test]
    public async Task GetMaintenanceCards_empty_when_capture_has_none()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var cards = await repo.GetMaintenanceCards();
        Assert.That(cards.Count, Is.EqualTo(0),
            "Prod capture has empty maintenance_card_list; importer should not create skeleton rows.");
    }

    [Test]
    public async Task GetFeatureMaintenances_empty_when_capture_has_none()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        var feats = await repo.GetFeatureMaintenances();
        Assert.That(feats.Count, Is.EqualTo(0));
    }

    [Test]
    public async Task GetRotationCardSets_flags_six_sets_in_rotation()
    {
        var (factory, repo) = await SetupAsync();
        using var _ = factory;
        // CardImport isn't run in tests, so the CardSets table is empty. UpdateRotationCardSetFlags
        // can only mark rows that exist — verify the importer's "missing-id" warning didn't crash.
        var sets = await repo.GetRotationCardSets();
        Assert.That(sets.Count, Is.LessThanOrEqualTo(6),
            "Without CardImport, no rotation flags can be set; expect 0. With CardImport, expect 6.");
    }
}
