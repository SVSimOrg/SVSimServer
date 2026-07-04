using Wizard;

public class SkillConditionEvolvableTurn : ISkillConditionChecker
{
	public bool judgeFlag { get; private set; }

	public SkillConditionEvolvableTurn(string flag)
	{
		judgeFlag = flag == "true";
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool preExecutionCheck = false)
	{
		if (judgeFlag)
		{
			return IsEvolvableTurn(playerInfoPair.ReadOnlySelf.EvolveWaitTurnCount);
		}
		return !IsEvolvableTurn(playerInfoPair.ReadOnlySelf.EvolveWaitTurnCount);
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool preExecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}

	public bool IsEvolvableTurn(int evolveWaitTurnCount)
	{
		return evolveWaitTurnCount <= 0;
	}
}
