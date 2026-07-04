using System.ComponentModel.DataAnnotations.Schema;
using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Singleton row (Id=1) for upcoming card-set pre-release window from /load/index data.pre_release_info.
/// Current capture has stale 1900/2019/2020 dates — likely "no active pre-release" sentinel.
/// Recapture target during an active pre-release window (typically a week before each new expansion).
/// </summary>
public class PreReleaseInfo : BaseEntity<int>
{
    public string PreReleaseId { get; set; } = string.Empty;

    public string NextCardSetId { get; set; } = string.Empty;

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public DateTime DisplayEndTime { get; set; }

    public DateTime FreeMatchStartTime { get; set; }

    public int CardMasterId { get; set; }

    public string DefaultCardMasterId { get; set; } = string.Empty;

    public string PreReleaseCardMasterId { get; set; } = string.Empty;

    public bool IsPreRotationFreeMatchTerm { get; set; }

    [Column(TypeName = "jsonb")]
    public string RotationCardSetIdList { get; set; } = "[]";

    [Column(TypeName = "jsonb")]
    public string ReprintedBaseCardIds { get; set; } = "{}";

    [Column(TypeName = "jsonb")]
    public string LatestReprintedBaseCardIds { get; set; } = "{}";
}
