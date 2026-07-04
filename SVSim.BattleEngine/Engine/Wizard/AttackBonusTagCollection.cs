using System.Collections.Generic;

namespace Wizard;

public class AttackBonusTagCollection : TagCollection
{
	public AttackBonusTagCollection()
		: base(TagCollectionType.AttackBonus)
	{
	}

	private AttackBonusTagCollection(AttackBonusTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new AttackBonusTagCollection(this);
	}

	public float GetAttackBonus(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead || !base.HasTag)
		{
			return 0f;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		float num = 0f;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
			{
				num += aIPlayTag.EvalArg(tagOwner, playPtn, selfField, situation);
			}
		}
		return num;
	}
}
