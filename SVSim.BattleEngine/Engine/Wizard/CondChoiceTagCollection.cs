using System;
using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class CondChoiceTagCollection : TagCollection
{
	public CondChoiceTagCollection()
		: base(TagCollectionType.CondChoice)
	{
	}

	private CondChoiceTagCollection(CondChoiceTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new CondChoiceTagCollection(this);
	}

	public List<BattleCardBase> GetCondChoiceTargets(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, IEnumerable<BattleCardBase> candidates)
	{
		if (!base.HasTag)
		{
			return null;
		}
		List<BattleCardBase> list = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, null))
			{
				int id = aIPlayTag.EvalID();
				list = AIParamQuery.AddElementToList(candidates.FirstOrDefault((BattleCardBase c) => c.CardId == id), list);
			}
		}
		return list;
	}

	public List<AIVirtualCard> GetCondChoiceTargets(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, List<AIVirtualCard> candidates, int choiceCount, AISituationInfo situation)
	{
		if (!base.HasTag || choiceCount > candidates.Count)
		{
			throw new Exception("CondChoiceTagCollection.GetCondChoiceTargets() error!! candidates are not enough!!!!! choiceCount = " + choiceCount);
		}
		List<AIVirtualCard> list = null;
		int num = choiceCount;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation))
			{
				int id = aIPlayTag.EvalID();
				list = AIParamQuery.AddElementToList(candidates.FirstOrDefault((AIVirtualCard c) => c.BaseId == id), list);
				num--;
				if (num <= 0)
				{
					break;
				}
			}
		}
		if (num > 0)
		{
			for (int num2 = 0; num2 < candidates.Count; num2++)
			{
				AIVirtualCard aIVirtualCard = candidates[num2];
				if (list == null || !list.Contains(aIVirtualCard))
				{
					list = AIParamQuery.AddElementToList(aIVirtualCard, list);
					num--;
					if (num <= 0)
					{
						break;
					}
				}
			}
		}
		return list;
	}
}
