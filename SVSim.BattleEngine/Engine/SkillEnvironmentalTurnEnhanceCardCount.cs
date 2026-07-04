public class SkillEnvironmentalTurnEnhanceCardCount : ISkillEnvironmentalFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillEnvironmentalTurnEnhanceCardCount(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.GetSpecificTurnEnhanceCardCount(_turnPlayerInfo);
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}
