namespace SVSim.Database.Services.Inventory;

/// <summary>
/// Logical source of a grant routed through <see cref="IInventoryTransaction.GrantAsync"/>.
/// Stored verbatim in <c>viewer_acquire_history.AcquireType</c> and surfaced on the
/// <c>/item_acquire_history/info</c> wire as <c>acquire_type</c>.
/// </summary>
/// <remarks>
/// Values are persisted to the database — renumbering after ship requires a migration.
/// Values 1 and 2 mirror the prod capture in
/// <c>data_dumps/captures/traffic_prod_misc_clicking.ndjson</c>; the rest are our own.
/// </remarks>
public enum GrantSource
{
    Unknown = 0,
    DailyBonus = 1,
    PackOpen = 2,
    PuzzleReward = 3,
    StoryFinish = 4,
    BattlePassClaim = 5,
    MissionReward = 6,
    ArenaTwoPickFinish = 7,
    ItemPurchase = 8,
    BuildDeckBuy = 9,
    SleeveBuy = 10,
    LeaderSkinBuy = 11,
    GachaPointExchange = 12,
    AchievementReward = 13,
    SerialCodeRedeem = 14,
    CardCosmeticCascade = 15,
    CardCraft           = 16,
    // Reserved high to stay visually distinct from gameplay sources; 17–98 are intentionally unused.
    AdminGrant = 99,
}

/// <summary>
/// Pre-localized text written into the <c>message</c> field of an item-acquire-history row.
/// The client renders this string verbatim, so all entries are user-facing English.
/// </summary>
public static class GrantSourceMessages
{
    /// <exception cref="ArgumentOutOfRangeException">An unmapped <see cref="GrantSource"/> value was passed.</exception>
    public static string For(GrantSource source) => source switch
    {
        GrantSource.Unknown             => "Unknown",
        GrantSource.DailyBonus          => "Daily Bonus",
        GrantSource.PackOpen            => "From buying card packs",
        GrantSource.PuzzleReward        => "From puzzle reward",
        GrantSource.StoryFinish         => "From story reward",
        GrantSource.BattlePassClaim     => "From battle pass reward",
        GrantSource.MissionReward       => "From mission reward",
        GrantSource.ArenaTwoPickFinish  => "From 2Pick reward",
        GrantSource.ItemPurchase        => "From shop purchase",
        GrantSource.BuildDeckBuy        => "From starter set purchase",
        GrantSource.SleeveBuy           => "From sleeve purchase",
        GrantSource.LeaderSkinBuy       => "From leader skin purchase",
        GrantSource.GachaPointExchange  => "From point exchange",
        GrantSource.AchievementReward   => "From achievement reward",
        GrantSource.SerialCodeRedeem    => "From serial code",
        GrantSource.CardCosmeticCascade => "Card cosmetic",
        GrantSource.CardCraft           => "From card crafting",
        GrantSource.AdminGrant          => "From admin grant",
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unhandled GrantSource"),
    };
}
