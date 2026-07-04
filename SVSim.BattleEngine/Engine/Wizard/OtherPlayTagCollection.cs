using System;
using System.Collections.Generic;

namespace Wizard;

public class OtherPlayTagCollection : TagCollection
{
	public static readonly AIPlayTagType[] ManagedTagTypes = new AIPlayTagType[11]
	{
		AIPlayTagType.OtherPlayEvo,
		AIPlayTagType.OtherPlayDamage,
		AIPlayTagType.OtherPlayRecoverPp,
		AIPlayTagType.OtherPlayAttachTag,
		AIPlayTagType.OtherPlayDestroy,
		AIPlayTagType.OtherPlayBuff,
		AIPlayTagType.OtherPlayToken,
		AIPlayTagType.OtherPlayRemoveTag,
		AIPlayTagType.OtherPlayQuick,
		AIPlayTagType.OtherPlayBounce,
		AIPlayTagType.OtherEnhanceEvo
	};

	private List<AIPlayTag> _removalTags;

	private List<AIPlayTag> _otherPlayTokenList;

	private List<AIPlayTag> _otherPlayRecoverPpList;

	public static readonly AIPlayTagType[] TAG_FOR_REMOVAL_CHECK = new AIPlayTagType[4]
	{
		AIPlayTagType.OtherPlayDamage,
		AIPlayTagType.OtherPlayDestroy,
		AIPlayTagType.OtherPlayBuff,
		AIPlayTagType.OtherPlayBounce
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => ManagedTagTypes;

	public bool HasRemovalTags
	{
		get
		{
			if (_removalTags != null)
			{
				return _removalTags.Count > 0;
			}
			return false;
		}
	}

	public bool HasOtherPlayTokenTags
	{
		get
		{
			if (_otherPlayTokenList != null)
			{
				return _otherPlayTokenList.Count > 0;
			}
			return false;
		}
	}

	public OtherPlayTagCollection()
		: base(TagCollectionType.WhenOtherPlay)
	{
		_removalTags = null;
	}

	private OtherPlayTagCollection(OtherPlayTagCollection tagCollection)
		: base(tagCollection)
	{
		_removalTags = AIPlayTagInitializingUtility.CloneTagList(tagCollection._removalTags);
		if (tagCollection._otherPlayRecoverPpList != null)
		{
			_otherPlayRecoverPpList = AIParamQuery.AddRangeToList(tagCollection._otherPlayRecoverPpList, _otherPlayRecoverPpList);
		}
		if (tagCollection._otherPlayTokenList != null)
		{
			_otherPlayTokenList = AIParamQuery.AddRangeToList(tagCollection._otherPlayTokenList, _otherPlayTokenList);
		}
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (IsRemovalCheckTagType(tag.Type))
		{
			_removalTags = AIParamQuery.AddElementToList(tag, _removalTags);
		}
		switch (tag.Type)
		{
		case AIPlayTagType.OtherPlayRecoverPp:
			_otherPlayRecoverPpList = AIParamQuery.AddElementToList(tag, _otherPlayRecoverPpList);
			break;
		case AIPlayTagType.OtherPlayToken:
			_otherPlayTokenList = AIParamQuery.AddElementToList(tag, _otherPlayTokenList);
			break;
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_removalTags != null)
		{
			_removalTags.Clear();
			_removalTags = null;
		}
		if (_otherPlayRecoverPpList != null)
		{
			_otherPlayRecoverPpList.Clear();
			_otherPlayRecoverPpList = null;
		}
		if (_otherPlayTokenList != null)
		{
			_otherPlayTokenList.Clear();
			_otherPlayTokenList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		base.RemoveTagFromManagedTagList(tag);
		RemoveTagFromList(_removalTags, tag);
		if (!RemoveTagFromList(_otherPlayRecoverPpList, tag))
		{
			RemoveTagFromList(_otherPlayTokenList, tag);
		}
	}

	public override TagCollection Clone()
	{
		return new OtherPlayTagCollection(this);
	}

	private bool IsRemovalCheckTagType(AIPlayTagType type)
	{
		return Array.IndexOf(TAG_FOR_REMOVAL_CHECK, type) >= 0;
	}

	public void EnqueueConditionPassedTags(AIVirtualCard holder, AIVirtualTargetSelectAction situation, AIVirtualField field, List<int> playPtn, PlaySimulationType playType)
	{
		if (!OtherWhenPlayCommonCheckCondition(holder, field, playPtn, situation) || base.TagList == null || base.TagList.Count <= 0)
		{
			return;
		}
		AIVirtualCard triggerCard = situation.Actor;
		AISkillProcessInformation aISkillProcessInformation = situation.RegisterNewProcessInfo(triggerCard, AISituationTriggerInformation.TriggerType.WhenPlay);
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag tag = base.TagList[i];
			if (!CheckTagCondition(tag, holder, situation, field, playPtn, playType))
			{
				continue;
			}
			aISkillProcessInformation.AddExecutingAction(delegate
			{
				if (tag.ArgumentExpressions is AIOtherWhenPlayTagArgument aIOtherWhenPlayTagArgument)
				{
					if (aIOtherWhenPlayTagArgument is IAIRemoveTagArgument iAIRemoveTagArgument)
					{
						iAIRemoveTagArgument.Execute(holder, field, situation, tag, playPtn);
					}
					else
					{
						aIOtherWhenPlayTagArgument.Execute(holder, field, playPtn, triggerCard, situation);
					}
				}
				else
				{
					AIConsoleUtility.LogError("EnqueueConditionPassedTags(): Unexcepted tag class.");
				}
			});
		}
	}

	private bool CheckTagCondition(AIPlayTag tag, AIVirtualCard holder, AIVirtualTargetSelectAction situation, AIVirtualField field, List<int> playPtn, PlaySimulationType playType)
	{
		if (!tag.CheckCondition(holder, playPtn, field, situation))
		{
			return false;
		}
		if (tag.ArgumentExpressions is AIOtherWhenPlayTagArgument { RequiredPlayType: not PlaySimulationType.Undefined } aIOtherWhenPlayTagArgument && aIOtherWhenPlayTagArgument.RequiredPlayType != playType)
		{
			return false;
		}
		return true;
	}

	public int GetTotalOtherPlayRecoverPp(AIVirtualCard tagOwner, AIVirtualCard playCard, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_otherPlayRecoverPpList == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < _otherPlayRecoverPpList.Count; i++)
		{
			AIPlayTag aIPlayTag = _otherPlayRecoverPpList[i];
			if (aIPlayTag.ArgumentExpressions is AIOtherWhenPlayRecoverPp aIOtherWhenPlayRecoverPp && aIOtherWhenPlayRecoverPp.CheckValidPlayCard(tagOwner, playCard, field, playPtn, situation) && aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				int recoverPpValue = aIOtherWhenPlayRecoverPp.GetRecoverPpValue(tagOwner, field, playPtn, situation);
				recoverPpValue = Math.Max(0, recoverPpValue);
				num += recoverPpValue;
			}
		}
		return num;
	}

