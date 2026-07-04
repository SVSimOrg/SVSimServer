namespace SVSim.Database.Models.Config;

/// <summary>
/// Take Two run mechanics: rarity weights, class/neutral mix, per-battle XP, season ids,
/// allowed-class allow-list. The pool's set scoping lives on <see cref="ChallengeConfig"/>;
/// this section is purely mechanics + the reward-schedule/challenge ids stamped on each run.
/// </summary>
[ConfigSection("ArenaTwoPick")]
public class ArenaTwoPickConfig
{
    public int RewardScheduleId { get; set; } = 1;
    public int ChallengeId { get; set; } = 1;
    public int SpotPointsPerBattle { get; set; } = 10;

    public double LegendaryRate { get; set; } = 0.06;
    public double GoldRate { get; set; } = 0.17;
    public double SilverRate { get; set; } = 0.33;
    public double BronzeRate { get; set; } = 0.44;

    public double NeutralMixRate { get; set; } = 0.25;

    /// <summary>TK2 entry ticket — item id 1 (challenge ticket). Distinct from the run-end
    /// REWARD ticket id (80001, throwback pack ticket).</summary>
    public int TicketItemId { get; set; } = 1;

    public int TicketCost { get; set; } = 1;
    public int CrystalCost { get; set; } = 150;
    public int RupyCost { get; set; } = 150;

    public List<int> AllowedClassIds { get; set; } = new();

    public static ArenaTwoPickConfig ShippedDefaults() => new()
    {
        AllowedClassIds = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 },
    };
}
