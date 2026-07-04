using System.Collections.Generic;

namespace Wizard;

public class BanishBonusTagCollection : TagCollection
{
	public BanishBonusTagCollection()
		: base(TagCollectionType.BanishBonus)
	{
	}

	private BanishBonusTagCollection(BanishBonusTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new BanishBonusTagCollection(this);
	}

	public float GetBanishBonus(AIVirtualCard tagOwner, List<int> playPtn)
	{
		if (tagOwner == null || !base.HasTag)
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
