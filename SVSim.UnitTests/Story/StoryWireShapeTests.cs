using System.Text.Json;
using NUnit.Framework;
using SVSim.EmulatedEntrypoint.Models.Dtos.Story;

namespace SVSim.UnitTests.Story;

[TestFixture]
public class StoryWireShapeTests
{
    private static readonly JsonSerializerOptions Opts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
        WriteIndented = false,
    };

    [Test]
    public void InfoResponse_serializes_to_expected_shape()
    {
        var dto = new InfoResponse
        {
            StoryMasterList = new()
            {
                new StoryMasterEntry
                {
                    StoryId = "100", SectionId = "1", CharaId = "2", ChapterId = "1",
                    IsLock = false, NextChapterId = "2",
                    ShowCoordinate = "1", XCoordinate = "100", YCoordinate = "-100",
                    IsCameraMovable = "1", ShowSubtitles = "0",
                    BattleExists = true, EnemyCharaId = "500010", EnemyClass = "2",
                    EnemyAiId = "2001", BgFileName = "6",
                    Battle3dFieldId = "4", BgmId = "0", ReleasePoint = "0",
                    BattleSettings = new() { new BattleSettingDto { DeckClassId = 2 } },
                    StoryReward = new() { new RewardDto {
                        RewardType = "1", RewardDetailId = "0", RewardNumber = "100" } },
                    IsReleased = true, IsSkipEnabled = true,
                }
            }
        };
        var actual = JsonSerializer.Serialize(dto, Opts);
        var expectedPath = Path.Combine(AppContext.BaseDirectory, "Story", "Fixtures",
            "snapshot-info-response.json");
        var expectedJson = File.ReadAllText(expectedPath);

        AssertJsonEquivalent(actual, expectedJson);
    }

    [Test]
    public void FinishResponse_default_serializes_to_expected_shape()
    {
        var dto = new FinishResponse();
        var actual = JsonSerializer.Serialize(dto, Opts);
        var expectedPath = Path.Combine(AppContext.BaseDirectory, "Story", "Fixtures",
            "snapshot-finish-response.json");
        var expectedJson = File.ReadAllText(expectedPath);
        AssertJsonEquivalent(actual, expectedJson);
    }

    private static void AssertJsonEquivalent(string actualJson, string expectedJson)
    {
        var a = JsonDocument.Parse(actualJson).RootElement;
        var e = JsonDocument.Parse(expectedJson).RootElement;
        Assert.That(JsonDeepEquals(a, e), Is.True,
            $"JSON mismatch.\nExpected: {expectedJson}\nActual: {actualJson}");
    }

    private static bool JsonDeepEquals(JsonElement a, JsonElement b)
    {
        if (a.ValueKind != b.ValueKind) return false;
        switch (a.ValueKind)
        {
            case JsonValueKind.Object:
                var ap = a.EnumerateObject().OrderBy(p => p.Name).ToList();
                var bp = b.EnumerateObject().OrderBy(p => p.Name).ToList();
                if (ap.Count != bp.Count) return false;
                for (int i = 0; i < ap.Count; i++)
                    if (ap[i].Name != bp[i].Name || !JsonDeepEquals(ap[i].Value, bp[i].Value))
                        return false;
                return true;
            case JsonValueKind.Array:
                var ae = a.EnumerateArray().ToList();
                var be = b.EnumerateArray().ToList();
                if (ae.Count != be.Count) return false;
                for (int i = 0; i < ae.Count; i++)
                    if (!JsonDeepEquals(ae[i], be[i])) return false;
                return true;
            default:
                return a.GetRawText() == b.GetRawText();
        }
    }
}
