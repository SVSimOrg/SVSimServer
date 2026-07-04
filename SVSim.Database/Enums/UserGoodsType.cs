namespace SVSim.Database.Enums;

/// <summary>
/// Mirrors the client's <c>Wizard.UserGoods.Type</c> enum (Shadowverse_Code/Wizard/UserGoods.cs).
/// These integers travel on the wire as <c>reward_type</c> on <c>reward_list</c> entries; the
/// client uses them in <c>PlayerStaticData.UpdateHaveUserGoodsNumByJsonData</c> to route the
/// grant into the right collection / currency total.
/// </summary>
public enum UserGoodsType
{
    RedEther = 1,
    Crystal = 2,
    // 3 is unused / placeholder in the client enum.
    Item = 4,
    Card = 5,
    Sleeve = 6,
    Emblem = 7,
    Degree = 8,
    Rupy = 9,
    Skin = 10,            // LeaderSkin in our schema
    SpotCard = 11,
    SpotCardPoint = 12,
    SpotCardOnlyLatestCardPack = 13,
    FreeGachaCount = 14,
    MyPageBG = 15,
}
