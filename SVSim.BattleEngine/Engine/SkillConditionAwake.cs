using Wizard;

public class SkillConditionAwake : ISkillConditionChecker
{
	public bool judgeFlg { get; private set; }

	public SkillConditionAwake(string flg)
	{
		judgeFlg = flg == "true";
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		int ppTotal = playerInfoPair.ReadOnlySelf.PpTotal;
		if (judgeFlg)
		{
			return IsAwake(ppTotal);
		}
		return !IsAwake(ppTotal);
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}

	public static bool IsAwake(int pp)
	{
		return pp >= 7;
	}
}
