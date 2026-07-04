using System.Collections.Generic;

namespace Wizard;

public class AIBuffRush : AITriggerAndTargetFiltersTagBase
{
	public AIScriptTokenArgType SelectType { get; private set; }

	protected int SELECT_TYPE_OFFSET => 1;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIBuffRush(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		AIScriptTokenArgType selectType = AIScriptTokenArgType.NONE;
		if (IsLegalSelectType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], out selectType))
		{
			SelectType = selectType;
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0 && SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AISkillSimulationUtility.GiveSkillToAll(targets, field, AIScriptTokenArgType.RUSH);
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForFollowerOnly(candidates, tagOwner, base.TargetFilters, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}
}
