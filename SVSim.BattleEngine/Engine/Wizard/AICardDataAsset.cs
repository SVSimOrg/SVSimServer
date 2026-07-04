using System.Collections.Generic;

namespace Wizard;

public class AICardDataAsset
{
	public List<AIPlayTagAsset> TagList;

	public int CardID { get; private set; }

	public int CardNum { get; private set; }

	public bool UseCommon { get; private set; }

	public string CardName { get; private set; }

	public string BattleBonus { get; private set; }

	public string PlayBonus { get; private set; }

	public string Priority { get; private set; }

	public AICardDataAsset(string[] columns)
	{
		int num = 0;
		CardID = AIScriptParser.ParseInt(columns[num++]);
		if (columns[num++] == "〇")
		{
			UseCommon = true;
		}
		else
		{
			UseCommon = false;
		}
		CardName = columns[num++];
		CardNum = AIScriptParser.ParseInt(columns[num++]);
		BattleBonus = columns[num++];
		PlayBonus = columns[num++];
		Priority = columns[num++];
		TagList = new List<AIPlayTagAsset>();
		int num2 = (columns.Length - num - 1) / 3;
		for (int i = 0; i < num2; i++)
		{
			AIPlayTagAsset item = new AIPlayTagAsset
			{
				Type = columns[num++],
				Arg = columns[num++],
				Condition = columns[num++]
			};
			TagList.Add(item);
		}
	}
}
