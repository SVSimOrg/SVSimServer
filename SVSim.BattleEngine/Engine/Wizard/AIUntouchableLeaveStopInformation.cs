namespace Wizard;

public class AIUntouchableLeaveStopInformation : AILeaveStopInformation
{
	public AIUntouchableLeaveStopInformation(AITagPreprocessCreationOptionBase option, AIVirtualCard provider)
		: base(option.TargetCard, provider)
	{
		base.Type = AITagPreprocessInfoType.UNTOUCHABLE_STOP;
	}

	protected override void StopMethod(AISituationInfo situation)
	{
		base.TargetCard.SubUntouchableCount();
	}
}
