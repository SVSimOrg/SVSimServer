namespace Wizard;

public class AITagPreprocessCreationOptionBase
{
	public AITagPreprocessInfoType PreprocessInfoType { get; protected set; }

	public AIVirtualCard TargetCard { get; private set; }

	public AITagPreprocessCreationOptionBase(AITagPreprocessInfoType infoType, AIVirtualCard targetCard)
	{
		PreprocessInfoType = infoType;
		TargetCard = targetCard;
	}
}
