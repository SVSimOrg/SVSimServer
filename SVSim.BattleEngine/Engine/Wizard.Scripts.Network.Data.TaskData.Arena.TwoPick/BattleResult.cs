using System.Collections.Generic;
using LitJson;

namespace Wizard.Scripts.Network.Data.TaskData.Arena.TwoPick;

public class BattleResult
{
	public int winCount;

	public List<bool> resultList = new List<bool>();

	public BattleResult()
	{
	}

	public BattleResult(JsonData data)
	{
		JsonData jsonData = data["result_list"];
		resultList = new List<bool>();
		for (int i = 0; i < jsonData.Count; i++)
		{
			bool item = jsonData[i].ToInt() == 1;
			resultList.Add(item);
		}
		winCount = data["win_count"].ToInt();
	}
}
