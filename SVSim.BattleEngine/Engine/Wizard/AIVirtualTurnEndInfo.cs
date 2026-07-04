namespace Wizard;

public class AIVirtualTurnEndInfo : AIVirtualActionInfo
{
	public AIVirtualTurnEndInfo(AIVirtualCard leader)
		: base(leader, AIOperationType.TURNEND, null)
	{
	}

	public override ulong GetHash()
	{
		return 9999991uL;
	}
}
