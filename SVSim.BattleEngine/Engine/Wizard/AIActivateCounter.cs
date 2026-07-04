using System.Collections.Generic;

namespace Wizard;

public class AIActivateCounter
{
	private AIPlayTag _sourceTag;

	private AIActivateCountTagArgument _sourceTagArgument;

	public int ActivateCount { get; private set; }

	public int TurnMaxActivateCount => _sourceTagArgument.TurnMaxActivateCount;

	public AIScriptTokenArgType ResetSideType => _sourceTagArgument.ResetSideType;

	public int SourceCardId => _sourceTagArgument.SkillOwnerId;

	public int CounterIndex => _sourceTagArgument.SkillIndex;

	public AIPlayTagType CounterType => _sourceTag.Type;

	public AIActivateCounter(AIPlayTag tag, int count = 0)
	{
		ActivateCount = count;
		_sourceTag = tag;
		_sourceTagArgument = tag.ArgumentExpressions as AIActivateCountTagArgument;
		if (_sourceTagArgument == null)
		{
			AIConsoleUtility.LogError("AIActivateCounter error!! _sourceTagArgument == null!!!!!!");
		}
	}

	public AIActivateCounter Clone()
	{
		return new AIActivateCounter(_sourceTag, ActivateCount);
	}

	public void CheckConditionAndIncrement(AIVirtualCard owner, AISituationInfo situation, AIVirtualCard triggerCard)
	{
		AIVirtualField selfField = owner.SelfField;
		List<int> bestPlayPtn = selfField.BestPlayPtn;
		if (CanIncrement() && _sourceTag.CheckCondition(owner, bestPlayPtn, selfField, situation) && (!(_sourceTagArgument is AIFilteringActivateCountArgument aIFilteringActivateCountArgument) || (triggerCard != null && AIFilteringUtility.CheckMatchTargetFiltering(triggerCard, null, aIFilteringActivateCountArgument.Filters, bestPlayPtn, owner, situation))))
		{
			ActivateCount++;
		}
	}

	public void CheckConditionAndIncrement(AIVirtualCard owner, AISituationInfo situation, List<AIVirtualCard> triggerCardList)
	{
		AIVirtualField selfField = owner.SelfField;
		List<int> bestPlayPtn = selfField.BestPlayPtn;
		if (!CanIncrement() || !_sourceTag.CheckCondition(owner, bestPlayPtn, selfField, situation))
		{
			return;
		}
		bool flag = false;
		if (_sourceTagArgument is AIFilteringActivateCountArgument aIFilteringActivateCountArgument)
		{
			if (triggerCardList == null || triggerCardList.Count <= 0)
			{
				return;
			}
			for (int i = 0; i < triggerCardList.Count; i++)
			{
				if (AIFilteringUtility.CheckMatchTargetFiltering(triggerCardList[i], null, aIFilteringActivateCountArgument.Filters, bestPlayPtn, owner, situation))
				{
					flag = true;
					break;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			ActivateCount++;
		}
	}

	private bool CanIncrement()
	{
		if (TurnMaxActivateCount == -1 || ActivateCount < TurnMaxActivateCount)
		{
			return true;
		}
		return false;
	}

	public int GetCountIfMatched(int sourceCardId, int index)
	{
		if (SourceCardId == sourceCardId && CounterIndex == index)
		{
			return ActivateCount;
		}
		return 0;
	}

	public bool IsSkillOccurred()
	{
		if (TurnMaxActivateCount == -1)
		{
			return false;
		}
		return ActivateCount >= TurnMaxActivateCount;
	}

	public void Reset(bool isOwnerTurn)
	{
		switch (ResetSideType)
		{
		case AIScriptTokenArgType.BOTH:
			ActivateCount = 0;
			break;
		case AIScriptTokenArgType.ALLY:
			if (isOwnerTurn)
			{
				ActivateCount = 0;
			}
			break;
		case AIScriptTokenArgType.OPPONENT:
			if (!isOwnerTurn)
			{
				ActivateCount = 0;
			}
			break;
		}
	}

	public bool IsDuplicate(AIPlayTag tag)
	{
		return tag.Hash == _sourceTag.Hash;
	}

	public bool InheritFromOtherCounter(AIActivateCounter counter)
	{
		if (IsDuplicate(counter))
		{
			ActivateCount = counter.ActivateCount;
			return true;
		}
		return false;
	}

	private bool IsDuplicate(AIActivateCounter counter)
	{
		return counter.IsDuplicate(_sourceTag);
	}
}
