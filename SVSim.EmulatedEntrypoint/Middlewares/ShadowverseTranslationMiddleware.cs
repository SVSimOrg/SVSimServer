using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MessagePack;
using MessagePack.Resolvers;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using SVSim.Database.Models;
using SVSim.Database.Models.Config;
using SVSim.Database.Services;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Extensions;
using SVSim.EmulatedEntrypoint.Infrastructure;
using SVSim.EmulatedEntrypoint.Models.Dtos;
using SVSim.EmulatedEntrypoint.Models.Dtos.Internal;
using SVSim.EmulatedEntrypoint.Security;
using SVSim.EmulatedEntrypoint.Services;

namespace SVSim.EmulatedEntrypoint.Middlewares;

/// <summary>
/// Translates incoming requests and outgoing responses from the Shadowverse client into the messagepack format.
/// </summary>
public class ShadowverseTranslationMiddleware : IMiddleware
{
    private readonly IActionDescriptorCollectionProvider _actionDescriptorCollectionProvider;
    private readonly ShadowverseSessionService _sessionService;
    private readonly IGameConfigService _gameConfig;
    private readonly ILogger<ShadowverseTranslationMiddleware> _logger;

    // Serialization policy MUST match what AddJsonOptions configured on the controllers, or the
    // model binder won't find the snake_case keys we write into the synthetic request body and
    // every request 400s with empty ModelState. WhenWritingNull is irrelevant for request
    // serialization but kept here for symmetry.
    private static readonly JsonSerializerOptions ControllerJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ShadowverseTranslationMiddleware(
        IActionDescriptorCollectionProvider actionDescriptorCollectionProvider,
        ShadowverseSessionService sessionService,
        IGameConfigService gameConfig,
        ILogger<ShadowverseTranslationMiddleware> logger)
    {
        _actionDescriptorCollectionProvider = actionDescriptorCollectionProvider;
        _sessionService = sessionService;
        _gameConfig = gameConfig;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        bool isUnity = context.Request.Headers.UserAgent.Any(agent => agent?.Contains("UnityPlayer") ?? false);
        string path = context.Request.Path;
        ActionDescriptor? endpointDescriptor =
            _actionDescriptorCollectionProvider.ActionDescriptors.Items.FirstOrDefault(ad =>
                $"/{ad.AttributeRouteInfo.Template}".Equals(path, StringComparison.InvariantCultureIgnoreCase));
        if (!isUnity || endpointDescriptor == null)
        {
            await next.Invoke(context);
            return;
        }

        // Portal endpoints (shadowverse-portal.com — deck builder, deck image) speak msgpack
        // and the standard envelope but skip AES on the wire. Detect via [NoWireEncryption] on
        // the controller or action; this flag toggles the two Encryption calls below but every
        // other step (msgpack pivot, JSON re-serialize for the binder, envelope wrap, base64 of
        // the response) stays identical.
        bool skipEncryption = false;
        if (endpointDescriptor is ControllerActionDescriptor cad)
        {
            skipEncryption =
                cad.MethodInfo.GetCustomAttributes(typeof(NoWireEncryptionAttribute), inherit: true).Length > 0 ||
                cad.ControllerTypeInfo.GetCustomAttributes(typeof(NoWireEncryptionAttribute), inherit: true).Length > 0;
        }

        // Replace response body stream to re-access it.
        using MemoryStream tempResponseBody = new MemoryStream();
        Stream originalResponsebody = context.Response.Body;
        context.Response.Body = tempResponseBody;

        // Pull out the request bytes into a stream
        using MemoryStream requestBytesStream = new MemoryStream();
        await context.Request.Body.CopyToAsync(requestBytesStream);
        byte[] requestBytes = requestBytesStream.ToArray();

        // Get encryption values for this request. Portal endpoints don't carry a SID/UDID pair
        // (they're anonymous-on-the-wire), so the lookup is skipped on the skip-encryption path
        // — there's nothing to decrypt against.
        string sid = context.Request.Headers[NetworkConstants.SessionIdHeaderName];
        Guid? mappedUdid = skipEncryption ? null : _sessionService.GetUdidFromSessionId(sid);
        if (mappedUdid is null && !skipEncryption)
        {
            // Per design (2026-05-25): warn and continue. Decrypt will fail with Guid.Empty as
            // the AES key, surfacing as a msgpack/decrypt error below — but now the *root cause*
            // (the SID wasn't in our dict, likely because the prior request didn't include a UDID
            // header or the server was restarted between handshake and this call) is in the log.
            _logger.LogWarning(
                "No UDID mapping for SID on {Path} (sid={Sid}). Falling back to Guid.Empty — the following decrypt/msgpack error is almost certainly caused by this.",
                path, sid);
        }
        string udid = mappedUdid.GetValueOrDefault().ToString();

        // Decrypt incoming data — unless this is a [NoWireEncryption] endpoint, in which case
        // the request body is already raw msgpack (the client sends portal requests via
        // _createBodyMsgpack with encrypt=false).
        byte[] decryptedBytes;
        try
        {
            decryptedBytes = skipEncryption ? requestBytes : Encryption.Decrypt(requestBytes, udid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Decrypt failed for {Path} (udid={Udid}, encryptedLen={EncryptedLen}). " +
                "If udid is all-zero, see the preceding 'No UDID mapping' warning.",
                path, udid, requestBytes.Length);
            throw;
        }

        // Peek the decrypted msgpack as a raw dict to extract the auth tuple BEFORE the typed
        // DTO deserialize drops anything the action's DTO doesn't model. Stash the result in
        // HttpContext.Items so SteamSessionAuthenticationHandler can read it without depending
        // on the DTO shape — that's the whole point of the decoupling, see
        // docs/superpowers/specs/2026-06-02-baseRequest-auth-footgun-improvement.md. Failures
        // here are non-fatal: the auth handler will surface a 401 with a more specific reason
        // (missing ticket vs corrupt body) than we could from middleware.
        if (!skipEncryption)
        {
            TryStashAuthFields(context, decryptedBytes);
        }

        var firstParam = endpointDescriptor.Parameters.FirstOrDefault();
        if (firstParam is null)
        {
            // Action method has no parameters — middleware can't bind the (encrypted+msgpacked)
            // body to anything. Fail loud with a specific message rather than NREing below on
            // .ParameterType. Authed actions can declare any DTO shape (auth fields are already
            // stashed via TryStashAuthFields above); they just need ONE parameter so the binder
            // has somewhere to put the rewritten JSON body.
            throw new InvalidOperationException(
                $"Action {endpointDescriptor.DisplayName} has no parameters; the SV translation " +
                "middleware needs at least one to bind the decrypted body. Add a request DTO " +
                "parameter — even an empty one (see ProfileIndexRequest for the minimal shape).");
        }
        Type requestType = firstParam.ParameterType;
        object? data;
        try
        {
            data = MessagePackSerializer.Deserialize(requestType, decryptedBytes);
        }
        catch (Exception ex)
        {
            // The most common cause is a Guid.Empty decrypt above producing garbage bytes — but
            // it can also be a genuine schema mismatch (DTO missing [Key], wrong types, etc.),
            // so include the first few bytes for triage.
            string bytePrefix = Convert.ToHexString(decryptedBytes.AsSpan(0, Math.Min(16, decryptedBytes.Length)));
            _logger.LogError(ex,
                "Msgpack deserialize failed for {Path} into {RequestType} (udid={Udid}, decryptedLen={DecryptedLen}, firstBytes={BytePrefix}). " +
                "If decrypted bytes look like noise, the SID→UDID mapping was missing (see warnings above).",
                path, requestType.Name, udid, decryptedBytes.Length, bytePrefix);
            throw;
        }
        // Re-serialize via System.Text.Json with the SAME options the controllers use, so the
        // model binder sees snake_case keys it can match. Using JsonConvert here writes the
        // CLR property names (PascalCase) and every property silently binds to default → 400.
        string json = JsonSerializer.Serialize(data, requestType, ControllerJsonOptions);
        StringContent newStream = new StringContent(json, Encoding.UTF8, "application/json");
        context.Request.Body = newStream.ReadAsStream();
        context.Request.Headers.ContentType = new StringValues("application/json");

        await next.Invoke(context);

        Viewer? viewer = context.GetViewer();

        // Read the controller's JSON response body. System.Text.Json was configured with
        // SnakeCaseLower + WhenWritingNull, so the JSON keys are already in the wire shape and
        // null/optional properties have been omitted. Parse to a JToken tree to preserve that
        // "absent vs null" information — going back through a typed DTO via JsonConvert would
        // re-introduce nulls for missing properties and they'd reach the client as msgpack Nil.
        using MemoryStream responseBytesStream = new MemoryStream();
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        await context.Response.Body.CopyToAsync(responseBytesStream);
        string responseJson = Encoding.UTF8.GetString(responseBytesStream.ToArray());
        object? responseData = string.IsNullOrEmpty(responseJson)
            ? null
            : ConvertJsonTreeToPlainObject(JToken.Parse(responseJson));

        // Build the headers as a strongly-typed POCO so this construction site stays type-safe
        // (the alternative — a Dictionary<string, object> with literal-string keys here — is the
        // anti-pattern documented in the feedback_no_lazy_response_dicts memory).
        DataHeaders typedHeaders = new DataHeaders
        {
            Servertime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            // SID intentionally empty. See docs/api-spec/common/envelope.md §"SID
            // rotation" — the client's SessionId is a hash-on-read property, so echoing
            // the request's SID poisons its backing field and the next request hashes
            // the hash, missing our SID→UDID dict and crashing decryption. To rotate
            // sessions in the future, use the "stable-prefix + counter" pattern from
            // that doc (Option B), and pre-hash the rotated value to index the map by
            // what the client will actually send back on the next request.
            Sid = "",
            // Pushed ONLY on /check/game_start. NetworkTask.Parse opens the
            // "new data is available" popup whenever required_res_ver is present in
            // data_headers AND the URL isn't GameStartCheck (NetworkTask.cs:128-138 — the
            // popup is unconditionally skipped on game_start). Emitting on game_start
            // silently bumps PlayerPrefs["RES_VER"] before ResourceDownloader runs;
            // emitting anywhere else would surface a spurious "new data" dialog on every
            // boot for any client whose cached RES_VER trails the server's current value.
            RequiredResVer = path.Equals("/check/game_start", StringComparison.OrdinalIgnoreCase)
                ? _gameConfig.Get<ResourceConfig>().RequiredResVer
                : null,
            // TODO error handling
            ResultCode = 1,
            // Anonymous endpoints (e.g. /check/special_title with [AllowAnonymous]) reach this
            // middleware without an authenticated viewer — the auth handler either declined or
            // failed to find a Steam-linked viewer. The wire still needs short_udid / viewer_id
            // populated (prod sends real numbers for the title check too, but 0 / 0 satisfies
            // the client's BaseTask.Parse which only reads result_code + servertime here).
            ShortUdid = skipEncryption ? 0 : (viewer?.ShortUdid ?? 0),
            ViewerId = skipEncryption ? 0 : (viewer?.Id ?? 0),
            // Echo the decrypted-against UDID. Most clients ignore this field; SignUpTask.Parse
            // requires it (validates against Certification.Udid on the response). Comes from
            // mappedUdid (the value used for AES); never from controller state.
            Udid = skipEncryption ? "" : (mappedUdid?.ToString() ?? "")
        };

        // Route the typed headers through the same STJ→JToken→dict pipeline that the controller
        // response (Data) goes through. STJ honours the global WhenWritingNull policy, so null
        // optional fields are absent from the JSON; ConvertJsonTreeToPlainObject preserves
        // "absent vs null" all the way to msgpack. Without this, MessagePack's contractless
        // resolver would walk the typed properties and emit "key":null for every nullable
        // field — RequiredResVer being the load-bearing case (a spurious null fires the
        // "new data available" popup via NetworkTask.isResourceVersionUp on every non-
        // game_start endpoint).
        string headersJson = JsonSerializer.Serialize(typedHeaders, ControllerJsonOptions);
        Dictionary<string, object?> headersDict =
            (ConvertJsonTreeToPlainObject(JToken.Parse(headersJson)) as Dictionary<string, object?>)
            ?? throw new InvalidOperationException(
                "DataHeaders JSON projection didn't yield a JSON object — this should be unreachable: " +
                "DataHeaders is a typed POCO that always serializes to a single JSON object root.");

        // Wrap the response in a datawrapper. Portal (no-encryption) endpoints emit an anonymous
        // envelope — viewer/udid/sid stay zero/empty — matching the prod portal traffic shape
        // captured in data_dumps/captures/traffic_prod_deckcode.ndjson.
        DataWrapper wrappedResponseData = new DataWrapper
        {
            Data = responseData,
            DataHeaders = headersDict
        };

        // Convert the response into a messagepack, encrypt it. ContractlessStandardResolver
        // walks the boxed object/list/primitive tree under both DataHeaders and Data —
        // emitting only the keys present in each dictionary. Null-valued optional fields are
        // already stripped upstream by the STJ + ConvertJsonTreeToPlainObject pipeline.
        var msgPackOptions = MessagePackSerializerOptions.Standard
            .WithResolver(ContractlessStandardResolver.Instance);
        // Both branches base64-wrap the response body — the client's NetworkManager.Connect
        // reads downloadHandler.text and calls Convert.FromBase64String on the no-encryption
        // path (Cute/NetworkManager.cs:194) and CryptAES.decrypt (which also base64-decodes
        // internally) on the encrypted path.
        byte[] packedData;
        try
        {
            packedData = MessagePackSerializer.Serialize(wrappedResponseData, msgPackOptions);
            if (!skipEncryption)
            {
                packedData = Encryption.Encrypt(packedData, udid);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Response msgpack{EncryptStep} failed for {Path} (viewerId={ViewerId}, udid={Udid}).",
                skipEncryption ? "" : "/encrypt", path, viewer?.Id, udid);
            throw;
        }
        await originalResponsebody.WriteAsync(Encoding.UTF8.GetBytes(Convert.ToBase64String(packedData)));
        context.Response.Body = originalResponsebody;
    }

