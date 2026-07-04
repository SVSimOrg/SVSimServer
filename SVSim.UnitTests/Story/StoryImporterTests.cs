using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SVSim.Bootstrap.Importers;
using SVSim.Database;

namespace SVSim.UnitTests.Story;

[TestFixture]
public class StoryImporterTests
{
    private static string FixturesDir =>
        Path.Combine(AppContext.BaseDirectory, "Story", "Fixtures");

    private static SVSimDbContext NewInMemoryContext(string name)
    {
        var opts = new DbContextOptionsBuilder<SVSimDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new SVSimDbContext(NullLogger<SVSimDbContext>.Instance, opts);
    }

    [Test]
    public async Task ImportAsync_inserts_worlds_sections_chapters_sbs_from_fixtures()
    {
        await using var ctx = NewInMemoryContext(nameof(ImportAsync_inserts_worlds_sections_chapters_sbs_from_fixtures));

        var importer = new StoryImporter();
        await importer.ImportAsync(ctx, FixturesDir);

        Assert.That(await ctx.StoryWorlds.CountAsync(), Is.EqualTo(1));
        Assert.That(await ctx.StorySections.CountAsync(), Is.EqualTo(1));
        Assert.That(await ctx.StoryChapters.CountAsync(), Is.EqualTo(2));
        Assert.That(await ctx.SpecialBattleSettings.CountAsync(), Is.EqualTo(1));

        var chapter2 = await ctx.StoryChapters
            .Include(c => c.BattleSettings).Include(c => c.Rewards)
            .FirstAsync(c => c.StoryId == 101);
        Assert.That(chapter2.SpecialBattleSettingId, Is.EqualTo(8));
        Assert.That(chapter2.Rewards.Count, Is.EqualTo(1));
        Assert.That(chapter2.BattleSettings.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task ImportAsync_is_idempotent_no_changes_on_rerun()
    {
        await using var ctx = NewInMemoryContext(nameof(ImportAsync_is_idempotent_no_changes_on_rerun));

        var importer = new StoryImporter();
        await importer.ImportAsync(ctx, FixturesDir);
        var afterFirst = await ctx.StoryChapters.CountAsync();

        await importer.ImportAsync(ctx, FixturesDir);
        var afterSecond = await ctx.StoryChapters.CountAsync();

        Assert.That(afterSecond, Is.EqualTo(afterFirst));
    }
}
