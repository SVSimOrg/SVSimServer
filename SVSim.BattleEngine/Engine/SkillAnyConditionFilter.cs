using System.Collections.Generic;
using System.Linq;
using Wizard;

public class SkillAnyConditionFilter
{
	public List<ConditionSkillFilterCollection> Filters { get; private set; }

	public string Text { get; protected set; }

	public static List<string> GetFiltersString(string text)
	{
		text = text.Substring(1, text.Length - 2);
		List<string> list = new List<string>();
		string text2 = string.Empty;
		int num = 0;
		while (text.Length > 0)
		{
			switch (text[0])
			{
			case '|':
				if (num == 0)
				{
					list.Add(text2);
					text2 = string.Empty;
				}
				break;
			case '{':
				num++;
				text2 += text[0];
				break;
			case '}':
				num--;
				text2 += text[0];
				break;
			default:
				text2 += text[0];
				break;
			}
			text = text.Substring(1, text.Length - 1);
		}
		if (text2 != string.Empty)
		{
			list.Add(text2);
		}
		return list;
	}

	public SkillAnyConditionFilter(string text, List<string> conditions, BattleCardBase ownerCard, SkillBase skill)
	{
		Text = text;
		Filters = new List<ConditionSkillFilterCollection>();
		foreach (string condition in conditions)
		{
			ConditionSkillFilterCollection conditionSkillFilterCollection = new ConditionSkillFilterCollection();
			List<SkillFilterCreator.ContentInfo> retOldInfos = new List<SkillFilterCreator.ContentInfo>();
			List<string> retNewInfos = new List<string>();
			SkillCreator.ParseCondition(condition, ref retOldInfos, ref retNewInfos);
			SkillCreator.SetupSkillConditionOld(conditionSkillFilterCollection, retOldInfos, ownerCard, skill);
			for (int i = 0; i < retNewInfos.Count; i++)
			{
				conditionSkillFilterCollection.VariableCompareFilter.Add(new SkillVariableComareFilter(retNewInfos[i]));
			}
			Filters.Add(conditionSkillFilterCollection);
		}
	}

	public bool Filtering(BattlePlayerReadOnlyInfoPair playerInfoPair, BattleCardBase ownerCard, SkillConditionCheckerOption checkerOption, SkillOptionValue optionValue, bool isPrePlay, SkillBase skill, bool isSkipTarget)
	{
		return Filters.Any((ConditionSkillFilterCollection f) => f.Filtering(playerInfoPair, ownerCard, checkerOption, optionValue, isPrePlay, skill, isSkipTarget));
	}
}
