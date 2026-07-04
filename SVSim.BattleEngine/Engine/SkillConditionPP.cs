using System;
using Wizard;

public class SkillConditionPP : ISkillConditionChecker
{
	private BattleCardBase _ownerCard;

	private int _ppBorder;

	private Func<int, int, bool> _compareFunc;

	public int PpBorder => _ppBorder;

	public string Operator { get; private set; }

	public SkillConditionPP(BattleCardBase ownerCard, int ppBorder, string op)
	{
		_ownerCard = ownerCard;
		_ppBorder = ppBorder;
		_compareFunc = SkillCompareFuncCreator.Create(op);
		Operator = op;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		int arg = playerInfoPair.ReadOnlySelf.Pp + GetCardCost();
		bool flag = _compareFunc(arg, _ppBorder);
		return option.IsSkipPpCheck || flag;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		int pp = playerInfoPair.ReadOnlySelf.Pp;
		bool flag = _compareFunc(pp, _ppBorder);
		return option.IsSkipPpCheck || flag;
	}

	private int GetCardCost()
	{
		if (_ownerCard.PlayedCost < 0)
		{
			return _ownerCard.Cost;
		}
		return _ownerCard.PlayedCost;
	}
}
