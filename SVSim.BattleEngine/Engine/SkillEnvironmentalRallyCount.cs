public class SkillEnvironmentalRallyCount : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.RallyCount;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.RallyCount;
	}
}
