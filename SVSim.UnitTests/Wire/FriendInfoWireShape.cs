using System.Text.Json;
using SVSim.EmulatedEntrypoint.Models.Dtos.Friend;

namespace SVSim.UnitTests.Wire;

public class FriendInfoWireShape
{
    [Test]
    public void FriendEntryDto_serialization_matches_prod_capture()
    {
        // Mirrors the prod capture's first `friends[]` entry from
        // data_dumps/captures/traffic_prod_misc_clicking.ndjson (gigapurin).
        var entry = new FriendEntryDto
        {
            DeviceType = "2",
            Name = "gigapurin",
            CountryCode = "USA",
            MaxFriend = "20",
            LastPlayTime = "2024-10-31 03:16:37",
            IsReceivedTwoPickMission = "1",
            Birth = "0",
            MissionChangeTime = "2017-09-15 02:36:09",
            MissionReceiveType = "0",
            IsOfficial = "0",
            IsOfficialMarkDisplayed = "0",
            ViewerId = 283562639,
            Rank = 11,
            EmblemId = 900311010,
            DegreeId = 300003,
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
        var json = JsonSerializer.Serialize(entry, options);

        var expected =
            "{\"device_type\":\"2\"," +
            "\"name\":\"gigapurin\"," +
            "\"country_code\":\"USA\"," +
            "\"max_friend\":\"20\"," +
            "\"last_play_time\":\"2024-10-31 03:16:37\"," +
            "\"is_received_two_pick_mission\":\"1\"," +
            "\"birth\":\"0\"," +
            "\"mission_change_time\":\"2017-09-15 02:36:09\"," +
            "\"mission_receive_type\":\"0\"," +
            "\"is_official\":\"0\"," +
            "\"is_official_mark_displayed\":\"0\"," +
            "\"viewer_id\":283562639," +
            "\"rank\":11," +
            "\"emblem_id\":900311010," +
            "\"degree_id\":300003}";

        Assert.That(json, Is.EqualTo(expected));
    }
}
