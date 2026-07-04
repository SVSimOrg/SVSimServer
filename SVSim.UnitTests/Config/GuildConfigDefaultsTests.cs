using NUnit.Framework;
using SVSim.Database.Models.Config;
using System.Linq;

namespace SVSim.UnitTests.Config;

[TestFixture]
public class GuildConfigDefaultsTests
{
    [Test]
    public void ShippedDefaults_carry_capture_matching_stamps_and_caps()
    {
        var c = GuildConfig.ShippedDefaults();

        Assert.That(c.MaxMemberNum, Is.EqualTo(30));
        Assert.That(c.MaxSubLeaderNum, Is.EqualTo(2));
        Assert.That(c.SearchResultCap, Is.EqualTo(50));
        Assert.That(c.UsableStampList, Is.EqualTo(Enumerable.Range(100001, 20).ToList()));
    }

    [Test]
    public void Property_initializers_alone_leave_stamp_list_empty()
    {
        // Defensive check: if someone moves UsableStampList default off ShippedDefaults
        // and into a property initializer, the config-section tier merge will silently
        // empty it. See feedback_config_defaults.md.
        var c = new GuildConfig();
        Assert.That(c.UsableStampList, Is.Empty,
            "UsableStampList must default empty on property init; populated only by ShippedDefaults().");
    }
}
