using System.Collections.Generic;

namespace Wizard;

public class AIResonanceDamage : AIFiltersAndSelectTypeArgument
{
	private readonly int DAMAGE_ARG_OFFSET = 1;

	public AIPolishConvertedExpression Damage { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIResonanceDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		Damage = _exprList[_exprList.Count - DAMAGE_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> list = AIFilteringUtility.MultipleFiltering(field.CardListSet.BothClassAndInplayCards, base.Filters, tagOwner, playPtn, situation);
		if (list != null && list.Count > 0)
		{
			int damage = (int)Damage.EvalArg(tagOwner, playPtn, field, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIDamageSimulationUtility.DamageAll(list, tagOwner, field, damage, situation);
			}
			else if (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIDamageSimulationUtility.DamageRandom(list, tagOwner, field, damage, situation);
			}
		}
	}
}
