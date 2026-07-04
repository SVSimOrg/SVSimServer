using System.Collections.Generic;

namespace Wizard;

public class AIHealEvo : AITriggerAndTargetFiltersTagBase
{
	public AIHealEvo(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets == null || targets.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard card = targets[i];
			if (tagOwner.IsSameCard(card) && !tagOwner.IsEvolution)
			{
				AIAutoEvolutionSimulationUtility.AutoEvolveSingle(tagOwner, field, situation);
			}
		}
	}
}
