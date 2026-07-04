using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class HealTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[6]
	{
		AIPlayTagType.HealBuff,
		AIPlayTagType.HealDamage,
		AIPlayTagType.HealToken,
		AIPlayTagType.HealHeal,
		AIPlayTagType.HealAttachTag,
		AIPlayTagType.HealEvo
	};

	private List<AIPlayTag> _healTokenTags;

	public List<AIPlayTag> HealAttachTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public bool HasHealAttachTag
	{
		get
		{
			if (HealAttachTagList != null)
			{
				return HealAttachTagList.Count > 0;
			}
			return false;
		}
	}

	public HealTagCollection()
		: base(TagCollectionType.WhenHeal)
	{
	}

	public HealTagCollection(HealTagCollection tagCollection)
		: base(tagCollection)
	{
		if (tagCollection.HasHealAttachTag)
		{
			HealAttachTagList = new List<AIPlayTag>(tagCollection.HealAttachTagList);
		}
	}

	public override TagCollection Clone()
	{
		return new HealTagCollection(this);
	}

	public void RegisterPassedConditionTags(AIVirtualCard tagOwner, AIVirtualCard healedCard, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner == null || tagOwner.IsDead || !base.HasTag)
		{
			return;
		}
		AIVirtualField field = tagOwner.SelfField;
		List<AIPlayTag> passedConditionTags = null;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				passedConditionTags = AIParamQuery.AddElementToList(aIPlayTag, passedConditionTags, isBlockDuplicate: true);
			}
		}
		if (passedConditionTags != null && passedConditionTags.Count > 0)
		{
			situation.RegisterNewProcessInfo(healedCard, AISituationTriggerInformation.TriggerType.Undefined).AddExecutingAction(delegate
			{
				ExecuteTagAction(passedConditionTags, healedCard, tagOwner, field, playPtn, situation);
			});
		}
	}

	private void ExecuteTagAction(List<AIPlayTag> tags, AIVirtualCard healedCard, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead)
		{
			return;
		}
		for (int i = 0; i < tags.Count; i++)
		{
			AIScriptArgumentExpressions argumentExpressions = tags[i].ArgumentExpressions;
			if (argumentExpressions is AITriggerAndTargetFiltersTagBase aITriggerAndTargetFiltersTagBase)
			{
				aITriggerAndTargetFiltersTagBase.Execute(tagOwner, field, playPtn, healedCard, situation);
			}
			else
			{
				argumentExpressions.Execute(tagOwner, field, playPtn, situation);
			}
		}
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		switch (tag.Type)
		{
		case AIPlayTagType.HealToken:
			_healTokenTags = AIParamQuery.AddElementToList(tag, _healTokenTags);
			break;
		case AIPlayTagType.HealAttachTag:
			HealAttachTagList = AIParamQuery.AddElementToList(tag, HealAttachTagList);
			break;
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (HealAttachTagList != null)
		{
			HealAttachTagList.Clear();
		}
		if (_healTokenTags != null)
		{
			_healTokenTags.Clear();
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		if (!RemoveTagFromList(HealAttachTagList, tag))
		{
			RemoveTagFromList(_healTokenTags, tag);
		}
	}
}
