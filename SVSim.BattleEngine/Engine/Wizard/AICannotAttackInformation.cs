using System.Collections.Generic;

namespace Wizard;

public class AICannotAttackInformation
{
	private List<AIScriptTokenBase> _filters;

	public AICannotAttackInformation(List<AIScriptTokenBase> filters)
	{
		_filters = filters;
	}

	public bool IsEqual(AICannotAttackInformation info)
	{
		return IsSameFilterList(info._filters);
	}

	private bool IsSameFilterList(List<AIScriptTokenBase> compare)
	{
		if (_filters == null || _filters.Count <= 0)
		{
			if (compare != null)
			{
				return compare.Count <= 0;
			}
			return true;
		}
		if (_filters.Count != compare.Count)
		{
			return false;
		}
		for (int i = 0; i < _filters.Count; i++)
		{
			if (!_filters[i].IsEqual(compare[i]))
			{
				return false;
			}
		}
		return true;
	}

	public bool IsCannotAttack(AIVirtualField field, AIVirtualAttackInfo situation)
	{
		if (situation.ActionType != AIOperationType.ATTACK)
		{
			return true;
		}
		List<AIVirtualCard> candidates = (situation.Actor.IsAlly ? field.CardListSet.EnemyClassAndInplayCards : field.CardListSet.AllyClassAndInplayCards);
		return AIFilteringUtility.CheckMatchTargetFiltering(situation.AttackTarget, candidates, _filters, field.BestPlayPtn, situation.Actor, situation);
	}
}
