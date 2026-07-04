using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cute;
using UnityEngine;

namespace Wizard;

public class SystemText
{
	private enum PARSE_TYPE
	{
		NORMAL,
		SINGULAR_PLURAL	}

	private string[] patternTbl = new string[2] { "{(?<VALUE>[^@{}]*?(?<INDEX>-?\\d+)[^@{}]*?)}", "{(?<VALUE>[^@{}]*?(?<INDEX>-?\\d+)[^@{}]*?)@(?<SINGULAR>[^@{}]+?)@(?<PLURAL>[^@{}]+?)}" };

	public Dictionary<string, string> TextDictionary { get; private set; }

	public string RegionCode { get; private set; }

	public SystemText()
	{
		RegionCode = CustomPreference.GetTextLanguage();
		SetPreInstallText();
	}

	private void SetPreInstallText()
	{
		TextDictionary = new Dictionary<string, string>();
		LoadAndParse("systemtext", TextDictionary);
		LoadAndParse("errortext", TextDictionary);
		LoadAndParse("errorheadertext", TextDictionary);
	}

	private void LoadAndParse(string tag, Dictionary<string, string> dic)
	{
		TextAsset textAsset = Resources.Load("Json/Text/" + tag) as TextAsset;
		LocalizeJson.Parse(dic, RegionCode, textAsset.ToString());
	}

	public string Get(string key)
	{
		return Get(key, enableDebugReturn: true);
	}

	public string Get(string key, bool enableDebugReturn)
	{
		if (TextDictionary.ContainsKey(key))
		{
			return TextDictionary[key];
		}
		return "";
	}

	public string Get(string id, params string[] values)
	{
		return Convert(Get(id), values);
	}

	public string Convert(string text, params string[] values)
	{
		text = Parse(PARSE_TYPE.NORMAL, text, values);
		text = Parse(PARSE_TYPE.SINGULAR_PLURAL, text, values);
		if (RegionCode == Global.LANG_TYPE.Kor.ToString())
		{
			text = HangulManager.ConvertRule(text);
		}
		return text;
	}

	private string Parse(PARSE_TYPE parseType, string text, params string[] values)
	{
		if (values == null)
		{
			return text;
		}
		foreach (Match item in Regex.Matches(text, patternTbl[(int)parseType]))
		{
			string value = item.Groups["INDEX"].Value;
			bool flag = value.Contains("-");
			int num = int.Parse(value);
			if (flag)
			{
				num *= -1;
			}
			if (values.Length <= num || values[num] == null)
			{
				continue;
			}
			string value2 = item.Groups["VALUE"].Value;
			value2 = value2.Replace(item.Groups["INDEX"].Value, (!flag) ? values[num] : "");
			if (parseType == PARSE_TYPE.SINGULAR_PLURAL)
			{
				if (!flag)
				{
					value2 += " ";
				}
				value2 += item.Groups[(int.Parse(values[num]) == 1) ? "SINGULAR" : "PLURAL"].Value;
			}
			text = text.Replace(item.Groups[0].Value, value2);
		}
		return text;
	}
}
