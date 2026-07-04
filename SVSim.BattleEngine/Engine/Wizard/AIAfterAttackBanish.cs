using System.Collections.Generic;

namespace Wizard;

public class AIAfterAttackBanish : AITriggerAndTargetFiltersTagBase
{

	public AIScriptTokenArgType SelectType { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 1;

	public AIAfterAttackBanish(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (AIPlayTagInitializingUtility.TryCreateSelectType(_exprList[_exprList.Count - 1], base.LegalSelectTypes, out var selectType))
		{
			SelectType = selectType;
		}
		else
		{
			SelectType = AIScriptTokenArgType.NONE;
		}
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AIBanishSimulationUtility.BanishAll(targets, situation);
		}
		else
		{
			AIConsoleUtility.LogError($"AIAfterAttackBanish.Execute(): Unsupported selecttype. type:{SelectType}");
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
