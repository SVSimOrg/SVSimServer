using System.Linq;
using Wizard;

public class SkillConditionAvarice : ISkillConditionChecker
{
	public bool IsTurnDrawCountGreaterThanTwo { get; private set; }

	public SkillConditionAvarice(string flag)
	{
		IsTurnDrawCountGreaterThanTwo = flag == "true";
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		int turnDrawCount = playerInfoPair.ReadOnlySelf.SkillInfoTurnDrawCards.Count();
		bool isForceAvarice = playerInfoPair.ReadOnlySelf.SkillInfoClass.SkillApplyInformation.IsForceAvarice;
		if (IsTurnDrawCountGreaterThanTwo)
		{
			if (!isForceAvarice)
			{
				return IsAvarice(turnDrawCount);
			}
			return true;
		}
		if (!isForceAvarice)
		{
			return !IsAvarice(turnDrawCount);
		}
		return false;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}

	public static bool IsAvarice(int turnDrawCount)
	{
		return turnDrawCount >= 2;
	}
}
