using System.Collections.Generic;

namespace Wizard;

public class AITurnStartDamage : AITurnStartTagArgument
{
	private AIPolishConvertedExpression _damageAmount;

	private AIPolishConvertedExpression _damageCount;

	protected override int SELECT_TYPE_OFFSET => 4;

	public AITurnStartDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_damageAmount = _exprList[_exprList.Count - 3];
		_damageCount = _exprList[_exprList.Count - 2];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		int damageAmount = GetDamageAmount(tagOwner, field, playPtn, situation);
		int damageCount = GetDamageCount(tagOwner, field, playPtn, situation);
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
		{
			for (int j = 0; j < damageCount; j++)
			{
				AIDamageSimulationUtility.DamageAll(targetsFromField, tagOwner, field, damageAmount, situation);
			}
			break;
		}
		case AIScriptTokenArgType.RANDOM_SELECT:
		{
			for (int i = 0; i < damageCount; i++)
			{
				AIDamageSimulationUtility.DamageRandom(targetsFromField, tagOwner, field, damageAmount, situation);
			}
			break;
		}
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
			AIConsoleUtility.LogError("AITurnStartDamage.GetDamageAmount() error!! _damageAmount is null");
			return 0;
		}
		return (int)_damageAmount.EvalArg(tagOwner, playPtn, field, situation);
	}

	private int GetDamageCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_damageAmount == null)
		{
			AIConsoleUtility.LogError("AITurnStartDamage.GetDamageCount() error!! _damageAmount is null");
			return 0;
		}
		return (int)_damageCount.EvalArg(tagOwner, playPtn, field, situation);
	}
}
