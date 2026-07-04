using Microsoft.Extensions.DependencyInjection;
using SVSim.Database.Services.Inventory;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services.Inventory;

public class InventoryServiceBeginTests
{
    [Test]
    public async Task BeginAsync_loads_viewer_with_canonical_graph()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        await using var tx = await inv.BeginAsync(viewerId);

        Assert.That(tx.Viewer, Is.Not.Null);
        Assert.That(tx.Viewer.Id, Is.EqualTo(viewerId));
        Assert.That(tx.Viewer.Cards, Is.Not.Null, "Cards collection must be loaded");
        Assert.That(tx.Viewer.Sleeves, Is.Not.Null, "Sleeves collection must be loaded");
        Assert.That(tx.Viewer.Items, Is.Not.Null, "Items collection must be loaded");
    }

    [Test]
    public async Task BeginAsync_throws_when_viewer_missing()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        Assert.ThrowsAsync<InventoryViewerNotFoundException>(
            async () => { await inv.BeginAsync(viewerId: 9999); });
    }

    [Test]
    public async Task BeginAsync_applies_extra_includes_via_configure()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var scope = factory.Services.CreateScope();
        var inv = scope.ServiceProvider.GetRequiredService<IInventoryService>();

        await using var tx = await inv.BeginAsync(viewerId, configure:
            cfg => cfg.WithInclude(v => v.MissionData));

        Assert.That(tx.Viewer.MissionData, Is.Not.Null);
    }
}
