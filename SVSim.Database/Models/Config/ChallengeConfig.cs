namespace SVSim.Database.Models.Config;

[ConfigSection("Challenge")]
public class ChallengeConfig
{
    // Wire-mirrored fields from format_info (ChallengeData.cs parser)
    public int LastCardPackSetId { get; set; }
    public string CardPoolName { get; set; } = "";
    public string CardPoolUrl  { get; set; } = "";
    public string AnnounceId   { get; set; } = "";
    public string StartTime    { get; set; } = "";
    public string EndTime      { get; set; } = "";
    public int TwoPickType     { get; set; } = 0;
    public int StrategyPickNum { get; set; } = 0;

    // Server-internal: which sets the TK2 pool draws from. Empty falls back to
    // RotationConfig.RotationCardSetIds at runtime in the card-pool service.
    public List<int> PoolCardSetIds { get; set; } = new();

    public static ChallengeConfig ShippedDefaults() => new();
}
