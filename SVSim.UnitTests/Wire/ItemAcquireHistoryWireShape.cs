using System.Text.Json;
using SVSim.EmulatedEntrypoint.Models.Dtos.ItemAcquireHistory;

namespace SVSim.UnitTests.Wire;

public class ItemAcquireHistoryWireShape
{
    [Test]
    public void Response_serialization_matches_prod_capture_keys()
    {
        var response = new ItemAcquireHistoryInfoResponse
        {
            Histories =
            {
                new ItemAcquireHistoryEntryDto
                {
                    RewardType = "9",
                    RewardDetailId = "0",
                    RewardCount = "20",
                    AcquireType = "1",
                    AcquireTime = "2026-06-09 16:59:44",
                    Message = "Daily Bonus: Day 2",
                },
            },
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
        var json = JsonSerializer.Serialize(response, options);

        // Prod capture line ~93 (formatted for diff legibility):
        var expected = "{\"histories\":[{" +
            "\"reward_type\":\"9\"," +
            "\"reward_detail_id\":\"0\"," +
            "\"reward_count\":\"20\"," +
            "\"acquire_type\":\"1\"," +
            "\"acquire_time\":\"2026-06-09 16:59:44\"," +
            "\"message\":\"Daily Bonus: Day 2\"" +
            "}]}";

        Assert.That(json, Is.EqualTo(expected));
    }
}
