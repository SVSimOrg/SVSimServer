using Microsoft.EntityFrameworkCore;

namespace SVSim.Database.Models;

/// <summary>One entry of <c>cardpack_banner_list</c> in /pack/info. Owned by PackConfigEntry.</summary>
[Owned]
public class PackBannerEntry
{
    public string BannerName { get; set; } = string.Empty;
    public string DialogTitle { get; set; } = string.Empty;
}
