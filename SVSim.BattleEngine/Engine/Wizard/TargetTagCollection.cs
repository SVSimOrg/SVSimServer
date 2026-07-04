using System.Collections.Generic;
using Cute;

namespace Wizard;

public class TargetTagCollection : TagCollection
{
	public TargetTagCollection()
		: base(TagCollectionType.Target)
	{
	}

	private TargetTagCollection(TargetTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new TargetTagCollection(this);
	}

	public List<AIVirtualCard> FilteringTargetCards(AIVirtualCard tagOwner, List<AIVirtualCard> targetCards, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		if (tagOwner == null || !base.HasTag)
		{
			return targetCards;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.ArgumentExpressions is AIFiltersArgument && aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				AIFiltersArgument aIFiltersArgument = aIPlayTag.ArgumentExpressions as AIFiltersArgument;
				List<AIVirtualCard> list2 = AIFilteringUtility.MultipleFiltering(targetCards, aIFiltersArgument.Filters, tagOwner, playPtn, situation);
				if (list2 != null)
				{
					list = AIParamQuery.AddRangeToList(list2, list);
				}
			}
		}
		if (!list.IsNotNullOrEmpty())
		{
			return targetCards;
		}
		return list;
	}
}
