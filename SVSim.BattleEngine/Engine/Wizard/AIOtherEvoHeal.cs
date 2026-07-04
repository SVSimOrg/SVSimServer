using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoHeal : AIOtherEvoTagArgument
{
	private AIPolishConvertedExpression _heal;

	protected override int SELECT_TYPE_ARG_OFFSET => 2;

	public AIOtherEvoHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_heal = _exprList[_exprList.Count - 1];
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			int heal = GetHeal(tagOwner, field, playPtn, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.HealAll(targets, field, heal, playPtn, situation);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				AISkillSimulationUtility.HealTarget(situation, field, base.SelectType, heal);
				break;
			case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
				break;
			}
		}
	}

	private int GetHeal(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_heal == null)
		{
			AIConsoleUtility.LogError("AIOtherEvoHeal error!! _heal is null");
			return 0;
		}
		return (int)_heal.EvalArg(tagOwner, playPtn, field, situation);
	}
}
