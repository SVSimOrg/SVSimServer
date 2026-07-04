using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MessagePack;
using MessagePack.Resolvers;
using Newtonsoft.Json.Linq;
using SVSim.EmulatedEntrypoint.Middlewares;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Responses;

namespace SVSim.UnitTests.Infrastructure;

/// <summary>
/// Exercises the JSON → JTree → msgpack pipeline that runs inside
/// <see cref="ShadowverseTranslationMiddleware"/>. The middleware itself is hard to unit-test
/// in isolation because it depends on HttpContext + the encryption layer; instead we re-run
/// the exact same transformation steps the middleware would and assert on the resulting
/// msgpack bytes converted back to JSON.
///
/// What this guards: that a controller returning an object with mixed null/non-null optional
/// fields produces a msgpack map containing ONLY the non-null fields, with snake_case keys.
/// That's the load-bearing property — the Unity client's <c>Keys.Contains</c> + <c>.ToInt()</c>
/// idiom NREs the moment the wire serializes a present-but-null field.
/// </summary>
public class WireSerializationTests
{
    private static readonly JsonSerializerOptions ControllerJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly MessagePackSerializerOptions MsgPackOptions =
        MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);

    /// <summary>
    /// Reproduces what the middleware does to a controller's response object: serialize via
    /// our JSON options (snake_case, omit null) → parse JSON tree → convert to plain CLR tree
    /// → msgpack-serialize → msgpack-to-JSON. Returns the JSON the client would see.
    /// </summary>
    private static JsonDocument RoundTripThroughWirePipeline<T>(T response)
    {
        var json = JsonSerializer.Serialize(response, ControllerJsonOptions);
        var tree = ShadowverseTranslationMiddleware.ConvertJsonTreeToPlainObject(JToken.Parse(json));
        var msgPackBytes = MessagePackSerializer.Serialize(tree, MsgPackOptions);
        var msgPackJson = MessagePackSerializer.ConvertToJson(msgPackBytes, MsgPackOptions);
        return JsonDocument.Parse(msgPackJson);
    }

    [Test]
    public void GameStartResponse_with_null_optional_fields_omits_them_from_msgpack()
    {
        var response = new GameStartResponse
        {
            NowViewerId = 42,
            NowName = "Tester",
            NowTutorialStep = "100",
            IsSetTransitionPassword = true,
            NowRank = new Dictionary<string, string> { { "1", "RankName_010" } },
            TransitionAccountData = new List<TransitionAccountData>
            {
                new() { SocialAccountId = "76561198000000001", SocialAccountType = "5", ConnectedViewerId = "42" }
            },
            // Both deliberately left at null — must NOT appear in the msgpack output.
            RewriteViewerId = null,
            AccountDeleteReservationStatus = null,
            TosState = 1, PolicyState = 1, KorAuthorityState = 0,
            TosId = 1, PolicyId = 1, KorAuthorityId = 0
        };

        using var doc = RoundTripThroughWirePipeline(response);
        var root = doc.RootElement;

        Assert.That(root.TryGetProperty("rewrite_viewer_id", out _), Is.False,
            "Null `rewrite_viewer_id` must not appear in msgpack output — client NREs on Keys.Contains+.ToInt() against Nil.");
        Assert.That(root.TryGetProperty("account_delete_reservation_status", out _), Is.False,
            "Null `account_delete_reservation_status` must not appear (presence triggers client behavior).");

        // Sanity: required fields still there, with snake_case keys.
        Assert.That(root.GetProperty("now_viewer_id").GetInt64(), Is.EqualTo(42));
        Assert.That(root.GetProperty("now_name").GetString(), Is.EqualTo("Tester"));
        Assert.That(root.GetProperty("now_tutorial_step").GetString(), Is.EqualTo("100"));
        Assert.That(root.GetProperty("tos_state").GetInt64(), Is.EqualTo(1));
    }

    [Test]
    public void GameStartResponse_with_set_optional_field_emits_it_in_msgpack()
    {
        // Mirror image of the omission test: when an optional field IS set, it must reach the
        // client. Otherwise we'd have papered over the bug by accidentally dropping everything.
        var response = new GameStartResponse
        {
            NowViewerId = 42,
            NowName = "Tester",
            NowTutorialStep = "100",
            IsSetTransitionPassword = false,
            RewriteViewerId = 999, // explicitly set
            AccountDeleteReservationStatus = 1,
            TosState = 1, PolicyState = 1, KorAuthorityState = 0,
            TosId = 1, PolicyId = 1, KorAuthorityId = 0
        };

        using var doc = RoundTripThroughWirePipeline(response);
        var root = doc.RootElement;

        Assert.That(root.GetProperty("rewrite_viewer_id").GetInt64(), Is.EqualTo(999));
        Assert.That(root.GetProperty("account_delete_reservation_status").GetInt64(), Is.EqualTo(1));
    }

    [Test]
    public void GameStartResponse_nested_transition_account_data_round_trips_with_snake_case_keys()
    {
        // The nested list of TransitionAccountData entries needs to keep its per-entry shape
        // (snake_case keys, three string fields) after going JSON → tree → msgpack → JSON.
        var response = new GameStartResponse
        {
            NowViewerId = 42,
            NowName = "Tester",
            NowTutorialStep = "100",
            TransitionAccountData = new List<TransitionAccountData>
            {
                new() { SocialAccountId = "111", SocialAccountType = "5", ConnectedViewerId = "42" }
            },
            TosState = 1, PolicyState = 1, KorAuthorityState = 0,
            TosId = 1, PolicyId = 1, KorAuthorityId = 0
        };

        using var doc = RoundTripThroughWirePipeline(response);
        var transitions = doc.RootElement.GetProperty("transition_account_data");
        Assert.That(transitions.ValueKind, Is.EqualTo(JsonValueKind.Array));
        Assert.That(transitions.GetArrayLength(), Is.EqualTo(1));

        var entry = transitions[0];
        Assert.That(entry.GetProperty("social_account_id").GetString(), Is.EqualTo("111"));
        Assert.That(entry.GetProperty("social_account_type").GetString(), Is.EqualTo("5"));
        Assert.That(entry.GetProperty("connected_viewer_id").GetString(), Is.EqualTo("42"));
    }
}
