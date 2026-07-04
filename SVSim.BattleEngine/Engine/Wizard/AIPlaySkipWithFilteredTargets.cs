using System.Collections.Generic;

namespace Wizard;

public class AIPlaySkipWithFilteredTargets : AIPlaySkipTagArgument
{
	public List<AIScriptTokenBase> Filters { get; protected set; }

	public AIPlaySkipWithFilteredTargets(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		InitializeFilter();
	}

	private void InitializeFilter()
	{
		Filters = GetFilters(_exprList);
	}

	protected List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		List<AIVirtualCard> candidateRange = GetCandidateRange(field);
		return GetFilteredTargets(candidateRange, owner, playPtn, situation, isBlockDead);
	}

	private List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, Filters, playPtn, situation, isBlockDead);
	}

	private List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.AllyInplayCards;
	}
}
