using System.Collections.Generic;

namespace Wizard;

public class LeaveBonusTagCollection : TagCollection
{
	public LeaveBonusTagCollection()
		: base(TagCollectionType.LeaveBonus)
	{
	}

	public LeaveBonusTagCollection(LeaveBonusTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new LeaveBonusTagCollection(this);
	}

	public float GetLeaveBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool useIgnoreInBattle)
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
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation) && aIPlayTag.ArgumentExpressions is AIBonusArgumentWithIgnoreInBattle aIBonusArgumentWithIgnoreInBattle)
			{
				num += aIBonusArgumentWithIgnoreInBattle.GetBonusValue(tagOwner, playPtn, situation, useIgnoreInBattle);
			}
		}
		return num;
	}
}
