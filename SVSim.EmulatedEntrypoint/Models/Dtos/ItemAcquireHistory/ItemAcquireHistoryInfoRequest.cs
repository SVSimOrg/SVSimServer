using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.ItemAcquireHistory;

/// <summary>
/// Empty request body — the endpoint takes no parameters. Does not inherit BaseRequest: the
/// translation middleware stashes the auth tuple into HttpContext.Items before the typed DTO
/// deserialize, so the Steam handler reads them from there. See ProfileIndexRequest +
/// AuthDecouplingTests for the pattern.
/// </summary>
[MessagePackObject(true)]
public sealed class ItemAcquireHistoryInfoRequest
{
}
