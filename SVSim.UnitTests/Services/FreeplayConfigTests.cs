using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models.Config;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

public class FreeplayConfigTests
{
    [Test]
    public void Freeplay_defaults_to_disabled_with_canonical_amounts()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var svc = new GameConfigService(db, new ConfigurationBuilder().Build());

        var cfg = svc.Get<FreeplayConfig>();

        Assert.That(cfg.Enabled, Is.False, "freeplay must be off unless explicitly enabled");
        Assert.That(cfg.CurrencyAmount, Is.EqualTo(99999UL));
        Assert.That(cfg.CardCopies, Is.EqualTo(3));
    }
}
