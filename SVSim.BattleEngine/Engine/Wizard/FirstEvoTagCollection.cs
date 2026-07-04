using System.Collections.Generic;

namespace Wizard;

public class FirstEvoTagCollection : TagCollection
{
	public FirstEvoTagCollection()
		: base(TagCollectionType.FirstEvo)
	{
	}

	private FirstEvoTagCollection(FirstEvoTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new FirstEvoTagCollection(this);
	}

	public bool IsFirstEvo(AIVirtualCard tagOwner, AIVirtualCard evoTarget, List<int> playPtn)
	{
		if (tagOwner == null || evoTarget == null || tagOwner.IsDead || evoTarget.IsDead || !base.HasTag)
		{
			return false;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, null) && aIPlayTag.ArgumentExpressions is AIFiltersArgument)
			{
				AIFiltersArgument aIFiltersArgument = aIPlayTag.ArgumentExpressions as AIFiltersArgument;
				if (AIFilteringUtility.CheckMatchTargetFiltering(evoTarget, null, aIFiltersArgument.Filters, playPtn, tagOwner, null))
				{
					return true;
				}
			}
		}
		return false;
	}
}
