using Wizard;

public class SkillConditionBeAttackedIsSelf : ISkillConditionChecker
{
	private BattleCardBase m_ownerCard;

	public SkillConditionBeAttackedIsSelf(BattleCardBase ownerCard)
	{
		m_ownerCard = ownerCard;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return m_ownerCard == option.AttackTargetCard;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}
}
