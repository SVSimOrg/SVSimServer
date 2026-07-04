using System.Security.Cryptography;
using System.Text;

namespace SVSim.BattleNode.Wire;

/// <summary>
/// AES-256-CBC encrypt/decrypt for the node socket channel. Port of
/// Cryptographer.EncryptRJ256ForNode / DecryptRJ256ForNode in the decompilation.
/// Key is prepended to ciphertext (cleartext); IV is the first 16 chars of the key.
/// </summary>
public static class NodeCrypto
{
    /// <summary>Length of the ASCII key, in chars (AES-256 = 32 bytes = 32 ASCII chars).</summary>
    private const int KeyLength = 32;

    /// <summary>IV length, in chars. The node derives the IV from the first half of the key.</summary>
    private const int IvLength = KeyLength / 2;

    /// <summary>
    /// Generate a fresh 32-char key for server-initiated encryption.
    /// Calls <paramref name="randHexDigit"/> 32 times; the result is masked with
    /// <c>&amp; 0xF</c> so a misbehaving caller that returns a larger int still produces
    /// exactly one hex digit per iteration (the internal contract is "32 hex chars").
    /// The 32-char ASCII string is then base64-encoded and truncated to 32 chars.
    /// </summary>
    /// <remarks>
    /// Differs from the client's <c>Cryptographer.generateKeyString</c> in input shape:
    /// the client uses <c>Random.Next(0, 65535).ToString("x")</c> per iteration (1–4 hex
    /// chars each). The output distribution is therefore different, but both produce a
    /// valid 32-char UTF-8 AES-256 key — and the client never validates the server's key
    /// since the server is decrypt-only in practice. Server-initiated encryption (e.g.
    /// for <c>synchronize</c> pushes) uses this method.
    /// </remarks>
    public static string GenerateKey(Func<int> randHexDigit)
    {
        var sb = new StringBuilder(KeyLength);
        for (var i = 0; i < KeyLength; i++)
        {
            sb.Append((randHexDigit() & 0xF).ToString("x"));
        }
        var ascii = Encoding.ASCII.GetBytes(sb.ToString());
        return Convert.ToBase64String(ascii).Substring(0, KeyLength);
    }

    /// <summary>Encrypt: returns key + base64(AES-256-CBC(plain)).</summary>
    public static string EncryptForNode(string plaintext, string key)
    {
        if (key.Length != KeyLength)
            throw new ArgumentException($"Key must be exactly {KeyLength} chars, got {key.Length}", nameof(key));
        using var aes = BuildAes(key);
        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return key + Convert.ToBase64String(cipherBytes);
    }

    /// <summary>Decrypt: input[0..32] is key, input[32..] is base64(ciphertext).</summary>
    public static string DecryptForNode(string encrypted)
    {
        if (encrypted.Length < KeyLength)
            throw new ArgumentException($"Encrypted blob is shorter than the {KeyLength}-char key prefix", nameof(encrypted));
        var key = encrypted.Substring(0, KeyLength);
        var cipherBytes = Convert.FromBase64String(encrypted.Substring(KeyLength));
        using var aes = BuildAes(key);
        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    /// Configure an AES-256-CBC instance with the node's IV derivation (first
    /// <see cref="IvLength"/> chars of the key, UTF-8). Callers own disposal. Assumes
    /// <paramref name="key"/> is the <see cref="KeyLength"/>-char ASCII key the encrypt /
    /// decrypt path has already validated.
    /// </summary>
    /// <remarks>
    /// SECURITY (latent — do NOT "tidy" this into a cached key): the IV is derived from the key, so a
    /// fixed key reuses a fixed IV — the classic CBC IV-reuse weakness (equal plaintext prefixes →
    /// equal ciphertext prefixes). It is masked ONLY because every server-initiated send mints a fresh
    /// key via <see cref="GenerateKey"/>, so (key, IV) never repeats in practice. A future change that
    /// CACHES the session key would silently reintroduce the leak — derive a per-message random IV
    /// first. Related: <see cref="GenerateKey"/> base64-truncates a hex string, so the effective key
    /// entropy is well below what "AES-256" implies. We mirror the client's scheme deliberately; both
    /// are acceptable only because this is a localhost relay, not a hostile-network transport.
    /// </remarks>
    private static Aes BuildAes(string key)
    {
        var aes = Aes.Create();
        aes.KeySize = 256;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = Encoding.UTF8.GetBytes(key);
        aes.IV = Encoding.UTF8.GetBytes(key.Substring(0, IvLength));
        return aes;
    }
}
