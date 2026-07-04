namespace Wizard;

public class AIVirtualTurnStartInfo : AIVirtualActionInfo
{
	public AIVirtualTurnStartInfo(AIVirtualCard leader)
		: base(leader, AIOperationType.TURNSTART, null)
	{
	}

	public override ulong GetHash()
	{
		return 99929uL;
	}
}
