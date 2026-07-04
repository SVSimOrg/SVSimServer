using Microsoft.Extensions.Caching.Memory;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.DeckBuilder;

namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// In-memory deck-code store with a 3-minute absolute TTL. Codes are lowercase 4-character
/// alphanumeric tokens — matches the shortest sample observed in prod (e.g. "t7rz" in
/// data_dumps/captures/traffic_prod_deckcode.ndjson). The portal's anonymous global namespace is
/// mirrored here: codes are not scoped to viewer.
/// </summary>
public sealed class DeckCodeService : IDeckCodeService
{
    public static readonly TimeSpan Ttl = TimeSpan.FromMinutes(3);

    private const string Alphabet = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int CodeLength = 4;          // 36^4 ≈ 1.7M codes
    private const int MaxMintAttempts = 8;     // collision retries — saturation is genuinely exceptional

    private readonly IMemoryCache _cache;
    private readonly IRandom _random;

    public DeckCodeService(IMemoryCache cache, IRandom random)
    {
        _cache = cache;
        _random = random;
    }

    public string Mint(DeckPayload payload)
    {
        for (int attempt = 0; attempt < MaxMintAttempts; attempt++)
        {
            string code = GenerateCode();
            string key = CacheKey(code);
            if (_cache.TryGetValue(key, out _)) continue;

            _cache.Set(key, payload, Ttl);
            return code;
        }

        // Hit only if the 4-char namespace is genuinely saturated within a 3-minute window.
        // At that load we'd want longer codes; throw loudly so the symptom doesn't get buried.
        throw new InvalidOperationException(
            $"Deck-code namespace saturated after {MaxMintAttempts} attempts. " +
            "Either traffic exploded or the cache is misconfigured.");
    }

    public DeckPayload? TryResolve(string code)
        => _cache.TryGetValue<DeckPayload>(CacheKey(code), out var payload) ? payload : null;

    private string GenerateCode()
    {
        Span<char> buf = stackalloc char[CodeLength];
        for (int i = 0; i < CodeLength; i++)
        {
            buf[i] = Alphabet[_random.Next(Alphabet.Length)];
        }
        return new string(buf);
    }

    internal static string CacheKey(string code) => $"deck_code:{code}";
}
