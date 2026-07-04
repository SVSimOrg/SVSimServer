using Microsoft.Extensions.Caching.Memory;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.DeckBuilder;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.UnitTests.Services;

public class DeckCodeServiceTests
{
    private static DeckCodeService NewService(out IMemoryCache cache, IRandom? random = null)
    {
        cache = new MemoryCache(new MemoryCacheOptions());
        return new DeckCodeService(cache, random ?? new SystemRandom());
    }

    [Test]
    public void Mint_returns_4char_lowercase_alphanumeric_code()
    {
        var svc = NewService(out _);

        var code = svc.Mint(new DeckPayload { Clan = "1", CardID = new() { 100211010 } });

        Assert.That(code, Has.Length.EqualTo(4));
        Assert.That(code, Does.Match("^[a-z0-9]+$"));
    }

    [Test]
    public void Resolve_returns_payload_when_code_unexpired()
    {
        var svc = NewService(out _);
        var original = new DeckPayload { Clan = "4", CardID = new() { 100414020, 100414020 } };

        var code = svc.Mint(original);
        var resolved = svc.TryResolve(code);

        Assert.That(resolved, Is.SameAs(original));
    }

    [Test]
    public void Resolve_returns_null_for_unknown_code()
    {
        var svc = NewService(out _);

        Assert.That(svc.TryResolve("nope"), Is.Null);
    }

    [Test]
    public void Resolve_returns_null_after_cache_eviction()
    {
        // Don't sleep for the 3-minute TTL — drop the entry directly to simulate expiry.
        var svc = NewService(out var cache);
        var code = svc.Mint(new DeckPayload { Clan = "1", CardID = new() { 100211010 } });
        cache.Remove(DeckCodeService.CacheKey(code));

        Assert.That(svc.TryResolve(code), Is.Null);
    }
}
