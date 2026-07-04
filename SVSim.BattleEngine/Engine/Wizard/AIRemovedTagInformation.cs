namespace Wizard;

public class AIRemovedTagInformation
{
	public AIPlayTag Tag;

	public int CardIndex { get; private set; }

	public bool IsAlly { get; private set; }

	public AIRemovedTagInformation(AIVirtualCard remover, AIPlayTag tag)
	{
		CardIndex = remover.CardIndex;
		IsAlly = remover.IsAlly;
		Tag = tag;
	}
}
