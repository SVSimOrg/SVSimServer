using MessagePack;
using System.Text.Json.Serialization;

namespace SVSim.EmulatedEntrypoint.Models.Dtos;

/// <summary>
/// Master Points season window. Client only reads end_time at /mypage/index
/// (MyPageTask.cs:113-114) when _masterResetNextTime hasn't been set yet, but
/// prod also emits id / period_num / necessary_score / begin_time — we mirror
/// them so the wire shape matches.
/// </summary>
[MessagePackObject]
public class MasterPointRankingPeriod
{
    [JsonPropertyName("id")]
    [Key("id")]
    public int Id { get; set; }

    [JsonPropertyName("period_num")]
    [Key("period_num")]
    public int PeriodNum { get; set; }

    /// <summary>Stored as long to mirror MasterPointRankingPeriodEntry.NecessaryScore (rank-point thresholds can grow large).</summary>
    [JsonPropertyName("necessary_score")]
    [Key("necessary_score")]
    public long NecessaryScore { get; set; }

    /// <summary>ISO datetime.</summary>
    [JsonPropertyName("begin_time")]
    [Key("begin_time")]
    public string BeginTime { get; set; } = string.Empty;

    /// <summary>ISO datetime. Required — client calls DateTime.Parse on it.</summary>
    [JsonPropertyName("end_time")]
    [Key("end_time")]
    public string EndTime { get; set; } = string.Empty;
}