    /// <summary>
    /// Pulls <c>viewer_id</c> / <c>steam_id</c> / <c>steam_session_ticket</c> out of the
    /// decrypted msgpack body and stashes them in <c>HttpContext.Items[AuthFields.ContextKey]</c>.
    /// Lets the Steam handler read the auth tuple from a separate channel so action DTOs no
    /// longer need to inherit <c>BaseRequest</c> just so the handler can find the ticket.
    /// Failures (corrupt body, non-map root, missing keys) are silent on purpose: the auth
    /// handler will surface a more specific 401 reason than we can here.
    /// </summary>
    private static void TryStashAuthFields(HttpContext context, byte[] decryptedBytes)
    {
        try
        {
            var raw = MessagePackSerializer.Deserialize<Dictionary<object, object?>>(
                decryptedBytes,
                MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance));
            if (raw is null) return;

            context.Items[Security.SteamSessionAuthentication.AuthFields.ContextKey] =
                new Security.SteamSessionAuthentication.AuthFields
                {
                    ViewerId           = TryGetString(raw, "viewer_id"),
                    SteamId            = TryGetUlong(raw, "steam_id"),
                    SteamSessionTicket = TryGetString(raw, "steam_session_ticket"),
                };
        }
        catch
        {
            // Malformed body — auth handler will fail with its own diagnostic.
        }
    }

    private static string? TryGetString(Dictionary<object, object?> raw, string key) =>
        raw.TryGetValue(key, out var v) ? v as string : null;

    private static ulong TryGetUlong(Dictionary<object, object?> raw, string key)
    {
        if (!raw.TryGetValue(key, out var v) || v is null) return 0;
        return v switch
        {
            ulong u  => u,
            long l   => unchecked((ulong)l),
            int i    => unchecked((ulong)(long)i),
            uint ui  => ui,
            string s => ulong.TryParse(s, out var parsed) ? parsed : 0,
            _        => 0,
        };
    }

    /// <summary>
    /// Walks a parsed JSON tree into the plain CLR shape MessagePack-CSharp's contractless
    /// resolver understands: objects → <c>Dictionary&lt;string, object?&gt;</c>, arrays →
    /// <c>List&lt;object?&gt;</c>, scalars unboxed to their nearest primitive. Crucially, JSON
    /// objects that lacked a key DON'T get one in the dictionary — preserving "absent" as a
    /// distinct state from "null" all the way to the msgpack writer.
    /// </summary>
    internal static object? ConvertJsonTreeToPlainObject(JToken? token)
    {
        if (token is null || token.Type == JTokenType.Null) return null;
        return token.Type switch
        {
            JTokenType.Object => token.Children<JProperty>()
                .ToDictionary(p => p.Name, p => ConvertJsonTreeToPlainObject(p.Value)),
            JTokenType.Array => token.Children().Select(ConvertJsonTreeToPlainObject).ToList(),
            JTokenType.Integer => token.Value<long>(),
            JTokenType.Float => token.Value<double>(),
            JTokenType.String => token.Value<string>(),
            JTokenType.Boolean => token.Value<bool>(),
            JTokenType.Date => token.Value<DateTime>(),
            JTokenType.Bytes => token.Value<byte[]>(),
            _ => token.ToString()
        };
    }
}
