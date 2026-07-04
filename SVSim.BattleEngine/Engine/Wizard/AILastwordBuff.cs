using System.Collections.Generic;

namespace Wizard;

public class AILastwordBuff : AIFiltersAndSelectTypeArgument
{

	public AIPolishConvertedExpression Attack { get; private set; }

	public AIPolishConvertedExpression Life { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 3;

	public AILastwordBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		Attack = _exprList[_exprList.Count - 2];
		Life = _exprList[_exprList.Count - 1];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, Attack, Life);
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIBuffSimulationUtility.BuffAll_old(targetsFromField, field, buffExecutingInfo_old, isTemp: false, playPtn, situation);
			}
			else if (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIBuffSimulationUtility.BuffRandom_old(targetsFromField, field, playPtn, situation, buffExecutingInfo_old, isTemp: false);
			}
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		bool isAttackEffective = !Attack.IsZeroOrNone();
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}
}
