namespace Wizard;

public class AITurnEndStopInformation : AITagPreprocessInformationBase
{
	public int TurnEndDecrement { get; protected set; }

	public AITurnEndStopInformation(AIVirtualCard card, int defaultValue = 0)
		: base(card)
	{
		TurnEndDecrement = defaultValue;
	}

	public bool ExecuteReservedAction(bool isAllyTurnEnd, AIVirtualTurnEndInfo situation)
	{
		if (TurnEndDecrement <= 0)
		{
			RunMethod(isAllyTurnEnd, situation);
			return true;
		}
		TurnEndDecrement--;
		return false;
	}

	protected virtual void RunMethod(bool isAllyTurnEnd, AIVirtualTurnEndInfo situation)
	{
	}
}
