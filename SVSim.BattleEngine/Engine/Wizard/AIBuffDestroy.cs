using System.Collections.Generic;

namespace Wizard;

public class AIBuffDestroy : AITriggerAndTargetFiltersTagBase
{
	private readonly int SELECT_TYPE_OFFSET = 1;

	public AIScriptTokenArgType SelectType { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AIBuffDestroy(string text)
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

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			if (SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.DestroyAll(targets, field, situation);
			}
			else if (SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AISkillSimulationUtility.DestroyRandom(targets, tagOwner, field, playPtn, situation);
			}
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
