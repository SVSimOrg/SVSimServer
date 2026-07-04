namespace Wizard;

public class EarthRiteRecordContainer
{
	public AIVirtualCard ConsumedTarget { get; private set; }

	public int ConsumedStack { get; private set; }

	public EarthRiteRecordContainer(AIVirtualCard target, int stack)
	{
		ConsumedTarget = target;
		ConsumedStack = stack;
	}
}
