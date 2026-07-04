using System.Collections.Generic;

namespace Wizard;

public class AllyPlayBonusPolicyCollection : AIPolicyCollection
{
	public float GetAllyPlayBonus(AIVirtualCard playCard, List<int> playPtn, AISituationInfo situation, ref float currentUseMinValue)
	{
		if (!base.HasPolicy)
		{
			return 0f;
		}
		float num = 0f;
		AIVirtualField selfField = playCard.SelfField;
		AIVirtualCard allyClass = selfField.AllyClass;
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (aIPolicyData.PolicyType != AIPolicyType.AllyPlayBonus || !(aIPolicyData.Argument is AIOtherPlayBonus) || !aIPolicyData.CheckCondition(allyClass, playPtn, selfField, situation))
			{
				continue;
			}
			AIOtherPlayBonus aIOtherPlayBonus = aIPolicyData.Argument as AIOtherPlayBonus;
			if (!AIFilteringUtility.CheckMatchTargetFiltering(playCard, null, aIOtherPlayBonus.Filters, playPtn, allyClass, situation))
			{
				continue;
			}
			if (aIOtherPlayBonus.IsUseMin)
			{
				float evaluateValue = aIOtherPlayBonus.GetEvaluateValue(allyClass, playPtn, situation);
				if (EnemyAI.IsLargerThan(currentUseMinValue, evaluateValue))
				{
					currentUseMinValue = evaluateValue;
				}
			}
			else
			{
				num += aIOtherPlayBonus.GetEvaluateValue(allyClass, playPtn, situation);
			}
		}
		return num;
	}
}
