namespace Wizard;

public class AIAttachedTagStopPreprocessOption : AITagPreprocessCreationOptionBase
{
	public AIPlayTag TargetTag;

	public AIAttachedTagStopPreprocessOption(AIVirtualCard targetCard)
		: base(AITagPreprocessInfoType.REMOVE_ATTACHED_TAG, targetCard)
	{
	}
}
