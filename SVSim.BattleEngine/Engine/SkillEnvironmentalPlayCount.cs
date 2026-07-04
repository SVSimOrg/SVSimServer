public class SkillEnvironmentalPlayCount : ISkillEnvironmentalFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	public SkillEnvironmentalPlayCount(string option)
	{
		_turnPlayerInfo = ((option == string.Empty) ? null : new TurnPlayerInfo(option));
	}

	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		if (_turnPlayerInfo == null)
		{
			return playerInfo.GetCurrentTurnPlayCount();
		}
		return playerInfo.GetSpecificTurnPlayCount(_turnPlayerInfo);
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		if (_turnPlayerInfo == null)
		{
			return playerInfo.GetCurrentTurnPlayCount() + 1;
		}
		return Filtering(playerInfo, option);
	}
}
