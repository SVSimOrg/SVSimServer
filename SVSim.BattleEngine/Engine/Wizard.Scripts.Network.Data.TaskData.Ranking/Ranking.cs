using System.Collections.Generic;
using LitJson;
using Wizard.Scripts.Network.Data.TableData.Ranking;

namespace Wizard.Scripts.Network.Data.TaskData.Ranking;

public class Ranking : HeaderData
{
	public List<RankingUser> rankingUserList;

	public Ranking()
	{
		Initialize();
	}

	public Ranking(JsonData data)
	{
		Initialize();
		if (data.Count >= 1)
		{
			for (int i = 0; i < data["ranking"].Count; i++)
			{
				RankingUser item = new RankingUser(data["ranking"][i]);
				rankingUserList.Add(item);
			}
		}
	}

	public void Initialize()
	{
		rankingUserList = new List<RankingUser>();
	}
}
