using SVSim.Database.Enums;

namespace SVSim.Database.Models.Config;

/// <summary>
/// The 3-round bracket schedule for an active Colosseum season — and the per-round
/// reward bundles paid out by <c>/finish</c> + <c>/retire</c>. Empty <see cref="Rounds"/>
/// is the default shipped state — lobby <c>/event_info</c> renders a benign payload with
/// no rounds active. Collection default is in <see cref="ShippedDefaults"/> per
/// feedback_config_defaults (property-initializer collection defaults silently empty out
/// under the tier merge).
/// </summary>
[ConfigSection("ColosseumRounds")]
public class ColosseumRoundsConfig
{
    public List<RoundEntry> Rounds { get; set; } = new();

    /// <summary>Champion-only reward bundle, paid alongside the final round's FinishRewards
    /// when <c>ColosseumProgressionService</c> determines the viewer cleared the bracket.</summary>
    public List<RewardEntry> ChampionRewards { get; set; } = new();

    public static ColosseumRoundsConfig ShippedDefaults() => new()
    {
        Rounds = new(),
        ChampionRewards = new(),
    };

    public class RoundEntry
    {
        /// <summary>1, 2, or 3 in the canonical 3-round schedule.</summary>
        public int RoundId { get; set; }

        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        /// <summary>Bracket groups within the round (e.g. distinct breakthrough thresholds
        /// for late-joiners). The first entry is the canonical lookup used by
        /// <c>ColosseumProgressionService</c> for the v1 single-bracket case.</summary>
        public List<GroupEntry> Groups { get; set; } = new();

        /// <summary>Paid by <c>/finish</c> when the viewer clears or otherwise ends this round
        /// successfully. Empty = no bonus for this round.</summary>
        public List<RewardEntry> FinishRewards { get; set; } = new();

        /// <summary>Paid by <c>/retire</c> when the viewer abandons mid-round. Client ignores
        /// these once <c>run.RoundId &gt;= FinalB</c>, but server still emits them per
        /// retire.md (log completeness).</summary>
        public List<RewardEntry> RetireRewards { get; set; } = new();
    }

    public class GroupEntry
    {
        public string Group { get; set; } = "";

        /// <summary>Max battles a viewer can play in this round before bracket termination.</summary>
        public int MaxBattleCount { get; set; }

        /// <summary>Wins required to promote out of this round.</summary>
        public int BreakthroughNumber { get; set; }

        /// <summary>Total bracket entries allotted to this group.</summary>
        public int EntryNumber { get; set; }
    }

    public class RewardEntry
    {
        public UserGoodsType Type { get; set; }
        public long DetailId { get; set; }
        public int Count { get; set; }

        /// <summary>Display name for the <c>rewards[]</c> receipt block. Defaults to empty;
        /// client uses system-text lookups when blank.</summary>
        public string Name { get; set; } = "";
    }
}
