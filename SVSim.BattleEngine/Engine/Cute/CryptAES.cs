using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Cute;

public class CryptAES
{
	public static byte[] encrypt(byte[] byteSrc)
	{
		return EncryptRJ256Api(byteSrc);
	}

	public static string encrypt(string byteSrc)
	{
		return Convert.ToBase64String(EncryptRJ256Api(Encoding.UTF8.GetBytes(byteSrc)));
	}

	public static string encryptForNode(string src)
	{
		return EncryptRJ256ForNode(src);
	}

	public static byte[] decrypt(string src)
	{
		return DecryptRJ256Api(Convert.FromBase64String(src));
	}

	public static string decryptForNode(string src)
	{
		return DecryptRJ256ForNode(src);
	}

	private static byte[] EncryptRJ256Api(byte[] toEncryptData)
	{
		using RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.KeySize = 256;
		rijndaelManaged.BlockSize = 128;
		byte[] array = new byte[0];
		byte[] array2 = new byte[0];
		string s = Cryptographer.generateKeyString();
		string s2 = Certification.Udid.Replace("-", "").Substring(0, 16);
		array = Encoding.UTF8.GetBytes(s);
		array2 = Encoding.UTF8.GetBytes(s2);
		ICryptoTransform transform = rijndaelManaged.CreateEncryptor(array, array2);
		using MemoryStream memoryStream = new MemoryStream();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(toEncryptData, 0, toEncryptData.Length);
		cryptoStream.FlushFinalBlock();
		byte[] array3 = memoryStream.ToArray();
		byte[] array4 = new byte[array3.Length + array.Length];
		Array.Copy(array3, 0, array4, 0, array3.Length);
		Array.Copy(array, 0, array4, array3.Length, array.Length);
		rijndaelManaged.Clear();
		memoryStream.Flush();
		memoryStream.Close();
		cryptoStream.Flush();
		cryptoStream.Close();
		return array4;
	}

	public static string EncryptRJ256ForNode(string prm_text_to_encrypt)
	{
		using AesManaged aesManaged = new AesManaged();
		string text = Cryptographer.generateKeyString();
		string s = text.Substring(0, 16);
		aesManaged.BlockSize = 128;
		aesManaged.KeySize = 256;
		aesManaged.IV = Encoding.UTF8.GetBytes(s);
		aesManaged.Key = Encoding.UTF8.GetBytes(text);
		aesManaged.Mode = CipherMode.CBC;
		aesManaged.Padding = PaddingMode.PKCS7;
		byte[] bytes = Encoding.UTF8.GetBytes(prm_text_to_encrypt);
		using ICryptoTransform cryptoTransform = aesManaged.CreateEncryptor();
		byte[] inArray = cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length);
		aesManaged.Clear();
		return text + Convert.ToBase64String(inArray);
	}

	private static byte[] DecryptRJ256Api(byte[] sEncryptedString)
	{
		using RijndaelManaged rijndaelManaged = new RijndaelManaged();
		rijndaelManaged.Mode = CipherMode.CBC;
		rijndaelManaged.KeySize = 256;
		rijndaelManaged.BlockSize = 128;
		byte[] array = new byte[32];
		byte[] array2 = new byte[16];
		byte[] array3 = new byte[sEncryptedString.Length - array.Length];
		Array.Copy(sEncryptedString, 0, array3, 0, array3.Length);
		Array.Copy(sEncryptedString, sEncryptedString.Length - array.Length, array, 0, array.Length);
		array2 = Encoding.UTF8.GetBytes(Certification.Udid.Replace("-", "").Substring(0, 16));
		ICryptoTransform transform = rijndaelManaged.CreateDecryptor(array, array2);
		byte[] array4 = new byte[array3.Length];
		using MemoryStream memoryStream = new MemoryStream(array3);
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Read);
		cryptoStream.Read(array4, 0, array4.Length);
		rijndaelManaged.Clear();
		cryptoStream.Flush();
		cryptoStream.Close();
		memoryStream.Flush();
		memoryStream.Close();
		return array4;
	}

	public static string DecryptRJ256ForNode(string prm_text_to_decrypt)
	{
		using AesManaged aesManaged = new AesManaged();
		string text = prm_text_to_decrypt.Substring(0, 32);
		string s = text.Substring(0, 16);
		string s2 = prm_text_to_decrypt.Substring(32);
		aesManaged.BlockSize = 128;
		aesManaged.KeySize = 256;
		aesManaged.Key = Encoding.UTF8.GetBytes(text);
		aesManaged.IV = Encoding.UTF8.GetBytes(s);
		aesManaged.Mode = CipherMode.CBC;
		aesManaged.Padding = PaddingMode.PKCS7;
		byte[] array = Convert.FromBase64String(s2);
		using ICryptoTransform cryptoTransform = aesManaged.CreateDecryptor();
		byte[] bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
		string result = Encoding.UTF8.GetString(bytes);
		aesManaged.Clear();
		return result;
	}
}
