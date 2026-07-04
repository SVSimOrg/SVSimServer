using System.Collections.Generic;

namespace Wizard;

public class BarrierBonusPolicyCollection : AIPolicyCollection
{
	public float GetBarrierBonus(AIVirtualCard card)
	{
		if (card == null || card.IsDead || !base.HasPolicy)
		{
			return 0f;
		}
		float num = 0f;
		AIVirtualField selfField = card.SelfField;
		List<int> bestPlayPtn = selfField.BestPlayPtn;
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (aIPolicyData.CheckCondition(card, bestPlayPtn, selfField, null))
			{
				num += aIPolicyData.EvalArg(card, bestPlayPtn, selfField);
			}
		}
		return num * (float)card.BarrierInfoCollection.BarrierCount;
	}
}
