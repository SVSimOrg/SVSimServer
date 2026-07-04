using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

public class MyPageControllerHomeDialogTests
{
    private const string BaseAuthBlock =
        @"""viewer_id"":""0"",""steam_id"":0,""steam_session_ticket"":""""";

    private const string RequestBody =
        $$"""{"carrier":"",{{BaseAuthBlock}}}""";

    private static async Task<JsonElement> PostIndexAsync(SVSimTestFactory factory, HttpClient client)
    {
        var resp = await client.PostAsync("/mypage/index",
            new StringContent(RequestBody, Encoding.UTF8, "application/json"));
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
        return doc.RootElement.Clone();
    }

    private static async Task SeedDialogAsync(SVSimTestFactory factory, HomeDialogEntry entry)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        db.HomeDialogEntries.Add(entry);
        await db.SaveChangesAsync();
    }

    [Test]
    public async Task Index_returns_active_home_dialog_on_first_call_of_session()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var now = DateTime.UtcNow;
        await SeedDialogAsync(factory, new HomeDialogEntry
        {
            Id = 1,
            TitleTextId = "HomeDialog_0066",
            Image = "home_dialog_000312",
            ButtonListJson = """[{"button_text_id":"HomeDialog_0002","scene":"card_pack","status":"80032"}]""",
            BeginTime = now.AddHours(-1),
            EndTime = now.AddHours(1),
            Type = 1,
            Priority = 0,
        });

        var root = await PostIndexAsync(factory, client);
        var list = root.GetProperty("home_dialog_list");
        Assert.That(list.GetArrayLength(), Is.EqualTo(1));

        var entry = list[0];
        Assert.That(entry.GetProperty("type").GetString(), Is.EqualTo("1"));
        Assert.That(entry.GetProperty("title_text_id").GetString(), Is.EqualTo("HomeDialog_0066"));
        Assert.That(entry.GetProperty("image").GetString(), Is.EqualTo("home_dialog_000312"));

        var buttons = entry.GetProperty("button_list");
        Assert.That(buttons.GetArrayLength(), Is.EqualTo(1));
        Assert.That(buttons[0].GetProperty("button_text_id").GetString(), Is.EqualTo("HomeDialog_0002"));
        Assert.That(buttons[0].GetProperty("scene").GetString(), Is.EqualTo("card_pack"));
        Assert.That(buttons[0].GetProperty("status").GetString(), Is.EqualTo("80032"));
    }

    [Test]
    public async Task Index_suppresses_already_fired_dialog_on_second_call()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var now = DateTime.UtcNow;
        await SeedDialogAsync(factory, new HomeDialogEntry
        {
            Id = 1,
            TitleTextId = "HomeDialog_0066",
            Image = "home_dialog_000312",
            ButtonListJson = "[]",
            BeginTime = now.AddHours(-1),
            EndTime = now.AddHours(1),
            Priority = 0,
        });

        var first = await PostIndexAsync(factory, client);
        Assert.That(first.GetProperty("home_dialog_list").GetArrayLength(), Is.EqualTo(1),
            "First call must emit the active dialog.");

        var second = await PostIndexAsync(factory, client);
        Assert.That(second.GetProperty("home_dialog_list").GetArrayLength(), Is.EqualTo(0),
            "Second call must suppress the already-fired dialog.");
    }

    [Test]
    public async Task Index_skips_expired_and_not_yet_active_dialogs()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var now = DateTime.UtcNow;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.HomeDialogEntries.AddRange(
                new HomeDialogEntry { Id = 1, TitleTextId = "expired", Image = "i", ButtonListJson = "[]", BeginTime = now.AddDays(-30), EndTime = now.AddDays(-1) },
                new HomeDialogEntry { Id = 2, TitleTextId = "not-yet", Image = "i", ButtonListJson = "[]", BeginTime = now.AddDays(1),   EndTime = now.AddDays(30) }
            );
            await db.SaveChangesAsync();
        }

        var root = await PostIndexAsync(factory, client);
        Assert.That(root.GetProperty("home_dialog_list").GetArrayLength(), Is.EqualTo(0));
    }

    [Test]
    public async Task Index_picks_highest_priority_first_then_walks_down_on_subsequent_calls()
    {
        using var factory = new SVSimTestFactory();
        long viewerId = await factory.SeedViewerAsync();
        using var client = factory.CreateAuthenticatedClient(viewerId);

        var now = DateTime.UtcNow;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
            db.HomeDialogEntries.AddRange(
                new HomeDialogEntry { Id = 1, TitleTextId = "low",  Image = "i", ButtonListJson = "[]", BeginTime = now.AddHours(-1), EndTime = now.AddHours(1), Priority = 5  },
                new HomeDialogEntry { Id = 2, TitleTextId = "high", Image = "i", ButtonListJson = "[]", BeginTime = now.AddHours(-1), EndTime = now.AddHours(1), Priority = 10 }
            );
            await db.SaveChangesAsync();
        }

        var first  = await PostIndexAsync(factory, client);
        var second = await PostIndexAsync(factory, client);
        var third  = await PostIndexAsync(factory, client);

        Assert.That(first.GetProperty("home_dialog_list")[0].GetProperty("title_text_id").GetString(),
            Is.EqualTo("high"), "First call must emit the highest-priority dialog.");
        Assert.That(second.GetProperty("home_dialog_list")[0].GetProperty("title_text_id").GetString(),
            Is.EqualTo("low"), "Second call must walk down to the next-priority unfired dialog.");
        Assert.That(third.GetProperty("home_dialog_list").GetArrayLength(), Is.EqualTo(0),
            "Third call must be empty — both dialogs fired.");
    }
}
