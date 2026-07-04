using System.Collections.Generic;

namespace Wizard;

public class AIAttackBreakDamage : AIFiltersAndSelectTypeArgument
{
	private readonly int DAMAGE_ARG_OFFSET = 1;

	public AIPolishConvertedExpression Damage { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIAttackBreakDamage(string text)
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
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, field.BestPlayPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int damage = (int)Damage.EvalArg(tagOwner, field.BestPlayPtn, field, situation);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIDamageSimulationUtility.DamageAll(targetsFromField, tagOwner, field, damage, situation);
			}
			else if (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIDamageSimulationUtility.DamageRandom(targetsFromField, tagOwner, field, damage, situation);
			}
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}
