using NUnit.Framework;
using SVSim.Database.Enums;
using SVSim.Database.Models.Config;

namespace SVSim.UnitTests.Config;

public class LoginBonusConfigTests
{
    [Test]
    public void ShippedDefaults_matches_prod_normal_cycle()
    {
        var cfg = LoginBonusConfig.ShippedDefaults();

        Assert.That(cfg.CampaignId, Is.EqualTo(3));
        Assert.That(cfg.Name, Is.EqualTo("Daily Bonus"));
        Assert.That(cfg.Img, Is.EqualTo("0"));
        Assert.That(cfg.Normal, Has.Count.EqualTo(15));

        // Day 1: 20 Rupy
        Assert.That(cfg.Normal[0].Day, Is.EqualTo(1));
        Assert.That(cfg.Normal[0].RewardType, Is.EqualTo(UserGoodsType.Rupy));
        Assert.That(cfg.Normal[0].RewardDetailId, Is.EqualTo(0));
        Assert.That(cfg.Normal[0].RewardNumber, Is.EqualTo(20));
        Assert.That(cfg.Normal[0].EffectId, Is.EqualTo(1));

        // Day 5: Item 80001 ×1 with effect 2 (the highlight ticket)
        Assert.That(cfg.Normal[4].RewardType, Is.EqualTo(UserGoodsType.Item));
        Assert.That(cfg.Normal[4].RewardDetailId, Is.EqualTo(80001));
        Assert.That(cfg.Normal[4].RewardNumber, Is.EqualTo(1));
        Assert.That(cfg.Normal[4].EffectId, Is.EqualTo(2));

        // Day 15: Item id 1 ×1 with effect 2 (cycle capstone)
        Assert.That(cfg.Normal[14].RewardType, Is.EqualTo(UserGoodsType.Item));
        Assert.That(cfg.Normal[14].RewardDetailId, Is.EqualTo(1));
        Assert.That(cfg.Normal[14].RewardNumber, Is.EqualTo(1));
    }
}
