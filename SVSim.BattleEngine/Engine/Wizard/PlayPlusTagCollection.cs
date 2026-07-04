using System.Collections.Generic;

namespace Wizard;

public class PlayPlusTagCollection : TagCollection
{
	public PlayPlusTagCollection()
		: base(TagCollectionType.PlayPlus)
	{
	}

	private PlayPlusTagCollection(PlayPlusTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new PlayPlusTagCollection(this);
	}

	public int GetPlayPlusCount(AIVirtualCard tagOwner, List<int> playPtn)
	{
		if ((tagOwner == null) | !base.HasTag)
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
