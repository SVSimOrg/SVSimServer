using System.Text.Json;
using System.Text.Json.Nodes;
using SVSim.BattleNode.Wire;

namespace SVSim.BattleNode.Protocol;

/// <summary>
/// The shared envelope on every encrypted msg / synchronize frame. Body is
/// <see cref="IMsgBody"/> — either a typed body record (outbound) or a
/// <see cref="RawBody"/> (inbound).
/// </summary>
public sealed record MsgEnvelope(
    NetworkBattleUri Uri,
    long ViewerId,
    string Uuid,
    string? Bid,
    int RetryAttempt,
    EmitCategory Cat,
    long? PubSeq,
    long? PlaySeq,
    IMsgBody Body)
{
    // Bare-camelCase wire serialization, single-sourced in Wire.WireJsonOptions (shared with
    // EngineIoHandshake). Every wire key here is explicit via the manual ToJson layering below.
    private static readonly JsonSerializerOptions Options = WireJsonOptions.CamelCase;

    /// <summary>The fixed envelope wire keys, single-sourced. <see cref="ReservedEnvelopeKeys"/>,
    /// the <see cref="ToJson"/> writes, and the <see cref="FromJson"/> reads all draw from here, so
    /// the three encodings can't drift — adding a key in one place but not another (which would let a
    /// body key silently shadow an envelope field) is no longer possible.</summary>
    private static class Keys
    {
        public const string Uri = "uri";
        public const string ViewerId = "viewerId";
        public const string Uuid = "uuid";
        public const string Bid = "bid";
        public const string Try = "try";
        public const string Cat = "cat";
        public const string PubSeq = "pubSeq";
        public const string PlaySeq = "playSeq";
    }

    private static readonly HashSet<string> ReservedEnvelopeKeys = new()
    {
        Keys.Uri, Keys.ViewerId, Keys.Uuid, Keys.Bid, Keys.Try, Keys.Cat, Keys.PubSeq, Keys.PlaySeq,
    };

    public static string ToJson(MsgEnvelope env)
    {
        // Envelope fields MUST come before body fields on the wire. The client's
        // RealTimeNetworkAgent.SetNetworkInfo iterates the dict in insertion order and
        // clears _selfDeck on the "uri" key (via GameMgr.InitializeSelfInfo). Any body
        // field processed before "uri" is wiped before Matching.StartBattleLoad reads
        // it back. The prod wire emits envelope keys first; we must too.
        var result = new JsonObject();
        result[Keys.Uri] = env.Uri.ToString();
        result[Keys.ViewerId] = env.ViewerId;
        result[Keys.Uuid] = env.Uuid;
        result[Keys.Try] = env.RetryAttempt;
        result[Keys.Cat] = (int)env.Cat;
        if (env.Bid is not null) result[Keys.Bid] = env.Bid;
        if (env.PubSeq.HasValue) result[Keys.PubSeq] = env.PubSeq.Value;
        if (env.PlaySeq.HasValue) result[Keys.PlaySeq] = env.PlaySeq.Value;

        if (env.Body is RawBody raw)
        {
            // Inbound-echo path: flatten Entries to top-level keys.
            foreach (var (k, v) in raw.Entries)
            {
                if (ReservedEnvelopeKeys.Contains(k))
                    throw new ArgumentException(
                        $"RawBody key '{k}' collides with a reserved envelope field. " +
                        $"Move it to a typed field on MsgEnvelope.",
                        nameof(env));
                result[k] = ToJsonNode(v);
            }
        }
        else
        {
            // Typed body: serialize via [JsonPropertyName] attributes on the record,
            // then layer each field onto `result` after the envelope keys. DeepClone
            // because S.T.Json JsonNodes can only have one parent; reassigning a node
            // owned by `bodyNode` to `result` would throw without the clone.
            var bodyNode = (JsonObject)JsonSerializer.SerializeToNode(env.Body, env.Body.GetType(), Options)!;
            foreach (var prop in bodyNode)
            {
                result[prop.Key] = prop.Value?.DeepClone();
            }
        }

        return result.ToJsonString(Options);
    }

    /// <summary>
    /// Convert a boxed CLR value (as stored in <see cref="RawBody.Entries"/>) to a JsonNode.
    /// Explicit type switch on the runtime type — `JsonValue.Create(object?)` would create
    /// a `JsonValueCustomized&lt;object&gt;` that requires a TypeInfoResolver at serialize time
    /// (introduced in S.T.Json 8.0 source-gen mode).
    /// </summary>
    private static JsonNode? ToJsonNode(object? value) => value switch
    {
        null => null,
        string s => JsonValue.Create(s),
        bool b => JsonValue.Create(b),
        long l => JsonValue.Create(l),
        int i => JsonValue.Create(i),
        double d => JsonValue.Create(d),
        decimal m => JsonValue.Create(m),
        // Inbound-parsed nested objects come through as Dictionary<string, object?>; nested
        // arrays as List<object?>. FromJson is the source of these shapes — see ToObject.
        IDictionary<string, object?> dict => DictToJsonObject(dict),
        IReadOnlyList<object?> list => ListToJsonArray(list),
        _ => throw new InvalidOperationException(
            $"RawBody contains a value of unsupported type {value.GetType().FullName}. " +
            "Only primitives, nested dicts (object), and nested lists are recognized."),
    };

    private static JsonObject DictToJsonObject(IDictionary<string, object?> dict)
    {
        var obj = new JsonObject();
        foreach (var (k, v) in dict) obj[k] = ToJsonNode(v);
        return obj;
    }

    private static JsonArray ListToJsonArray(IReadOnlyList<object?> list)
    {
        var arr = new JsonArray();
        foreach (var v in list) arr.Add(ToJsonNode(v));
        return arr;
    }

    public static MsgEnvelope FromJson(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var uri = Enum.Parse<NetworkBattleUri>(root.GetProperty(Keys.Uri).GetString()!);
        var viewerId = root.GetProperty(Keys.ViewerId).GetInt64();
        var uuid = root.GetProperty(Keys.Uuid).GetString()!;
        var bid = root.TryGetProperty(Keys.Bid, out var bidEl) ? bidEl.GetString() : null;
        var retryAttempt = root.TryGetProperty(Keys.Try, out var tryEl) ? tryEl.GetInt32() : 0;
        var cat = root.TryGetProperty(Keys.Cat, out var catEl) ? (EmitCategory)catEl.GetInt32() : EmitCategory.Battle;
        var pubSeq = root.TryGetProperty(Keys.PubSeq, out var psEl) ? psEl.GetInt64() : (long?)null;
        var playSeq = root.TryGetProperty(Keys.PlaySeq, out var plsEl) ? plsEl.GetInt64() : (long?)null;

        var bodyDict = new Dictionary<string, object?>();
        foreach (var prop in root.EnumerateObject())
        {
            if (ReservedEnvelopeKeys.Contains(prop.Name)) continue;
            bodyDict[prop.Name] = ToObject(prop.Value);
        }

        return new MsgEnvelope(uri, viewerId, uuid, bid, retryAttempt, cat, pubSeq, playSeq, new RawBody(bodyDict));
    }

    internal static object? ToObject(JsonElement el) => el.ValueKind switch
    {
        JsonValueKind.String => el.GetString(),
        // Extracted to a helper because writing the conditional inline as
        //   el.TryGetInt64(out var l) ? l : el.GetDouble()
        // unifies the conditional's branches to the common implicit-convertible type. long→double
        // is implicit; so the result type collapses to double and the long value silently widens.
        // Downstream OfType<long> filters then drop the (now boxed-double) entries, which broke
        // the mulligan idxList extraction. Separate method returns object explicitly so each
        // branch boxes its own runtime type.
        JsonValueKind.Number => ParseNumber(el),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null => null,
        JsonValueKind.Array => el.EnumerateArray().Select(ToObject).ToList(),
        JsonValueKind.Object => el.EnumerateObject().ToDictionary(p => p.Name, p => ToObject(p.Value)),
        _ => el.GetRawText(),
    };

    private static object ParseNumber(JsonElement el)
    {
        if (el.TryGetInt64(out var l)) return l;
        return el.GetDouble();
    }
}
