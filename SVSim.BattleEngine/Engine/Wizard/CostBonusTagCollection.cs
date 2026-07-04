using System.Collections.Generic;

namespace Wizard;

public class CostBonusTagCollection : TagCollection
{
	public CostBonusTagCollection()
		: base(TagCollectionType.CostBonus)
	{
	}

	private CostBonusTagCollection(CostBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new CostBonusTagCollection(this);
	}

	public int GetCostBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		int num = 0;
		if (tagOwner == null || tagOwner.IsDead || tagOwner.IsOnField || !base.HasTag)
		{
			return num;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				num += (int)aIPlayTag.EvalArg(tagOwner, playPtn, selfField, situation);
			}
		}
		return num;
	}
}
