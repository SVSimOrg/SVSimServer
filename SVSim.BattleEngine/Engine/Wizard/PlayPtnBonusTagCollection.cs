using System.Collections.Generic;

namespace Wizard;

public class PlayPtnBonusTagCollection : TagCollection
{
	public PlayPtnBonusTagCollection()
		: base(TagCollectionType.PlayptnBonus)
	{
	}

	private PlayPtnBonusTagCollection(PlayPtnBonusTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new PlayPtnBonusTagCollection(this);
	}

	public float GetPlayPtnBonus(AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag)
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
