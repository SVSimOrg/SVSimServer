using System.Collections.Generic;

namespace Wizard;

public class AIGetOnBanish : AITriggerAndTargetFiltersTagBase
{
	private AIScriptTokenArgType _selectType;

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIGetOnBanish(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		AIPolishConvertedExpression expression = _exprList[_exprList.Count - 1];
		_selectType = AIPlayTagInitializingUtility.CreateSingleArgType(expression, base.LegalSelectTypes);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		switch (_selectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AIBanishSimulationUtility.BanishAll(targets, situation);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
			AIBanishSimulationUtility.BanishRandom(targets, tagOwner, field, playPtn, situation);
			break;
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
