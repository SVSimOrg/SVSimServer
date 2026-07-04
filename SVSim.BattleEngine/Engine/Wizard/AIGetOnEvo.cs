using System.Collections.Generic;

namespace Wizard;

public class AIGetOnEvo : AITriggerAndTargetFiltersTagBase
{
	private AIScriptTokenArgType _selectType;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIGetOnEvo(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_selectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1], base.LegalSelectTypes);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		AIAutoEvolutionSimulationUtility.AutoEvolution(field, targets, playPtn, situation, _selectType);
	}
}
