using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Database;
using SVSim.Database.Models;
using SVSim.Database.Repositories.Globals;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Repositories;

public class GlobalsRepositoryHomeDialogTests
{
    [Test]
    public async Task GetActiveHomeDialogsAsync_returns_only_rows_inside_window_ordered_by_priority_desc_then_id_asc()
    {
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IGlobalsRepository>();

        var now = new DateTime(2026, 6, 8, 12, 0, 0, DateTimeKind.Utc);

        db.HomeDialogEntries.AddRange(
            new HomeDialogEntry { Id = 1, TitleTextId = "past",        Image = "i", BeginTime = now.AddDays(-30), EndTime = now.AddDays(-1), Priority = 0  },
            new HomeDialogEntry { Id = 2, TitleTextId = "active-lo",   Image = "i", BeginTime = now.AddDays(-1),  EndTime = now.AddDays(1),  Priority = 5  },
            new HomeDialogEntry { Id = 3, TitleTextId = "active-hi",   Image = "i", BeginTime = now.AddDays(-1),  EndTime = now.AddDays(1),  Priority = 10 },
            new HomeDialogEntry { Id = 4, TitleTextId = "future",      Image = "i", BeginTime = now.AddDays(1),   EndTime = now.AddDays(30), Priority = 99 },
            new HomeDialogEntry { Id = 5, TitleTextId = "active-mid",  Image = "i", BeginTime = now.AddDays(-1),  EndTime = now.AddDays(1),  Priority = 5  }
        );
        await db.SaveChangesAsync();

        var result = await repo.GetActiveHomeDialogsAsync(now);

        Assert.That(result.Select(r => r.Id), Is.EqualTo(new[] { 3, 2, 5 }),
            "Expected priority-DESC then Id-ASC ordering; only entries whose window covers `now`.");
    }

    [Test]
    public async Task GetActiveHomeDialogsAsync_excludes_row_whose_end_time_equals_now()
    {
        // Window is [begin, end) — exclusive on the upper bound.
        using var factory = new SVSimTestFactory();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        var repo = scope.ServiceProvider.GetRequiredService<IGlobalsRepository>();

        var now = new DateTime(2026, 6, 8, 12, 0, 0, DateTimeKind.Utc);
        db.HomeDialogEntries.Add(new HomeDialogEntry
        {
            Id = 1, TitleTextId = "just-expired", Image = "i",
            BeginTime = now.AddHours(-1), EndTime = now, Priority = 0,
        });
        await db.SaveChangesAsync();

        var result = await repo.GetActiveHomeDialogsAsync(now);
        Assert.That(result, Is.Empty);
    }
}
