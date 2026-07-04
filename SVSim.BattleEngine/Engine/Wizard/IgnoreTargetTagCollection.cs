using System.Collections.Generic;

namespace Wizard;

public class IgnoreTargetTagCollection : TagCollection
{
	public IgnoreTargetTagCollection()
		: base(TagCollectionType.IgnoreTarget)
	{
	}

	private IgnoreTargetTagCollection(IgnoreTargetTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new IgnoreTargetTagCollection(this);
	}

	public List<AIVirtualCard> FilteringIgnoreTargets(AIVirtualCard tagOwner, List<AIVirtualCard> candidates, List<int> playPtn, AISituationInfo situation, int selectCount)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return AIGlobalEmptyList.EmptyVirtualCardList;
		}
		if (tagOwner == null || !base.HasTag)
		{
			return candidates;
		}
		List<AIVirtualCard> list = new List<AIVirtualCard>(candidates);
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (list.Count <= selectCount)
			{
				break;
			}
			AIPlayTag aIPlayTag = base.TagList[i];
			if (!aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation) || !(aIPlayTag.ArgumentExpressions is AIFiltersArgument aIFiltersArgument))
			{
				continue;
			}
			List<AIVirtualCard> filteredTargets = aIFiltersArgument.GetFilteredTargets(list, tagOwner, playPtn, situation);
			if (filteredTargets != null)
			{
				for (int j = 0; j < filteredTargets.Count; j++)
				{
					list.Remove(filteredTargets[j]);
				}
			}
		}
		return list;
	}
}
