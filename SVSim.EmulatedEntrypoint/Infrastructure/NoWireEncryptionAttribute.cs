namespace SVSim.EmulatedEntrypoint.Infrastructure;

/// <summary>
/// Applied to a controller or action that speaks the same msgpack + standard envelope as the
/// rest of the game API but WITHOUT the AES wrapper. Used for endpoints hosted on
/// <c>shadowverse-portal.com</c> (deck builder, deck image), which use plaintext msgpack on the
/// wire — see <c>docs/api-spec/endpoints/deck-builder/*.md</c>. The translation middleware
/// detects the attribute and skips <c>Encryption.Decrypt</c> / <c>Encryption.Encrypt</c>; the
/// base64 wrap on the response and the msgpack ↔ JSON pivot stay the same.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public sealed class NoWireEncryptionAttribute : Attribute { }
