using System.Collections.Generic;

namespace Wizard;

public class AIEvoAttackableCount : AIEvoTagArgument
{
	private AIPolishConvertedExpression _attackableCount;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIEvoAttackableCount(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_attackableCount = _exprList[_exprList.Count - 1];
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT
		};
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int attackableCount = (int)_attackableCount.EvalArg(tagOwner, playPtn, field, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIAttackableCountSimulationUtility.ExecuteChangeAttackableCountAll(targetsFromField, attackableCount);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
				AIAttackableCountSimulationUtility.ExecuteChangeAttackableCountTargetSelect(attackableCount, base.SelectType, situation);
				break;
			}
		}
	}
}
