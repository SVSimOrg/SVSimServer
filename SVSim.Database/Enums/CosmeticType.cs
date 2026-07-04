namespace SVSim.Database.Enums;

/// <summary>
/// Subset of UserGoods.Type values that can be granted as a card-acquisition cosmetic.
/// Numeric values MUST match Wizard/UserGoods.cs:8-22 so wire serialization
/// (reward_type in /pack/open response) is direct passthrough.
/// </summary>
public enum CosmeticType
{
    Sleeve   = 6,
    Emblem   = 7,
    Degree   = 8,
    Skin     = 10,
    MyPageBG = 15,
}
