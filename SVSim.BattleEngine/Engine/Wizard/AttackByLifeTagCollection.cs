using System.Collections.Generic;

namespace Wizard;

public class AttackByLifeTagCollection : TagCollection
{
	public AttackByLifeTagCollection()
		: base(TagCollectionType.AttackByLife)
	{
	}

	private AttackByLifeTagCollection(AttackByLifeTagCollection param)
		: base(param)
	{
	}

	public override TagCollection Clone()
	{
		return new AttackByLifeTagCollection(this);
	}

	public int GetRealAttack(AIVirtualCard tagOwner, AIVirtualCard skillTarget, int attack)
	{
		if (skillTarget.IsDead)
		{
			return attack;
		}
		if (base.HasTag)
		{
			for (int i = 0; i < base.TagList.Count; i++)
			{
				AIPlayTag aIPlayTag = base.TagList[i];
				AIVirtualField selfField = tagOwner.SelfField;
				List<int> bestPlayPtn = selfField.BestPlayPtn;
				if (aIPlayTag.CheckCondition(tagOwner, bestPlayPtn, selfField, null))
				{
					AIFiltersArgument aIFiltersArgument = aIPlayTag.ArgumentExpressions as AIFiltersArgument;
					List<AIVirtualCard> candidates = (tagOwner.IsAlly ? selfField.AllyInplayCards : selfField.EnemyInplayCards);
					if (AIFilteringUtility.CheckMatchTargetFiltering(skillTarget, candidates, aIFiltersArgument.Filters, bestPlayPtn, tagOwner, null))
					{
						skillTarget.AttackByLifeCount++;
						return skillTarget.Life;
					}
				}
			}
		}
		return attack;
	}
}
