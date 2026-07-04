using SVSim.Database.Enums;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// The subset of <see cref="UserGoodsType"/> that <c>IInventoryTransaction.GrantAsync</c>
/// can grant via the gift-inbox claim flow. Producers (serial codes, future event mailers)
/// validate against this before creating <c>ViewerPresent</c> rows so unsupported types
/// fail at production time rather than at the player's claim.
/// </summary>
/// <remarks>
/// Excluded from gift grants:
/// <list type="bullet">
/// <item><c>SpotCard</c>, <c>SpotCardOnlyLatestCardPack</c> — InventoryTransaction throws NotSupportedException.</item>
/// <item><c>FreeGachaCount</c> — InventoryTransaction has no grant case (default arm throws NotImplementedException).</item>
/// </list>
/// </remarks>
public static class GiftRewardTypes
{
    public static bool IsSupported(UserGoodsType type) => type switch
    {
        UserGoodsType.Crystal       => true,
        UserGoodsType.Rupy          => true,
        UserGoodsType.RedEther      => true,
        UserGoodsType.SpotCardPoint => true,
        UserGoodsType.Item          => true,
        UserGoodsType.Card          => true,
        UserGoodsType.Sleeve        => true,
        UserGoodsType.Emblem        => true,
        UserGoodsType.Degree        => true,
        UserGoodsType.Skin          => true,
        UserGoodsType.MyPageBG      => true,
        UserGoodsType.SpotCard                   => false,
        UserGoodsType.SpotCardOnlyLatestCardPack => false,
        UserGoodsType.FreeGachaCount             => false,
        _ => false,
    };

    public static bool IsSupported(int wireType) =>
        Enum.IsDefined(typeof(UserGoodsType), wireType) &&
        IsSupported((UserGoodsType)wireType);
}
