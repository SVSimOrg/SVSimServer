namespace Wizard;

public class AIAttachedTagLeaveStopInformation : AILeaveStopInformation
{
	private AIPlayTag _targetTag;

	public AIAttachedTagLeaveStopInformation(AIAttachedTagStopPreprocessOption option, AIVirtualCard provider)
		: base(option.TargetCard, provider)
	{
		base.Type = AITagPreprocessInfoType.REMOVE_ATTACHED_TAG;
		_targetTag = option.TargetTag;
	}

	protected override void StopMethod(AISituationInfo situation)
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
