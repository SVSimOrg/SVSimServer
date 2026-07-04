using System.Collections.Generic;

namespace Wizard;

public class SelfAndOtherEvoTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagType = new AIPlayTagType[10]
	{
		AIPlayTagType.SelfAndOtherEvoDamage,
		AIPlayTagType.SelfAndOtherEvoAttachTag,
		AIPlayTagType.SelfAndOtherEvoShield,
		AIPlayTagType.SelfAndOtherEvoDestroy,
		AIPlayTagType.SelfAndOtherEvoBounce,
		AIPlayTagType.SelfAndOtherEvoToken,
		AIPlayTagType.SelfAndOtherEvoDraw,
		AIPlayTagType.SelfAndOtherEvoAddCemetery,
		AIPlayTagType.SelfAndOtherEvoHeal,
		AIPlayTagType.SelfAndOtherEvoTokenDraw
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagType;

	public SelfAndOtherEvoTagCollection()
		: base(TagCollectionType.WhenSelfAndOtherEvo)
	{
	}

	private SelfAndOtherEvoTagCollection(SelfAndOtherEvoTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new SelfAndOtherEvoTagCollection(this);
	}

	public void RegisterPassedConditionTags(AIVirtualCard tagOwner, AIVirtualCard evolver, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead || !base.HasTag)
		{
			return;
		}
		List<AIPlayTag> passedConditionTags = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				passedConditionTags = AIParamQuery.AddElementToList(aIPlayTag, passedConditionTags);
			}
		}
		if (passedConditionTags == null)
		{
			return;
		}
		situation.RegisterNewProcessInfo(evolver, AISituationTriggerInformation.TriggerType.Evolver).AddExecutingAction(delegate
		{
			if (!tagOwner.IsDead)
			{
				for (int j = 0; j < passedConditionTags.Count; j++)
				{
					if (passedConditionTags[j].ArgumentExpressions is AIOtherEvoTagArgument aIOtherEvoTagArgument)
					{
						aIOtherEvoTagArgument.Execute(tagOwner, field, playPtn, evolver, situation);
					}
				}
			}
		});
	}

	public bool CheckDestroyBySelfAndOtherEvoTags(AIVirtualCard tagOwner, AIVirtualCard destroyTarget, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag)
		{
			return false;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIOtherEvoTagArgument aIOtherEvoTagArgument && aIOtherEvoTagArgument.IsTargetGoingToDie(tagOwner, destroyTarget, situation))
			{
				return true;
			}
		}
		return false;
	}
}