	protected virtual bool OtherWhenPlayCommonCheckCondition(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return true;
	}

	public void RemovalPrediction(AIVirtualCard owner, AIVirtualCard triggerCard, AIVirtualCard target, LifeRecord life, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!HasRemovalTags || !OtherWhenPlayCommonCheckCondition(owner, field, playPtn, situation))
		{
			return;
		}
		for (int i = 0; i < _removalTags.Count; i++)
		{
			AIPlayTag aIPlayTag = _removalTags[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation))
			{
				if (aIPlayTag.ArgumentExpressions is AIOtherWhenPlayTagArgument aIOtherWhenPlayTagArgument && aIOtherWhenPlayTagArgument.CheckTriggerLegal(triggerCard, owner, playPtn, situation))
				{
					aIOtherWhenPlayTagArgument.TargetLifePrediction(target, owner, field, playPtn, situation, life);
				}
				if (life.CurrentLife <= 0)
				{
					break;
				}
			}
		}
	}

	public void MultipleRemovalPrediction(AIVirtualCard owner, AIVirtualCard triggerCard, List<AIVirtualCard> targetList, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<LifeRecord> lifeList)
	{
		if (!HasRemovalTags || !OtherWhenPlayCommonCheckCondition(owner, field, playPtn, situation))
		{
			return;
		}
		for (int i = 0; i < _removalTags.Count; i++)
		{
			AIPlayTag aIPlayTag = _removalTags[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIOtherWhenPlayTagArgument aIOtherWhenPlayTagArgument && aIOtherWhenPlayTagArgument.CheckTriggerLegal(triggerCard, owner, playPtn, situation))
			{
				aIOtherWhenPlayTagArgument.MultipleTargetLifePrediction(targetList, owner, field, playPtn, situation, lifeList);
			}
		}
	}

	public List<AITokenInformation> GetAllySideTokenIds(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (situation == null || situation.ActionType != AIOperationType.PLAY)
		{
			AIConsoleUtility.LogError("OtherPlayTagCollection.GetAllySideTokenIds() error!! situation == null or situation.ActionType != PLAY");
			return null;
		}
		if (!HasOtherPlayTokenTags)
		{
			return null;
		}
		AIVirtualCard actor = situation.Actor;
		List<AITokenInformation> list = null;
		for (int i = 0; i < _otherPlayTokenList.Count; i++)
		{
			AIPlayTag aIPlayTag = _otherPlayTokenList[i];
			if (aIPlayTag.ArgumentExpressions is AIOtherWhenPlayToken aIOtherWhenPlayToken && aIOtherWhenPlayToken.CheckTriggerLegal(actor, tagOwner, playPtn, situation) && aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				List<AITokenInformation> allySideTokenIds = aIOtherWhenPlayToken.GetAllySideTokenIds(tagOwner, field, playPtn, situation);
				if (allySideTokenIds != null)
				{
					list = AIParamQuery.AddRangeToList(allySideTokenIds, list);
				}
			}
		}
		return list;
	}

	public AITokenIdCollection GetBothSideTokenIdCollection(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (situation == null || situation.ActionType != AIOperationType.PLAY)
		{
			AIConsoleUtility.LogError("OtherPlayTagCollection.GetBothSideTokenIdCollection() error!! situation == null or situation.ActionType != PLAY");
			return null;
		}
		if (!HasOtherPlayTokenTags)
		{
			return null;
		}
		AIVirtualCard actor = situation.Actor;
		AITokenIdCollection aITokenIdCollection = null;
		for (int i = 0; i < _otherPlayTokenList.Count; i++)
		{
			AIPlayTag aIPlayTag = _otherPlayTokenList[i];
			if (aIPlayTag.ArgumentExpressions is AIOtherWhenPlayToken aIOtherWhenPlayToken && aIOtherWhenPlayToken.CheckTriggerLegal(actor, tagOwner, playPtn, situation) && aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				AITokenIdCollection bothSideTokenIdCollection = aIOtherWhenPlayToken.GetBothSideTokenIdCollection(tagOwner, field, playPtn, situation);
				aITokenIdCollection = AITokenIdCollection.CombineTwoCollection(aITokenIdCollection, bothSideTokenIdCollection);
			}
		}
		return aITokenIdCollection;
	}
}
