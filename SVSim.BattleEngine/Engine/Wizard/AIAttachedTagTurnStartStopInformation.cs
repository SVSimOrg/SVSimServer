namespace Wizard;

public class AIAttachedTagTurnStartStopInformation : AITurnStartStopInformation
{
	private AIPlayTag _targetTag;

	public AIAttachedTagTurnStartStopInformation(AIAttachedTagStopPreprocessOption option)
		: base(option.TargetCard)
	{
		base.Type = AITagPreprocessInfoType.REMOVE_ATTACHED_TAG;
		_targetTag = option.TargetTag;
	}

	public override void ExecuteReservedAction(bool isAllyTurnEnd, AISituationInfo situation)
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
