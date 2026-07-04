public class SkillEnvironmentalTurnEvolveCount : ISkillEnvironmentalFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillEnvironmentalTurnEvolveCount(string option)
	{
		_turnPlayerInfo = ((option == string.Empty) ? null : new TurnPlayerInfo(option));
	}

	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		if (_turnPlayerInfo == null)
		{
			return playerInfo.GetCurrentTurnEvolveCount();
		}
		return playerInfo.GetSpecificTurnEvolveCount(_turnPlayerInfo);
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}
