namespace SVSim.EmulatedEntrypoint.Services;

/// <summary>
/// Single primitive for "something happened that mission/achievement progress should react to."
/// Emitters at battle/story finish call this with the FULL list of event keys their event matches
/// at all granularity levels (broad → narrow). Service does dumb exact-match against catalog.
/// </summary>
public interface IMissionProgressService
{
    /// <param name="viewerId">Viewer the event applies to.</param>
    /// <param name="eventKeys">Broad-to-narrow keys. Example: a swordcraft ranked win
    /// passes ["ranked_win", "ranked_win:swordcraft"]. The order doesn't matter for counter
    /// increments (each key gets its own counter) but the order is the conventional one used
    /// by emitters for readability.</param>
    /// <param name="delta">Count delta per key (default 1).</param>
    Task RecordEventAsync(long viewerId, IReadOnlyList<string> eventKeys, int delta = 1, CancellationToken ct = default);
}
