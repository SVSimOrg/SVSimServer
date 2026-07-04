using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SVSim.Bootstrap.Importers;
using SVSim.Database;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Controllers;

/// <summary>
/// Drives the importer + controller against the full production pack seed. Guards against
/// regressions in either layer caused by future seed refreshes.
/// </summary>
public class PackControllerFullCatalogTests
{
    [Test]
    public async Task Info_round_trips_every_active_pack_from_production_seed()
    {
        // The production seed (packs.json) is overlaid by a 3-pack test fixture in the default test
        // output dir (see SVSim.UnitTests.csproj). For this test we need the FULL prod catalog,
        // so we point PackImporter at a temp seed dir holding only the upstream production seed
        // (copied from the Bootstrap project's source-tree Data/seeds/).
        var prodSeed = LocateProdSeed("packs.json");
        var tempSeedDir = Path.Combine(Path.GetTempPath(), "svsim-pack-prod-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempSeedDir);
        try
        {
            File.Copy(prodSeed, Path.Combine(tempSeedDir, "packs.json"));

            using var factory = new SVSimTestFactory();
            // Run the default seed pipeline first so the per-domain importers populate surrounding tables,
            // then re-run PackImporter against the prod seed to overwrite the fixture-loaded packs.
            await factory.SeedGlobalsAsync();
            using (var scope = factory.Services.CreateScope())
            {
                var ctx = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
                await new PackImporter().ImportAsync(ctx, tempSeedDir);
            }

            long viewerId = await factory.SeedViewerAsync();

            // Snapshot the clock BEFORE the request so the expected count derives from the
            // same moment the controller will read. PackController calls DateTime.UtcNow
            // directly (not via TimeProvider), so we can't share a single instant — the
            // sub-millisecond window between this and the controller's read is the only
            // race exposure and any pack whose complete_date falls inside it is on the
            // boundary either way.
            var now = DateTime.UtcNow;
            int expectedActive = CountActiveInSeed(prodSeed, now);

            using var client = factory.CreateAuthenticatedClient(viewerId);
            var response = await client.PostAsync(
                "/pack/info",
                new StringContent("""{"viewer_id":"0","steam_id":0,"steam_session_ticket":""}""",
                    Encoding.UTF8, "application/json"));

            var body = await response.Content.ReadAsStringAsync();
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), body);

            using var doc = JsonDocument.Parse(body);
            var list = doc.RootElement.GetProperty("pack_config_list");

            Assert.That(list.GetArrayLength(), Is.EqualTo(expectedActive),
                $"Importer+controller must surface every active pack in the prod seed " +
                $"(expected {expectedActive} active as of {now:O}).");

            // Schema-fidelity spot check: at least one pack with a non-default pack_category
            // proves the field survives the JSON round trip. Doesn't pin to a specific pack id
            // so it stays valid as seasonal packs roll over.
            bool sawNonDefaultCategory = false;
            for (int i = 0; i < list.GetArrayLength(); i++)
            {
                if (list[i].GetProperty("pack_category").GetInt32() != 0)
                {
                    sawNonDefaultCategory = true;
                    break;
                }
            }
            Assert.That(sawNonDefaultCategory, Is.True,
                "At least one active pack should carry a non-default pack_category — " +
                "the prod seed always has e.g. category 1 LegendCardPacks alongside the " +
                "category 0 standard packs. If this fails, either the round-trip dropped " +
                "the field or the seed no longer contains any non-default category.");
        }
        finally
        {
            try { Directory.Delete(tempSeedDir, recursive: true); } catch { /* best-effort cleanup */ }
        }
    }

    /// <summary>
    /// Mirrors <c>PackRepository.GetActivePacks</c>'s filter (IsEnabled &amp;&amp;
    /// CommenceDate &lt;= now &lt;= CompleteDate) against the raw seed file. is_enabled
    /// defaults to true to match <c>PackSeed.IsEnabled</c>. Uses the same wire-date parser
    /// as the importer so any date-string quirk is parsed identically on both sides.
    /// </summary>
    private static int CountActiveInSeed(string seedPath, DateTime when)
    {
        using var doc = JsonDocument.Parse(File.ReadAllText(seedPath));
        int count = 0;
        foreach (var pack in doc.RootElement.EnumerateArray())
        {
            bool enabled = !pack.TryGetProperty("is_enabled", out var en) || en.GetBoolean();
            if (!enabled) continue;

            var commence = ImporterBase.ParseWireDateTime(pack.GetProperty("commence_date").GetString());
            var complete = ImporterBase.ParseWireDateTime(pack.GetProperty("complete_date").GetString());
            if (commence <= when && complete >= when) count++;
        }
        return count;
    }

    /// <summary>
    /// The test output dir's <c>Data/seeds/packs.json</c> is the fixture overlay (3 packs). The
    /// upstream production seed lives in the Bootstrap project's source tree. Walk up from the
    /// test binary dir to the repo root and locate it there.
    /// </summary>
    private static string LocateProdSeed(string fileName)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            var candidate = Path.Combine(dir.FullName, "SVSim.Bootstrap", "Data", "seeds", fileName);
            if (File.Exists(candidate)) return candidate;
            dir = dir.Parent;
        }
        throw new FileNotFoundException(
            $"Could not locate SVSim.Bootstrap/Data/seeds/{fileName} above {AppContext.BaseDirectory}.");
    }
}
