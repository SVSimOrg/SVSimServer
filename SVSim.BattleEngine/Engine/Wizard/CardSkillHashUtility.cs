using System.Collections.Generic;

namespace Wizard;

public static class CardSkillHashUtility
{

	public static List<string> GetSingleSkillHashStringList(SkillCollectionBase skillCollection)
	{
		List<string> list = null;
		foreach (SkillBase item in skillCollection)
		{
			list = AIParamQuery.AddElementToList(GetSingleSkillBaseHash(item).ToString(), list);
		}
		return list;
	}

	public static ulong GetSingleSkillBaseHash(SkillBase skill)
	{
		ulong num = 0uL;
		SkillCreator.SkillBuildInfo buildInfo = skill.SkillPrm.buildInfo;
		num += (ulong)((long)buildInfo._type.GetHashCode() * 6007L);
		num += (ulong)((long)buildInfo._timing.GetHashCode() * 17L);
		num += (ulong)((long)buildInfo._option.GetHashCode() * 47L);
		string text = "";
		for (int i = 0; i < buildInfo._parsedConditionNew.Count; i++)
		{
			string text2 = buildInfo._parsedConditionNew[i];
			if (i > 0)
			{
				text += "&";
			}
			text += text2;
		}
		num += (ulong)((long)text.GetHashCode() * 107L);
		string text3 = "";
		for (int j = 0; j < buildInfo._parsedTargetNew.Count; j++)
		{
			string text4 = buildInfo._parsedTargetNew[j];
			if (j > 0)
			{
				text3 += "&";
			}
			text3 += text4;
		}
		return num + (ulong)((long)text3.GetHashCode() * 227L);
	}
}
