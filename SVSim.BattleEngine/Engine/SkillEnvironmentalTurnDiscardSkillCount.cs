public class SkillEnvironmentalTurnDiscardSkillCount : ISkillEnvironmentalFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillEnvironmentalTurnDiscardSkillCount(string option)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
	}

	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.GetSpecificTurnSkillDiscardCount(_turnPlayerInfo);
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}
