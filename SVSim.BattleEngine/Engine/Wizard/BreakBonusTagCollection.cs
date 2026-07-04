using System.Collections.Generic;

namespace Wizard;

public class BreakBonusTagCollection : TagCollection
{
	public BreakBonusTagCollection()
		: base(TagCollectionType.BreakBonus)
	{
	}

	private BreakBonusTagCollection(BreakBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new BreakBonusTagCollection(this);
	}

	public float GetBreakBonus(AIVirtualCard tagOwner, List<int> playPtn)
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
			if (!aIPlayTag.IsHoldingEVAL() && aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, null))
			{
				num += aIPlayTag.EvalArg(tagOwner, playPtn, selfField, null);
			}
		}
		return num;
	}
}
