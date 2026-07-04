using System.Text.Json;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.UnitTests.Wire;

public class ProfileIndexWireShape
{
    [Test]
    public void UserClass_serialization_matches_prod_capture_for_main_class()
    {
        // Construct a UserClass mirroring the prod capture's class-1 entry from
        // data_dumps/captures/traffic_prod_misc_clicking.ndjson line 26.
        var entry = new UserClass
        {
            ClassId = 1,
            IsAvailable = 1,
            Level = 5,
            Exp = 600,
            IsRandomLeaderSkin = 0,
            LeaderSkinId = 1,
            LeaderSkinIds = new List<int> { 1 },
            DefaultLeaderSkinId = 1,
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
        var json = JsonSerializer.Serialize(entry, options);

        // Field order matches declaration order in UserClass.cs.
        var expected =
            "{\"class_id\":1," +
            "\"is_available\":1," +
            "\"level\":5," +
            "\"exp\":600," +
            "\"is_random_leader_skin\":0," +
            "\"leader_skin_id\":1," +
            "\"leader_skin_id_list\":[1]," +
            "\"default_leader_skin_id\":1}";

        Assert.That(json, Is.EqualTo(expected));
    }
}
