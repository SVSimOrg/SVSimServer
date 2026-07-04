public class SkillEnvironmentalGameUsedWhiteRitualCountFilter : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.GameUsedWhiteRitualCount;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerinfo, option);
	}
}
