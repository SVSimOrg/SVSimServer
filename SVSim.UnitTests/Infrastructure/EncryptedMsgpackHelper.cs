using System.Net.Http.Headers;
using MessagePack;
using MessagePack.Resolvers;
using SVSim.EmulatedEntrypoint.Constants;
using SVSim.EmulatedEntrypoint.Security;

namespace SVSim.UnitTests.Infrastructure;

/// <summary>
/// Builds a request that mirrors what the Unity client posts: msgpack-serialized body, AES-
/// encrypted with the viewer's UDID, plus the UDID/SID headers and Unity user-agent that the
/// translation middleware uses to recognize the wire format.
/// </summary>
internal static class EncryptedMsgpackHelper
{
    private static readonly MessagePackSerializerOptions ContractlessOpts =
        MessagePackSerializerOptions.Standard.WithResolver(ContractlessStandardResolver.Instance);

    /// <summary>
    /// Pairs a fresh UDID with a unique SID and registers the mapping on the running test host's
    /// <see cref="ShadowverseSessionService"/> via the SessionidMappingMiddleware path (a GET to
    /// any endpoint with both headers seeds the dict). Returns the pair for reuse on subsequent
    /// POSTs in the same test.
    /// </summary>
    public static (Guid Udid, string Sid) NewSessionIds()
    {
        var udid = Guid.NewGuid();
        var sid = Guid.NewGuid().ToString("N");
        return (udid, sid);
    }

    /// <summary>
    /// Builds a POST request to <paramref name="path"/> shaped like a real Unity client call:
    /// msgpack body (contractless dictionary), AES-encrypted with <paramref name="udid"/>, with
    /// the Unity user-agent and UDID/SID headers wired up. Caller sends it via
    /// <see cref="HttpClient.SendAsync(HttpRequestMessage)"/>.
    /// </summary>
    public static HttpRequestMessage BuildPost(
        string path,
        IReadOnlyDictionary<string, object?> body,
        Guid udid,
        string sid)
    {
        byte[] msgpackBody = MessagePackSerializer.Serialize(body, ContractlessOpts);
        byte[] encryptedBody = Encryption.Encrypt(msgpackBody, udid.ToString());

        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = new ByteArrayContent(encryptedBody),
        };
        request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        request.Headers.UserAgent.ParseAdd("UnityPlayer/2022.3.0 (test)");
        request.Headers.Add(NetworkConstants.UdidHeaderName, Encryption.Encode(udid.ToString()));
        request.Headers.Add(NetworkConstants.SessionIdHeaderName, sid);
        return request;
    }
}
