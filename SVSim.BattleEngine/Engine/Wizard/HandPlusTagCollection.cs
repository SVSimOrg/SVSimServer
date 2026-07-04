using System.Collections.Generic;

namespace Wizard;

public class HandPlusTagCollection : TagCollection
{
	public HandPlusTagCollection()
		: base(TagCollectionType.HandPlus)
	{
	}

	private HandPlusTagCollection(HandPlusTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new HandPlusTagCollection(this);
	}

	public int GetHandPlus(AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner == null || !base.HasTag)
		{
			return 0;
		}
		int num = 0;
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, null))
			{
				num += (int)aIPlayTag.EvalArg(tagOwner, playPtn, selfField, null);
			}
		}
		return num;
	}
}
