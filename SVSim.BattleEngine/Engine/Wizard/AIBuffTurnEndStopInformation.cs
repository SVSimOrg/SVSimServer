namespace Wizard;

public class AIBuffTurnEndStopInformation : AITurnEndStopInformation
{
	public AIBuffTurnEndStopInformation(AIVirtualCard card)
		: base(card)
	{
		base.Type = AITagPreprocessInfoType.STATUS_CHANGE_STOP;
	}

	protected override void RunMethod(bool isAllyTurnEnd, AIVirtualTurnEndInfo situation)
	{
		if (base.TargetCard != null)
		{
			base.TargetCard.RemoveTempBuff();
		}
	}
}
