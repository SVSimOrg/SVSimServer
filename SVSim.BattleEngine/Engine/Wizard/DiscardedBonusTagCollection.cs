using System.Collections.Generic;

namespace Wizard;

public class DiscardedBonusTagCollection : TagCollection
{
	public DiscardedBonusTagCollection()
		: base(TagCollectionType.DiscardedBonus)
	{
	}

	public DiscardedBonusTagCollection(DiscardedBonusTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new DiscardedBonusTagCollection(this);
	}

	public float GetDiscardedBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isIgnoreInBattle)
	{
		if (tagOwner == null || tagOwner.IsDead || tagOwner.IsOnField)
		{
			return 0f;
		}
		float num = 0f;
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				AIBonusArgumentWithIgnoreInBattle aIBonusArgumentWithIgnoreInBattle = aIPlayTag.ArgumentExpressions as AIBonusArgumentWithIgnoreInBattle;
				num += aIBonusArgumentWithIgnoreInBattle.GetBonusValue(tagOwner, playPtn, situation, isIgnoreInBattle);
			}
		}
		return num;
	}
}
