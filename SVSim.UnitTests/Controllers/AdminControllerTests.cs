using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Entities.Story;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.EmulatedEntrypoint.Models.Dtos.Requests.Admin;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Admin;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// End-to-end coverage for <c>/admin/import_viewer</c>. The endpoint is [AllowAnonymous] +
/// [RequireAdminSecret], so tests reach it through <see cref="SVSimTestFactory.CreateAdminClient"/>
/// which bakes in the <c>X-Admin-Secret</c> header from <c>appsettings.Testing.json</c>. The
/// fresh-user path exercises the nav-graph NRE fix inside <c>ViewerRepository.RegisterViewer</c>;
/// the existing-user path exercises the owned-type Steam-id lookup. Negative-path tests at the
/// bottom of the file cover the header gate itself.
/// </summary>
public class AdminControllerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    [Test]
    public async Task ImportViewer_fresh_user_creates_viewer_and_returns_ids()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateAdminClient();

        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = 76_561_198_222_333_444UL,
            DisplayName = "Fresh User",
            CountryCode = "USA",
            TutorialState = 100,
            Currency = new ImportCurrency { Crystals = 12345 }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());

        var body = await response.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);

        Assert.That(body, Is.Not.Null);
        Assert.That(body!.ViewerId, Is.GreaterThan(0), "RegisterViewer must persist and return a non-zero id.");
        Assert.That(body.WasCreated, Is.True);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var stored = await db.Viewers
            .Include(v => v.SocialAccountConnections)
            .Include(v => v.Currency)
            .Include(v => v.Info)
            .FirstAsync(v => v.Id == body.ViewerId);

        Assert.That(stored.DisplayName, Is.EqualTo("Fresh User"));
        Assert.That(stored.Currency.Crystals, Is.EqualTo(12345UL),
            "ImportViewer should overwrite the seed-config crystal default with the requested value.");
        Assert.That(stored.Info.CountryCode, Is.EqualTo("USA"));
        Assert.That(stored.SocialAccountConnections.Count, Is.EqualTo(1));
        Assert.That(stored.SocialAccountConnections[0].AccountId, Is.EqualTo(76_561_198_222_333_444UL));
        Assert.That(stored.SocialAccountConnections[0].AccountType, Is.EqualTo(SocialAccountType.Steam));
    }

    [Test]
    public async Task ImportViewer_existing_user_updates_in_place()
    {
        using var factory = new SVSimTestFactory();
        const ulong steamId = 76_561_198_555_666_777UL;
        long seededId = await factory.SeedViewerAsync(steamId: steamId, displayName: "Original Name");

        using var client = factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            DisplayName = "Updated Name",
            CountryCode = "JPN"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());

        var body = await response.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);

        Assert.That(body, Is.Not.Null);
        Assert.That(body!.ViewerId, Is.EqualTo(seededId),
            "Re-importing the same SteamId must reuse the existing viewer row, not create a new one.");
        Assert.That(body.WasCreated, Is.False);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var stored = await db.Viewers
            .Include(v => v.Info)
            .FirstAsync(v => v.Id == seededId);

        Assert.That(stored.DisplayName, Is.EqualTo("Updated Name"));
        Assert.That(stored.Info.CountryCode, Is.EqualTo("JPN"));

        var viewerCount = await db.Viewers.CountAsync(v =>
            v.SocialAccountConnections.Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId));
        Assert.That(viewerCount, Is.EqualTo(1), "Owned-type dedup must not produce a second row.");
    }

    [Test]
    public async Task ImportViewer_missing_steam_id_returns_400()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateAdminClient();

        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = 0,
            DisplayName = "No Steam"
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task ImportViewer_imports_owned_cards_and_skips_unknown()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateAdminClient();

        // 10001001 is in the minimal test card set; 99999999 is not.
        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = 76_561_198_111_222_333UL,
            OwnedCards = new List<ImportCard>
            {
                new() { CardId = 10001001L, Count = 2, IsProtected = true },
                new() { CardId = 99999999L, Count = 1, IsProtected = false },
            }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());
        var body = await response.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);
        Assert.That(body!.SkippedCardCount, Is.EqualTo(1), "Unknown 99999999 must be skipped and counted.");

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var stored = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card)
            .FirstAsync(v => v.Id == body.ViewerId);

        Assert.That(stored.Cards.Count, Is.EqualTo(1), "Only the known card should be stored.");
        var owned = stored.Cards.Single();
        Assert.That(owned.Card.Id, Is.EqualTo(10001001L));
        Assert.That(owned.Count, Is.EqualTo(2));
        Assert.That(owned.IsProtected, Is.True);
    }

    [Test]
    public async Task ImportViewer_clamps_card_count_to_max_copies()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateAdminClient();

        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = 76_561_198_111_222_334UL,
            OwnedCards = new List<ImportCard> { new() { CardId = 10001002L, Count = 5 } }
        });
        var body = await response.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var stored = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card)
            .FirstAsync(v => v.Id == body!.ViewerId);
        Assert.That(stored.Cards.Single().Count, Is.EqualTo(3),
            "Count must clamp to OwnedCardEntry.MaxCopies (3).");
    }

    [Test]
    public async Task ImportViewer_replaces_existing_card_collection()
    {
        using var factory = new SVSimTestFactory();
        const ulong steamId = 76_561_198_111_222_335UL;
        long viewerId = await factory.SeedViewerAsync(steamId: steamId);
        await factory.SeedOwnedCardAsync(viewerId, 10001001L, count: 3);

        using var client = factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            OwnedCards = new List<ImportCard> { new() { CardId = 10001002L, Count = 1 } }
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var stored = await db.Viewers.Include(v => v.Cards).ThenInclude(c => c.Card)
            .FirstAsync(v => v.Id == viewerId);
        Assert.That(stored.Cards.Select(c => c.Card.Id), Is.EquivalentTo(new[] { 10001002L }),
            "Full replace: the pre-seeded 10001001 must be gone, only 10001002 present.");
    }

    [Test]
    public async Task ImportViewer_imports_items_and_replaces_existing()
    {
        using var factory = new SVSimTestFactory();
        const ulong steamId = 76_561_198_111_222_336UL;
        long viewerId = await factory.SeedViewerAsync(steamId: steamId);
        // Registers the ItemEntry master row (70001) and gives an initial owned count to be replaced.
        await factory.SeedOwnedItemAsync(viewerId, itemId: 70001, count: 1);

        using var client = factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Items = new List<ImportItem>
            {
                new() { ItemId = 70001, Count = 5 },
                new() { ItemId = 88888, Count = 9 }, // unknown master id -> skipped silently
            }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());
        Assert.That(await factory.GetOwnedItemCountAsync(viewerId, 70001), Is.EqualTo(5),
            "Full replace: 70001 count updated to 5.");
        Assert.That(await factory.GetOwnedItemCountAsync(viewerId, 88888), Is.EqualTo(0),
            "Unknown item master id must not be inserted.");
    }

    [Test]
    public async Task ImportViewer_imports_deck_with_correct_format_and_skips_unknown_cards()
    {
        using var factory = new SVSimTestFactory();
        const ulong steamId = 76_561_198_111_222_337UL;
        long viewerId = await factory.SeedViewerAsync(steamId: steamId);

        int classId, leaderSkinId; long sleeveId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            classId = (await db.Classes.FirstAsync()).Id;
            sleeveId = (await db.Sleeves.FirstAsync()).Id;
            leaderSkinId = (await db.LeaderSkins.FirstAsync()).Id;
        }

        using var client = factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Decks = new List<ImportDeck>
            {
                new()
                {
                    DeckFormat = 1, // wire Rotation
                    DeckNo = 1,
                    DeckName = "Imported Rotation",
                    ClassId = classId,
                    SleeveId = sleeveId,
                    LeaderSkinId = leaderSkinId,
                    CardIdArray = new List<long> { 10001001L, 10001001L, 99999999L }, // last is unknown
                }
            }
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());
        var body = await response.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);
        Assert.That(body!.SkippedCardCount, Is.EqualTo(1), "Unknown deck card 99999999 counts as skipped.");

        using var scope2 = factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var stored = await db2.Viewers
            .Include(v => v.Decks).ThenInclude(d => d.Cards).ThenInclude(c => c.Card)
            .FirstAsync(v => v.Id == viewerId);
        var deck = stored.Decks.Single(d => d.Name == "Imported Rotation");
        Assert.That(deck.Format, Is.EqualTo(Format.Rotation));
        Assert.That(deck.Cards.Single().Card.Id, Is.EqualTo(10001001L));
        Assert.That(deck.Cards.Single().Count, Is.EqualTo(2), "Two copies of 10001001 grouped.");
    }

    [Test]
    public async Task ImportViewer_myrotation_deck_gets_rotation_id()
    {
        using var factory = new SVSimTestFactory();
        await factory.SeedGlobalsAsync(); // populates MyRotationSettings
        const ulong steamId = 76_561_198_111_222_338UL;
        long viewerId = await factory.SeedViewerAsync(steamId: steamId);

        int classId, leaderSkinId; long sleeveId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            classId = (await db.Classes.FirstAsync()).Id;
            sleeveId = (await db.Sleeves.FirstAsync()).Id;
            leaderSkinId = (await db.LeaderSkins.FirstAsync()).Id;
        }

        using var client = factory.CreateAdminClient();
        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Decks = new List<ImportDeck>
            {
                new()
                {
                    DeckFormat = 5, // wire MyRotation
                    DeckNo = 1,
                    DeckName = "Imported MyRot",
                    ClassId = classId,
                    SleeveId = sleeveId,
                    LeaderSkinId = leaderSkinId,
                    CardIdArray = new List<long> { 10001001L },
                }
            }
        });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());
        var body = await response.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);

        using var scope2 = factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var deck = await db2.Set<SVSim.Database.Models.ShadowverseDeckEntry>()
            .FirstAsync(d => d.Name == "Imported MyRot");
        Assert.That(deck.Format, Is.EqualTo(Format.MyRotation));
        Assert.That(deck.MyRotationId, Is.Not.Null.And.Not.Empty,
            "MyRotation decks need a rotation id or the client NREs on click.");
    }

    [Test]
    public async Task ImportViewer_fresh_user_has_no_decks_when_none_imported()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateAdminClient();

        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = 76_561_198_111_222_339UL,
            DisplayName = "No Decks"
        });
        var body = await response.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var stored = await db.Viewers.Include(v => v.Decks).FirstAsync(v => v.Id == body!.ViewerId);
        Assert.That(stored.Decks, Is.Empty,
            "Default-deck cloning was removed; a fresh viewer with no imported decks has none.");
    }

    [Test]
    public async Task ImportViewer_binds_new_fields_from_literal_client_json()
    {
        using var factory = new SVSimTestFactory();
        const ulong steamId = 76_561_198_111_222_340UL;
        long viewerId = await factory.SeedViewerAsync(steamId: steamId);
        await factory.SeedOwnedItemAsync(viewerId, itemId: 70001, count: 0); // register item master

        int classId, leaderSkinId; long sleeveId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            classId = (await db.Classes.FirstAsync()).Id;
            sleeveId = (await db.Sleeves.FirstAsync()).Id;
            leaderSkinId = (await db.LeaderSkins.FirstAsync()).Id;
        }

        string json = $$"""
        {
          "steam_id": {{steamId}},
          "owned_cards": [ { "card_id": 10001001, "count": 2, "is_protected": true } ],
          "items": [ { "item_id": 70001, "count": 4 } ],
          "decks": [ {
            "deck_format": 1,
            "deck_no": 2,
            "deck_name": "Wire Deck",
            "class_id": {{classId}},
            "sleeve_id": {{sleeveId}},
            "leader_skin_id": {{leaderSkinId}},
            "is_random_leader_skin": 0,
            "card_id_array": [10001001, 10001002]
          } ]
        }
        """;

        using var client = factory.CreateAdminClient();
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/admin/import_viewer", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());

        using var scope2 = factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var stored = await db2.Viewers
            .Include(v => v.Cards).ThenInclude(c => c.Card)
            .Include(v => v.Items).ThenInclude(i => i.Item)
            .Include(v => v.Decks)
            .FirstAsync(v => v.Id == viewerId);

        Assert.That(stored.Cards.Any(c => c.Card.Id == 10001001L && c.Count == 2 && c.IsProtected), Is.True,
            "owned_cards snake_case keys must bind (card_id/count/is_protected).");
        Assert.That(stored.Items.Any(i => i.Item.Id == 70001 && i.Count == 4), Is.True,
            "items snake_case keys must bind (item_id/count).");
        Assert.That(stored.Decks.Any(d => d.Name == "Wire Deck" && d.Format == Format.Rotation), Is.True,
            "decks snake_case keys must bind (deck_format/deck_no/class_id/card_id_array/...).");
    }

    [Test]
    public async Task ImportViewer_tolerates_numeric_my_rotation_id_and_skips_empty_decks()
    {
        using var factory = new SVSimTestFactory();
        const ulong steamId = 76_561_198_111_222_341UL;
        long viewerId = await factory.SeedViewerAsync(steamId: steamId);

        int classId, leaderSkinId; long sleeveId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            classId = (await db.Classes.FirstAsync()).Id;
            sleeveId = (await db.Sleeves.FirstAsync()).Id;
            leaderSkinId = (await db.LeaderSkins.FirstAsync()).Id;
        }

        // Mirrors a real prod dump: empty MyRotation slots carry "my_rotation_id": 0 (a NUMBER,
        // not a string), and dozens of empty slots accompany the few real decks.
        string json = $$"""
        {
          "steam_id": {{steamId}},
          "decks": [
            { "deck_format": 2, "deck_no": 1, "deck_name": "Real", "class_id": {{classId}},
              "sleeve_id": {{sleeveId}}, "leader_skin_id": {{leaderSkinId}},
              "is_random_leader_skin": 0, "card_id_array": [10001001, 10001002] },
            { "deck_format": 5, "deck_no": 1, "deck_name": "", "class_id": 0,
              "sleeve_id": 3000011, "leader_skin_id": 0, "is_random_leader_skin": 0,
              "my_rotation_id": 0, "card_id_array": [] },
            { "deck_format": 1, "deck_no": 3, "deck_name": "", "class_id": 1,
              "sleeve_id": 3000011, "leader_skin_id": 0, "is_random_leader_skin": 0,
              "card_id_array": [] }
          ]
        }
        """;

        using var client = factory.CreateAdminClient();
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/admin/import_viewer", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            await response.Content.ReadAsStringAsync());

        using var scope2 = factory.Services.CreateScope();
        var db2 = scope2.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var stored = await db2.Viewers.Include(v => v.Decks).FirstAsync(v => v.Id == viewerId);
        Assert.That(stored.Decks.Count, Is.EqualTo(1),
            "Empty deck slots must be skipped; only the real (non-empty) deck imports.");
        Assert.That(stored.Decks.Single().Name, Is.EqualTo("Real"));
    }

    [Test]
    public async Task ImportViewer_MissionMeta_RoundTrips()
    {
        using var factory = new SVSimTestFactory();
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000001UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            MissionMeta = new ImportMissionMeta
            {
                HasReceivedPickTwoMission = true,
                MissionReceiveType = 2,
                MissionChangeTime = 1_700_000_000L
            }
        });
        resp.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewer = await db.Viewers
            .Include(v => v.MissionData)
            .SingleAsync(v => v.SocialAccountConnections
                .Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId));

        Assert.That(viewer.MissionData, Is.Not.Null);
        Assert.That(viewer.MissionData!.HasReceivedPickTwoMission, Is.True);
        Assert.That(viewer.MissionData.MissionReceiveType, Is.EqualTo(2));
        // MissionChangeTime is DateTime in storage; verify it survives the round-trip from unix seconds.
        Assert.That(viewer.MissionData.MissionChangeTime,
            Is.EqualTo(DateTimeOffset.FromUnixTimeSeconds(1_700_000_000L).UtcDateTime));
    }

    [Test]
    public async Task ImportViewer_Missions_RoundTripsWithCounter()
    {
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.MissionCatalog.Add(new MissionCatalogEntry
            {
                Id = 9001, LotType = 6 /* daily */,
                EventType = "battle_win_total", EventArg = null
            });
            await db.SaveChangesAsync();
        }
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000002UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Missions = new List<ImportMission>
            {
                new() { MissionId = 9001, MissionStatus = 1, TotalCount = 7 }
            }
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);
        Assert.That(body!.SkippedMissionCount, Is.EqualTo(0));
        Assert.That(body.SkippedMissionCounterCount, Is.EqualTo(0));

        using var scope = factory.Services.CreateScope();
        var verifyDb = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewerId = await verifyDb.Viewers
            .Where(v => v.SocialAccountConnections
                .Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId))
            .Select(v => v.Id).SingleAsync();
        Assert.That(verifyDb.ViewerMissions.Count(m => m.ViewerId == viewerId && m.MissionCatalogId == 9001), Is.EqualTo(1));
        Assert.That(verifyDb.ViewerEventCounters
            .Count(c => c.ViewerId == viewerId && c.EventKey == "battle_win_total" && c.Count == 7), Is.EqualTo(1));
    }

    [Test]
    public async Task ImportViewer_Missions_UnknownMissionIdSkipped()
    {
        using var factory = new SVSimTestFactory();
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000003UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Missions = new List<ImportMission>
            {
                new() { MissionId = 999999, MissionStatus = 1, TotalCount = 3 }
            }
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);
        Assert.That(body!.SkippedMissionCount, Is.EqualTo(1));

        using var scope = factory.Services.CreateScope();
        var verifyDb2 = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        Assert.That(verifyDb2.ViewerMissions.Any(), Is.False);
    }

    [Test]
    public async Task ImportViewer_Missions_CatalogPresentButCounterUnresolvable()
    {
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var seedDb = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            seedDb.MissionCatalog.Add(new MissionCatalogEntry
            {
                Id = 9002, LotType = 6,
                EventType = null /* unresolvable */, EventArg = null
            });
            await seedDb.SaveChangesAsync();
        }
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000004UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Missions = new List<ImportMission>
            {
                new() { MissionId = 9002, MissionStatus = 1, TotalCount = 5 }
            }
        });
        var body = await resp.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);
        Assert.That(body!.SkippedMissionCount, Is.EqualTo(0));
        Assert.That(body.SkippedMissionCounterCount, Is.EqualTo(1));

        using var scope = factory.Services.CreateScope();
        var verifyDb3 = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        Assert.That(verifyDb3.ViewerMissions.Count(m => m.MissionCatalogId == 9002), Is.EqualTo(1));
        Assert.That(verifyDb3.ViewerEventCounters.Any(), Is.False);
    }

    [Test]
    public async Task ImportViewer_Missions_TwoMissionsSharingEventType_SingleCounterUpsert()
    {
        // Two weekly missions (LotType=2) share the same EventType — same (EventKey, WeekKey)
        // period — but occupy different slots (1 and 2 via explicit slot override) so the slot
        // unique-index is satisfied. Without the pre-materialized counterCache the second DB read
        // won't see the first mission's pending Add → duplicate insert / unique-constraint failure.
        // With the fix both missions upsert the SAME counter, resulting in exactly one row.
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var seedDb = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            seedDb.MissionCatalog.AddRange(
                new MissionCatalogEntry { Id = 9003, LotType = 2, EventType = "battle_win_total", EventArg = null },
                new MissionCatalogEntry { Id = 9004, LotType = 2, EventType = "battle_win_total", EventArg = null });
            await seedDb.SaveChangesAsync();
        }
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000005UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Missions = new List<ImportMission>
            {
                new() { MissionId = 9003, MissionStatus = 1, TotalCount = 10, Slot = 1 },
                new() { MissionId = 9004, MissionStatus = 1, TotalCount = 10, Slot = 2 },
            }
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);
        Assert.That(body!.SkippedMissionCount, Is.EqualTo(0));
        Assert.That(body.SkippedMissionCounterCount, Is.EqualTo(0));

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewerId = await verifyDb.Viewers
            .Where(v => v.SocialAccountConnections
                .Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId))
            .Select(v => v.Id).SingleAsync();

        // Must be exactly ONE counter row for (EventKey="battle_win_total"), not two.
        Assert.That(
            verifyDb.ViewerEventCounters.Count(c => c.ViewerId == viewerId && c.EventKey == "battle_win_total"),
            Is.EqualTo(1));
        Assert.That(
            verifyDb.ViewerEventCounters
                .Single(c => c.ViewerId == viewerId && c.EventKey == "battle_win_total").Count,
            Is.EqualTo(10));
    }

    [Test]
    public async Task ImportViewer_Achievements_RoundTripsWithCounter()
    {
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.AchievementCatalog.Add(new AchievementCatalogEntry
            {
                AchievementType = 501, Level = 3, EventType = "cards_owned_total", EventArg = null
            });
            await db.SaveChangesAsync();
        }
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000007UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Achievements = new List<ImportAchievement>
            {
                new() { AchievementType = 501, Level = 3, NowAchievedLevel = 3, ResultAnnounceSawLevel = 2, TotalCount = 42 }
            }
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);
        Assert.That(body!.SkippedAchievementCount, Is.EqualTo(0));
        Assert.That(body.SkippedAchievementCounterCount, Is.EqualTo(0));

        using var scope = factory.Services.CreateScope();
        var db2 = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewerId = await db2.Viewers
            .Where(v => v.SocialAccountConnections
                .Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId))
            .Select(v => v.Id).SingleAsync();
        var ach = await db2.ViewerAchievements
            .SingleAsync(a => a.ViewerId == viewerId && a.AchievementType == 501);
        Assert.That(ach.Level, Is.EqualTo(3));
        Assert.That(ach.NowAchievedLevel, Is.EqualTo(3));
        Assert.That(ach.ResultAnnounceSawLevel, Is.EqualTo(2));
        Assert.That(db2.ViewerEventCounters
            .Count(c => c.ViewerId == viewerId && c.EventKey == "cards_owned_total" && c.Count == 42), Is.EqualTo(1));
    }

    [Test]
    public async Task ImportViewer_Achievements_UnknownTypeSkipped()
    {
        using var factory = new SVSimTestFactory();
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000008UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Achievements = new List<ImportAchievement>
            {
                new() { AchievementType = 999999, Level = 1, NowAchievedLevel = 1, ResultAnnounceSawLevel = 0, TotalCount = 1 }
            }
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);
        Assert.That(body!.SkippedAchievementCount, Is.EqualTo(1));

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        Assert.That(db.ViewerAchievements.Any(), Is.False);
    }

    [Test]
    public async Task ImportViewer_Achievements_TwoAchievementsSharingEventType_SingleCounterUpsert()
    {
        // Two achievements with different (AchievementType, Level) but the same EventType — same
        // (EventKey, AllTime) cache key. Without the pre-materialized achievementCounterCache the
        // second DB read won't see the first achievement's pending Add → duplicate insert /
        // unique-constraint failure. With the fix both achievements upsert the SAME counter,
        // resulting in exactly one ViewerEventCounter row.
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var seedDb = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            seedDb.AchievementCatalog.AddRange(
                new AchievementCatalogEntry { AchievementType = 601, Level = 1, EventType = "battle_win_total", EventArg = null },
                new AchievementCatalogEntry { AchievementType = 602, Level = 1, EventType = "battle_win_total", EventArg = null });
            await seedDb.SaveChangesAsync();
        }
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000009UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Achievements = new List<ImportAchievement>
            {
                new() { AchievementType = 601, Level = 1, NowAchievedLevel = 1, ResultAnnounceSawLevel = 0, TotalCount = 20 },
                new() { AchievementType = 602, Level = 1, NowAchievedLevel = 1, ResultAnnounceSawLevel = 0, TotalCount = 20 },
            }
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);
        Assert.That(body!.SkippedAchievementCount, Is.EqualTo(0));
        Assert.That(body.SkippedAchievementCounterCount, Is.EqualTo(0));

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewerId = await verifyDb.Viewers
            .Where(v => v.SocialAccountConnections
                .Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId))
            .Select(v => v.Id).SingleAsync();

        // Must be exactly ONE counter row for (EventKey="battle_win_total"), not two.
        Assert.That(
            verifyDb.ViewerEventCounters.Count(c => c.ViewerId == viewerId && c.EventKey == "battle_win_total"),
            Is.EqualTo(1));
        Assert.That(
            verifyDb.ViewerEventCounters
                .Single(c => c.ViewerId == viewerId && c.EventKey == "battle_win_total").Count,
            Is.EqualTo(20));
    }

    [Test]
    public async Task ImportViewer_Missions_ExplicitSlotRoundTrips()
    {
        // A weekly catalog mission (LotType=2) posted with an explicit slot=3 must persist Slot=3,
        // not fall back to the LotType-derived default of 1.
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var seedDb = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            seedDb.MissionCatalog.Add(new MissionCatalogEntry
            {
                Id = 9005, LotType = 2 /* weekly */,
                EventType = null, EventArg = null
            });
            await seedDb.SaveChangesAsync();
        }
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000006UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Missions = new List<ImportMission>
            {
                new() { MissionId = 9005, MissionStatus = 1, TotalCount = 0, Slot = 3 }
            }
        });
        resp.EnsureSuccessStatusCode();

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewerId = await verifyDb.Viewers
            .Where(v => v.SocialAccountConnections
                .Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId))
            .Select(v => v.Id).SingleAsync();

        var mission = await verifyDb.ViewerMissions
            .SingleAsync(m => m.ViewerId == viewerId && m.MissionCatalogId == 9005);
        Assert.That(mission.Slot, Is.EqualTo(3));
    }

    [Test]
    public async Task ImportViewer_StoryProgress_OffsetsByFamily()
    {
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.StoryWorlds.Add(new StoryWorld { Id = 5 });           // Main
            db.StoryWorlds.Add(new StoryWorld { Id = 10_000_005 });  // Limited
            db.StoryWorlds.Add(new StoryWorld { Id = 20_000_005 });  // Event
            await db.SaveChangesAsync();
        }
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000007UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            StoryProgress = new List<ImportStoryProgress>
            {
                new() { StoryApiType = 1, StoryId = 5, IsFinish = true,  IsSkipped = false },
                new() { StoryApiType = 2, StoryId = 5, IsFinish = false, IsSkipped = true  },
                new() { StoryApiType = 3, StoryId = 5, IsFinish = true,  IsSkipped = true  }
            }
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);
        Assert.That(body!.SkippedStoryCount, Is.EqualTo(0));

        using var scope = factory.Services.CreateScope();
        var verifyDb = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewerId = await verifyDb.Viewers
            .Where(v => v.SocialAccountConnections
                .Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId))
            .Select(v => v.Id).SingleAsync();

        var ids = verifyDb.ViewerStoryProgress.Where(p => p.ViewerId == viewerId).Select(p => p.StoryId).ToList();
        Assert.That(ids, Is.EquivalentTo(new[] { 5, 10_000_005, 20_000_005 }));
    }

    [Test]
    public async Task ImportViewer_StoryProgress_SubChapterUsesSubId()
    {
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.StoryWorlds.Add(new StoryWorld { Id = 10 });   // parent chapter
            db.StoryWorlds.Add(new StoryWorld { Id = 11 });   // sub_chapter (own row)
            await db.SaveChangesAsync();
        }
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000008UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            StoryProgress = new List<ImportStoryProgress>
            {
                new() { StoryApiType = 1, StoryId = 10, IsFinish = true,  IsSkipped = false },
                new() { StoryApiType = 1, StoryId = 10, SubChapterId = 11, IsFinish = true, IsSkipped = true }
            }
        });
        resp.EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var verifyDb = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewerId = await verifyDb.Viewers
            .Where(v => v.SocialAccountConnections
                .Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId))
            .Select(v => v.Id).SingleAsync();
        var ids = verifyDb.ViewerStoryProgress.Where(p => p.ViewerId == viewerId).Select(p => p.StoryId).ToList();
        Assert.That(ids, Is.EquivalentTo(new[] { 10, 11 }));
    }

    [Test]
    public async Task ImportViewer_StoryProgress_UnknownStoryIdSkipped()
    {
        using var factory = new SVSimTestFactory();
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000009UL;

        var resp = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            StoryProgress = new List<ImportStoryProgress>
            {
                new() { StoryApiType = 1, StoryId = 9999, IsFinish = true, IsSkipped = false }
            }
        });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<ImportViewerResponse>(JsonOptions);
        Assert.That(body!.SkippedStoryCount, Is.EqualTo(1));

        using var scope = factory.Services.CreateScope();
        var verifyDb = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        Assert.That(verifyDb.ViewerStoryProgress.Any(), Is.False);
    }

    [Test]
    public async Task ImportViewer_Missions_ShrinkingReImport_OrphanCounterRemoved()
    {
        // First import: two missions with distinct event types → two ViewerEventCounter rows.
        // Second import: only the "kept" mission re-sent → orphan counter for "orphan_event"
        // must be deleted; kept_event counter must remain with the new TotalCount.
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.MissionCatalog.AddRange(
                new MissionCatalogEntry { Id = 9100, LotType = 6, EventType = "orphan_event", EventArg = null },
                new MissionCatalogEntry { Id = 9101, LotType = 6, EventType = "kept_event",   EventArg = null });
            await db.SaveChangesAsync();
        }

        var client = factory.CreateAdminClient();
        const ulong steamId = 70000000000000020UL;

        // First POST: both missions present (explicit slots to avoid the (ViewerId,Slot) unique index).
        var resp1 = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Missions = new List<ImportMission>
            {
                new() { MissionId = 9100, MissionStatus = 1, TotalCount = 10, Slot = 0 },
                new() { MissionId = 9101, MissionStatus = 1, TotalCount = 5,  Slot = 1 },
            }
        });
        resp1.EnsureSuccessStatusCode();

        long viewerId;
        using (var scope = factory.Services.CreateScope())
        {
            var verifyDb = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            viewerId = await verifyDb.Viewers
                .Where(v => v.SocialAccountConnections
                    .Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId))
                .Select(v => v.Id).SingleAsync();
            // Sanity: two counters after first import.
            Assert.That(verifyDb.ViewerEventCounters.Count(c => c.ViewerId == viewerId), Is.EqualTo(2),
                "After first import, both orphan_event and kept_event counters must exist.");
        }

        // Second POST: only the kept mission.
        var resp2 = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Missions = new List<ImportMission>
            {
                new() { MissionId = 9101, MissionStatus = 1, TotalCount = 7, Slot = 1 },
            }
        });
        resp2.EnsureSuccessStatusCode();

        using var verifyScope = factory.Services.CreateScope();
        var db2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var counters = db2.ViewerEventCounters.Where(c => c.ViewerId == viewerId).ToList();
        Assert.That(counters.Count, Is.EqualTo(1),
            "After shrinking re-import, the orphan_event counter must be deleted; only kept_event survives.");
        Assert.That(counters.Single().EventKey, Is.EqualTo("kept_event"),
            "Surviving counter must be for kept_event.");
        Assert.That(counters.Single().Count, Is.EqualTo(7),
            "kept_event counter must reflect the new TotalCount from the second import.");
    }

    [Test]
    public async Task ImportViewer_Achievements_ShrinkingReImport_OrphanCounterRemoved()
    {
        // First import: two achievements with distinct event types → two ViewerEventCounter rows.
        // Second import: only the "kept" achievement re-sent → orphan counter for "orphan_ach"
        // must be deleted; kept_ach counter must remain with the new TotalCount.
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.AchievementCatalog.AddRange(
                new AchievementCatalogEntry { AchievementType = 9200, Level = 1, EventType = "orphan_ach", EventArg = null },
                new AchievementCatalogEntry { AchievementType = 9201, Level = 1, EventType = "kept_ach",   EventArg = null });
            await db.SaveChangesAsync();
        }

        var client = factory.CreateAdminClient();
        const ulong steamId = 70000000000000021UL;

        // First POST: both achievements present.
        var resp1 = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Achievements = new List<ImportAchievement>
            {
                new() { AchievementType = 9200, Level = 1, NowAchievedLevel = 1, ResultAnnounceSawLevel = 0, TotalCount = 15 },
                new() { AchievementType = 9201, Level = 1, NowAchievedLevel = 1, ResultAnnounceSawLevel = 0, TotalCount = 8 },
            }
        });
        resp1.EnsureSuccessStatusCode();

        long viewerId;
        using (var scope = factory.Services.CreateScope())
        {
            var verifyDb = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            viewerId = await verifyDb.Viewers
                .Where(v => v.SocialAccountConnections
                    .Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId))
                .Select(v => v.Id).SingleAsync();
            // Sanity: two counters after first import.
            Assert.That(verifyDb.ViewerEventCounters.Count(c => c.ViewerId == viewerId), Is.EqualTo(2),
                "After first import, both orphan_ach and kept_ach counters must exist.");
        }

        // Second POST: only the kept achievement.
        var resp2 = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = steamId,
            Achievements = new List<ImportAchievement>
            {
                new() { AchievementType = 9201, Level = 1, NowAchievedLevel = 1, ResultAnnounceSawLevel = 0, TotalCount = 12 },
            }
        });
        resp2.EnsureSuccessStatusCode();

        using var verifyScope = factory.Services.CreateScope();
        var db2 = verifyScope.ServiceProvider.GetRequiredService<SVSimDbContext>();

        var counters = db2.ViewerEventCounters.Where(c => c.ViewerId == viewerId).ToList();
        Assert.That(counters.Count, Is.EqualTo(1),
            "After shrinking re-import, the orphan_ach counter must be deleted; only kept_ach survives.");
        Assert.That(counters.Single().EventKey, Is.EqualTo("kept_ach"),
            "Surviving counter must be for kept_ach.");
        Assert.That(counters.Single().Count, Is.EqualTo(12),
            "kept_ach counter must reflect the new TotalCount from the second import.");
    }

    [Test]
    public async Task ImportViewer_AllNewSections_AreIdempotentOnReImport()
    {
        using var factory = new SVSimTestFactory();
        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.MissionCatalog.Add(new MissionCatalogEntry { Id = 8001, LotType = 6, EventType = "x", EventArg = null });
            db.AchievementCatalog.Add(new AchievementCatalogEntry { AchievementType = 800, Level = 1, EventType = "y", EventArg = null });
            db.StoryWorlds.Add(new StoryWorld { Id = 50 });
            await db.SaveChangesAsync();
        }
        var client = factory.CreateAdminClient();
        ulong steamId = 70000000000000010UL;

        var req = new ImportViewerRequest
        {
            SteamId = steamId,
            MissionMeta = new ImportMissionMeta { HasReceivedPickTwoMission = true, MissionReceiveType = 1, MissionChangeTime = 1L },
            Missions = new List<ImportMission> { new() { MissionId = 8001, MissionStatus = 1, TotalCount = 5 } },
            Achievements = new List<ImportAchievement> { new() { AchievementType = 800, Level = 1, NowAchievedLevel = 1, ResultAnnounceSawLevel = 0, TotalCount = 9 } },
            StoryProgress = new List<ImportStoryProgress> { new() { StoryApiType = 1, StoryId = 50, IsFinish = true } }
        };
        (await client.PostAsJsonAsync("/admin/import_viewer", req)).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync("/admin/import_viewer", req)).EnsureSuccessStatusCode();

        using var scope = factory.Services.CreateScope();
        var verifyDb = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var viewerId = await verifyDb.Viewers
            .Where(v => v.SocialAccountConnections
                .Any(s => s.AccountType == SocialAccountType.Steam && s.AccountId == steamId))
            .Select(v => v.Id).SingleAsync();

        Assert.That(verifyDb.ViewerMissions.Count(m => m.ViewerId == viewerId), Is.EqualTo(1));
        Assert.That(verifyDb.ViewerAchievements.Count(a => a.ViewerId == viewerId), Is.EqualTo(1));
        Assert.That(verifyDb.ViewerStoryProgress.Count(p => p.ViewerId == viewerId), Is.EqualTo(1));
        Assert.That(verifyDb.ViewerEventCounters.Count(c => c.ViewerId == viewerId), Is.EqualTo(2));
    }

    // --- X-Admin-Secret gate ---

    [Test]
    public async Task ImportViewer_without_secret_header_returns_401()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient(); // no X-Admin-Secret header

        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = 76_561_198_999_999_001UL
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task ImportViewer_with_wrong_secret_returns_401()
    {
        using var factory = new SVSimTestFactory();
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Admin-Secret", "not-the-real-secret");

        var response = await client.PostAsJsonAsync("/admin/import_viewer", new ImportViewerRequest
        {
            SteamId = 76_561_198_999_999_002UL
        });

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
}
