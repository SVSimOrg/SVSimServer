using System.Collections.Generic;

namespace Wizard;

public class OtherLeaveBonusTagCollection : TagCollection
{
	public OtherLeaveBonusTagCollection()
		: base(TagCollectionType.OtherLeaveBonus)
	{
	}

	private OtherLeaveBonusTagCollection(OtherLeaveBonusTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new OtherLeaveBonusTagCollection(this);
	}

	public float GetOtherLeaveBonus(AIVirtualCard tagOwner, AIVirtualCard targetCard, AIVirtualField field, List<int> playPtn, bool useIgnoreInBattle)
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
