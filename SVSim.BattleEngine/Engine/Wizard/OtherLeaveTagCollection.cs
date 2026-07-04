using System.Collections.Generic;

namespace Wizard;

public class OtherLeaveTagCollection : TagCollection
{
	private static AIPlayTagType[] _managedTagTypes = new AIPlayTagType[2]
	{
		AIPlayTagType.OtherLeaveDamage,
		AIPlayTagType.OtherLeaveToken
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public OtherLeaveTagCollection()
		: base(TagCollectionType.WhenOtherLeave)
	{
	}

	private OtherLeaveTagCollection(OtherLeaveTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new OtherLeaveTagCollection(this);
	}

	public void RegisterPassedConditionTags(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIVirtualCard leftCard)
	{
		if (!base.HasTag || tagOwner == null || tagOwner.IsDead || leftCard == null)
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
		if (passedConditionTags != null)
		{
			situation.RegisterNewProcessInfo(leftCard, AISituationTriggerInformation.TriggerType.Leave).AddExecutingAction(delegate
			{
				ExecuteTagAction(tagOwner, field, playPtn, situation, leftCard, passedConditionTags);
			});
		}
	}

	private void ExecuteTagAction(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIVirtualCard leftCard, List<AIPlayTag> passedConditionTags)
	{
		if (tagOwner.IsDead || passedConditionTags == null)
		{
			return;
		}
		for (int i = 0; i < passedConditionTags.Count; i++)
		{
			AIPlayTag aIPlayTag = passedConditionTags[i];
			if (aIPlayTag.ArgumentExpressions is AITriggerAndTargetFiltersTagBase aITriggerAndTargetFiltersTagBase)
			{
				aITriggerAndTargetFiltersTagBase.Execute(tagOwner, field, playPtn, leftCard, situation);
			}
			else
			{
				aIPlayTag.ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
			}
		}
	}
}
