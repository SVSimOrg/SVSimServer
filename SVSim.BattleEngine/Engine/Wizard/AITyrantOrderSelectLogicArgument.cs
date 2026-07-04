using System.Collections.Generic;

namespace Wizard;

public class AITyrantOrderSelectLogicArgument : AISelectLogicArgumentBase
{
	public override AIScriptTokenArgType LogicType => AIScriptTokenArgType.TYRANT_ORDER_LOGIC;

	public AITyrantOrderSelectLogicArgument(List<string> args)
		: base(args)
	{
	}

	public override AIVirtualCard SelectSingleTarget(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		AIConsoleUtility.LogError("AITyrantOrderSelectLogicArgument: This logic is cant use single target select");
		return null;
	}

	public override List<AIVirtualCard> SelectMultipleSelectedTargets(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>(candidates);
		list.RemoveAll((AIVirtualCard c) => c.IsLeader || c.IsDead || c.IsIndependent || c.IsSneak || c.IsUntouchable);
		if (list.Count == selectCount)
		{
			return list;
		}
		if (list.Count < selectCount)
		{
			AIConsoleUtility.LogError("AITyrantOrderSelectLogicArgument:SelectMultipleSelectedTargets() - Targets dont meet the minimum number.");
			return null;
		}
		AIReverseEvaluateValueListOrder aIReverseEvaluateValueListOrder = new AIReverseEvaluateValueListOrder();
		for (int num = 0; num < list.Count; num++)
		{
			AIVirtualCard aIVirtualCard = list[num];
			float value = ((!aIVirtualCard.IsForceTargeting) ? ((!aIVirtualCard.IsAmulet) ? AISimulationRemovalUtility.CalculateRemovalValue(aIVirtualCard, field, playPtn, situation, AIRemovalType.Destroy, null) : 0f) : float.MinValue);
			aIReverseEvaluateValueListOrder.AddData(value, aIVirtualCard);
		}
		return aIReverseEvaluateValueListOrder.GetCardList(selectCount);
	}
}
