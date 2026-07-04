using System.Collections.Generic;

namespace Wizard;

public class AIFiltersArgument : AIScriptArgumentExpressions
{
	public List<AIScriptTokenBase> Filters { get; protected set; }

	protected virtual int NON_FILTER_FIRST_OFFSET => 0;

	public AIFiltersArgument(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count > NON_FILTER_FIRST_OFFSET)
		{
			InitializeFilter();
		}
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, List<AIVirtualCard> triggerCardList, AISituationInfo situation = null, bool isBlockDeadCard = true)
	{
		if (triggerCardList != null && triggerCardList.Count > 0)
		{
			List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(triggerCardList, Filters, tagOwner, playPtn, situation, isBlockDeadCard);
			if (list != null && list.Count > 0)
			{
				RunMethod(tagOwner, field, playPtn, situation);
			}
		}
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AIVirtualCard triggetCard, AISituationInfo situation = null)
	{
		if (AIFilteringUtility.CheckMatchTargetFiltering(triggetCard, null, Filters, playPtn, tagOwner, situation))
		{
			RunMethod(tagOwner, field, playPtn, situation);
		}
	}

	protected virtual void RunMethod(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
	}

	public virtual List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		List<AIVirtualCard> candidateRange = GetCandidateRange(field);
		return GetFilteredTargets(candidateRange, owner, playPtn, situation, isBlockDead);
	}

	public virtual List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.MultipleFiltering(candidates, Filters, tagOwner, playPtn, situation, isBlockDead);
	}

	protected virtual List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothClassAndInplayCards;
	}

	protected virtual void InitializeFilter()
	{
		if (NON_FILTER_FIRST_OFFSET > 0)
		{
			List<AIPolishConvertedExpression> range = _exprList.GetRange(0, _exprList.Count - NON_FILTER_FIRST_OFFSET);
			Filters = GetFilters(range);
		}
		else
		{
			Filters = GetFilters(_exprList);
		}
	}
}
