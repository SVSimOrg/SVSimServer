using System.Collections.Generic;

namespace Wizard;

public class HandBonusTagCollection : TagCollection
{
	public static float DEFAULT_HAND_BONUS = 0.1f;

	public HandBonusTagCollection()
		: base(TagCollectionType.HandBonus)
	{
	}

	private HandBonusTagCollection(HandBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new HandBonusTagCollection(this);
	}

	public float GetHandBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isIgnoreInFusion)
	{
		float num = DEFAULT_HAND_BONUS;
		if (tagOwner == null || !base.HasTag)
		{
			return num;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			AIBonusArgumentWithIgnoreInBattle aIBonusArgumentWithIgnoreInBattle = aIPlayTag.ArgumentExpressions as AIBonusArgumentWithIgnoreInBattle;
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation) && aIBonusArgumentWithIgnoreInBattle != null)
			{
				num += aIBonusArgumentWithIgnoreInBattle.GetBonusValue(tagOwner, playPtn, situation, isIgnoreInFusion);
			}
		}
		return num;
	}
}
