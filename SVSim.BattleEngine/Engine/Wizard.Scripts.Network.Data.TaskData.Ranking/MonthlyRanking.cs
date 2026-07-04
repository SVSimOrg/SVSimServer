using LitJson;
using Wizard.Scripts.Network.Data.TableData.Ranking;

namespace Wizard.Scripts.Network.Data.TaskData.Ranking;

public class MonthlyRanking : Ranking
{
	public RankingPeriod period;

	public MonthlyRanking()
	{
		Initialize();
	}

	public MonthlyRanking(JsonData data)
		: base(data)
	{
		Initialize();
		if (data.Count >= 1)
		{
			period = new RankingPeriod(data["period"]);
		}
	}

	public new void Initialize()
	{
		period = new RankingPeriod();
	}
}
