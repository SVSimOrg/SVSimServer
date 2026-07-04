using MessagePack;

namespace SVSim.EmulatedEntrypoint.Models.Dtos.Profile;

/// <summary>
/// Empty request body — the endpoint takes no parameters and deliberately does NOT inherit
/// BaseRequest. The translation middleware pulls the auth tuple
/// (viewer_id / steam_id / steam_session_ticket) straight out of the decrypted msgpack dict
/// into <c>HttpContext.Items[AuthFields.ContextKey]</c> before deserializing into this DTO,
/// so the Steam handler reads them from there rather than re-parsing the rewritten body.
/// See <c>AuthDecouplingTests</c> for the integration test that pins this contract down.
/// </summary>
[MessagePackObject(true)]
public sealed class ProfileIndexRequest
{
}
