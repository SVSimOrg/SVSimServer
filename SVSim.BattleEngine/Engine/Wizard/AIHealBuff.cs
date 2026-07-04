using System.Collections.Generic;

namespace Wizard;

public class AIHealBuff : AITriggerAndTargetFiltersTagBase
{

	public AIScriptTokenArgType SelectType { get; private set; }

	public AIPolishConvertedExpression Attack { get; private set; }

	public AIPolishConvertedExpression Life { get; private set; }

	public AIHealBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		AIPolishConvertedExpression arg = _exprList[_exprList.Count - 3];
		AIScriptTokenArgType selectType = AIScriptTokenArgType.NONE;
		if (!IsLegalSelectType(arg, out selectType))
		{
			SelectType = AIScriptTokenArgType.ALL_SELECT;
		}
		else
		{
			SelectType = selectType;
		}
		Attack = _exprList[_exprList.Count - 2];
		Life = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, Attack, Life);
			if (SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIBuffSimulationUtility.BuffAll_old(targets, field, buffExecutingInfo_old, isTemp: false, playPtn, situation);
			}
			else if (SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIBuffSimulationUtility.BuffRandom_old(targets, field, playPtn, situation, buffExecutingInfo_old, isTemp: false);
			}
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		bool isAttackEffective = !Attack.IsZeroOrNone();
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}
}
