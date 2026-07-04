using System;
using System.Linq;
using Wizard;

public class SkillCemeteryFilter : ISkillConditionChecker
{
	private int m_count;

	private Func<int, int, bool> m_compareFunc;

	public SkillCemeteryFilter(int count, string op)
	{
		m_count = count;
		m_compareFunc = SkillCompareFuncCreator.Create(op);
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return CheckCemetery(playerInfoPair.ReadOnlySelf.SkillInfoCemeterys.Count());
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}

	public bool CheckCemetery(int cemeteryNum)
	{
		return m_compareFunc(cemeteryNum, m_count);
	}
}
