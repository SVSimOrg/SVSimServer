using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Cute;

public class Cryptographer
{

	private static string encode_buf;

	private static Random cRandom = new Random();

	private static int random()
	{
		return cRandom.Next(1, 9);
	}

	public static string generateIvString()
	{
		string text = "";
		for (int i = 0; i < 32; i++)
		{
			text += $"{random()}";
		}
		return text;
	}

	public static string generateKeyString()
	{
		string text = "";
		for (int i = 0; i < 32; i++)
		{
			text += $"{cRandom.Next(0, 65535):x}";
		}
		return Convert.ToBase64String(Encoding.ASCII.GetBytes(text.ToString())).Substring(0, 32);
	}

	public static string encode(string dat)
	{
		int length = dat.Length;
		encode_buf = $"{length:x4}";
		foreach (char value in dat)
		{
			encode_buf += $"{random(),1:x}";
			encode_buf += $"{random(),1:x}";
			encode_buf += (char)(Convert.ToInt32(value) + 10);
			encode_buf += $"{random(),1:x}";
		}
		encode_buf += generateIvString();
		return encode_buf;
	}

	public static string decode(string dat)
	{
		if (dat == null || dat.Length < 4)
		{
			return dat;
		}
		int num = int.Parse(dat.Substring(0, 4), NumberStyles.AllowHexSpecifier);
		string text = "";
		int num2 = 2;
		string text2 = dat.Substring(4, dat.Length - 4);
		foreach (char value in text2)
		{
			if (num2 % 4 == 0)
			{
				text += (char)(Convert.ToInt32(value) - 10);
			}
			num2++;
			if (text.Length >= num)
			{
				break;
			}
		}
		return text;
	}

	public static string ComputeHash(string data)
	{
		if (string.IsNullOrEmpty(data))
		{
			return null;
		}
		SHA1CryptoServiceProvider sHA1CryptoServiceProvider = new SHA1CryptoServiceProvider();
		byte[] bytes = Encoding.UTF8.GetBytes(data);
		byte[] array = sHA1CryptoServiceProvider.ComputeHash(bytes);
		string text = "";
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			text += $"{b:x2}";
		}
		sHA1CryptoServiceProvider.Clear();
		return text;
	}

	public static string MakeMd5(string input)
	{
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] bytes = Encoding.UTF8.GetBytes(input + "r!I@ws8e5i=");
		byte[] array = mD5CryptoServiceProvider.ComputeHash(bytes);
		string text = "";
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			text += b.ToString("x2");
		}
		mD5CryptoServiceProvider.Clear();
		return text;
	}
}
