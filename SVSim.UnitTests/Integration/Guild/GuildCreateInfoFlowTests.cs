using System.Net.Http.Json;
using System.Text.Json;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Integration.Guild;

public class GuildCreateInfoFlowTests
{
    [Test]
    public async Task PostingCreate_then_Info_returns_populated_guild()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(
            steamId: 76_561_198_200_000_001UL,
            displayName: "AlphaIntPlayer");

        using var client = factory.CreateAuthenticatedClient(viewerId);

        // POST /guild/create — mirror the arena integration test style: include the base-request fields
        const string Vid = "0"; const int Sid = 0; const string Stk = "";
        var create = await client.PostAsync("/guild/create",
            System.Net.Http.Json.JsonContent.Create(new { guild_name = "AlphaInt", activity = 1, join_condition = 1,
                viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        Assert.That(create.IsSuccessStatusCode, Is.True, $"create failed ({create.StatusCode}): {await create.Content.ReadAsStringAsync()}");

        // POST /guild/info — expect JOINING state with guild populated
        var info = await client.PostAsync("/guild/info",
            System.Net.Http.Json.JsonContent.Create(new { viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk }));
        var json = await info.Content.ReadAsStringAsync();
        Assert.That(info.IsSuccessStatusCode, Is.True, $"info failed: {json}");

        // In test mode (no UnityPlayer user-agent) the translation middleware is a no-op;
        // the controller's raw DTO is returned directly (no {data_headers, data} envelope).
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("guild_status").GetString(), Is.EqualTo("2"),
            "guild_status should be JOINING (2)");
        Assert.That(root.GetProperty("guild").GetProperty("detail").GetProperty("guild_name").GetString(),
            Is.EqualTo("AlphaInt"));
        Assert.That(root.GetProperty("guild").GetProperty("members").GetArrayLength(),
            Is.EqualTo(1));
    }

    [Test]
    public async Task PostingCreate_twice_returns_error_second_time()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync(
            steamId: 76_561_198_200_000_002UL,
            displayName: "DoubleCreatePlayer");

        using var client = factory.CreateAuthenticatedClient(viewerId);

        const string Vid = "0"; const int Sid = 0; const string Stk = "";
        var createBody = new { guild_name = "OnlyOnce", activity = 1, join_condition = 1,
            viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk };

        // First create should succeed
        var first = await client.PostAsync("/guild/create",
            System.Net.Http.Json.JsonContent.Create(createBody));
        Assert.That(first.IsSuccessStatusCode, Is.True, $"First create failed: {await first.Content.ReadAsStringAsync()}");
        var firstJson = await first.Content.ReadAsStringAsync();
        using var firstDoc = JsonDocument.Parse(firstJson);
        // In test mode (no envelope), result_code may be present (failure) or absent (success EmptyResponse).
        // Success path returns EmptyResponse ({}), so result_code will not be present.
        var firstRoot = firstDoc.RootElement;
        if (firstRoot.TryGetProperty("result_code", out var rc1))
        {
            // If present it must not be 2 (error code).
            Assert.That(rc1.GetInt32(), Is.Not.EqualTo(2), "First create must not error");
        }

        // Second create for the same viewer should return an error envelope (result_code = 2).
        var secondBody = new { guild_name = "OnlyOnceAgain", activity = 1, join_condition = 1,
            viewer_id = Vid, steam_id = Sid, steam_session_ticket = Stk };
        var second = await client.PostAsync("/guild/create",
            System.Net.Http.Json.JsonContent.Create(secondBody));
        Assert.That(second.IsSuccessStatusCode, Is.True, "HTTP level should still be 200");
        var secondJson = await second.Content.ReadAsStringAsync();
        using var secondDoc = JsonDocument.Parse(secondJson);
        // Error path: the MapErrorToWire helper returns { result_code = 2 }
        Assert.That(secondDoc.RootElement.TryGetProperty("result_code", out var rc2), Is.True,
            $"Second create should return result_code field, got: {secondJson}");
        Assert.That(rc2.GetInt32(), Is.EqualTo(2), "Second create should fail with result_code=2");
    }
}
