using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SVSim.BattleNode.Wire;

file static class SocketIoJsonOptions
{
    internal static readonly JsonSerializerOptions EventNameOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}

/// <summary>
/// Socket.IO v2 packet. Wire form: <c>&lt;type&gt;&lt;N&gt;-&lt;ackId?&gt;[json-args]</c> where
/// <c>&lt;N&gt;-</c> appears only on binary types (5/6). For binary events/acks, the JSON contains
/// placeholders <c>{"_placeholder":true,"num":N}</c> that index into <see cref="BinaryAttachments"/>.
/// </summary>
public sealed class SocketIoFrame
{
    public SocketIoPacketType Type { get; }
    public int? AckId { get; }
    public int AttachmentCount { get; }
    public string? EventName { get; }
    public JsonElement[] RawArgs { get; }
    public IReadOnlyList<byte[]> BinaryAttachments { get; }

    public SocketIoFrame(
        SocketIoPacketType type,
        int? ackId,
        int attachmentCount,
        string? eventName,
        JsonElement[] rawArgs,
        IReadOnlyList<byte[]> binaryAttachments)
    {
        Type = type;
        AckId = ackId;
        AttachmentCount = attachmentCount;
        EventName = eventName;
        RawArgs = rawArgs;
        BinaryAttachments = binaryAttachments;
    }

    /// <summary>
    /// Parse the text portion of a SIO frame. For binary events the attachments arrive as separate
    /// WS frames after the text — the caller wires them up via <see cref="WithAttachments"/>.
    /// </summary>
    public static SocketIoFrame Parse(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            throw new ArgumentException("Empty SIO payload", nameof(raw));

        var typeChar = raw[0];
        if (typeChar < '0' || typeChar > '6')
            throw new ArgumentException($"Invalid SIO type char '{typeChar}'", nameof(raw));
        var type = (SocketIoPacketType)(typeChar - '0');
        var cursor = 1;

        var attachmentCount = 0;
        if (type is SocketIoPacketType.BinaryEvent or SocketIoPacketType.BinaryAck)
        {
            var dashIdx = raw.IndexOf('-', cursor);
            if (dashIdx < 0)
                throw new ArgumentException("Binary frame missing '-' separator", nameof(raw));
            if (!int.TryParse(raw.AsSpan(cursor, dashIdx - cursor), out attachmentCount))
                throw new ArgumentException("Binary frame attachment count not parseable", nameof(raw));
            cursor = dashIdx + 1;
        }

        // Namespace prefix (only present if '/' starts here, terminated by ','). v1 only
        // uses the default namespace; anything else is a protocol surprise we should
        // surface rather than silently route to default. If we ever support non-default
        // namespaces, capture into a property and let callers branch.
        if (cursor < raw.Length && raw[cursor] == '/')
        {
            var commaIdx = raw.IndexOf(',', cursor);
            var ns = commaIdx >= 0 ? raw.Substring(cursor, commaIdx - cursor) : raw.Substring(cursor);
            throw new ArgumentException(
                $"Socket.IO namespaces aren't supported — got '{ns}'. v1 expects default namespace only.",
                nameof(raw));
        }

        int? ackId = null;
        if (cursor < raw.Length && char.IsDigit(raw[cursor]))
        {
            var start = cursor;
            while (cursor < raw.Length && char.IsDigit(raw[cursor])) cursor++;
            if (!int.TryParse(raw.AsSpan(start, cursor - start), out var parsedAckId))
                throw new ArgumentException("SIO ack-id overflows int32", nameof(raw));
            ackId = parsedAckId;
        }

        var argsJson = cursor < raw.Length ? raw.Substring(cursor) : string.Empty;
        JsonElement[] allElements;
        if (string.IsNullOrEmpty(argsJson))
        {
            allElements = Array.Empty<JsonElement>();
        }
        else
        {
            using var doc = JsonDocument.Parse(argsJson);
            allElements = doc.RootElement.EnumerateArray().Select(el => el.Clone()).ToArray();
        }

        string? eventName = null;
        JsonElement[] rawArgs;
        if (type is SocketIoPacketType.Event or SocketIoPacketType.BinaryEvent && allElements.Length > 0)
        {
            eventName = allElements[0].GetString();
            // RawArgs excludes the leading event-name element so callers index args from 0.
            rawArgs = allElements.Length > 1 ? allElements[1..] : Array.Empty<JsonElement>();
        }
        else
        {
            rawArgs = allElements;
        }

        return new SocketIoFrame(type, ackId, attachmentCount, eventName, rawArgs, Array.Empty<byte[]>());
    }

