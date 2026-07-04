using System.Text.Json.Serialization;
using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Ranking;

/// <summary>
/// One row in a /ranking/* leaderboard's `ranking[]` array. Stub server never
/// emits these; the type exists so the DTO compiles and so wire-shape tests can
/// parse captured prod frames into it. Wire-type quirks (per capture frame 65):
/// viewer_id/score/ranking_rank are STRINGS; rank/emblem_id/degree_id are NUMBERS.
/// </summary>
[MessagePackObject]
public sealed class RankingEntryDto
{
    [JsonPropertyName("viewer_id"), Key("viewer_id")]
    public string ViewerId { get; set; } = "0";

    [JsonPropertyName("score"), Key("score")]
    public string Score { get; set; } = "0";

    [JsonPropertyName("ranking_rank"), Key("ranking_rank")]
    public string RankingRank { get; set; } = "0";

    [JsonPropertyName("name"), Key("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("country_code"), Key("country_code")]
    public string CountryCode { get; set; } = "";

    [JsonPropertyName("rank"), Key("rank")]
    public int Rank { get; set; }

    [JsonPropertyName("emblem_id"), Key("emblem_id")]
    public long EmblemId { get; set; }

    [JsonPropertyName("degree_id"), Key("degree_id")]
    public long DegreeId { get; set; }

    [JsonPropertyName("last_play_time"), Key("last_play_time")]
    public string LastPlayTime { get; set; } = "";

    [JsonPropertyName("guild_name"), Key("guild_name")]
    public string GuildName { get; set; } = "";
}
