using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Responses.Card;

/// <summary>
/// /card/protect response data — empty. The client just needs result_code=1 in the envelope's
/// data_headers; it mutates its own FavoriteCardList from its request-side knowledge.
/// </summary>
[MessagePackObject]
public class CardProtectResponse
{
}