    /// <summary>
    /// Return a new frame with the given binary attachments attached. Throws if the count doesn't
    /// match the header's declared attachment count.
    /// </summary>
    public SocketIoFrame WithAttachments(IReadOnlyList<byte[]> attachments)
    {
        if (attachments.Count != AttachmentCount)
            throw new ArgumentException(
                $"Attachment count mismatch: header says {AttachmentCount}, got {attachments.Count}");
        return new SocketIoFrame(Type, AckId, AttachmentCount, EventName, RawArgs, attachments);
    }

    /// <summary>
    /// Build a binary event frame for the given event name + binary attachments.
    /// The JSON args become <c>[eventName, {_placeholder:true,num:0}, {_placeholder:true,num:1}, ...]</c>.
    /// </summary>
    public static SocketIoFrame BinaryEventWithAttachments(string eventName, IReadOnlyList<byte[]> attachments)
    {
        // Build placeholders via the typed Nodes API; event name is stored separately.
        var placeholders = new JsonArray();
        for (var i = 0; i < attachments.Count; i++)
        {
            placeholders.Add(new JsonObject
            {
                ["_placeholder"] = true,
                ["num"] = i,
            });
        }

        return new SocketIoFrame(
            SocketIoPacketType.BinaryEvent,
            ackId: null,
            attachmentCount: attachments.Count,
            eventName: eventName,
            rawArgs: NodesToElements(placeholders),
            binaryAttachments: attachments);
    }

    /// <summary>Build an ack response whose single argument echoes the inbound frame's pubSeq
    /// (the client's ordered-delivery cursor — load-bearing, not a placeholder).</summary>
    public static SocketIoFrame AckResponse(int ackId, int pubSeqEcho)
    {
        var args = new JsonArray { pubSeqEcho };
        return new SocketIoFrame(
            SocketIoPacketType.Ack, ackId, 0, null, NodesToElements(args), Array.Empty<byte[]>());
    }

    /// <summary>
    /// Convert a <see cref="JsonArray"/> into the <see cref="JsonElement"/>[] that
    /// <see cref="RawArgs"/> stores. The current storage type is <see cref="JsonElement"/>
    /// because <see cref="Parse"/> produces it from <see cref="JsonDocument"/>; this helper
    /// keeps the typed-construction call sites without changing <see cref="RawArgs"/>.
    /// </summary>
    private static JsonElement[] NodesToElements(JsonArray nodes)
    {
        using var doc = JsonDocument.Parse(nodes.ToJsonString());
        return doc.RootElement.EnumerateArray().Select(el => el.Clone()).ToArray();
    }

    /// <summary>
    /// Encode to the wire form: (text payload, ordered list of binary attachments).
    /// The caller is responsible for sending the text frame first then each binary attachment frame.
    /// </summary>
    public (string Text, IReadOnlyList<byte[]> Binaries) Encode()
    {
        var sb = new StringBuilder();
        sb.Append((int)Type);
        if (Type is SocketIoPacketType.BinaryEvent or SocketIoPacketType.BinaryAck)
        {
            sb.Append(AttachmentCount).Append('-');
        }
        if (AckId.HasValue) sb.Append(AckId.Value);
        // Re-serialize args — for event/binary-event types, re-prepend the event name.
        bool hasJsonPayload = EventName is not null || RawArgs.Length > 0;
        if (hasJsonPayload)
        {
            sb.Append('[');
            if (EventName is not null)
            {
                sb.Append(JsonSerializer.Serialize(EventName, SocketIoJsonOptions.EventNameOptions));
                if (RawArgs.Length > 0) sb.Append(',');
            }
            for (var i = 0; i < RawArgs.Length; i++)
            {
                if (i > 0) sb.Append(',');
                sb.Append(RawArgs[i].GetRawText());
            }
            sb.Append(']');
        }
        return (sb.ToString(), BinaryAttachments);
    }
}
