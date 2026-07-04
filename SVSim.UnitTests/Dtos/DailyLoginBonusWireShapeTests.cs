using System.Text.Json;
using NUnit.Framework;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.UnitTests.Dtos;

public class DailyLoginBonusWireShapeTests
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    [Test]
    public void Reward_numeric_fields_serialize_as_strings()
    {
        var r = new LoginBonusReward
        {
            EffectId = "1", RewardType = "9", RewardDetailId = "0", RewardNumber = "20"
        };

        var json = JsonSerializer.Serialize(r, Opts);
        using var doc = JsonDocument.Parse(json);
        Assert.That(doc.RootElement.GetProperty("effect_id").ValueKind,        Is.EqualTo(JsonValueKind.String));
        Assert.That(doc.RootElement.GetProperty("reward_type").ValueKind,      Is.EqualTo(JsonValueKind.String));
        Assert.That(doc.RootElement.GetProperty("reward_detail_id").ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(doc.RootElement.GetProperty("reward_number").ValueKind,    Is.EqualTo(JsonValueKind.String));
    }

    [Test]
    public void Campaign_id_and_img_serialize_as_strings()
    {
        var c = new LoginBonusCampaign { CampaignId = "3", Img = "0", Name = "Daily Bonus" };
        var json = JsonSerializer.Serialize(c, Opts);
        using var doc = JsonDocument.Parse(json);
        Assert.That(doc.RootElement.GetProperty("campaign_id").ValueKind, Is.EqualTo(JsonValueKind.String));
        Assert.That(doc.RootElement.GetProperty("img").ValueKind,         Is.EqualTo(JsonValueKind.String));
    }

    [Test]
    public void Campaign_supports_is_one_day_multi_rewards_flag()
    {
        var c = new LoginBonusCampaign { IsOneDayMultiRewards = false };
        var json = JsonSerializer.Serialize(c, Opts);
        using var doc = JsonDocument.Parse(json);
        Assert.That(doc.RootElement.TryGetProperty("is_one_day_multi_rewards", out var v), Is.True);
        Assert.That(v.ValueKind, Is.EqualTo(JsonValueKind.False));
    }

    [Test]
    public void DailyLoginBonus_campaign_is_an_array()
    {
        var d = new DailyLoginBonus { Normal = new LoginBonusCampaign(), Campaign = new List<LoginBonusCampaign>() };
        var json = JsonSerializer.Serialize(d, Opts);
        using var doc = JsonDocument.Parse(json);
        Assert.That(doc.RootElement.GetProperty("campaign").ValueKind, Is.EqualTo(JsonValueKind.Array));
    }
}
