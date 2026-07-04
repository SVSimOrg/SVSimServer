using System.Collections.Generic;

namespace Wizard;

public class GetOnTriggerTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[3]
	{
		AIPlayTagType.GetOnBanish,
		AIPlayTagType.GetOnDamage,
		AIPlayTagType.GetOnEvo
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public GetOnTriggerTagCollection()
		: base(TagCollectionType.WhenGetOn)
	{
	}

	public GetOnTriggerTagCollection(GetOnTriggerTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new GetOnTriggerTagCollection(this);
	}

	public void RegisterPassedConditionTags(AIVirtualCard tagOwner, AIVirtualCard getOnCard, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag || tagOwner == null || tagOwner.IsDead)
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
				passedConditionTags = AIParamQuery.AddElementToList(aIPlayTag, passedConditionTags, isBlockDuplicate: true);
			}
		}
		if (passedConditionTags != null)
		{
			situation.RegisterNewProcessInfo(getOnCard, AISituationTriggerInformation.TriggerType.Undefined).AddExecutingAction(delegate
			{
				ExecuteTagAction(passedConditionTags, getOnCard, tagOwner, field, playPtn, situation);
			});
		}
	}

	private void ExecuteTagAction(List<AIPlayTag> tags, AIVirtualCard getOnCard, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		for (int i = 0; i < tags.Count; i++)
		{
			AIPlayTag aIPlayTag = tags[i];
			if (aIPlayTag.ArgumentExpressions is AITriggerAndTargetFiltersTagBase aITriggerAndTargetFiltersTagBase)
			{
				aITriggerAndTargetFiltersTagBase.Execute(tagOwner, field, playPtn, getOnCard, situation);
			}
			else
			{
				aIPlayTag.ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
			}
		}
	}
}
