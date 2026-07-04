using Wizard;

public class SingleExecutionInfoCreator : ExecutionInfoCreatorBase
{
	public SingleExecutionInfoCreator(SkillBase skill)
		: base(skill)
	{
	}

	public override bool CheckCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool isPrePlay, bool isSkipTargetAiSelect = false)
	{
		isSkipTargetAiSelect = !isPrePlay && IsSkipTargetAiSelect();
		return base.CheckCondition(playerInfoPair, option, isPrePlay, isSkipTargetAiSelect);
	}

	public override bool IsSkipTargetAiSelect()
	{
		return IsSkipTargetSkill(_skill);
	}
}
