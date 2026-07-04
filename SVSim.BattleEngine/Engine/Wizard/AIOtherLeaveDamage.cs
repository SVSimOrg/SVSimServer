using System.Collections.Generic;

namespace Wizard;

public class AIOtherLeaveDamage : AITriggerAndTargetFiltersTagBase
{
	private AIPolishConvertedExpression _damageAmount;

	private AIScriptTokenArgType _selectType;

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AIOtherLeaveDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damageAmount = _exprList[_exprList.Count - 1];
		_selectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], base.LegalSelectTypes);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		int damageAmount = GetDamageAmount(tagOwner, field, playPtn, situation);
		switch (_selectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AIDamageSimulationUtility.DamageAll(targets, tagOwner, field, damageAmount, situation);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
			AIDamageSimulationUtility.DamageRandom(targets, tagOwner, field, damageAmount, situation);
			break;
		default:
			AIConsoleUtility.LogError($"AIOtherLeaveDamage()：SelectType={_selectType} 未対応");
			break;
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}

	private int GetDamageAmount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_damageAmount == null)
		{
			AIConsoleUtility.LogError("AIOtherLeaveDamage error!! _damageAmount is null");
			return 0;
		}
		return (int)_damageAmount.EvalArg(tagOwner, playPtn, field, situation);
	}
}
