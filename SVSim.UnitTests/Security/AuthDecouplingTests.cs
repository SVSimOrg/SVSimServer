using System.Net;
using SVSim.UnitTests.Infrastructure;

namespace SVSim.UnitTests.Security;

/// <summary>
/// Pins down the wire-level contract that authed endpoints work even when their
/// <c>[FromBody]</c> DTO doesn't inherit <c>BaseRequest</c>. The translation middleware
/// extracts the auth tuple (<c>viewer_id</c> / <c>steam_id</c> / <c>steam_session_ticket</c>)
/// from the raw decrypted msgpack dict and stashes it in <c>HttpContext.Items</c> before the
/// typed DTO deserialize runs, so the Steam handler can read the ticket without depending on
/// the action's DTO shape.
///
/// History: this was a recurring footgun (2026-05-25 basic-puzzle, 2026-05-28 deck-code,
/// 2026-06-02 Phase 3 Bot, 2026-06-10 profile/index + item_acquire_history/info) where
/// every per-DTO workaround eventually got forgotten somewhere else. See
/// <c>docs/superpowers/specs/2026-06-02-baseRequest-auth-footgun-improvement.md</c> for the
/// design.
/// </summary>
[TestFixture]
public class AuthDecouplingTests
{
    [Test]
    public async Task ProfileIndex_succeeds_when_DTO_does_not_inherit_BaseRequest()
    {
        const ulong steamId = 76_561_198_000_000_999UL;
        await using var factory = new SVSimTestFactory(useRealAuthHandler: true);
        await factory.SeedViewerAsync(steamId: steamId);

        var (udid, sid) = EncryptedMsgpackHelper.NewSessionIds();
        var body = new Dictionary<string, object?>
        {
            ["viewer_id"]            = "test-viewer-id-blob",
            ["steam_id"]             = steamId,
            ["steam_session_ticket"] = "deadbeef", // hex-decoded by SteamSessionService; DevAlwaysValidSteamServer accepts any bytes
        };
        var request = EncryptedMsgpackHelper.BuildPost("/profile/index", body, udid, sid);

        using var client = factory.CreateClient();
        var response = await client.SendAsync(request);

        // The DTO (ProfileIndexRequest) has no [Key]'d fields — without the auth-field stash,
        // the msgpack-to-DTO-to-JSON pivot strips viewer_id/steam_id/steam_session_ticket and
        // the handler 401s on "missing steam_session_ticket". Option A keeps them alive in
        // HttpContext.Items so the handler still authenticates.
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK),
            "/profile/index should authenticate against a DTO that does not inherit BaseRequest. " +
            "If this fails with 401, the translation middleware probably stopped stashing AuthFields " +
            "into HttpContext.Items before DTO deserialization.");
    }
}
