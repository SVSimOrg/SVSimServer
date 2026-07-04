using Wizard;

public class SkillConditionDeckSelfSummonedSelf : ISkillConditionChecker
{
	private BattleCardBase _ownerCard;

	public SkillConditionDeckSelfSummonedSelf(BattleCardBase ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return option.DeckSelfSummonedCards.Contains(_ownerCard);
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}
}
