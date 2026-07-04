using System.Collections.Generic;

namespace Wizard;

public class BounceTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTypes = new AIPlayTagType[1] { AIPlayTagType.BounceDamage };

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTypes;

	public BounceTagCollection()
		: base(TagCollectionType.WhenBounce)
	{
	}

	private BounceTagCollection(BounceTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new BounceTagCollection(this);
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
	}

	public override void Clear()
	{
		base.Clear();
	}

	public void Execute(AIVirtualCard tagOwner, AIVirtualCard bouncedCard, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return;
		}
		List<AIPlayTag> passedConditionTags = null;
		AIVirtualField field = tagOwner.SelfField;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				if (passedConditionTags == null)
				{
					passedConditionTags = new List<AIPlayTag>();
				}
				passedConditionTags.Add(aIPlayTag);
			}
		}
		if (passedConditionTags == null)
		{
			return;
		}
		situation.RegisterNewProcessInfo(tagOwner, AISituationTriggerInformation.TriggerType.Bounce).AddExecutingAction(delegate
		{
			for (int j = 0; j < passedConditionTags.Count; j++)
			{
				ExecuteSingleTag(passedConditionTags[j], tagOwner, bouncedCard, field, situation);
			}
		});
	}

	private void ExecuteSingleTag(AIPlayTag tag, AIVirtualCard tagOwner, AIVirtualCard bouncedCard, AIVirtualField field, AISituationInfo situation)
	{
		if (tag.ArgumentExpressions is AITriggerAndTargetFiltersTagBase)
		{
			(tag.ArgumentExpressions as AITriggerAndTargetFiltersTagBase).Execute(tagOwner, field, field.BestPlayPtn, situation.BounceCardList, situation, isBlockDeadCard: false);
		}
	}
}
