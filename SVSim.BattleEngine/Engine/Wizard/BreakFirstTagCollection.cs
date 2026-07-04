using System.Collections.Generic;

namespace Wizard;

public class BreakFirstTagCollection : TagCollection
{
	public BreakFirstTagCollection()
		: base(TagCollectionType.BreakFirst)
	{
	}

	private BreakFirstTagCollection(BreakFirstTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new BreakFirstTagCollection(this);
	}

	public bool EnabledBreakFirst(AIVirtualCard tagOwner, List<int> playPtn)
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
