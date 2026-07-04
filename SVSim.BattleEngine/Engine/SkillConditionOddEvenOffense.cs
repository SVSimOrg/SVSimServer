using Wizard;

public class SkillConditionOddEvenOffense : ISkillConditionChecker
{
	private BattleCardBase _ownerCard;

	private bool _judgeFlag;

	private bool _isOdd;

	public SkillConditionOddEvenOffense(BattleCardBase ownerCard, string flag, bool isOdd)
	{
		_ownerCard = ownerCard;
		_judgeFlag = flag == "true";
		_isOdd = isOdd;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return CheckOffense(option) == _judgeFlag;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}

	private bool CheckOffense(SkillConditionCheckerOption option)
	{
		if (_isOdd)
		{
			return _ownerCard.Atk % 2 == 1;
		}
		return _ownerCard.Atk % 2 == 0;
	}
}
