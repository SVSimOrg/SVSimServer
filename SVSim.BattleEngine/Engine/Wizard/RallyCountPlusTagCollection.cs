namespace Wizard;

public class RallyCountPlusTagCollection : TagCollection
{
	public RallyCountPlusTagCollection()
		: base(TagCollectionType.RallyCountPlus)
	{
	}

	private RallyCountPlusTagCollection(RallyCountPlusTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new RallyCountPlusTagCollection(this);
	}

	public bool CanAppendRallyCount(AIVirtualCard tagOwner, AIVirtualCard summonedCard)
	{
		if (tagOwner == null || tagOwner.IsDead || summonedCard == null || summonedCard.IsDead || tagOwner.IsAlly != summonedCard.IsAlly || !base.HasTag)
		{
			return false;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			AIFiltersArgument aIFiltersArgument = aIPlayTag.ArgumentExpressions as AIFiltersArgument;
			if (aIPlayTag.CheckCondition(tagOwner, EnemyAI.EmptyPlayPtn, selfField, null) && AIFilteringUtility.CheckMatchTargetFiltering(summonedCard, null, aIFiltersArgument.Filters, EnemyAI.EmptyPlayPtn, tagOwner, null))
			{
				return true;
			}
		}
		return false;
	}
}
