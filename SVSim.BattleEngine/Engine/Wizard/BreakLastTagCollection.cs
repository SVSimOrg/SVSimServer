using System.Collections.Generic;

namespace Wizard;

public class BreakLastTagCollection : TagCollection
{
	public BreakLastTagCollection()
		: base(TagCollectionType.BreakLast)
	{
	}

	private BreakLastTagCollection(BreakLastTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new BreakLastTagCollection(this);
	}

	public bool EnabledBreakLast(AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner == null || !base.HasTag)
		{
			return false;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			if (base.TagList[i].CheckCondition(tagOwner, playPtn, selfField, null))
			{
				return true;
			}
		}
		return false;
	}
}
