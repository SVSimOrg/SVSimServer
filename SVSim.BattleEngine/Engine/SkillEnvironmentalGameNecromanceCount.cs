public class SkillEnvironmentalGameNecromanceCount : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.GameNecromanceCount;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerinfo, option);
	}
}
