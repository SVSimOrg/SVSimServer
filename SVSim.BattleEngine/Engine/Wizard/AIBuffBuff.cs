using System.Collections.Generic;

namespace Wizard;

public class AIBuffBuff : AITriggerAndTargetFiltersTagBase
{
	private static readonly AIScriptTokenArgType[] PermOrTempArgs = new AIScriptTokenArgType[2]
	{
		AIScriptTokenArgType.PERM,
		AIScriptTokenArgType.TEMP
	};

	private AIScriptTokenArgType _selectType;

	private AIPolishConvertedExpression _attackBuffValue;

	private AIPolishConvertedExpression _lifeBuffValue;

	private AIScriptTokenArgType _permOrTemp;

	protected override int NON_FILTER_FIRST_OFFSET => 4;

	public AIBuffBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_selectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 4], base.LegalSelectTypes);
		_attackBuffValue = _exprList[_exprList.Count - 3];
		_lifeBuffValue = _exprList[_exprList.Count - 2];
		_permOrTemp = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1], PermOrTempArgs);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, _attackBuffValue, _lifeBuffValue);
		bool isTemp = _permOrTemp == AIScriptTokenArgType.TEMP;
		switch (_selectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AIBuffSimulationUtility.BuffAll_old(targets, field, buffExecutingInfo_old, isTemp, playPtn, situation);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
			AIBuffSimulationUtility.BuffRandom_old(targets, field, playPtn, situation, buffExecutingInfo_old, isTemp);
			break;
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		bool isAttackEffective = !_attackBuffValue.IsZeroOrNone();
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.AllReferableCards;
	}
}
