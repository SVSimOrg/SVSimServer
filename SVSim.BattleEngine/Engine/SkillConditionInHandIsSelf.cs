using System.Linq;
using Wizard;
using Wizard.Battle;

public class SkillConditionInHandIsSelf : ISkillConditionChecker
{
	private BattleCardBase m_ownerCard;

	public SkillConditionInHandIsSelf(BattleCardBase ownerCard)
	{
		m_ownerCard = ownerCard;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (option.InHandCard == null)
		{
			return false;
		}
		return option.InHandCard.Any((IReadOnlyBattleCardInfo s) => s == m_ownerCard);
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}
}
