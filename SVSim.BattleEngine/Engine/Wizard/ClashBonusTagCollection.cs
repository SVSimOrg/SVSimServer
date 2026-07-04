using System.Collections.Generic;

namespace Wizard;

public class ClashBonusTagCollection : TagCollection
{
	public ClashBonusTagCollection()
		: base(TagCollectionType.ClashBonus)
	{
	}

	private ClashBonusTagCollection(ClashBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new ClashBonusTagCollection(this);
	}

	public float GetClashBonus(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn)
	{
		float num = 0f;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, null))
			{
				num += aIPlayTag.EvalArg(tagOwner, playPtn, field, null);
			}
		}
		return num;
	}
}
