using MessagePack;
using System.Text.Json.Serialization;
using SVSim.Database.Enums;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

[MessagePackObject]
public class UserRankInfo
{
    // Serialized as wire deck_format via FormatJsonConverter (registered globally in
    // Program.cs). Storing as Format makes wrong-int-scope bugs (sending internal enum
    // ints instead of wire codes) a compile error.
    [JsonPropertyName("deck_format")]
    [Key("deck_format")]
    public Format DeckFormat { get; set; }
    [JsonPropertyName("rank")]
    [Key("rank")]
    public int Rank { get; set; }
    [JsonPropertyName("battle_point")]
    [Key("battle_point")]
    public int BattlePoints { get; set; }
    [JsonPropertyName("successive_win_number")]
    [Key("successive_win_number")]
    public int WinStreak { get; set; }
    [JsonPropertyName("successive_losses_number")]
    [Key("successive_losses_number")]
    public int LossStreak { get; set; }
    [JsonPropertyName("is_promotion")]
    [Key("is_promotion")]
    public int IsPromotion { get; set; }
    [JsonPropertyName("is_master_rank")]
    [Key("is_master_rank")]
    public int IsMasterRank { get; set; }
    [JsonPropertyName("is_grand_master_rank")]
    [Key("is_grand_master_rank")]
    public int IsGrandMasterRank { get; set; }
    [JsonPropertyName("master_point")]
    [Key("master_point")]
    public int MasterPoints { get; set; }
    [JsonPropertyName("period_grand_master_point")]
    [Key("period_grand_master_point")]
    public int PeriodGrandMasterPoints { get; set; }
    [JsonPropertyName("target_grand_master_point")]
    [Key("target_grand_master_point")]
    public int TargetGrandMasterPoints { get; set; }
    [JsonPropertyName("current_grand_master_point")]
    [Key("current_grand_master_point")]
    public int CurrentGrandMasterPoints { get; set; }
    [JsonPropertyName("user_promotion_match")]
    [Key("user_promotion_match")]
    public UserPromotionMatch? UserPromotionMatch { get; set; }
}