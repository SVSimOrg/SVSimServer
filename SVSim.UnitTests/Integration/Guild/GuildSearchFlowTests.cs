using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Entities.Guild;
using SVSim.UnitTests.Infrastructure;
using GuildEntity = SVSim.Database.Entities.Guild.Guild;

namespace SVSim.UnitTests.Integration.Guild;

public class GuildSearchFlowTests
{
    private const string Vid = "0";
    private const int Sid = 0;
    private const string Stk = "";

    /// <summary>
    /// Seeds three guilds with different activity / join_condition / member counts directly
    /// via DbContext (bypassing the service to keep the test focused on /search_guild).
    /// </summary>
    private static async Task<(int SmallFree, int MedApproval, int LargeInvite)> SeedThreeGuildsAsync(
        SVSimTestFactory factory, long leaderId)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var now = DateTime.UtcNow;

        // Small guild (bucket=1: 1..10 members), activity=1, join_condition=1 (Free)
        var gSmall = new GuildEntity
        {
            GuildId = 100_000_901,
            Name = "SmallFreeGuild",
            Description = "",
            LeaderViewerId = leaderId,
            EmblemId = 100_000_000L,
            Activity = GuildActivity.All,
            JoinCondition = GuildJoinCondition.Free,
            CreatedAt = now,
        };
        db.Guilds.Add(gSmall);
        // 5 members (leader + 4 extras)
        db.GuildMembers.Add(new GuildMember { GuildId = 100_000_901, ViewerId = leaderId, Role = GuildRole.Leader, JoinedAt = now });
        for (int i = 1; i <= 4; i++)
            db.GuildMembers.Add(new GuildMember { GuildId = 100_000_901, ViewerId = 900_000_000 + i, Role = GuildRole.Regular, JoinedAt = now });

        // Medium guild (bucket=2: 11..25 members), activity=2, join_condition=2 (Approval)
        var gMed = new GuildEntity
        {
            GuildId = 100_000_902,
            Name = "MedApprovalGuild",
            Description = "",
            LeaderViewerId = leaderId,
            EmblemId = 100_000_000L,
            Activity = (GuildActivity)2,
            JoinCondition = GuildJoinCondition.Approval,
            CreatedAt = now,
        };
        db.Guilds.Add(gMed);
        // 12 members
        for (int i = 1; i <= 12; i++)
            db.GuildMembers.Add(new GuildMember { GuildId = 100_000_902, ViewerId = 900_001_000 + i, Role = GuildRole.Regular, JoinedAt = now });

        // Large guild (bucket=3: 26..MaxMemberNum members), activity=3, join_condition=3 (Invite)
        var gLarge = new GuildEntity
        {
            GuildId = 100_000_903,
            Name = "LargeInviteGuild",
            Description = "",
            LeaderViewerId = leaderId,
            EmblemId = 100_000_000L,
            Activity = (GuildActivity)3,
            JoinCondition = GuildJoinCondition.OnlyInvite,
            CreatedAt = now,
        };
        db.Guilds.Add(gLarge);
        // 27 members
        for (int i = 1; i <= 27; i++)
            db.GuildMembers.Add(new GuildMember { GuildId = 100_000_903, ViewerId = 900_002_000 + i, Role = GuildRole.Regular, JoinedAt = now });

        await db.SaveChangesAsync();

