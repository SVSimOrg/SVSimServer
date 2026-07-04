using System.Collections.Generic;

namespace Wizard;

public class BreakBeforePlayTagCollection : TagCollection
{
	public BreakBeforePlayTagCollection()
		: base(TagCollectionType.BreakBeforePlay)
	{
	}

	private BreakBeforePlayTagCollection(BreakBeforePlayTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new BreakBeforePlayTagCollection(this);
	}

	public bool EnabledBreakBeforePlay(AIVirtualCard tagOwner, List<int> playPtn)
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
