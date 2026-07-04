using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Cute;

public class Utility
{
	private enum CSVParserState
	{
		BEGIN_FIELD,
		PLAIN,
		QUOTE,
		END_FIELD
	}

	public class EzCrypt
	{

		private int numScale;

		private int[] table;

		private int key;

		public EzCrypt(int key)
		{
			numScale = ".SWK2hm4sVd8fOxZr0tqBncwX6P5k3HTCL_IzGYeMlyQFEbNvDjio9J7paRUAg1u-".Length;
			this.key = key;
			table = new int[128];
			for (int i = 0; i < 128; i++)
			{
				table[i] = ".SWK2hm4sVd8fOxZr0tqBncwX6P5k3HTCL_IzGYeMlyQFEbNvDjio9J7paRUAg1u-".IndexOf((char)i);
			}
		}
	}

	public class LeanSemaphore
	{
		private int max;

		public int value { get; private set; }

		public LeanSemaphore(int maxAcquire)
		{
			max = maxAcquire;
			Reset();
		}

		public bool TryWait()
		{
			if (value > 0)
			{
				value--;
				return true;
			}
			return false;
		}

		public void Post()
		{
			value++;
		}

		public void Reset()
		{
			value = max;
		}

		public void Reset(int maxAcquire)
		{
			max = maxAcquire;
			Reset();
		}
	}

	private static readonly string LangTypeKorString = Global.LANG_TYPE.Kor.ToString();

	public static ArrayList ConvertCSV(string csvText, bool removeTitle = true)
	{
		if (CustomPreference.GetTextLanguage() == LangTypeKorString)
		{
			HangulManager.ConvertRule(csvText);
		}
		int length = csvText.Length;
		int i;
		for (i = 0; Convert.ToInt32(csvText[i]) == 65279 && i < length; i++)
		{
		}
		ArrayList arrayList = new ArrayList();
		ArrayList arrayList2 = new ArrayList();
		int num = i;
		int num2 = i;
		bool flag = removeTitle;
		bool flag2 = false;
		CSVParserState cSVParserState = CSVParserState.BEGIN_FIELD;
		for (int j = i; j < length; j++)
		{
			switch (csvText[j])
			{
			case ',':
				switch (cSVParserState)
				{
				case CSVParserState.BEGIN_FIELD:
					arrayList2.Add("");
					break;
				case CSVParserState.PLAIN:
				case CSVParserState.END_FIELD:
				{
					string text = csvText.Substring(num, num2 + 1 - num);
					arrayList2.Add(flag2 ? text.Replace("\"\"", "\"") : text);
					flag2 = false;
					cSVParserState = CSVParserState.BEGIN_FIELD;
					break;
				}
				case CSVParserState.QUOTE:
					num2 = j;
					break;
				}
				break;
			case '\t':
			case ' ':
				switch (cSVParserState)
				{
				case CSVParserState.QUOTE:
					num2 = j;
					break;
				}
				break;
			case '\r':
				if (j < length - 1 && csvText[j + 1] == '\n')
				{
					j++;
				}
				goto case '\n';
			case '\n':
				switch (cSVParserState)
				{
				case CSVParserState.PLAIN:
				case CSVParserState.END_FIELD:
				{
					string text2 = csvText.Substring(num, num2 + 1 - num);
					arrayList2.Add(flag2 ? text2.Replace("\"\"", "\"") : text2);
					flag2 = false;
					if (!flag)
					{
						arrayList2.TrimToSize();
						arrayList.Add(arrayList2);
					}
					else
					{
						flag = false;
					}
					arrayList2 = new ArrayList(arrayList2.Count);
					cSVParserState = CSVParserState.BEGIN_FIELD;
					break;
				}
				case CSVParserState.BEGIN_FIELD:
					if (arrayList2.Count > 0)
					{
						arrayList2.Add("");
						if (!flag)
						{
							arrayList2.TrimToSize();
							arrayList.Add(arrayList2);
						}
						else
						{
							flag = false;
						}
						arrayList2 = new ArrayList(arrayList2.Count);
					}
					break;
				case CSVParserState.QUOTE:
					num2 = j;
					break;
				}
				break;
			case '"':
				switch (cSVParserState)
				{
				case CSVParserState.BEGIN_FIELD:
					num = j + 1;
					num2 = j;
					cSVParserState = CSVParserState.QUOTE;
					break;
				case CSVParserState.PLAIN:
				case CSVParserState.END_FIELD:
					throw new ApplicationException("不正なCSV");
				case CSVParserState.QUOTE:
					if (j < length - 1)
					{
						if (csvText[j + 1] == '"')
						{
							j++;
							flag2 = true;
							num2 = j;
						}
						else
						{
							cSVParserState = CSVParserState.END_FIELD;
						}
					}
					else
					{
						cSVParserState = CSVParserState.END_FIELD;
					}
					break;
				}
				break;
			default:
				switch (cSVParserState)
				{
				case CSVParserState.BEGIN_FIELD:
					num = j;
					num2 = j;
					cSVParserState = CSVParserState.PLAIN;
					break;
				case CSVParserState.END_FIELD:
					throw new ApplicationException("Could not parse CSV: extra character found outside quotation.");
				case CSVParserState.PLAIN:
				case CSVParserState.QUOTE:
					num2 = j;
					break;
				}
				break;
			}
		}
		switch (cSVParserState)
		{
		case CSVParserState.BEGIN_FIELD:
			if (arrayList2.Count > 0 && !flag)
			{
				arrayList2.Add("");
				arrayList2.TrimToSize();
				arrayList.Add(arrayList2);
			}
			break;
		case CSVParserState.PLAIN:
		case CSVParserState.END_FIELD:
			if (!flag)
			{
				arrayList2.Add(csvText.Substring(num, num2 + 1 - num));
				arrayList2.TrimToSize();
				arrayList.Add(arrayList2);
			}
			break;
		case CSVParserState.QUOTE:
			throw new ApplicationException("不正なCSV");
		}
		return arrayList;
	}

