using Wizard;

public class SkillConditionBeAttackedIsDestroy : ISkillConditionChecker
{
	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		BattleCardBase attackTargetCard = option.AttackTargetCard;
		if (attackTargetCard != null)
		{
			if (attackTargetCard.IsDead)
			{
				return !attackTargetCard.DeathTypeInfo.BanishDestroy;
			}
			return false;
		}
		return false;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}
}
