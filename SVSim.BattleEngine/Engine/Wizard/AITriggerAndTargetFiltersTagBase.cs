using System.Collections.Generic;

namespace Wizard;

public class AITriggerAndTargetFiltersTagBase : AIScriptArgumentExpressions
{
	public List<AIScriptTokenBase> TriggerFilters { get; private set; }

	public List<AIScriptTokenBase> TargetFilters { get; protected set; }

	protected virtual int NON_FILTER_FIRST_OFFSET => 0;

	public AITriggerAndTargetFiltersTagBase(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		InitializeFilters();
	}

	protected void InitializeFilters()
	{
		if (_exprList.Count <= 0 || !AIPlayTagInitializingUtility.IsInitOfFilterSet(_exprList[0]))
		{
			return;
		}
		int num = -1;
		for (int i = 1; i < _exprList.Count; i++)
		{
			if (AIPlayTagInitializingUtility.IsInitOfFilterSet(_exprList[i]))
			{
				num = i;
				break;
			}
		}
		if (num <= 0)
		{
			TriggerFilters = GetFilters(_exprList.GetRange(0, _exprList.Count - NON_FILTER_FIRST_OFFSET));
			TargetFilters = null;
		}
		else
		{
			TriggerFilters = GetFilters(_exprList.GetRange(0, num));
			TargetFilters = GetFilters(_exprList.GetRange(num, _exprList.Count - NON_FILTER_FIRST_OFFSET - num));
		}
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AIVirtualCard triggerCard, AISituationInfo situation)
	{
		if (CheckTriggerLegal(triggerCard, tagOwner, playPtn, situation))
		{
			List<AIVirtualCard> targets = GetTargets(tagOwner, field, playPtn, situation);
			if (targets != null && targets.Count > 0)
			{
				RunTagMethod(targets, field, tagOwner, playPtn, situation);
			}
		}
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, List<AIVirtualCard> triggerCardList, AISituationInfo situation = null, bool isBlockDeadCard = true)
	{
		if (CheckTriggerLegal(triggerCardList, tagOwner, playPtn, situation, isBlockDeadCard))
		{
			List<AIVirtualCard> targets = GetTargets(tagOwner, field, playPtn, situation);
			if (targets != null && targets.Count > 0)
			{
				RunTagMethod(targets, field, tagOwner, playPtn, situation);
			}
		}
	}

	protected virtual void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (targets != null)
		{
			_ = targets.Count;
			_ = 0;
		}
	}

	protected virtual List<AIVirtualCard> GetTargets(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (TargetFilters == null || TargetFilters.Count <= 0)
		{
			return new List<AIVirtualCard> { tagOwner };
		}
		List<AIVirtualCard> candidateRange = GetCandidateRange(field);
		return GetFilteredTargets(candidateRange, tagOwner, playPtn, situation);
	}

	public bool CheckTriggerLegal(AIVirtualCard triggerCard, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		return AIFilteringUtility.CheckMatchTargetFiltering(triggerCard, null, TriggerFilters, playPtn, tagOwner, situation);
	}

	public bool CheckTriggerLegal(List<AIVirtualCard> triggers, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDeadCard)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(triggers, TriggerFilters, tagOwner, playPtn, situation, isBlockDeadCard);
		if (list == null || list.Count == 0)
		{
			return false;
		}
		return true;
	}

	protected virtual List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.MultipleFiltering(candidates, TargetFilters, tagOwner, playPtn, situation, isBlockDead);
	}

	protected virtual List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothClassAndInplayCards;
	}
}
