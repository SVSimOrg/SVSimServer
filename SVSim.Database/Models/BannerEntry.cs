using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// One mypage banner from /mypage/index data.banner. Id is synthetic ordinal (1-N) since the wire
/// has no explicit ID. Highly time-varying content — recapture aggressively before EOS.
/// </summary>
public class BannerEntry : BaseEntity<int>
{
    public string ImageName { get; set; } = string.Empty;

    public string Click { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public int ChangeTime { get; set; }

    public int RemainingTime { get; set; }

    [Column(TypeName = "jsonb")]
    public string ImagePaths { get; set; } = "[]";
}
