using System;

namespace Wizard.Battle.View;

public class CardIconControl
{
	public static string[] SplitAndCompleteIconStr(string iconStr, string[] skillTypeStr)
	{
		int skillCount = skillTypeStr[0].Split(',').Length;
		int num = 0;
		if (skillTypeStr.Length > 1)
		{
			num = skillTypeStr[1].Split(',').Length;
		}
		if (iconStr == null)
		{
			string[] array = new string[2]
			{
				CompleteIconDefaultParam(skillCount),
				null
			};
			if (num > 0)
			{
				array[1] = CompleteIconDefaultParam(num);
			}
			return array;
		}
		iconStr = iconStr.Replace(" ", "");
		if (string.IsNullOrEmpty(iconStr))
		{
			string[] array2 = new string[2]
			{
				CompleteIconDefaultParam(skillCount),
				null
			};
			if (num > 0)
			{
				array2[1] = CompleteIconDefaultParam(num);
			}
			return array2;
		}
		return iconStr.Split(new string[1] { "//" }, StringSplitOptions.RemoveEmptyEntries);
	}

	public static string CompleteIconDefaultParam(int skillCount)
	{
		string text = string.Empty;
		for (int i = 0; i < skillCount; i++)
		{
			text = ((!string.IsNullOrEmpty(text)) ? (text + ",none") : "none");
		}
		return text;
	}
}
