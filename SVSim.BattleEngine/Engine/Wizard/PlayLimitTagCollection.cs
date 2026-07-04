using System.Collections.Generic;

namespace Wizard;

public class PlayLimitTagCollection : TagCollection
{
	public PlayLimitTagCollection()
		: base(TagCollectionType.PlayLimit)
	{
	}

	private PlayLimitTagCollection(PlayLimitTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new PlayLimitTagCollection(this);
	}

	public float GetPlayLimit(AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner == null || tagOwner.IsDead || tagOwner.IsOnField || !base.HasTag)
		{
			return 0f;
		}
		float num = 0f;
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, null))
			{
				num += aIPlayTag.EvalArg(tagOwner, playPtn, selfField, null);
			}
		}
		return num;
	}
}
