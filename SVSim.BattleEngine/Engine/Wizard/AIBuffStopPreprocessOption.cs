namespace Wizard;

public class AIBuffStopPreprocessOption : AITagPreprocessCreationOptionBase
{
	public AIBuffStopPreprocessOption(AIVirtualCard target)
		: base(AITagPreprocessInfoType.STATUS_CHANGE_STOP, target)
	{
	}
}
