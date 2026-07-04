public class SkillEnvironmentalTurnReturnSkillCount : ISkillEnvironmentalFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillEnvironmentalTurnReturnSkillCount(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.GetSpecificTurnSkillReturnCardCount(_turnPlayerInfo);
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}
