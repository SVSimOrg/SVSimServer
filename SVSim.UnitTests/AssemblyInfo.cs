using NUnit.Framework;

// Tests within a single fixture run concurrently as well as across fixtures. Each
// SVSimTestFactory owns a private SQLite :memory: connection so DB state isn't shared.
// The two previously process-static repo caches (BattlePassRepository._curveCache,
// MissionCatalogRepository._maxLevelCache) now live in the DI-registered IMemoryCache,
// which is per-host — each WebApplicationFactory builds its own service provider so the
// cache is naturally bounded to a single fixture's DB.
//
// Fixtures with shared instance state must opt out via [FixtureLifeCycle(InstancePerTestCase)]
// (see StoryServiceTests) or move state into the test method.
[assembly: Parallelizable(ParallelScope.All)]
