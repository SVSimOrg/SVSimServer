using System.Text.Json;
using MessagePack;
using SVSim.EmulatedEntrypoint.Models.Dtos.Replay;

namespace SVSim.UnitTests.Wire;

/// <summary>
/// Literal-prod-JSON parity for /replay/info. Captured from
/// data_dumps/captures/traffic_prod_misc_clicking.ndjson frame 96.
/// Catches future wire-key/wire-type drift (the kind that bit card_id vs
/// cardID on the deck-code endpoint, 2026-05-28).
/// </summary>
public class ReplayInfoWireShape
{
    private const string CapturedFrame = """
        {
          "replay_list": [
            {
              "battle_type": "4", "two_pick_type": "0", "deck_format": "2",
              "battle_id": "234471983876", "is_limit_turn": "0",
              "opponent_name": "Foo", "class_id": "8", "opponent_class_id": "5",
              "sub_class_id": "0", "opponent_sub_class_id": "0",
              "rotation_id": "0", "opponent_rotation_id": "0",
              "opponent_country_code": "", "chara_id": "8", "opponent_chara_id": "805",
              "opponent_emblem_id": "721341010", "opponent_degree_id": "120023",
              "is_win": "0", "battle_start_time": "2026-06-04 17:13:13",
              "create_time": "2026-06-04 17:16:06"
            }
          ],
          "feature_maintenance_list": []
        }
        """;

    [Test]
    public void Parses_captured_prod_frame_into_dto()
    {
        var resp = JsonSerializer.Deserialize<ReplayInfoResponseDto>(CapturedFrame);

        Assert.That(resp, Is.Not.Null);
        Assert.That(resp!.ReplayList, Has.Count.EqualTo(1));

        var item = resp.ReplayList[0];
        Assert.That(item.BattleId, Is.EqualTo("234471983876"));
        Assert.That(item.OpponentName, Is.EqualTo("Foo"));
        Assert.That(item.OpponentEmblemId, Is.EqualTo("721341010"));
        Assert.That(item.OpponentDegreeId, Is.EqualTo("120023"));
        Assert.That(item.IsWin, Is.EqualTo("0"));
        Assert.That(item.BattleStartTime, Is.EqualTo("2026-06-04 17:13:13"));
    }

    [Test]
    public void Serialized_dto_uses_snake_case_wire_keys()
    {
        var dto = new ReplayInfoResponseDto
        {
            ReplayList = new List<ReplayInfoItemDto>
            {
                new()
                {
                    BattleType = "4", BattleId = "234471983876", OpponentName = "Foo",
                    OpponentEmblemId = "721341010", OpponentDegreeId = "120023",
                    IsWin = "0", BattleStartTime = "2026-06-04 17:13:13",
                    CreateTime = "2026-06-04 17:16:06",
                },
            },
        };

        var json = JsonSerializer.Serialize(dto, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        });

        Assert.That(json, Does.Contain("\"battle_id\":\"234471983876\""));
        Assert.That(json, Does.Contain("\"opponent_emblem_id\":\"721341010\""));
        Assert.That(json, Does.Contain("\"battle_start_time\":\"2026-06-04 17:13:13\""));
        // Ensure no camel-case / PascalCase leakage.
        Assert.That(json, Does.Not.Contain("\"BattleId\""));
        Assert.That(json, Does.Not.Contain("\"battleId\""));
    }

    [Test]
    public void Round_trips_through_MessagePack_preserving_wire_keys()
    {
        var original = new ReplayInfoItemDto
        {
            BattleType = "4", BattleId = "234471983876", OpponentName = "Foo",
            OpponentEmblemId = "721341010", IsWin = "1",
            BattleStartTime = "2026-06-04 17:13:13",
        };

        var bytes = MessagePackSerializer.Serialize(original);
        var roundTripped = MessagePackSerializer.Deserialize<ReplayInfoItemDto>(bytes);

        Assert.That(roundTripped.BattleId, Is.EqualTo("234471983876"));
        Assert.That(roundTripped.OpponentEmblemId, Is.EqualTo("721341010"));
        Assert.That(roundTripped.IsWin, Is.EqualTo("1"));
    }
}
