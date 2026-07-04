using System.Linq;
using NUnit.Framework;
using SVSim.Database.Models.Config;

namespace SVSim.UnitTests.Database.Config;

[TestFixture]
public class MatchingConfigTests
{
    [Test]
    public void Default_threshold_is_15_seconds()
    {
        var cfg = new MatchingConfig();
        Assert.That(cfg.RankBattleAiFallbackThresholdSeconds, Is.EqualTo(15));
    }

    [Test]
    public void ShippedDefaults_returns_a_new_instance_with_default_threshold()
    {
        var cfg = MatchingConfig.ShippedDefaults();
        Assert.That(cfg.RankBattleAiFallbackThresholdSeconds, Is.EqualTo(15));
    }

    [Test]
    public void Has_ConfigSection_attribute_with_name_Matching()
    {
        var attr = typeof(MatchingConfig)
            .GetCustomAttributes(typeof(ConfigSectionAttribute), false)
            .Cast<ConfigSectionAttribute>()
            .FirstOrDefault();
        Assert.That(attr, Is.Not.Null);
        Assert.That(attr!.Name, Is.EqualTo("Matching"));
    }
}
