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
public class ColosseumSeasonConfigTests
{
    [Test]
    public void ShippedDefaults_emits_no_period()
    {
        var cfg = ColosseumSeasonConfig.ShippedDefaults();
        Assert.That(cfg.IsColosseumPeriod, Is.False,
            "default ship state is no event scheduled — lobby reads must render the empty payload");
    }

    [Test]
    public void Has_ConfigSection_attribute_with_name_ColosseumSeason()
    {
        var attr = typeof(ColosseumSeasonConfig)
            .GetCustomAttributes(typeof(ConfigSectionAttribute), false)
            .Cast<ConfigSectionAttribute>()
            .FirstOrDefault();
        Assert.That(attr, Is.Not.Null);
        Assert.That(attr!.Name, Is.EqualTo("ColosseumSeason"));
    }

    [Test]
    public void Get_through_GameConfigService_round_trips_shipped_defaults()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var svc = new GameConfigService(db, new ConfigurationBuilder().Build());

        var cfg = svc.Get<ColosseumSeasonConfig>();

        Assert.That(cfg.IsColosseumPeriod, Is.False);
        Assert.That(cfg.PoolCardSetIds, Is.Empty);
    }
}
