using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>
/// Per-pack gacha-point exchange config. Owned by <see cref="PackConfigEntry"/>; null when the
/// pack does not participate in gacha-point exchange. Wire shape (from /pack/info):
/// <c>{"pack_id":"10001","gacha_point":0,"increase_gacha_point":"1","exchangeable_gacha_point":400,"is_exchangeable_gacha_point":false}</c>.
/// v1 only persists the static catalog values; per-viewer accrual is deferred.
/// </summary>
[Owned]
public class PackGachaPointConfig
{
    public int ExchangeablePoint { get; set; }
    public int IncreaseGachaPoint { get; set; }
}
