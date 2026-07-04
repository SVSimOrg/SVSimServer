public class SkillEnvironmentalTurnWhenHealingCount : ISkillEnvironmentalFilter
{
	private readonly TurnPlayerInfo _turnPlayerInfo;

	protected readonly bool _isTextKeyword;

	public SkillEnvironmentalTurnWhenHealingCount(string option, bool isTextKeyword)
	{
		_turnPlayerInfo = new TurnPlayerInfo(option);
		_isTextKeyword = isTextKeyword;
	}

	public int Filtering(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return playerInfo.GetSpecificTurnWhenHealingCount(_turnPlayerInfo, _isTextKeyword);
	}

	public int FilteringPrePlay(IBattlePlayerReadOnlyInfo playerInfo, SkillConditionCheckerOption option)
	{
		return Filtering(playerInfo, option);
	}
}
