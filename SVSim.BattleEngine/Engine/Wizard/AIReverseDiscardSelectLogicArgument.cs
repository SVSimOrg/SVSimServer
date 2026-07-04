using System.Collections.Generic;

namespace Wizard;

public class AIReverseDiscardSelectLogicArgument : AISelectLogicArgumentBase
{
	public AIReverseDiscardSelectLogicArgument(List<string> args)
		: base(args)
	{
	}

	public override AIVirtualCard SelectSingleTarget(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		AIReverseEvaluateValueListOrder aIReverseEvaluateValueListOrder = new AIReverseEvaluateValueListOrder();
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			float value = aIVirtualCard.GetHandBonus(playPtn, situation, isIgnoreInFusion: false) + aIVirtualCard.GetDiscardedBonus(playPtn, situation, isIgnroeInBattle: true);
			aIReverseEvaluateValueListOrder.AddData(value, aIVirtualCard);
		}
		return aIReverseEvaluateValueListOrder.GetFirst();
	}

	public override List<AIVirtualCard> SelectMultipleSelectedTargets(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest)
	{
		AIConsoleUtility.Log("REVERSE_DISCARD_LOGIC は複数選択に未対応です");
		return null;
	}
}
