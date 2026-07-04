using SVSim.EmulatedEntrypoint.Models.Dtos.Responses.DeckBuilder;

namespace SVSim.EmulatedEntrypoint.Services;

public interface IDeckCodeService
{
    /// <summary>
    /// Stores <paramref name="payload"/> under a freshly minted token and returns it. The token
    /// is valid for <see cref="DeckCodeService.Ttl"/> from this call.
    /// </summary>
    string Mint(DeckPayload payload);

    /// <summary>
    /// Returns the deck payload for an unexpired code, or null on miss/expired.
    /// </summary>
    DeckPayload? TryResolve(string code);
}
