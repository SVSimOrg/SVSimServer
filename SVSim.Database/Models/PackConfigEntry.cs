using SVSim.Database.Common;
using SVSim.Database.Enums;

namespace SVSim.Database.Models;

/// <summary>
/// One row of /pack/info's <c>pack_config_list</c>. PK = <c>parent_gacha_id</c> (the wire id the
/// client treats as "this pack"). Child gachas and banners are owned collections — replaced
/// wholesale on importer re-runs.
/// </summary>
public class PackConfigEntry : BaseEntity<int>
{
    public int BasePackId { get; set; }
    public int GachaType { get; set; }
    public PackCategory PackCategory { get; set; }
    public int PosterType { get; set; }

    public DateTime CommenceDate { get; set; }
    public DateTime CompleteDate { get; set; }
    public DateTime? SalesPeriodTime { get; set; }

    public int SleeveId { get; set; }
    public int SpecialSleeveId { get; set; }

    public int OverrideDrawEffectPackId { get; set; }
    public int OverrideUiEffectPackId { get; set; }

    public string GachaDetail { get; set; } = string.Empty;

    public bool IsHide { get; set; }
    public bool IsNew { get; set; }
    public bool IsPreRelease { get; set; }

    public int OpenCountLimit { get; set; }

    /// <summary>
    /// Server admin gate. True for live-capture-derived rows; false for synthesized stubs
    /// (operator opt-in per pack). Filtered in PackRepository.GetActivePacks; distinct from
    /// the wire-mirror IsHide.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    public PackGachaPointConfig? GachaPointConfig { get; set; }

    public List<PackBannerEntry> Banners { get; set; } = new();
    public List<PackChildGachaEntry> ChildGachas { get; set; } = new();
}
