using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SVSim.Database;
using SVSim.Database.Enums;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;
using SVSim.EmulatedEntrypoint.Services;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Services;

public class ArenaTwoPickCardPoolServiceTests
{
    /// <summary>
    /// Seeds a fresh in-memory DB with cards. Each card tuple is
    /// (id, classId, setId, rarity, collectible).
    /// classId == 0 means neutral (Class navigation = null).
    /// </summary>
    private static async Task<SVSimDbContext> SeedCardsAsync(
        params (long id, int classId, int setId, Rarity rarity, bool collectible)[] cards)
    {
        var factory = new SVSimTestFactory();
        var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SVSimDbContext>();
        await db.Database.EnsureCreatedAsync();

        // Collect required class ids and ensure ClassEntry rows exist.
        var requiredClassIds = cards.Select(c => c.classId).Where(id => id != 0).Distinct().ToList();
        foreach (var cid in requiredClassIds)
        {
            if (!db.Classes.Any(c => c.Id == cid))
                db.Classes.Add(new ClassEntry { Id = cid, Name = $"Class{cid}" });
        }
        await db.SaveChangesAsync();

        // Load all needed classes into the context's Local cache so navigation assignments below work.
        await db.Classes.Where(c => requiredClassIds.Contains(c.Id)).LoadAsync();
        var classLookup = db.Classes.Local.ToDictionary(c => c.Id);

        // Group cards by set and create CardSet entries with navigation.
        var bySet = cards.GroupBy(c => c.setId);
        foreach (var group in bySet)
        {
            var set = new ShadowverseCardSetEntry
            {
                Id = group.Key,
                Name = $"TestSet{group.Key}",
                IsInRotation = true,
            };
            foreach (var c in group)
            {
                var classEntry = c.classId == 0
                    ? null
                    : classLookup[c.classId];
                set.Cards.Add(new ShadowverseCardEntry
                {
                    Id = c.id,
                    Name = $"Card{c.id}",
                    Rarity = c.rarity,
                    Class = classEntry,
                    CollectionInfo = c.collectible
                        ? new CardCollectionInfo { CraftCost = 200, DustReward = 50 }
                        : null,
                });
            }
            db.CardSets.Add(set);
        }
        await db.SaveChangesAsync();
        return db;
    }

    private sealed class FakeRandom : IRandom
    {
        private readonly Queue<double> _doubles;
        private readonly Queue<int> _ints;
        public FakeRandom(IEnumerable<double> doubles, IEnumerable<int> ints)
        { _doubles = new(doubles); _ints = new(ints); }
        public double NextDouble() => _doubles.Dequeue();
        public int Next(int maxExclusive) => _ints.Dequeue();
    }

    [Test]
    public async Task GeneratePickSets_returns_two_pairs_with_monotonic_ids_and_correct_turn()
    {
        await using var db = await SeedCardsAsync(
            (100001010L, 1, 10015, Rarity.Bronze, true),
            (900001010L, 0, 10015, Rarity.Bronze, true));
        var config = new ArenaTwoPickConfig();
        var challenge = new ChallengeConfig { PoolCardSetIds = new() { 10015 } };
        var svc = new ArenaTwoPickCardPoolService(db, StubConfig(config, challenge));
        var rng = new FakeRandom(
            doubles: Enumerable.Repeat(0.99, 8),
            ints: Enumerable.Repeat(0, 8));

        var pairs = svc.GeneratePickSetsForTurn(classId: 1, turn: 3, startingPairId: 42, rng);

        Assert.That(pairs.Count, Is.EqualTo(2));
        Assert.That(pairs[0].Id, Is.EqualTo(42));
        Assert.That(pairs[1].Id, Is.EqualTo(43));
        Assert.That(pairs[0].Turn, Is.EqualTo(3));
        Assert.That(pairs[1].Turn, Is.EqualTo(3));
        Assert.That(pairs[0].SetNum, Is.EqualTo(1));
        Assert.That(pairs[1].SetNum, Is.EqualTo(2));
        Assert.That(pairs[0].IsSelected, Is.False);
    }

    [Test]
    public async Task Empty_PoolCardSetIds_falls_back_to_RotationConfig_RotationCardSetIds()
    {
        await using var db = await SeedCardsAsync(
            (100001010L, 1, 10015, Rarity.Bronze, true));
        var config = new ArenaTwoPickConfig();
        var challenge = new ChallengeConfig { PoolCardSetIds = new() };
        var rotation = new RotationConfig { RotationCardSetIds = new() { 10015 } };
        var svc = new ArenaTwoPickCardPoolService(db, StubConfig(config, challenge, rotation));
        var rng = new FakeRandom(
            doubles: Enumerable.Repeat(0.99, 8),
            ints: Enumerable.Repeat(0, 8));

        var pairs = svc.GeneratePickSetsForTurn(classId: 1, turn: 1, startingPairId: 1, rng);
        Assert.That(pairs.Count, Is.EqualTo(2));
        Assert.That(pairs[0].CardId1, Is.EqualTo(100001010L));
    }

    private static IGameConfigService StubConfig(ArenaTwoPickConfig a, ChallengeConfig c, RotationConfig? r = null) =>
        new StubGameConfigService(a, c, r ?? new RotationConfig());

    private sealed class StubGameConfigService : IGameConfigService
    {
        private readonly ArenaTwoPickConfig _a;
        private readonly ChallengeConfig _c;
        private readonly RotationConfig _r;
        public StubGameConfigService(ArenaTwoPickConfig a, ChallengeConfig c, RotationConfig r) { _a = a; _c = c; _r = r; }
        public T Get<T>() where T : class, new() =>
            typeof(T) == typeof(ArenaTwoPickConfig) ? (T)(object)_a :
            typeof(T) == typeof(ChallengeConfig)    ? (T)(object)_c :
            typeof(T) == typeof(RotationConfig)     ? (T)(object)_r :
            new T();
    }
}
