namespace Wizard;

public class AIAttachedTagTurnEndStopInformation : AITurnEndStopInformation
{
	private AIPlayTag _targetTag;

	public AIAttachedTagTurnEndStopInformation(AIAttachedTagStopPreprocessOption option, int defaultIncrement)
		: base(option.TargetCard, defaultIncrement)
	{
		base.Type = AITagPreprocessInfoType.REMOVE_ATTACHED_TAG;
		_targetTag = option.TargetTag;
	}

	protected override void RunMethod(bool isAllyTurnEnd, AIVirtualTurnEndInfo situation)
	{
		AIRemoveTagUtility.RemoveTemporaryAttachedTag(base.TargetCard, base.TargetCard.SelfField, _targetTag, situation);
	}

	public override AITagPreprocessCreationOptionBase CreateOptionInfoForOverride(AIVirtualCard overridedTarget)
	{
		return new AIAttachedTagStopPreprocessOption(overridedTarget)
		{
			TargetTag = _targetTag
		};
	}
}
