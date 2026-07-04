public class SkillEnvironmentalMaxEPFilter : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		if (playerinfo.EvolveWaitTurnCount <= 0)
		{
			return playerinfo.EpTotal;
		}
		return 0;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerinfo, option);
	}
}
