public class SkillEnvironmentalBpFilter : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return playerinfo.Bp;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerinfo, option);
	}
}
