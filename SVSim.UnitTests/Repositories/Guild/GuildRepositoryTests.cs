using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SVSim.Database;
using SVSim.Database.Entities.Guild;
using SVSim.Database.Repositories.Guild;
using GuildEntity = SVSim.Database.Entities.Guild.Guild;

namespace SVSim.UnitTests.Repositories.Guild;

[TestFixture]
public class GuildRepositoryTests
{
    private SVSimDbContext Db()
        => new(NullLogger<SVSimDbContext>.Instance,
            new DbContextOptionsBuilder<SVSimDbContext>()
            .UseInMemoryDatabase($"guild-repo-{Guid.NewGuid()}")
            .Options);

    [Test]
    public async Task Add_and_load_by_id_round_trips()
    {
        await using var db = Db();
        var repo = new GuildRepository(db);
        var g = new GuildEntity
        {
            GuildId = 100_000_007, Name = "Repo", Description = "",
            LeaderViewerId = 1, EmblemId = 100000000,
            Activity = GuildActivity.All, JoinCondition = GuildJoinCondition.Free,
            CreatedAt = DateTime.UtcNow,
        };
        await repo.AddAsync(g);

        var loaded = await repo.GetActiveByIdAsync(100_000_007);
        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Name, Is.EqualTo("Repo"));
    }

    [Test]
    public async Task SearchAsync_filters_by_activity_and_join_condition()
    {
        await using var db = Db();
        var repo = new GuildRepository(db);
        await repo.AddAsync(new GuildEntity { GuildId = 1, Name = "Alpha", Activity = GuildActivity.Royal, JoinCondition = GuildJoinCondition.Free, CreatedAt = DateTime.UtcNow });
        await repo.AddAsync(new GuildEntity { GuildId = 2, Name = "Beta", Activity = GuildActivity.Elf, JoinCondition = GuildJoinCondition.Approval, CreatedAt = DateTime.UtcNow });

        var royalFree = await repo.SearchAsync("", (int)GuildActivity.Royal, (int)GuildJoinCondition.Free, 0, 30, 50);
        Assert.That(royalFree.Select(x => x.Name), Is.EquivalentTo(new[] { "Alpha" }));

        var any = await repo.SearchAsync("", 0, 0, 0, 30, 50);
        Assert.That(any, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task SearchAsync_bucket_3_includes_through_MaxMemberNum()
    {
        await using var db = Db();
        var repo = new GuildRepository(db);
        var bigId = 1;
        await repo.AddAsync(new GuildEntity { GuildId = bigId, Name = "Big", Activity = GuildActivity.All, JoinCondition = GuildJoinCondition.Free, CreatedAt = DateTime.UtcNow });
        for (int i = 0; i < 27; i++)
            db.GuildMembers.Add(new GuildMember { GuildId = bigId, ViewerId = 1000 + i, Role = GuildRole.Regular, JoinedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var large = await repo.SearchAsync("", 0, 0, 3, 30, 50);
        Assert.That(large.Select(x => x.GuildId), Has.Member(bigId));

        var small = await repo.SearchAsync("", 0, 0, 1, 30, 50);
        Assert.That(small.Select(x => x.GuildId), Has.No.Member(bigId));
    }

    [Test]
    public async Task SearchAsync_bucket_2_matches_guilds_with_11_to_25_members()
    {
        await using var db = Db();
        var repo = new GuildRepository(db);

        // Guild with 15 members — should appear in bucket 2
        var midId = 10;
        await repo.AddAsync(new GuildEntity { GuildId = midId, Name = "Mid", Activity = GuildActivity.All, JoinCondition = GuildJoinCondition.Free, CreatedAt = DateTime.UtcNow });
        for (int i = 0; i < 15; i++)
            db.GuildMembers.Add(new GuildMember { GuildId = midId, ViewerId = 2000 + i, Role = GuildRole.Regular, JoinedAt = DateTime.UtcNow });

        // Guild with 5 members — should appear in bucket 1 only
        var smallId = 11;
        await repo.AddAsync(new GuildEntity { GuildId = smallId, Name = "Small", Activity = GuildActivity.All, JoinCondition = GuildJoinCondition.Free, CreatedAt = DateTime.UtcNow });
        for (int i = 0; i < 5; i++)
            db.GuildMembers.Add(new GuildMember { GuildId = smallId, ViewerId = 3000 + i, Role = GuildRole.Regular, JoinedAt = DateTime.UtcNow });

        await db.SaveChangesAsync();

        var bucket2 = await repo.SearchAsync("", 0, 0, 2, 30, 50);
        Assert.That(bucket2.Select(x => x.GuildId), Has.Member(midId), "15-member guild should be in bucket 2");
        Assert.That(bucket2.Select(x => x.GuildId), Has.No.Member(smallId), "5-member guild must not appear in bucket 2");
    }
}
