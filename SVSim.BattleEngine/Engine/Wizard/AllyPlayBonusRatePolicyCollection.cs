using System.Collections.Generic;

namespace Wizard;

public class AllyPlayBonusRatePolicyCollection : AIPolicyCollection
{
	public float GetAllyPlayBonusRate(AIVirtualCard playCard, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasPolicy)
		{
			return 1f;
		}
		float num = 1f;
		AIVirtualField selfField = playCard.SelfField;
		AIVirtualCard allyClass = selfField.AllyClass;
		for (int i = 0; i < base.PolicyList.Count; i++)
		{
			AIPolicyData aIPolicyData = base.PolicyList[i];
			if (aIPolicyData.Argument is AIOtherPlayBonusRate && aIPolicyData.CheckCondition(allyClass, playPtn, selfField, situation))
			{
				AIOtherPlayBonusRate aIOtherPlayBonusRate = aIPolicyData.Argument as AIOtherPlayBonusRate;
				num *= aIOtherPlayBonusRate.GetBonusRate(allyClass, playCard, playPtn, situation);
			}
		}
		return num;
	}
}
