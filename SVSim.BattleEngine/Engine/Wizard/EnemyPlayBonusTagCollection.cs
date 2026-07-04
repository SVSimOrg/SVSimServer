using System.Collections.Generic;

namespace Wizard;

public class EnemyPlayBonusTagCollection : TagCollection
{
	public EnemyPlayBonusTagCollection()
		: base(TagCollectionType.EnemyPlayBonus)
	{
	}

	private EnemyPlayBonusTagCollection(EnemyPlayBonusTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new EnemyPlayBonusTagCollection(this);
	}

	public float GetEnemyPlayBonus(AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag || targetCard.IsDead)
		{
			return 0f;
		}
		float num = 0f;
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.ArgumentExpressions is AIOtherPlayBonus)
			{
				AIOtherPlayBonus aIOtherPlayBonus = aIPlayTag.ArgumentExpressions as AIOtherPlayBonus;
				if (AIFilteringUtility.CheckMatchTargetFiltering(targetCard, selfField.AllyHandCards, aIOtherPlayBonus.Filters, playPtn, tagOwner, null) && aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
				{
					num += aIOtherPlayBonus.GetEvaluateValue(targetCard, playPtn, situation);
				}
			}
		}
		return num;
	}
}
