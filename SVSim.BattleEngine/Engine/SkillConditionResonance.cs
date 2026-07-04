using System.Linq;
using Wizard;

public class SkillConditionResonance : ISkillConditionChecker
{
	public bool judgeFlg { get; private set; }

	public SkillConditionResonance(string flg)
	{
		judgeFlg = flg == "true";
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsResonance(playerInfoPair.ReadOnlySelf.SkillInfoDeckCards.Count()) == judgeFlg;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}

	public static bool IsResonance(int count)
	{
		return count % 2 == 0;
	}
}
