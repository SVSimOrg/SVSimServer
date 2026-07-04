using SVSim.Database.Enums;

namespace SVSim.Database.Models.Config;

/// <summary>
/// Event-level configuration for an active Arena Colosseum (Grand Prix) season. Default
/// <see cref="ShippedDefaults"/> emits <c>IsColosseumPeriod = false</c> so the lobby read
/// endpoints render an empty "no event scheduled" payload without crashing the client.
/// Flipping the event on is an admin operation per
/// <c>docs/operations/grand-prix-event-setup.md</c> — write a row to <c>GameConfigs</c>.
/// </summary>
[ConfigSection("ColosseumSeason")]
public class ColosseumSeasonConfig
{
    /// <summary>Master gate. <c>false</c> = lobby reads render an empty info block and
    /// entry rejects. The client (<c>Wizard/ColosseumEntryInfoTask.cs</c>) reads this and
    /// skips parsing the rest of the colosseum_info object.</summary>
    public bool IsColosseumPeriod { get; set; }

    /// <summary>Stamped onto every <see cref="ViewerArenaColosseumRun"/> at entry time.</summary>
    public int SeasonId { get; set; }

    public string ColosseumName { get; set; } = "";

    /// <summary>Bracket format. Stamped onto the run at entry time.</summary>
    public Format DeckFormat { get; set; } = Format.Rotation;

    /// <summary>Server stores bool; the wire shape is the STRING "0"/"1" per Wizard/ColosseumEntryInfoTask.cs's
    /// <c>jsonData.ToString() == "1"</c> parse. The response DTO converts at serialization time.</summary>
    public bool IsNormalTwoPick { get; set; }

    /// <summary>Wire string used by the client as a theme/color code. Empty when not in special mode.</summary>
    public string IsSpecialMode { get; set; } = "";

    public string? AnnounceId { get; set; }

    public DateTime EventStartTime { get; set; }
    public DateTime EventEndTime { get; set; }

    /// <summary>How many bracket entries get eliminated in the final round before champion-determination.</summary>
    public int FinalRoundEliminateCount { get; set; }

    public string CardPoolName { get; set; } = "";

    /// <summary>Card-set ids used as the 2-Pick / Chaos draft pool override for this season.
    /// Phase 3 reads this through <c>ArenaTwoPickCardPoolService</c> instead of
    /// <c>ChallengeConfig.PoolCardSetIds</c>.</summary>
    public List<int> PoolCardSetIds { get; set; } = new();

    public int RupyCost { get; set; }
    public int TicketCost { get; set; }
    public int CrystalCost { get; set; }

    public bool IsAllowedFreeEntry { get; set; }

    public bool IsAllCardEnabled { get; set; }

    /// <summary>Number of strategies offered per pick in 2-Pick Chaos mode.</summary>
    public int StrategyPickNum { get; set; }

    /// <summary>Drives the client's first-time tutorial tip popup for this cup
    /// (<c>Wizard/ColosseumEntryInfoTask.cs:129</c>: <c>NeedsFirstTips = (is_display_tips == 1)</c>).
    /// Server emits wire int 0/1.</summary>
    public bool IsDisplayTips { get; set; }

    /// <summary>Tutorial tip content id, paired with <see cref="IsDisplayTips"/>. Wire int.</summary>
    public int TipsId { get; set; }

    public DateTime SalesPeriodStart { get; set; }
    public DateTime SalesPeriodEnd { get; set; }

    public static ColosseumSeasonConfig ShippedDefaults() => new()
    {
        IsColosseumPeriod = false,
    };
}
