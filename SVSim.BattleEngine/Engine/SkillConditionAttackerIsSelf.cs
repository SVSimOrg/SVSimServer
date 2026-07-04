using Wizard;

public class SkillConditionAttackerIsSelf : ISkillConditionChecker
{
	private BattleCardBase m_ownerCard;

	public SkillConditionAttackerIsSelf(BattleCardBase ownerCard)
	{
		m_ownerCard = ownerCard;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return m_ownerCard == option.AttackerCard;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}
}
