using System.Collections;
using System.Collections.Generic;
using LitJson;

namespace Wizard.Scripts.Network.Data.TableData.Ranking;

public class RankingPeriodList : HeaderData
{
	public List<RankingPeriod> Master { get; private set; }

	public List<RankingPeriod> RankMatchClass { get; private set; }

	public List<RankingPeriod> TwoPick { get; private set; }

	public List<RankingPeriod> Sealed { get; private set; }

	public List<RankingPeriod> CrossoverMasterPoint { get; private set; }

	public List<RankingPeriod> CrossoverClassWin { get; private set; }

	public RankingPeriodList(JsonData data)
	{
		Master = ToPeriodList(data["master_point"]);
		RankMatchClass = ToPeriodList(data["rank_match"]);
		TwoPick = ToPeriodList(data["two_pick"]);
		Sealed = ToPeriodList(data["sealed"]);
		if (data.TryGetValue("crossover_master_point", out var value))
		{
			CrossoverMasterPoint = ToPeriodList(value);
		}
		if (data.TryGetValue("crossover_rank_match", out var value2))
		{
			CrossoverClassWin = ToPeriodList(value2);
		}
	}

	private static List<RankingPeriod> ToPeriodList(JsonData data)
	{
		List<RankingPeriod> list = new List<RankingPeriod>();
		foreach (JsonData item in (IEnumerable)data)
		{
			list.Add(new RankingPeriod(item));
		}
		return list;
	}
}
