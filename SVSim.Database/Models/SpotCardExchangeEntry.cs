using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One catalog entry of the /spot_card_exchange/top shop — a card the viewer can buy with
/// spot points. PK = wire card_id. Distinct from <see cref="SpotCardEntry"/> (which is the
/// /load/index data.spot_cards rental-cost list — a different concept).
/// <para>
/// <see cref="TsRotationId"/> matches the card_set_id; cards cycle out of the exchange when
/// their set rotates. <see cref="IsPreRelease"/> distinguishes the pre-release-pool subset
/// gated by <c>pre_release_spot_card_exchange_limit</c>.
/// </para>
/// </summary>
public class SpotCardExchangeEntry : BaseEntity<long>
{
    public long CardId { get => Id; set => Id = value; }

    /// <summary>Wire <c>class</c> field — clan id (0=Neutral, 1=Forestcraft, ..., 8).</summary>
    public int ClassId { get; set; }

    public int ExchangePoint { get; set; }

    /// <summary>Wire <c>ts_rotation_id</c> — card_set_id this card belongs to.</summary>
    public long TsRotationId { get; set; }

    public bool IsPreRelease { get; set; }

    public bool IsEnabled { get; set; }
}
