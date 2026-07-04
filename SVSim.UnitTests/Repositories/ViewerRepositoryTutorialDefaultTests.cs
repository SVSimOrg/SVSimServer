using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.Database;
using SVSim.Database.Repositories.Viewer;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

public class ViewerRepositoryTutorialDefaultTests
{
    [Test]
    public async Task RegisterAnonymousViewer_starts_at_tutorial_step_1()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();

        var viewer = await repo.RegisterAnonymousViewer(Guid.NewGuid());

        Assert.That(viewer.MissionData.TutorialState, Is.EqualTo(1),
            "Fresh signups start at TUTORIAL_STEP0=1 (matches the prod capture in " +
            "traffic_prod_tutorial.ndjson where game_start returned now_tutorial_step=\"1\"). " +
            "Step 0 (PRE_TUTORIAL_STEP) is a pre-existence state — NextSceneSwitcher would " +
            "route it to AreaSelect at section 0, which has no chapter data and crashes the " +
            "client. Tests that want a pre-completed tutorial should use SeedViewerAsync " +
            "(which defaults to 100).");
    }

    [Test]
    public async Task RegisterAnonymousViewer_starts_with_empty_display_name()
    {
        // The client's Wizard.Title/UserNameInput.Start does:
        //   IsFinished = !string.IsNullOrEmpty(PlayerStaticData.UserName);
        // Any non-empty seeded value (including the prior " - " placeholder) makes the
        // name-input dialog skip itself, and the /tutorial/update_action #1 +
        // /account/update_name calls never fire. Empty is what triggers the dialog.
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IViewerRepository>();

        var viewer = await repo.RegisterAnonymousViewer(System.Guid.NewGuid());

        Assert.That(viewer.DisplayName, Is.Empty,
            "Anonymous signups MUST start with empty DisplayName so the client's " +
            "UserNameInput.Start IsNullOrEmpty short-circuit fails and the dialog runs.");
    }
}
