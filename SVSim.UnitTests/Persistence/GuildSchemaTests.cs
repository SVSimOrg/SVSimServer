using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using SVSim.Database;
using SVSim.Database.Entities.Guild;

namespace SVSim.UnitTests.Persistence;

[TestFixture]
public class GuildSchemaTests
{
    private SVSimDbContext NewCtx() =>
        new(NullLogger<SVSimDbContext>.Instance,
            new DbContextOptionsBuilder<SVSimDbContext>()
            .UseInMemoryDatabase($"guild-schema-{System.Guid.NewGuid()}")
            .Options);

    [Test]
    public async Task Can_insert_guild_and_member_round_trip()
    {
        await using var db = NewCtx();
        var g = new Guild
        {
            GuildId = 100_000_001, Name = "Alpha", Description = "",
            LeaderViewerId = 42, EmblemId = 100000000,
            Activity = GuildActivity.All, JoinCondition = GuildJoinCondition.Free,
            CreatedAt = new System.DateTime(2026, 1, 1, 0, 0, 0, System.DateTimeKind.Utc),
        };
        g.Members.Add(new GuildMember
        {
            ViewerId = 42, Role = GuildRole.Leader,
            JoinedAt = g.CreatedAt,
        });
        db.Guilds.Add(g);
        await db.SaveChangesAsync();

        var loaded = await db.Guilds.Include(x => x.Members).SingleAsync();
        Assert.That(loaded.GuildId, Is.EqualTo(100_000_001));
        Assert.That(loaded.Members, Has.Count.EqualTo(1));
        Assert.That(loaded.Members[0].Role, Is.EqualTo(GuildRole.Leader));
    }

    [Test]
    public async Task GuildMember_ViewerId_uniqueness_is_enforced()
    {
        await using var db = NewCtx();
        db.Guilds.Add(new Guild { GuildId = 1, Name = "G1", Activity = GuildActivity.All, JoinCondition = GuildJoinCondition.Free, CreatedAt = System.DateTime.UtcNow,
            Members = { new GuildMember { ViewerId = 7, Role = GuildRole.Leader, JoinedAt = System.DateTime.UtcNow } } });
        db.Guilds.Add(new Guild { GuildId = 2, Name = "G2", Activity = GuildActivity.All, JoinCondition = GuildJoinCondition.Free, CreatedAt = System.DateTime.UtcNow,
            Members = { new GuildMember { ViewerId = 7, Role = GuildRole.Leader, JoinedAt = System.DateTime.UtcNow } } });

        // InMemory provider does NOT enforce unique indexes — this test asserts the model is configured;
        // we verify enforcement under Postgres in an integration test in Task 3.
        var idx = db.Model.FindEntityType(typeof(GuildMember))!
            .GetIndexes().Single(i => i.Properties.Single().Name == nameof(GuildMember.ViewerId));
        Assert.That(idx.IsUnique, Is.True);

        await Task.Yield();
    }
}
