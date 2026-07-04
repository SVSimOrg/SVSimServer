using System.Collections.Generic;

namespace Wizard;

public class IgnoreFanfareBonusTagCollection : TagCollection
{
	public IgnoreFanfareBonusTagCollection()
		: base(TagCollectionType.IgnoreFanfareBonus)
	{
	}

	private IgnoreFanfareBonusTagCollection(IgnoreFanfareBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new IgnoreFanfareBonusTagCollection(this);
	}

	public bool IsEnableIgnoreFanfareBonus(AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag)
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
