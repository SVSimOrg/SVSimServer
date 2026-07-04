public class SkillEnvironmentalShortageDeckLose : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		if (!playerInfo.IsShortageDeckLose)
		{
			return 0;
		}
		return 1;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		if (!playerInfo.IsShortageDeckLose)
		{
			return 0;
		}
		return 1;
	}
}
