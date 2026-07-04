using System.Collections.Generic;

namespace Wizard;

public class AIBreakRecoverAttackableCount : AITriggerAndTargetFiltersTagBase
{
	protected override int NON_FILTER_FIRST_OFFSET => 0;

	public AIBreakRecoverAttackableCount(string text)
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
		foreach (AIVirtualCard target in targets)
		{
			target.RecoverAttackableCount();
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
