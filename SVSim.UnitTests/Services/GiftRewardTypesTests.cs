using SVSim.Database.Enums;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.UnitTests.Services;

public class GiftRewardTypesTests
{
    [TestCase(UserGoodsType.Crystal)]
    [TestCase(UserGoodsType.Rupy)]
    [TestCase(UserGoodsType.RedEther)]
    [TestCase(UserGoodsType.SpotCardPoint)]
    [TestCase(UserGoodsType.Item)]
    [TestCase(UserGoodsType.Card)]
    [TestCase(UserGoodsType.Sleeve)]
    [TestCase(UserGoodsType.Emblem)]
    [TestCase(UserGoodsType.Degree)]
    [TestCase(UserGoodsType.Skin)]
    [TestCase(UserGoodsType.MyPageBG)]
    public void IsSupported_returns_true_for_inventory_service_grant_set(UserGoodsType type)
    {
        Assert.That(GiftRewardTypes.IsSupported(type), Is.True);
    }

    [TestCase(UserGoodsType.SpotCard)]
    [TestCase(UserGoodsType.SpotCardOnlyLatestCardPack)]
    [TestCase(UserGoodsType.FreeGachaCount)]
    public void IsSupported_returns_false_for_inventory_service_unsupported_types(UserGoodsType type)
    {
        Assert.That(GiftRewardTypes.IsSupported(type), Is.False);
    }

    [TestCase(1, ExpectedResult = true)]   // RedEther
    [TestCase(2, ExpectedResult = true)]   // Crystal
    [TestCase(3, ExpectedResult = false)]  // gap in UserGoodsType enum
    [TestCase(4, ExpectedResult = true)]   // Item
    [TestCase(11, ExpectedResult = false)] // SpotCard
    [TestCase(14, ExpectedResult = false)] // FreeGachaCount
    [TestCase(15, ExpectedResult = true)]  // MyPageBG
    [TestCase(99, ExpectedResult = false)] // out of range
    [TestCase(-1, ExpectedResult = false)] // negative
    public bool IsSupported_int_overload_validates_enum_definition(int wireType) =>
        GiftRewardTypes.IsSupported(wireType);
}
