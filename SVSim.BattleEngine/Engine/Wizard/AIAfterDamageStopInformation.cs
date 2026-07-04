namespace Wizard;

public class AIAfterDamageStopInformation : AITagPreprocessInformationBase
{
	public AIAfterDamageStopInformation(AIVirtualCard card)
		: base(card)
	{
	}

	public bool ExecuteReservedAction(AIVirtualCard damagedCard)
	{
		if (damagedCard.IsSameCard(base.TargetCard))
		{
			StopMethod();
			return true;
		}
		return false;
	}

	protected virtual void StopMethod()
	{
	}
}
