public class SkillEnvironmentalGameResonanceStartCountFilter : ISkillEnvironmentalFilter
{
	public int Filtering(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return playerinfo.GameResonanceStartCount;
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerinfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerinfo, option);
	}
}
