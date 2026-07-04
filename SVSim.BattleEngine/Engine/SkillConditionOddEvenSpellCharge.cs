using Wizard;

public class SkillConditionOddEvenSpellCharge : ISkillConditionChecker
{
	private BattleCardBase _ownerCard;

	private bool _judgeFlag;

	private bool _isOdd;

	public SkillConditionOddEvenSpellCharge(BattleCardBase ownerCard, string flag, bool isOdd)
	{
		_ownerCard = ownerCard;
		_judgeFlag = flag == "true";
		_isOdd = isOdd;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return CheckSpellCharge(option) == _judgeFlag;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}

	private bool CheckSpellCharge(SkillConditionCheckerOption option)
	{
		bool flag = _ownerCard.SpellChargeCount % 2 == 0;
		if (_isOdd)
		{
			return (flag ? option.AddChargeCount : (option.AddChargeCount - 1)) > 0;
		}
		return (flag ? (option.AddChargeCount - 1) : option.AddChargeCount) > 0;
	}
}
