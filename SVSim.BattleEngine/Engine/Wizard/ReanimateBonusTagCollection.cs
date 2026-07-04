using System.Collections.Generic;

namespace Wizard;

public class ReanimateBonusTagCollection : TagCollection
{
	public ReanimateBonusTagCollection()
		: base(TagCollectionType.ReanimateBonus)
	{
	}

	private ReanimateBonusTagCollection(ReanimateBonusTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new ReanimateBonusTagCollection(this);
	}

	public float GetReanimateBonus(AIVirtualCard tagOwner, List<int> playPtn)
	{
		float num = 0f;
		if (tagOwner == null || !base.HasTag)
		{
			return num;
		}
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
