using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ArenaColosseum;

/// <summary>
/// Event-level descriptor used by <c>/event_info</c> only — distinct shape from
/// <see cref="ColosseumInfo"/>: only <c>format</c>, the event window, the announce id,
/// and the final-round eliminate count. The client's <c>ColosseumDetailTask</c> reads
/// these five fields plus the three string-keyed rounds.
/// </summary>
[MessagePackObject]
public class ColosseumEventInfo
{
    /// <summary>Event format. Mapped via <c>ApiRuleParseAndSet</c> on the client.</summary>
    [JsonPropertyName("format")] [Key("format")]
    public int Format { get; set; }

    [JsonPropertyName("start_time")] [Key("start_time")]
    public string StartTime { get; set; } = "";

    [JsonPropertyName("end_time")] [Key("end_time")]
    public string EndTime { get; set; } = "";

    /// <summary>Optional — emit <c>null</c> when no announce content is configured.</summary>
    [JsonPropertyName("announce_id")] [Key("announce_id")]
    public string? AnnounceId { get; set; }

    [JsonPropertyName("final_round_eliminate_count")] [Key("final_round_eliminate_count")]
    public int FinalRoundEliminateCount { get; set; }
}

[MessagePackObject]
public class ColosseumRoundDetail
{
    [JsonPropertyName("start_time")] [Key("start_time")]
    public string StartTime { get; set; } = "";

    [JsonPropertyName("end_time")] [Key("end_time")]
    public string EndTime { get; set; } = "";

    [JsonPropertyName("is_now_round")] [Key("is_now_round")]
    public bool IsNowRound { get; set; }

    [JsonPropertyName("round_detail")] [Key("round_detail")]
    public List<ColosseumGroupRow> RoundDetail { get; set; } = new();
}

[MessagePackObject]
public class ColosseumGroupRow
{
    [JsonPropertyName("group")] [Key("group")]
    public string Group { get; set; } = "";

    [JsonPropertyName("max_battle_count")] [Key("max_battle_count")]
    public int MaxBattleCount { get; set; }

    [JsonPropertyName("breakthrough_number")] [Key("breakthrough_number")]
    public int BreakthroughNumber { get; set; }

    [JsonPropertyName("entry_number")] [Key("entry_number")]
    public int EntryNumber { get; set; }
}
