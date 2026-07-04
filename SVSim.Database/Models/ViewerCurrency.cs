using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

[Owned]
public class ViewerCurrency
{
    public ulong Crystals { get; set; }
    public ulong AndroidCrystals { get; set; }
    public ulong IosCrystals { get; set; }
    public ulong SteamCrystals { get; set; }
    public ulong DmmCrystals { get; set; }
    public ulong FreeCrystals { get; set; }
    public ulong LifeTotalCrystals { get; set; }
    public ulong RedEther { get; set; }
    public ulong Rupees { get; set; }

    /// <summary>
    /// Spot card points — currency earned from battles/missions, spent at /spot_card_exchange/exchange.
    /// Wire field <c>spot_point</c> in /load/index and /spot_card_exchange/top; reward_type 12
    /// (<see cref="Enums.UserGoodsType.SpotCardPoint"/>) in reward_list entries.
    /// </summary>
    public ulong SpotPoints { get; set; }
}