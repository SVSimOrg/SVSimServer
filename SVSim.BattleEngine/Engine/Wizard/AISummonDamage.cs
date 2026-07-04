using System.Collections.Generic;

namespace Wizard;

public class AISummonDamage : AIFiltersArgument
{

	public AIScriptTokenArgType SelectType { get; private set; }

	public AIPolishConvertedExpression Damage { get; private set; }

	protected override int NON_FILTER_FIRST_OFFSET => 2;

	public AISummonDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		AIPolishConvertedExpression arg = _exprList[_exprList.Count - 2];
		AIScriptTokenArgType selectType = AIScriptTokenArgType.NONE;
		if (!IsLegalSelectType(arg, out selectType))
		{
			SelectType = AIScriptTokenArgType.ALL_SELECT;
		}
		else
		{
			SelectType = selectType;
		}
		Damage = _exprList[_exprList.Count - 1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (situation == null || situation.Actor == null)
		{
			return;
		}
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int damage = (int)Damage.EvalArg(tagOwner, playPtn, field, situation);
			if (SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIDamageSimulationUtility.DamageAll(targetsFromField, tagOwner, field, damage, situation);
			}
			else if (SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIDamageSimulationUtility.DamageRandom(targetsFromField, tagOwner, field, damage, situation);
			}
		}
	}
}
