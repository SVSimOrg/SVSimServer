using System.Collections.Generic;

namespace Wizard;

public class OtherEvoTagCollection : TagCollection
{
	private static readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[7]
	{
		AIPlayTagType.OtherEvoBuff,
		AIPlayTagType.OtherEvoDamage,
		AIPlayTagType.OtherEvoBanish,
		AIPlayTagType.OtherEvoSubtractCountdown,
		AIPlayTagType.OtherEvoEvo,
		AIPlayTagType.OtherEvoToken,
		AIPlayTagType.OtherEvoShield
	};

	private List<AIPlayTag> _otherEvoTokenList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public OtherEvoTagCollection()
		: base(TagCollectionType.WhenOtherEvo)
	{
	}

	private OtherEvoTagCollection(OtherEvoTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (tag.Type == AIPlayTagType.OtherEvoToken)
		{
			_otherEvoTokenList = AIParamQuery.AddElementToList(tag, _otherEvoTokenList);
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_otherEvoTokenList != null)
		{
			_otherEvoTokenList.Clear();
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		base.RemoveTagFromManagedTagList(tag);
		RemoveTagFromList(_otherEvoTokenList, tag);
	}

	public override TagCollection Clone()
	{
		return new OtherEvoTagCollection(this);
	}

	public void RegisterPassedConditionTags(AIVirtualCard tagOwner, AIVirtualCard evolver, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead || !base.HasTag || tagOwner.IsSameCard(evolver))
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
			situation.RegisterNewProcessInfo(evolver, AISituationTriggerInformation.TriggerType.Evolver).AddExecutingAction(delegate
			{
				ExecuteTagAction(passedConditionTags, tagOwner, evolver, field, playPtn, situation);
			});
		}
	}

	private void ExecuteTagAction(List<AIPlayTag> tags, AIVirtualCard tagOwner, AIVirtualCard evolver, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (tagOwner.IsDead)
		{
			return;
		}
		for (int i = 0; i < tags.Count; i++)
		{
			if (tags[i].ArgumentExpressions is AITriggerAndTargetFiltersTagBase aITriggerAndTargetFiltersTagBase)
			{
				aITriggerAndTargetFiltersTagBase.Execute(tagOwner, field, playPtn, evolver, situation);
			}
		}
	}

	public bool CheckDestroyByOtherEvoTags(AIVirtualCard tagOwner, AIVirtualCard destroyTarget, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
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

	public int GetOtherEvoTokenSummonCount(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_otherEvoTokenList == null || _otherEvoTokenList.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < _otherEvoTokenList.Count; i++)
		{
			AIPlayTag aIPlayTag = _otherEvoTokenList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIOtherEvoToken aIOtherEvoToken)
			{
				num += aIOtherEvoToken.GetTokenCount(tagOwner, field, playPtn, situation);
			}
		}
		return num;
	}
}
