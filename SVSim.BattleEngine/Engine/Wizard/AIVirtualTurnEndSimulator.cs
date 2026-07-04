namespace Wizard;

public static class AIVirtualTurnEndSimulator
{
	public static void TurnEnd(AIVirtualTurnEndInfo situation, AIVirtualField field)
	{
		if (situation.ActionType != AIOperationType.TURNEND)
		{
			AIConsoleUtility.LogError("AIVirtualTurnEndSimulator:TurnEnd() error!! situation is not [TURNEND] ActionType!!!!!");
			return;
		}
		for (int i = 0; i < field.CardListSet.BothClassAndInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.BothClassAndInplayCards[i];
			if (!aIVirtualCard.IsDead)
			{
				aIVirtualCard.CallOnAfterBattleSimulation(situation);
			}
		}
		situation.ExecuteAllSkillProcess();
		field.TagPreprocessContainer.SimulateAllTurnEndInfo(situation.Actor.IsAlly, situation);
	}
}
