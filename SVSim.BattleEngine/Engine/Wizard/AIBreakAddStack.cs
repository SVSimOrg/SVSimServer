using System.Collections.Generic;

namespace Wizard;

public class AIBreakAddStack : AITriggerAndTargetFiltersTagBase
{
	private readonly int RITUAL_COUNT_OFFSET = 1;

	public AIPolishConvertedExpression RitualCount { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => RITUAL_COUNT_OFFSET;

	public AIBreakAddStack(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		RitualCount = _exprList[_exprList.Count - RITUAL_COUNT_OFFSET];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets == null || targets.Count <= 0)
		{
			AIConsoleUtility.LogError("AIBreakAddStack : RunTagMethod() Error!!! Missing Target");
		}
		else
		{
			AIWhiteRitualSimulationUtility.AddWhiteRitualSingle((int)RitualCount.EvalArg(tagOwner, playPtn, field, situation), targets);
		}
	}

	protected override List<AIVirtualCard> GetTargets(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> candidateRange = GetCandidateRange(field);
		return GetFilteredTargets(candidateRange, tagOwner, playPtn, situation);
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForWhiteRitualOnly(candidates, tagOwner, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
