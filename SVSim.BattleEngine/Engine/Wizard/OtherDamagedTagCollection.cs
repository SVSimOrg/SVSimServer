using System.Collections.Generic;

namespace Wizard;

public class OtherDamagedTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[5]
	{
		AIPlayTagType.OtherDamagedDamage,
		AIPlayTagType.OtherDamagedHeal,
		AIPlayTagType.OtherDamagedSetLeaderMaxLife,
		AIPlayTagType.OtherDamagedSubtractCountdown,
		AIPlayTagType.OtherDamagedBanish
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public OtherDamagedTagCollection()
		: base(TagCollectionType.WhenOtherDamaged)
	{
	}

	private OtherDamagedTagCollection(OtherDamagedTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new OtherDamagedTagCollection(this);
	}

	public void RegisterPassedConditionTags(AIVirtualCard tagOwner, AIVirtualCard damagedCard, int defaultDamage, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead || damagedCard.IsDead || !base.HasTag)
		{
			return;
		}
		List<AIPlayTag> passedConditionTags = null;
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
			AISkillProcessInformation aISkillProcessInformation = situation.RegisterNewProcessInfo(tagOwner, AISituationTriggerInformation.TriggerType.Damage);
			aISkillProcessInformation.OwnProcessRecord.RegisterDefaultDamage(defaultDamage);
			aISkillProcessInformation.AddExecutingAction(delegate
			{
				ExecuteTagAction(passedConditionTags, tagOwner, damagedCard, field, playPtn, situation);
			});
		}
	}

	private void ExecuteTagAction(List<AIPlayTag> tags, AIVirtualCard tagOwner, AIVirtualCard damagedCard, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead)
		{
			return;
		}
		for (int i = 0; i < tags.Count; i++)
		{
			AIPlayTag aIPlayTag = tags[i];
			if (aIPlayTag.ArgumentExpressions is AIFiltersArgument aIFiltersArgument)
			{
				aIFiltersArgument.Execute(tagOwner, field, playPtn, damagedCard, situation);
			}
			else if (aIPlayTag.ArgumentExpressions is AITriggerAndTargetFiltersTagBase aITriggerAndTargetFiltersTagBase)
			{
				aITriggerAndTargetFiltersTagBase.Execute(tagOwner, field, playPtn, damagedCard, situation);
			}
			else
			{
				AIConsoleUtility.LogError($"AIOtherDamagedTagCollection.ExecuteTagAction() error!! {aIPlayTag.Type} is invalid TagType");
			}
		}
	}
}
