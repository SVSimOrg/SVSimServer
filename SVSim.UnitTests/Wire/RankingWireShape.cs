using System.Text.Json;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Ranking;

namespace SVSim.UnitTests.Wire;

/// <summary>
/// Literal-prod-JSON parity for /ranking/*. Captured from
/// data_dumps/captures/traffic_prod_misc_clicking.ndjson frames 64 (period list)
/// and 66 (master_point_rotation_info response).
/// </summary>
public class RankingWireShape
{
    private const string PeriodListSample = """
        {
          "rank_match": [
            { "id": "122", "period_num": "121", "begin_time": "2026-06-01 02:00:00", "end_time": "2026-07-01 01:59:59" }
          ],
          "master_point": [
            { "id": "120", "period_num": "119", "necessary_score": "0", "begin_time": "2026-06-01 02:00:00", "end_time": "2026-07-01 01:59:59" }
          ],
          "two_pick": [
            { "id": "119", "period_num": "119", "type": "2", "begin_time": "2026-06-01 02:00:00", "end_time": "2026-07-01 01:59:59", "over_460": "1" }
          ],
          "sealed": [
            { "id": "62", "period_num": "62", "begin_time": "2024-06-01 00:00:00", "end_time": "2024-07-01 05:29:59" }
          ],
          "crossover_rank_match": [],
          "crossover_master_point": []
        }
        """;

    private const string MonthlyRankingSample = """
        {
          "period": { "id": "120", "period_num": "119", "begin_time": "2026-06-01 02:00:00", "end_time": "2026-07-01 01:59:59" },
          "ranking": [
            {
              "viewer_id": "735500540", "score": "43700", "ranking_rank": "1",
              "name": "ABC", "country_code": "",
              "rank": 29, "emblem_id": 1313240100, "degree_id": 430015,
              "last_play_time": "2026-06-09 16:23:30", "guild_name": "G"
            }
          ]
        }
        """;

    [Test]
    public void Parses_period_list_into_dto()
    {
        var resp = JsonSerializer.Deserialize<PeriodListResponseDto>(PeriodListSample);
        Assert.That(resp, Is.Not.Null);
        Assert.That(resp!.RankMatch, Has.Count.EqualTo(1));
        Assert.That(resp.MasterPoint[0].NecessaryScore, Is.EqualTo("0"));
        Assert.That(resp.TwoPick[0].Type, Is.EqualTo("2"));
        Assert.That(resp.TwoPick[0].Over460, Is.EqualTo("1"));
        Assert.That(resp.CrossoverRankMatch, Is.Empty);
    }

    [Test]
    public void Parses_monthly_ranking_into_dto()
    {
        var resp = JsonSerializer.Deserialize<MonthlyRankingResponseDto>(MonthlyRankingSample);
        Assert.That(resp, Is.Not.Null);
        Assert.That(resp!.Period.Id, Is.EqualTo("120"));
        Assert.That(resp.Ranking, Has.Count.EqualTo(1));
        var row = resp.Ranking[0];
        Assert.That(row.ViewerId, Is.EqualTo("735500540"));
        Assert.That(row.Score, Is.EqualTo("43700"));
        Assert.That(row.RankingRank, Is.EqualTo("1"));
        Assert.That(row.Rank, Is.EqualTo(29));
        Assert.That(row.EmblemId, Is.EqualTo(1313240100L));
        Assert.That(row.DegreeId, Is.EqualTo(430015L));
    }

    [Test]
    public void Serialized_period_list_emits_snake_case_keys()
    {
        var dto = new PeriodListResponseDto
        {
            RankMatch = new() { new PeriodEntryDto { Id = "1", PeriodNum = "1", BeginTime = "b", EndTime = "e" } },
        };
        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        });
        Assert.That(json, Does.Contain("\"rank_match\""));
        Assert.That(json, Does.Contain("\"crossover_rank_match\""));
        Assert.That(json, Does.Contain("\"period_num\""));
        Assert.That(json, Does.Not.Contain("\"RankMatch\""));
    }

    [Test]
    public void Round_trips_through_MessagePack()
    {
        var original = new PeriodEntryDto { Id = "122", PeriodNum = "121", BeginTime = "2026-06-01 02:00:00", EndTime = "2026-07-01 01:59:59" };
        var bytes = MessagePackSerializer.Serialize(original);
        var roundTripped = MessagePackSerializer.Deserialize<PeriodEntryDto>(bytes);
        Assert.That(roundTripped.Id, Is.EqualTo("122"));
        Assert.That(roundTripped.PeriodNum, Is.EqualTo("121"));
    }
}
