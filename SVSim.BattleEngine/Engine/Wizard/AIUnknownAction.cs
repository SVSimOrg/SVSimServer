namespace Wizard;

public class AIUnknownAction : AISituationInfo
{
	public AIUnknownAction(AIVirtualCard actor)
		: base(actor, null, null, AIOperationType.UNKNOWN)
	{
	}
}
