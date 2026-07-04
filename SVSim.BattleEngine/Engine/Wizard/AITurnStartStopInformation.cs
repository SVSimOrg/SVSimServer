namespace Wizard;

public class AITurnStartStopInformation : AITagPreprocessInformationBase
{
	public AITurnStartStopInformation(AIVirtualCard card)
		: base(card)
	{
	}

	public virtual void ExecuteReservedAction(bool isAllyTurnEnd, AISituationInfo situation)
	{
	}
}
