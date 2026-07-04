using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Per-feature maintenance toggle from /load/index data.feature_maintenance_list. Empty in current
/// prod capture; recapture target if a feature ever gets disabled before EOS.
/// </summary>
public class FeatureMaintenanceEntry : BaseEntity<int>
{
    public string FeatureKey { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public string Data { get; set; } = "{}";
}