	public static List<string[]> ConvertCSV_Array(string csvText, bool removeTitle = true)
	{
		if (CustomPreference.GetTextLanguage() == LangTypeKorString)
		{
			HangulManager.ConvertRule(csvText);
		}
		int length = csvText.Length;
		int i;
		for (i = 0; Convert.ToInt32(csvText[i]) == 65279 && i < length; i++)
		{
		}
		int num = 1;
		for (int j = i; j < length; j++)
		{
			switch (csvText[j])
			{
			case ',':
				num++;
				continue;
			default:
				continue;
			case '\n':
				break;
			}
			break;
		}
		List<string[]> list = new List<string[]>();
		string[] array = new string[num];
		int num2 = 0;
		int num3 = i;
		int num4 = i;
		bool flag = removeTitle;
		bool flag2 = false;
		CSVParserState cSVParserState = CSVParserState.BEGIN_FIELD;
		for (int k = i; k < length; k++)
		{
			switch (csvText[k])
			{
			case ',':
				switch (cSVParserState)
				{
				case CSVParserState.BEGIN_FIELD:
					array[num2++] = "";
					break;
				case CSVParserState.PLAIN:
				case CSVParserState.END_FIELD:
				{
					string text = csvText.Substring(num3, num4 + 1 - num3);
					array[num2++] = (flag2 ? text.Replace("\"\"", "\"") : text);
					flag2 = false;
					cSVParserState = CSVParserState.BEGIN_FIELD;
					break;
				}
				case CSVParserState.QUOTE:
					num4 = k;
					break;
				}
				break;
			case '\t':
			case ' ':
				switch (cSVParserState)
				{
				case CSVParserState.QUOTE:
					num4 = k;
					break;
				}
				break;
			case '\r':
				if (k < length - 1 && csvText[k + 1] == '\n')
				{
					k++;
				}
				goto case '\n';
			case '\n':
				switch (cSVParserState)
				{
				case CSVParserState.PLAIN:
				case CSVParserState.END_FIELD:
				{
					string text2 = csvText.Substring(num3, num4 + 1 - num3);
					array[num2++] = (flag2 ? text2.Replace("\"\"", "\"") : text2);
					flag2 = false;
					if (!flag)
					{
						list.Add(array);
					}
					else
					{
						flag = false;
					}
					array = new string[num];
					num2 = 0;
					cSVParserState = CSVParserState.BEGIN_FIELD;
					break;
				}
				case CSVParserState.BEGIN_FIELD:
					if (num2 > 0)
					{
						array[num2++] = "";
						if (!flag)
						{
							list.Add(array);
						}
						else
						{
							flag = false;
						}
						array = new string[num];
						num2 = 0;
					}
					break;
				case CSVParserState.QUOTE:
					num4 = k;
					break;
				}
				break;
			case '"':
				switch (cSVParserState)
				{
				case CSVParserState.BEGIN_FIELD:
					num3 = k + 1;
					num4 = k;
					cSVParserState = CSVParserState.QUOTE;
					break;
				case CSVParserState.PLAIN:
				case CSVParserState.END_FIELD:
					throw new ApplicationException("不正なCSV");
				case CSVParserState.QUOTE:
					if (k < length - 1)
					{
						if (csvText[k + 1] == '"')
						{
							k++;
							flag2 = true;
							num4 = k;
						}
						else
						{
							cSVParserState = CSVParserState.END_FIELD;
						}
					}
					else
					{
						cSVParserState = CSVParserState.END_FIELD;
					}
					break;
				}
				break;
			default:
				switch (cSVParserState)
				{
				case CSVParserState.BEGIN_FIELD:
					num3 = k;
					num4 = k;
					cSVParserState = CSVParserState.PLAIN;
					break;
				case CSVParserState.END_FIELD:
					throw new ApplicationException("Could not parse CSV: extra character found outside quotation.");
				case CSVParserState.PLAIN:
				case CSVParserState.QUOTE:
					num4 = k;
					break;
				}
				break;
			}
		}
		switch (cSVParserState)
		{
		case CSVParserState.BEGIN_FIELD:
			if (num2 > 0 && !flag)
			{
				array[num2] = "";
				list.Add(array);
			}
			break;
		case CSVParserState.PLAIN:
		case CSVParserState.END_FIELD:
			if (!flag)
			{
				array[num2] = csvText.Substring(num3, num4 + 1 - num3);
				list.Add(array);
			}
			break;
		case CSVParserState.QUOTE:
			throw new ApplicationException("不正なCSV");
		}
		return list;
	}

	public static StringBuilder CreateHash(byte[] data)
	{
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] array = mD5CryptoServiceProvider.ComputeHash(data);
		mD5CryptoServiceProvider.Clear();
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array2 = array;
		foreach (byte b in array2)
		{
			stringBuilder.Append(b.ToString("x2"));
		}
		return stringBuilder;
	}

	public static StringBuilder CreateHash(string data)
	{
		return CreateHash(Encoding.UTF8.GetBytes(data));
	}

	public static string GetRuntimePlatform()
	{
		string result = "Windows";
		if (Application.platform == RuntimePlatform.Android)
		{
			result = "Android";
		}
		else if (Application.platform == RuntimePlatform.IPhonePlayer)
		{
			result = "iOS";
		}
		else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
		{
			result = "Mac";
		}
		return result;
	}
}
