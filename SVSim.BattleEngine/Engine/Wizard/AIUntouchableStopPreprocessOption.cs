namespace Wizard;

public class AIUntouchableStopPreprocessOption : AITagPreprocessCreationOptionBase
{
	public AIUntouchableStopPreprocessOption(AIVirtualCard targetCard)
		: base(AITagPreprocessInfoType.UNTOUCHABLE_STOP, targetCard)
	{
	}
}
