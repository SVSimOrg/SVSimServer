using System.Text.Json;
using SVSim.EmulatedEntrypoint.Models.Dtos;

namespace SVSim.UnitTests.Wire;

public class UserMyPageWireShape
{
    [Test]
    public void MyPageBgSetting_serialization_emits_strings_matching_prod_capture()
    {
        var setting = new MyPageBgSetting
        {
            MyPageId = "1213410310",
            SelectType = "1",
            MyPageIdList = new List<string>
            {
                "1211410310", "1212410310", "1213410310", "1214410310",
                "1215410310", "1216410310", "1217410310", "1218410310",
            },
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        };
        var json = JsonSerializer.Serialize(setting, options);

        // Prod capture line 12 / line 56 of traffic_prod_misc_clicking.ndjson, user_mypage_info.user_mypage_setting:
        var expected =
            "{\"mypage_id\":\"1213410310\"," +
            "\"select_type\":\"1\"," +
            "\"mypage_id_list\":[" +
            "\"1211410310\",\"1212410310\",\"1213410310\",\"1214410310\"," +
            "\"1215410310\",\"1216410310\",\"1217410310\",\"1218410310\"" +
            "]}";

        Assert.That(json, Is.EqualTo(expected));
    }
}
