using System;
using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class Crossover
{
	public class PeriodData
	{
		public DateTime BeginTime = DateTime.MaxValue;

		public DateTime EndTime = DateTime.MinValue;
	}

	public PeriodData RankMatchPeriod = new PeriodData();

	public PeriodData FreeMatchPeriod = new PeriodData();

	public PeriodData GatheringPeriod = new PeriodData();

	public PeriodData PracticePeriod = new PeriodData();

	public List<int> CardSetIdList = new List<int>();

	public List<int> ReprintedBaseCardIds = new List<int>();

	public CrossoverRestrictedCard RestrictedCard = new CrossoverRestrictedCard();

	public static int AUTO_CREATE_MAIN_AND_NEUTRAL_MAX => 31;

	public static int AUTO_CREATE_SUB_AND_NEUTRAL_MAX => 16;

	public bool IsWithinAnyPeriod
	{
		get
		{
			if (!IsWithinGatheringPeriod && !IsWithinFreeMatchPeriod && !IsWithinRankMatchPeriod)
			{
				return IsWithinPracticePeriod;
			}
			return true;
		}
	}

	public bool IsWithinGatheringPeriod => IsWithinPeriod(GatheringPeriod);

	public bool IsWithinFreeMatchPeriod => IsWithinPeriod(FreeMatchPeriod);

	public bool IsWithinRankMatchPeriod => IsWithinPeriod(RankMatchPeriod);

	public bool IsWithinPracticePeriod => IsWithinPeriod(PracticePeriod);

	private bool IsWithinPeriod(PeriodData period)
	{
		DateTime nowTime_UTC = PlayerStaticData.UserTime.GetNowTime_UTC();
		if (nowTime_UTC >= period.BeginTime)
		{
			return nowTime_UTC <= period.EndTime;
		}
		return false;
	}
}
