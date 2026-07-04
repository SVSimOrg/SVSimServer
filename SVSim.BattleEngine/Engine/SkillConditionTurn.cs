using Wizard;

public class SkillConditionTurn : ISkillConditionChecker
{
	private readonly BattleCardBase _ownerCard;

	public bool judgeFlg { get; private set; }

	public SkillConditionTurn(string flg, BattleCardBase ownerCard)
	{
		judgeFlg = flg == "self";
		_ownerCard = ownerCard;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsTurn(_ownerCard);
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}

	public bool IsTurn(BattleCardBase ownerCard)
	{
		return ownerCard.SelfBattlePlayer.IsSelfTurn == judgeFlg;
	}
}
