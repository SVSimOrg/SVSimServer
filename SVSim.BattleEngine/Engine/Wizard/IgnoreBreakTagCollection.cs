using System.Collections.Generic;

namespace Wizard;

public class IgnoreBreakTagCollection : TagCollection
{
	public IgnoreBreakTagCollection()
		: base(TagCollectionType.IgnoreBreak)
	{
	}

	private IgnoreBreakTagCollection(IgnoreBreakTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new IgnoreBreakTagCollection(this);
	}

	public bool IsIgnoreBreak(AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner == null || !base.HasTag)
		{
			return false;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (!aIPlayTag.IsHoldingEVAL() && aIPlayTag.CheckCondition(tagOwner, playPtn, tagOwner.SelfField, null))
			{
				return true;
			}
		}
		return false;
	}
}
