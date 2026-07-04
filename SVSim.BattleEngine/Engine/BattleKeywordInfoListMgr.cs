using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Wizard;
using Wizard.Battle.View;

public class BattleKeywordInfoListMgr : MonoBehaviour
{
	private const string KEYWORD = "KEYWORD";

	private static readonly string[] KEYWORD_PATTERNS = new string[2] { "\\[u\\]\\[(ffcd45|524522)\\](?<KEYWORD>.*?)\\[-\\]\\[/u\\]", "\\[b\\](?<KEYWORD>.*?)\\[/b\\]" };

	public static IList<string> GetKeywords(CardParameter cardParameter)
	{
		return GetKeywords(cardParameter.SkillDescription + cardParameter.EvoSkillDescription);
	}

	public static IList<string> GetKeywords(string skillDescription)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < KEYWORD_PATTERNS.Length; i++)
		{
			foreach (Match item in Regex.Matches(skillDescription, KEYWORD_PATTERNS[i]))
			{
				string value = item.Groups["KEYWORD"].Value;
				if (!list.Contains(value))
				{
					list.Add(value);
				}
			}
		}
		return list;
	}

	public static List<int> GetCardIdsInDesc(string desc)
	{
		string text = "KEYWORD";
		string pattern = "\\[card\\](?<" + text + ">.*?)\\[/card\\]";
		List<int> list = new List<int>();
		foreach (Match item2 in Regex.Matches(desc, pattern))
		{
			int item = int.Parse(item2.Groups[text].Value);
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}
}
