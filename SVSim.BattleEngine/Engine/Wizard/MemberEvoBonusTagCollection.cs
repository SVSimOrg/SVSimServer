using System.Collections.Generic;

namespace Wizard;

public class MemberEvoBonusTagCollection : TagCollection
{
	public MemberEvoBonusTagCollection()
		: base(TagCollectionType.MemberEvoBonus)
	{
	}

	private MemberEvoBonusTagCollection(MemberEvoBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new MemberEvoBonusTagCollection(this);
	}

	public float GetMemberEvoBonus(AIVirtualCard tagOwner, AISituationInfo situation, List<int> playPtn)
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
			if (aIPlayTag.ArgumentExpressions is AIOtherEvoBonus)
			{
				AIOtherEvoBonus aIOtherEvoBonus = aIPlayTag.ArgumentExpressions as AIOtherEvoBonus;
				if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
				{
					num += aIOtherEvoBonus.GetEvaluateValue(tagOwner, situation, playPtn);
				}
			}
		}
		return num;
	}
}
