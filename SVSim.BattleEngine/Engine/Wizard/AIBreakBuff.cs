using System.Collections.Generic;

namespace Wizard;

public class AIBreakBuff : AITriggerAndTargetFiltersTagBase
{
	private readonly int LIFE_ARG_OFFSET = 1;

	private readonly int ATTACK_ARG_OFFSET = 2;

	private readonly int SELECT_TYPE_ARG_OFFSET = 3;

	public AIPolishConvertedExpression Attack { get; private set; }

	public AIPolishConvertedExpression Life { get; private set; }

	public AIScriptTokenArgType SelectType { get; protected set; }

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_ARG_OFFSET;

	public AIBreakBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		Attack = _exprList[_exprList.Count - ATTACK_ARG_OFFSET];
		Life = _exprList[_exprList.Count - LIFE_ARG_OFFSET];
		SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_ARG_OFFSET], base.LegalSelectTypes);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (targets != null && targets.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, Attack, Life);
			switch (SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIBuffSimulationUtility.BuffAll_old(targets, field, buffExecutingInfo_old, isTemp: false, playPtn, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIBuffSimulationUtility.BuffRandom_old(targets, field, playPtn, situation, buffExecutingInfo_old, isTemp: false);
				break;
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
		return field.CardListSet.BothInplayCards;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}
}
