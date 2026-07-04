using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

/// <summary>
/// Round-level Colosseum descriptor. Shared by <c>/top</c> and <c>/get_fee_info</c> via
/// the client's static <c>ColosseumEntryInfoTask.SetColosseumInfo</c> helper. When
/// <see cref="IsColosseumPeriod"/> is <c>false</c>, the client skips parsing every other
/// field — server still emits this minimal payload so the lobby renders cleanly.
/// <para>
/// Also drives <c>/mypage/index data.colosseum_info</c> — the home-screen tab gating and
/// the per-endpoint lobby reads share one source of truth (the
/// <see cref="ColosseumSeasonConfig"/> POCO) projected via
/// <see cref="ColosseumLobbyInfoBuilder"/>. <c>/event_info</c> uses a different,
/// event-level shape — <see cref="ColosseumEventInfo"/>.
/// </para>
/// </summary>
[MessagePackObject]
public class ColosseumLobbyInfo
{
    /// <summary>Master gate. <c>false</c> = lobby renders empty.</summary>
    [JsonPropertyName("is_colosseum_period")] [Key("is_colosseum_period")]
    public bool IsColosseumPeriod { get; set; }

    /// <summary>Format enum (Rotation=0, Unlimited=1, TwoPick=10, HOF=31, ...).</summary>
    [JsonPropertyName("deck_format")] [Key("deck_format")]
    public int? DeckFormat { get; set; }

    /// <summary>STRING wire shape: <c>"0"</c>/<c>"1"</c>. Client parses with
    /// <c>jsonData.ToString() == "1"</c>.</summary>
    [JsonPropertyName("is_normal_two_pick")] [Key("is_normal_two_pick")]
    public string? IsNormalTwoPick { get; set; }

    [JsonPropertyName("colosseum_name")] [Key("colosseum_name")]
    public string? ColosseumName { get; set; }

    [JsonPropertyName("is_round_period")] [Key("is_round_period")]
    public bool? IsRoundPeriod { get; set; }

    /// <summary>Wire STRING used by the client as a UI color/theme code.</summary>
    [JsonPropertyName("is_special_mode")] [Key("is_special_mode")]
    public string? IsSpecialMode { get; set; }

    [JsonPropertyName("card_pool_name")] [Key("card_pool_name")]
    public string? CardPoolName { get; set; }

    /// <summary>Present during round period — current stage number (1..3).</summary>
    [JsonPropertyName("now_round")] [Key("now_round")]
    public int? NowRound { get; set; }

    /// <summary>Present outside round period — next stage number.</summary>
    [JsonPropertyName("next_round")] [Key("next_round")]
    public int? NextRound { get; set; }

    [JsonPropertyName("start_time")] [Key("start_time")]
    public string? StartTime { get; set; }

    [JsonPropertyName("end_time")] [Key("end_time")]
    public string? EndTime { get; set; }

    [JsonPropertyName("is_display_tips")] [Key("is_display_tips")]
    public int? IsDisplayTips { get; set; }

    [JsonPropertyName("colosseum_id")] [Key("colosseum_id")]
    public int? ColosseumId { get; set; }

    [JsonPropertyName("tips_id")] [Key("tips_id")]
    public int? TipsId { get; set; }

    [JsonPropertyName("is_all_card_enabled")] [Key("is_all_card_enabled")]
    public int? IsAllCardEnabled { get; set; }

    /// <summary>Reuses the captured <c>/mypage/index</c> shape — single
    /// <c>sales_period_time</c> field per prod capture.</summary>
    [JsonPropertyName("sales_period_info")] [Key("sales_period_info")]
    public ColosseumSalesPeriodInfo? SalesPeriodInfo { get; set; }

    [JsonPropertyName("strategy_pick_num")] [Key("strategy_pick_num")]
    public int? StrategyPickNum { get; set; }
}