        return (100_000_901, 100_000_902, 100_000_903);
    }

    private static async Task<JsonElement> PostSearchAsync(HttpClient client, string guildName, int activity, int joinCondition, int memberConditionRange)
    {
        var resp = await client.PostAsync("/guild/search_guild",
            JsonContent.Create(new
            {
                guild_name = guildName,
                activity,
                join_condition = joinCondition,
                member_condition_range = memberConditionRange,
                viewer_id = Vid,
                steam_id = Sid,
                steam_session_ticket = Stk,
            }));
        Assert.That(resp.IsSuccessStatusCode, Is.True, $"search_guild HTTP {resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}");
        var json = await resp.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private static List<string> GetListNames(JsonElement root)
    {
        var names = new List<string>();
        var arr = root.GetProperty("list");
        foreach (var entry in arr.EnumerateArray())
            names.Add(entry.GetProperty("guild_name").GetString()!);
        return names;
    }

    [Test]
    public async Task SearchGuild_no_filter_returns_all_guilds()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_300_000_001UL, "SearchLeader1");
        await SeedThreeGuildsAsync(factory, leaderId);

        using var client = factory.CreateAuthenticatedClient(leaderId);
        var root = await PostSearchAsync(client, "", 0, 0, 0);

        var names = GetListNames(root);
        Assert.That(names, Does.Contain("SmallFreeGuild"), "All-filter should include SmallFreeGuild");
        Assert.That(names, Does.Contain("MedApprovalGuild"), "All-filter should include MedApprovalGuild");
        Assert.That(names, Does.Contain("LargeInviteGuild"), "All-filter should include LargeInviteGuild");
    }

    [Test]
    public async Task SearchGuild_activity_filter_returns_matching_guilds_only()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_300_000_002UL, "SearchLeader2");
        await SeedThreeGuildsAsync(factory, leaderId);

        using var client = factory.CreateAuthenticatedClient(leaderId);
        // activity=2 should return only MedApprovalGuild
        var root = await PostSearchAsync(client, "", 2, 0, 0);
        var names = GetListNames(root);

        Assert.That(names, Does.Contain("MedApprovalGuild"), "activity=2 should match MedApprovalGuild");
        Assert.That(names, Does.Not.Contain("SmallFreeGuild"), "activity=2 should not match SmallFreeGuild (activity=1)");
        Assert.That(names, Does.Not.Contain("LargeInviteGuild"), "activity=2 should not match LargeInviteGuild (activity=3)");
    }

    [Test]
    public async Task SearchGuild_join_condition_filter_returns_matching_guilds_only()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_300_000_003UL, "SearchLeader3");
        await SeedThreeGuildsAsync(factory, leaderId);

        using var client = factory.CreateAuthenticatedClient(leaderId);
        // join_condition=1 (Free) should return only SmallFreeGuild
        var root = await PostSearchAsync(client, "", 0, 1, 0);
        var names = GetListNames(root);

        Assert.That(names, Does.Contain("SmallFreeGuild"), "join_condition=1 should match SmallFreeGuild");
        Assert.That(names, Does.Not.Contain("MedApprovalGuild"), "join_condition=1 should not match MedApprovalGuild (approval)");
        Assert.That(names, Does.Not.Contain("LargeInviteGuild"), "join_condition=1 should not match LargeInviteGuild (invite)");
    }

    [Test]
    public async Task SearchGuild_bucket1_returns_small_guilds()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_300_000_004UL, "SearchLeader4");
        await SeedThreeGuildsAsync(factory, leaderId);

        using var client = factory.CreateAuthenticatedClient(leaderId);
        // bucket=1: 1..10 members
        var root = await PostSearchAsync(client, "", 0, 0, 1);
        var names = GetListNames(root);

        Assert.That(names, Does.Contain("SmallFreeGuild"), "bucket=1 should include SmallFreeGuild (5 members)");
        Assert.That(names, Does.Not.Contain("MedApprovalGuild"), "bucket=1 should not include MedApprovalGuild (12 members)");
        Assert.That(names, Does.Not.Contain("LargeInviteGuild"), "bucket=1 should not include LargeInviteGuild (27 members)");
    }

    [Test]
    public async Task SearchGuild_bucket3_returns_large_guilds()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_300_000_005UL, "SearchLeader5");
        await SeedThreeGuildsAsync(factory, leaderId);

        using var client = factory.CreateAuthenticatedClient(leaderId);
        // bucket=3: 26..MaxMemberNum
        var root = await PostSearchAsync(client, "", 0, 0, 3);
        var names = GetListNames(root);

        Assert.That(names, Does.Contain("LargeInviteGuild"), "bucket=3 should include LargeInviteGuild (27 members)");
        Assert.That(names, Does.Not.Contain("SmallFreeGuild"), "bucket=3 should not include SmallFreeGuild (5 members)");
        Assert.That(names, Does.Not.Contain("MedApprovalGuild"), "bucket=3 should not include MedApprovalGuild (12 members)");
    }

    [Test]
    public async Task SearchGuild_name_prefix_returns_matching_guilds_only()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_300_000_006UL, "SearchLeader6");
        await SeedThreeGuildsAsync(factory, leaderId);

        using var client = factory.CreateAuthenticatedClient(leaderId);
        // prefix "Small" should match only SmallFreeGuild
        var root = await PostSearchAsync(client, "Small", 0, 0, 0);
        var names = GetListNames(root);

        Assert.That(names, Does.Contain("SmallFreeGuild"), "name 'Small' should match SmallFreeGuild");
        Assert.That(names, Does.Not.Contain("MedApprovalGuild"), "name 'Small' should not match MedApprovalGuild");
        Assert.That(names, Does.Not.Contain("LargeInviteGuild"), "name 'Small' should not match LargeInviteGuild");
    }

    [Test]
    public async Task SearchGuild_response_list_entries_are_flat_not_wrapped()
    {
        using var factory = new SVSimTestFactory();
        var leaderId = await factory.SeedViewerAsync(76_561_198_300_000_007UL, "SearchLeader7");
        await SeedThreeGuildsAsync(factory, leaderId);

        using var client = factory.CreateAuthenticatedClient(leaderId);
        var root = await PostSearchAsync(client, "SmallFreeGuild", 0, 0, 0);

        var arr = root.GetProperty("list");
        Assert.That(arr.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(arr.GetArrayLength(), Is.GreaterThanOrEqualTo(1));

        var entry = arr[0];
        // guild_id is directly on the entry, not under a "detail" wrapper
        Assert.That(entry.TryGetProperty("guild_id", out _), Is.True, "guild_id should be flat on each list entry");
        Assert.That(entry.TryGetProperty("detail", out _), Is.False, "no 'detail' wrapper on search list entries");
        Assert.That(entry.TryGetProperty("guild_name", out var gn), Is.True);
        Assert.That(gn.GetString(), Is.EqualTo("SmallFreeGuild"));
        // leader_name must be populated from the seeded viewer's DisplayName, not empty string
        Assert.That(entry.TryGetProperty("leader_name", out var ln), Is.True, "leader_name field must be present");
        Assert.That(ln.GetString(), Is.EqualTo("SearchLeader7"), "leader_name must match the seeded leader's DisplayName");
    }
}
