using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace SVSim.EmulatedEntrypoint.Security;

/// <summary>
/// Helper class for encrypting/decrypting requests bodies and responses to/from the game client.
/// </summary>
public static class Encryption
{
    private const int EncryptionKeySize = 256;
    private const int EncryptionBlockSize = 128;
    private const CipherMode EncryptionMode = CipherMode.CBC;
    private const int UdIdKeySize = 16;
    private const int KeyStringSize = 32;
    private const int EncodingValueOffset = 10;
    
    /// <summary>
    /// Encrypts an array of bytes using RJ256 with a subset of the user's UdId as the key.
    /// </summary>
    /// <param name="sourceData">the data to encrypt</param>
    /// <param name="udId">the UdId of the user this data is encrypted for</param>
    /// <returns>the encrypted bytes</returns>
    public static byte[] Encrypt(byte[] sourceData, string udId)
    {
        using (var rj = Aes.Create())
        {
            rj.KeySize = EncryptionKeySize;
            rj.Mode = EncryptionMode;
            rj.BlockSize = EncryptionBlockSize;
            string keyString = GenerateKeyString();
            string udIdKey = udId.Replace("-", string.Empty).Substring(0, UdIdKeySize);
            byte[] keyStringBytes = Encoding.UTF8.GetBytes(keyString);
            byte[] rgbIV = Encoding.UTF8.GetBytes(udIdKey);
            ICryptoTransform transform = rj.CreateEncryptor(keyStringBytes, rgbIV);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                {
                    cs.Write(sourceData);
                    cs.FlushFinalBlock();
                    byte[] encryptedResults = ms.ToArray();
                    byte[] encryptedResultsAndKey = new byte[encryptedResults.Length + keyStringBytes.Length];
                    Array.Copy(encryptedResults, 0, encryptedResultsAndKey, 0, encryptedResults.Length);
                    Array.Copy(keyStringBytes, 0, encryptedResultsAndKey, encryptedResults.Length, keyStringBytes.Length);
                    return encryptedResultsAndKey;
                }
            }
        }
    }

    /// <summary>
    /// Decrypts data that has been encrypted with the given UdId.
    /// </summary>
    /// <param name="encryptedData">Previously encrypted data</param>
    /// <param name="udId">The UdId previously used to encrypt the data</param>
    /// <returns>the decrypted bytes</returns>
    public static byte[] Decrypt(byte[] encryptedData, string udId)
    {
        using (var rj = Aes.Create())
        {
            rj.KeySize = EncryptionKeySize;
            rj.Mode = EncryptionMode;
            rj.BlockSize = EncryptionBlockSize;
            //rj.Padding = PaddingMode.None;
            byte[] rgbIv = Encoding.UTF8.GetBytes(udId.Replace("-", string.Empty).Substring(0, UdIdKeySize));
            byte[] keyBytes = new byte[KeyStringSize];
            byte[] encryptedValueBytes = new byte[encryptedData.Length - KeyStringSize];
            Array.Copy(encryptedData, encryptedData.Length - keyBytes.Length, keyBytes, 0, keyBytes.Length);
            Array.Copy(encryptedData, 0, encryptedValueBytes, 0, encryptedValueBytes.Length);
            ICryptoTransform transform = rj.CreateDecryptor(keyBytes, rgbIv);
            byte[] decryptedValueBytes = new byte[encryptedValueBytes.Length];
            rj.Key = keyBytes;
            return rj.DecryptCbc(encryptedValueBytes, rgbIv);
        }
    }

    public static string Encode(string sourceData)
    {
        int length = sourceData.Length;
        string encodeBuf = $"{length:x4}";
        foreach (char value in sourceData)
        {
            encodeBuf += $"{GetRandom(),1:x}";
            encodeBuf += $"{GetRandom(),1:x}";
            encodeBuf += ((char)(Convert.ToInt32(value) + EncodingValueOffset)).ToString();
            encodeBuf += $"{GetRandom(),1:x}";
        }

        encodeBuf += GenerateIvString();
        return encodeBuf;
    }

    public static string? Decode(string? encodedData)
    {
        if (encodedData == null || encodedData.Length < 4)
        {
            return encodedData;
        }
        int num = int.Parse(encodedData.Substring(0, 4), NumberStyles.AllowHexSpecifier);
        string text = "";
        int num2 = 2;
        foreach (char value in encodedData.Substring(4, encodedData.Length - 4))
        {
            if (num2 % 4 == 0)
            {
                text += ((char)(Convert.ToInt32(value) - EncodingValueOffset)).ToString();
            }
            num2++;
            if (text.Length >= num)
            {
                break;
            }
        }
        return text;
    }
    
    // TODO Clean this up and de-magic number it
    private static string GenerateIvString()
    {
        string text = "";
        for (int i = 0; i < KeyStringSize; i++)
        {
            text += $"{GetRandom()}";
        }
        return text;
    }
    
    private static string GenerateKeyString()
    {
        string text = "";
        for (int i = 0; i < KeyStringSize; i++)
        {
            text += $"{Random.Shared.Next(0, ushort.MaxValue):x}";
        }
        return Convert.ToBase64String(Encoding.ASCII.GetBytes(text)).Substring(0, KeyStringSize);
    }
    
    private static int GetRandom()
    {
        const int MinRandomValue = 1;
        const int MaxRandomValue = 9;
        return Random.Shared.Next(MinRandomValue, MaxRandomValue);
    }
}