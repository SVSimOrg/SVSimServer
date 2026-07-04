using Wizard;

public class SkillConditionReanimatedSelf : ISkillConditionChecker
{
	private BattleCardBase m_ownerCard;

	public SkillConditionReanimatedSelf(BattleCardBase ownerCard)
	{
		m_ownerCard = ownerCard;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return option.ReanimatedCards.Contains(m_ownerCard);
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}
}
