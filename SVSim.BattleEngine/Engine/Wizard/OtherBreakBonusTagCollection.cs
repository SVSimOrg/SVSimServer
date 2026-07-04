using System.Collections.Generic;

namespace Wizard;

public class OtherBreakBonusTagCollection : TagCollection
{
	public OtherBreakBonusTagCollection()
		: base(TagCollectionType.OtherBreakBonus)
	{
	}

	private OtherBreakBonusTagCollection(OtherBreakBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new OtherBreakBonusTagCollection(this);
	}

	public float GetOtherBreakBonus(AIVirtualCard tagOwner, AIVirtualCard targetCard, AIVirtualField field, List<int> playPtn, bool useIgnoreInBattle)
	{
		if (!base.HasTag || targetCard == null)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, null))
			{
				AIOtherBreakBonus aIOtherBreakBonus = aIPlayTag.ArgumentExpressions as AIOtherBreakBonus;
				num += aIOtherBreakBonus.GetBonusValue(tagOwner, targetCard, field, playPtn, useIgnoreInBattle);
			}
		}
		return num;
	}
}
