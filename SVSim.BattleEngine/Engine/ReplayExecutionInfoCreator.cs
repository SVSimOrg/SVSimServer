using Wizard;

public class ReplayExecutionInfoCreator : NetworkExecutionInfoCreator
{
	public ReplayExecutionInfoCreator(SkillBase skill)
		: base(skill)
	{
	}

	public override bool CheckCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay, bool isSkipTarget = false)
	{
		isSkipTarget = !isPrePlay && IsSkipTargetAiSelect();
		return base.CheckCondition(playerInfoPair, option, isPrePlay, isSkipTarget);
	}

	public override bool IsSkipTargetAiSelect()
	{
		return IsSkipTargetSkill(_skill);
	}
}
