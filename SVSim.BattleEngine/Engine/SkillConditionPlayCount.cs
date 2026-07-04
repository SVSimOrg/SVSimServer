using System;
using Wizard;

public class SkillConditionPlayCount : ISkillConditionChecker
{
	private readonly int _count;

	private readonly Func<int, int, bool> _compareFunc;

	public SkillConditionPlayCount(int count, string op)
	{
		_count = count;
		_compareFunc = SkillCompareFuncCreator.Create(op);
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return _compareFunc(playerInfoPair.ReadOnlySelf.GetCurrentTurnPlayCount(), _count);
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return _compareFunc(playerInfoPair.ReadOnlySelf.GetCurrentTurnPlayCount(), _count - 1);
	}
}
