using MessagePack;
using SVSim.Database.Models;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class RankInfo
{
    [JsonPropertyName("rank_id")]
    [Key("rank_id")]
    public int RankId { get; set; }
    [JsonPropertyName("rank_name")]
    [Key("rank_name")]
    public string RankName { get; set; } = string.Empty;
    [JsonPropertyName("necessary_point")]
    [Key("necessary_point")]
    public int NecessaryPoints { get; set; }
    [JsonPropertyName("accumulate_point")]
    [Key("accumulate_point")]
    public int AccumulatePoints { get; set; }
    [JsonPropertyName("lower_limit_point")]
    [Key("lower_limit_point")]
    public int LowerLimitPoints { get; set; }
    [JsonPropertyName("base_add_bp")]
    [Key("base_add_bp")]
    public int BaseAddBp { get; set; }
    [JsonPropertyName("base_drop_bp")]
    [Key("base_drop_bp")]
    public int BaseDropBp { get; set; }
    [JsonPropertyName("streak_bonus_pt")]
    [Key("streak_bonus_pt")]
    public int StreakBonusPoints { get; set; }
    [JsonPropertyName("win_bonus")]
    [Key("win_bonus")]
    public double WinBonus { get; set; }
    [JsonPropertyName("lose_bonus")]
    [Key("lose_bonus")]
    public double LoseBonus { get; set; }
    [JsonPropertyName("max_win_bonus")]
    [Key("max_win_bonus")]
    public int MaxWinBonus { get; set; }
    [JsonPropertyName("max_lose_bonus")]
    [Key("max_lose_bonus")]
    public int MaxLoseBonus { get; set; }
    [JsonPropertyName("is_promotion_war")]
    [Key("is_promotion_war")]
    public int IsPromotionWar { get; set; }
    [JsonPropertyName("match_count")]
    [Key("match_count")]
    public int MatchCount { get; set; }
    [JsonPropertyName("necessary_win")]
    [Key("necessary_win")]
    public int NecessaryWins { get; set; }
    [JsonPropertyName("reset_lose")]
    [Key("reset_lose")]
    public int ResetLose { get; set; }
    [JsonPropertyName("accumulate_master_point")]
    [Key("accumulate_master_point")]
    public int AccumulateMasterPoints { get; set; }
    
    public RankInfo(RankInfoEntry rankEntry)
    {
        RankId = rankEntry.Id;
        RankName = rankEntry.Name;
        NecessaryPoints = rankEntry.NecessaryPoint;
        AccumulatePoints = rankEntry.AccumulatePoint;
        LowerLimitPoints = rankEntry.LowerLimitPoint;
        BaseAddBp = rankEntry.BaseAddBp;
        BaseDropBp = rankEntry.BaseDropBp;
        StreakBonusPoints = rankEntry.StreakBonusPt;
        WinBonus = rankEntry.WinBonus;
        LoseBonus = rankEntry.LoseBonus;
        MaxWinBonus = rankEntry.MaxWinBonus;
        MaxLoseBonus = rankEntry.MaxLoseBonus;
        IsPromotionWar = rankEntry.IsPromotionWar;
        MatchCount = rankEntry.MatchCount;
        NecessaryWins = rankEntry.NecessaryWin;
        ResetLose = rankEntry.ResetLose;
        AccumulateMasterPoints = rankEntry.AccumulateMasterPoint;
    }

    public RankInfo()
    {
    }
}