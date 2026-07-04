using SVSim.Database.Common;

namespace SVSim.Database.Models;

/// <summary>
/// Top-level entity for a promotional serial code. Admin inserts these directly via SQL;
/// there is no JSON seed or admin endpoint. Case-sensitive match on <see cref="Code"/>.
/// </summary>
public class SerialCodeEntry : BaseEntity<int>
{
    /// <summary>User-typed code. Case-sensitive; unique index enforces no duplicates.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Player-facing mail body, copied onto every <c>ViewerPresent</c> created at redemption.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>When the code becomes valid. NULL = always valid from creation.</summary>
    public DateTime? StartAt { get; set; }

    /// <summary>When the code expires. NULL = never expires.</summary>
    public DateTime? EndAt { get; set; }

    /// <summary>Admin kill-switch. False = treat as if it doesn't exist.</summary>
    public bool IsEnabled { get; set; }

    public List<SerialCodeRewardEntry> Rewards { get; set; } = new List<SerialCodeRewardEntry>();
}
