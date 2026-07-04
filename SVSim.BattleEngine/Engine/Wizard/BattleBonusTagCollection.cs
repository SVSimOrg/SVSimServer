using System.Collections.Generic;

namespace Wizard;

public class BattleBonusTagCollection : TagCollection
{
	public BattleBonusTagCollection()
		: base(TagCollectionType.BattleBonus)
	{
	}

	private BattleBonusTagCollection(BattleBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new BattleBonusTagCollection(this);
	}

	public float GetBattleBonus(AIVirtualCard tagOwner, List<int> playPtn)
	{
		float num = 0f;
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag)
		{
			return num;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		_ = selfField.ParamQuery;
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
