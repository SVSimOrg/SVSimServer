using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Integration;

public class ArenaTwoPickEndToEndTests
{
    [Test]
    public async Task Full_draft_then_retire_at_zero_wins_grants_seed_rewards()
    {
        using var factory = new SVSimTestFactory();

        // Load globals: challenge-config (pool_card_set_ids includes 10015), item master
        // (includes 80001, the run-end reward ticket), and arena-two-pick rewards.
        await factory.SeedGlobalsAsync();

        // Seed card set 10015 with one Bronze collectible card per class (1-8) + one neutral.
        // The card pool service queries sets in ChallengeConfig.PoolCardSetIds, which already
        // includes 10015 via the seeded challenge-config.json.
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();

            var set = new ShadowverseCardSetEntry { Id = 10015, Name = "TK2PoolSet", IsInRotation = true };

            // One card per class id 1-8 (already seeded by ReferenceDataImporter/classes.csv).
            for (int classId = 1; classId <= 8; classId++)
            {
                var cls = await db.Classes.FindAsync(classId);
                if (cls is null)
                {
                    cls = new ClassEntry { Id = classId, Name = $"Class{classId}" };
                    db.Classes.Add(cls);
                    await db.SaveChangesAsync();
                }
                set.Cards.Add(new ShadowverseCardEntry
                {
                    Id = 10015_000_00L + classId,
                    Name = $"TK2ClassCard{classId}",
                    Rarity = Rarity.Bronze,
                    Class = cls,
                    CollectionInfo = new CardCollectionInfo { CraftCost = 200, DustReward = 50 },
                });
            }

            // One neutral card.
            set.Cards.Add(new ShadowverseCardEntry
            {
                Id = 10015_000_09L,
                Name = "TK2NeutralCard",
                Rarity = Rarity.Bronze,
                Class = null,
                CollectionInfo = new CardCollectionInfo { CraftCost = 200, DustReward = 50 },
            });

            db.CardSets.Add(set);
            await db.SaveChangesAsync();

            // Seed the reward catalog.
            await new ArenaTwoPickRewardImporter().ImportAsync(
                db, Path.Combine(AppContext.BaseDirectory, "Data", "seeds"));
        }

        // Seed viewer with 5 entry tickets (item id 1 = challenge ticket).
        long viewerId = await factory.SeedViewerAsync();
        await factory.SeedOwnedItemAsync(viewerId, itemId: 1, count: 5,
            itemName: "TK2 Entry Ticket", itemType: 2);

        // Capture starting Rupees so the retire assertion can compute expected post-state
        // regardless of the default-grants config value (currently 50 000).
        var (_, startRupees, _) = await factory.GetViewerCurrencyAsync(viewerId);

        using var client = factory.CreateAuthenticatedClient(viewerId);

        // Every TK2 request DTO inherits BaseRequest; the [ApiController] auto-400 path
        // rejects bodies missing the envelope fields. Each PostAsync below carries them.
        const string Vid = "0";
        const int Sid = 0;
        const string Stk = "";

        // 1) /top → entry_info:null (no active run).
        var top = await client.PostAsync("/arena_two_pick/top",
            JsonContent.Create(new { mode = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        Assert.That(top.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        StringAssert.Contains("\"entry_info\":null", await top.Content.ReadAsStringAsync());

        // 2) /entry → deducts 1 entry ticket (id 1, post-state = 4), returns 3 candidate class ids.
        var entry = await client.PostAsync("/arena_two_pick/entry",
            JsonContent.Create(new { consume_item_type = 3, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        Assert.That(entry.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"/entry failed: {await entry.Content.ReadAsStringAsync()}");
        using var entryDoc = JsonDocument.Parse(await entry.Content.ReadAsStringAsync());
        var candidates = entryDoc.RootElement.GetProperty("candidate_class_ids")
            .EnumerateArray().Select(e => e.GetInt32()).ToList();
        Assert.That(candidates.Count, Is.EqualTo(3), "Entry must offer exactly 3 candidate classes");

        // 3) /class_choose with first candidate → returns candidate_card_list.
        var classChoose = await client.PostAsync("/arena_two_pick/class_choose",
            JsonContent.Create(new { class_id = candidates[0], viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        Assert.That(classChoose.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"/class_choose failed: {await classChoose.Content.ReadAsStringAsync()}");
        using var classDoc = JsonDocument.Parse(await classChoose.Content.ReadAsStringAsync());
        long firstPickId = long.Parse(
            classDoc.RootElement.GetProperty("candidate_card_list")[0]
                .GetProperty("id").GetString()!);

        // 4) 15 rounds of /card_choose, always picking the first candidate set.
        long pickId = firstPickId;
        for (int turn = 1; turn <= 15; turn++)
        {
            var cc = await client.PostAsync("/arena_two_pick/card_choose",
                JsonContent.Create(new { selected_id = pickId, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
            Assert.That(cc.StatusCode, Is.EqualTo(HttpStatusCode.OK),
                $"turn {turn} /card_choose failed: {await cc.Content.ReadAsStringAsync()}");

            if (turn == 15) break;

            // Parse next candidate list for the following turn.
            using var ccDoc = JsonDocument.Parse(await cc.Content.ReadAsStringAsync());
            pickId = long.Parse(
                ccDoc.RootElement.GetProperty("candidate_card_list")[0]
                    .GetProperty("id").GetString()!);
        }

        // 5) /retire at 0 wins → 1 ticket (80001) + 100 rupies from the seed table.
        //    Entry ticket (id 1): 5 - 1 debit = 4 remaining (not in reward_list).
        //    Reward ticket (id 80001): starts at 0, granted 1 → post-state = 1.
        var retire = await client.PostAsync("/arena_two_pick/retire",
            JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        Assert.That(retire.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            $"/retire failed: {await retire.Content.ReadAsStringAsync()}");
        using var retDoc = JsonDocument.Parse(await retire.Content.ReadAsStringAsync());

        var rewards = retDoc.RootElement.GetProperty("rewards").EnumerateArray().ToList();
        Assert.That(rewards.Count, Is.EqualTo(2), "0-win rewards = 1 ticket + 100 rupy");

        var rewardList = retDoc.RootElement.GetProperty("reward_list").EnumerateArray().ToList();

        // reward_type 9 = Rupy; post-state = startRupees + 100.
        var rupyEntry = rewardList.Single(r => r.GetProperty("reward_type").GetInt32() == 9);
        var expectedRupees = (startRupees + 100).ToString();
        Assert.That(rupyEntry.GetProperty("reward_num").GetString(), Is.EqualTo(expectedRupees),
            $"post-state rupy = {startRupees} + 100");

        // reward_type 4 = Item (reward ticket 80001); post-state = 0 (start) + 1 (grant) = 1.
        var ticketEntry = rewardList.Single(r => r.GetProperty("reward_type").GetInt32() == 4);
        Assert.That(ticketEntry.GetProperty("reward_num").GetString(), Is.EqualTo("1"),
            "post-state reward ticket (80001) = 0 + 1 grant = 1");

        // 6) /top → entry_info:null again (run was deleted by /retire).
        var topAgain = await client.PostAsync("/arena_two_pick/top",
            JsonContent.Create(new { mode = 0, viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        Assert.That(topAgain.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        StringAssert.Contains("\"entry_info\":null", await topAgain.Content.ReadAsStringAsync());
    }
}
