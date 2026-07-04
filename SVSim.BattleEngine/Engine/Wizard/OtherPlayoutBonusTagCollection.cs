using System.Collections.Generic;

namespace Wizard;

public class OtherPlayoutBonusTagCollection : TagCollection
{
	public OtherPlayoutBonusTagCollection()
		: base(TagCollectionType.OtherPlayoutBonus)
	{
	}

	private OtherPlayoutBonusTagCollection(OtherPlayoutBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new OtherPlayoutBonusTagCollection(this);
	}

	public int GetPlayoutDamageBonus(AIVirtualCard tagOwner, AIVirtualCard targetCard, AIVirtualField field, List<int> playPtn)
	{
		if (!base.HasTag || targetCard == null || targetCard.IsDead)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, null))
			{
				AIOtherPlayoutDamageBonus aIOtherPlayoutDamageBonus = aIPlayTag.ArgumentExpressions as AIOtherPlayoutDamageBonus;
				num += aIOtherPlayoutDamageBonus.GetPlayoutDamageBonus(tagOwner, targetCard, field, playPtn);
			}
		}
		return num;
	}
}
