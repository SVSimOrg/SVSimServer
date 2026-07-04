using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Viewer;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

[TestFixture]
public class ViewerRepositoryMatchContextTests
{
    [Test]
    public async Task LoadForMatchContext_includes_info_selected_emblem_and_degree()
    {
        await using var factory = new SVSimTestFactory();
        var vid = await factory.SeedViewerAsync();

        int emblemId, degreeId;
        using (var seedScope = factory.Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            // Use existing rows from the reference-data seed — inserting EmblemEntry/DegreeEntry
            // with a hardcoded Id collides with the seeded catalog (UNIQUE constraint).
            var emblem = await db.Emblems.FirstAsync();
            var degree = await db.Degrees.FirstAsync();
            emblemId = emblem.Id;
            degreeId = degree.Id;

            var viewer = await db.Viewers.FindAsync(vid);
            viewer!.Info.CountryCode = "KOR";
            viewer.Info.IsOfficial = true;
            viewer.Info.SelectedEmblem = emblem;
            viewer.Info.SelectedDegree = degree;
            viewer.DisplayName = "DraftedPlayer";
            await db.SaveChangesAsync();
        }

        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();
        var loaded = await repo.LoadForMatchContextAsync(vid);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.DisplayName, Is.EqualTo("DraftedPlayer"));
        Assert.That(loaded.Info.CountryCode, Is.EqualTo("KOR"));
        Assert.That(loaded.Info.IsOfficial, Is.True);
        Assert.That(loaded.Info.SelectedEmblem.Id, Is.EqualTo(emblemId));
        Assert.That(loaded.Info.SelectedDegree.Id, Is.EqualTo(degreeId));
    }
}
