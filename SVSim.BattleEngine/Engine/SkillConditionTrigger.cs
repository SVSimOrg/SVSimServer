using System.Linq;
using Wizard;

public class SkillConditionTrigger : ISkillConditionChecker
{
	private BattleCardBase ownerCard;

	public bool judgeFlg { get; private set; }

	public SkillConditionTrigger(BattleCardBase ownerCard, string flg)
	{
		judgeFlg = flg == "true";
		this.ownerCard = ownerCard;
	}

	public bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (ownerCard.Skills.Where((SkillBase s) => s is Skill_trigger).Count() <= 0)
		{
			return false;
		}
		return ownerCard.SkillApplyInformation.IsTrigger == judgeFlg;
	}

	public bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		return IsRight(playerInfoPair, option);
	}
}
