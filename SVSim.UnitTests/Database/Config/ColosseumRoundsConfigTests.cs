using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.Database;
using SVSim.Database.Models.Config;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Database.Config;

[TestFixture]
public class ColosseumRoundsConfigTests
{
    [Test]
    public void ShippedDefaults_emits_empty_rounds()
    {
        var cfg = ColosseumRoundsConfig.ShippedDefaults();
        Assert.That(cfg.Rounds, Is.Empty,
            "default ship state has no rounds — /event_info renders a benign empty payload");
    }

    [Test]
    public void Has_ConfigSection_attribute_with_name_ColosseumRounds()
    {
        var attr = typeof(ColosseumRoundsConfig)
            .GetCustomAttributes(typeof(ConfigSectionAttribute), false)
            .Cast<ConfigSectionAttribute>()
            .FirstOrDefault();
        Assert.That(attr, Is.Not.Null);
        Assert.That(attr!.Name, Is.EqualTo("ColosseumRounds"));
    }

    [Test]
    public void Get_through_GameConfigService_round_trips_shipped_defaults()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var svc = new GameConfigService(db, new ConfigurationBuilder().Build());

        var cfg = svc.Get<ColosseumRoundsConfig>();

        Assert.That(cfg.Rounds, Is.Empty);
    }
}
