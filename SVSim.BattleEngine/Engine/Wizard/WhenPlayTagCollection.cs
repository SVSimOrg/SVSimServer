using System;
using System.Collections.Generic;

namespace Wizard;

public class WhenPlayTagCollection : TagCollection, IAITargetSelectTagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[1] { AIPlayTagType.None };

	private List<AIPlayTag> _removalTags;

	protected List<AIPlayTag> _healTags;

	protected List<AIPlayTag> _copyTagTags;

	private List<AIPlayTag> _targetSelectList;

	private List<AIPlayTag> _secondSelectList;

	protected List<AIPlayTag> _tokenDrawList;

	protected List<AIPlayTag> _summonTokenList;

	protected List<AIPlayTag> _recoverPpTagList;

	protected List<AIPlayTag> _removeKeywordSkillTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public bool HasSummonTokenTags
	{
		get
		{
			if (_summonTokenList != null)
			{
				return _summonTokenList.Count > 0;
			}
			return false;
		}
	}

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

	public bool HasRemoveKeywordSkillTags
	{
		get
		{
			if (_removeKeywordSkillTagList != null)
			{
				return _removeKeywordSkillTagList.Count > 0;
			}
			return false;
		}
	}

	public WhenPlayTagCollection(TagCollectionType type)
		: base(type)
	{
		_removalTags = null;
		_healTags = null;
		_copyTagTags = null;
		_targetSelectList = null;
		_secondSelectList = null;
		_tokenDrawList = null;
		_summonTokenList = null;
		_recoverPpTagList = null;
		_removeKeywordSkillTagList = null;
	}

	protected WhenPlayTagCollection(WhenPlayTagCollection tagCollection)
		: base(tagCollection)
	{
		_removalTags = tagCollection.CreateRemovalListClone();
		_healTags = AIPlayTagInitializingUtility.CloneTagList(tagCollection._healTags);
		_targetSelectList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._targetSelectList);
		_secondSelectList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._secondSelectList);
		_tokenDrawList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._tokenDrawList);
		_summonTokenList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._summonTokenList);
		_recoverPpTagList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._recoverPpTagList);
		_removeKeywordSkillTagList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._removeKeywordSkillTagList);
	}

	public override TagCollection Clone()
	{
		return new WhenPlayTagCollection(this);
	}

	public List<AIPlayTag> CreateRemovalListClone()
	{
		return AIPlayTagInitializingUtility.CloneTagList(_removalTags);
	}

	public void EnqueueConditionPassedTags(AIVirtualTargetSelectAction situation, AIVirtualField field, List<int> playPtn, bool isRemovalCheck)
	{
		AIVirtualCard owner = situation.Actor;
		if (!WhenPlayCommonCheckCondition(owner, field, playPtn, situation))
		{
			return;
		}
		List<AIPlayTag> list = (isRemovalCheck ? _removalTags : base.TagList);
		if (list == null || list.Count <= 0)
		{
			return;
		}
		AISkillProcessInformation aISkillProcessInformation = situation.RegisterNewProcessInfo(situation.Actor, AISituationTriggerInformation.TriggerType.Undefined);
		for (int i = 0; i < list.Count; i++)
		{
			AIPlayTag tag = list[i];
			if (!tag.CheckCondition(owner, playPtn, field, situation))
			{
				continue;
			}
			if (tag.ArgumentExpressions is AIWhenPlayTagArgument aIWhenPlayTagArgument)
			{
				if (aIWhenPlayTagArgument.IsImmediate)
				{
					aIWhenPlayTagArgument.Execute(owner, field, playPtn, situation);
					continue;
				}
			}
			else
			{
				AIConsoleUtility.LogError("WhenPlayTagCollection.EnqueueConditionPassedTags() error!! tag " + tag.Type.ToString() + " is not AIWhenPlayTagArgument!!!!!");
			}
			aISkillProcessInformation.AddExecutingAction(delegate
			{
				tag.ArgumentExpressions.Execute(owner, field, playPtn, situation);
			});
		}
	}

	public void ExecuteForPlayPtnEvaluation(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!base.HasTag || !WhenPlayCommonCheckCondition(owner, field, playPtn, situation))
		{
			return;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIWhenPlayTagArgument aIWhenPlayTagArgument)
			{
				aIWhenPlayTagArgument.ExecuteForPlayPtnEvaluation(owner, field, playPtn, situation);
			}
		}
	}

	public void PseudoExecute(AIVirtualField field, PlayedCardInfo playInfo, AISinglePlayptnRecord record, AIVirtualTargetSelectAction situation)
	{
		AIVirtualCard card = playInfo.Card;
		List<int> playPtn = record.PlayPtn;
		if (situation != null)
		{
			situation.ReservedPlayType = playInfo.PlayType;
		}
		if (!base.HasTag || !WhenPlayCommonCheckCondition(card, field, playPtn, situation))
		{
			return;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.CheckCondition(card, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIWhenPlayTagArgument aIWhenPlayTagArgument)
			{
				aIWhenPlayTagArgument.PseudoExecute(field, record, playInfo, situation);
			}
		}
	}

	public void RemovalPrediction(AIVirtualCard owner, AIVirtualCard target, LifeRecord life, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!HasRemovalTags || !WhenPlayCommonCheckCondition(owner, field, playPtn, situation))
		{
			return;
		}
		for (int i = 0; i < _removalTags.Count; i++)
		{
			AIPlayTag aIPlayTag = _removalTags[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation))
			{
				if (aIPlayTag.ArgumentExpressions is AIWhenPlayTagArgument aIWhenPlayTagArgument)
				{
					aIWhenPlayTagArgument.TargetLifePrediction(target, owner, field, playPtn, situation, life);
				}
				if (life.CurrentLife <= 0)
				{
					break;
				}
			}
		}
	}

	public void MultipleRemovalPrediction(AIVirtualCard owner, List<AIVirtualCard> targetList, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<LifeRecord> lifeList)
	{
		if (!HasRemovalTags || !WhenPlayCommonCheckCondition(owner, field, playPtn, situation))
		{
			return;
		}
		for (int i = 0; i < _removalTags.Count; i++)
		{
			AIPlayTag aIPlayTag = _removalTags[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIWhenPlayTagArgument aIWhenPlayTagArgument)
			{
				aIWhenPlayTagArgument.MultipleTargetLifePrediction(targetList, owner, field, playPtn, situation, lifeList);
			}
		}
	}

	public List<AIVirtualCard> RemoveGuardPrediction(AIVirtualCard owner, List<AIVirtualCard> targetList, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!HasRemoveKeywordSkillTags || !WhenPlayCommonCheckCondition(owner, field, playPtn, situation))
		{
			return null;
		}
		List<AIVirtualCard> list = null;
		for (int i = 0; i < _removeKeywordSkillTagList.Count; i++)
		{
			AIPlayTag aIPlayTag = _removeKeywordSkillTagList[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIWhenPlayRemoveKeywordSkill aIWhenPlayRemoveKeywordSkill)
			{
				list = AIParamQuery.AddRangeToList(aIWhenPlayRemoveKeywordSkill.RemoveGuardPredictionInPlayOut(targetList, owner, playPtn, situation), list);
			}
		}
		return list;
	}

	public void AddSelectInfoToSelectInfoList(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, ref List<AIVirtualTargetSelectInfo> selectInfoList)
	{
		AIVirtualTargetSelectInfo selectInfo = GetSelectInfo(owner, field, situation);
		if (selectInfo != null)
		{
			selectInfoList = AIParamQuery.AddElementToList(selectInfo, selectInfoList);
			AIVirtualTargetSelectInfo selectInfo2 = GetSelectInfo(owner, field, situation, AIScriptTokenArgType.SECOND_TARGET_SELECT);
			if (selectInfo2 != null)
			{
				selectInfoList = AIParamQuery.AddElementToList(selectInfo2, selectInfoList);
			}
		}
	}

	public AITokenIdCollection GetBothSideWhenPlaySummonTokenIdList(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (!WhenPlayCommonCheckCondition(tagOwner, field, playPtn, situation))
		{
			return null;
		}
		AITokenIdCollection aITokenIdCollection = null;
		if (_summonTokenList != null)
		{
			for (int i = 0; i < _summonTokenList.Count; i++)
			{
				AIPlayTag aIPlayTag = _summonTokenList[i];
				if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIWhenPlayTokenArgumentBase aIWhenPlayTokenArgumentBase)
				{
					AITokenIdCollection bothSideTokenIdCollection = aIWhenPlayTokenArgumentBase.GetBothSideTokenIdCollection(tagOwner, field, playPtn, situation);
					if (bothSideTokenIdCollection != null && bothSideTokenIdCollection.HasToken)
					{
						aITokenIdCollection = AITokenIdCollection.CombineTwoCollection(aITokenIdCollection, bothSideTokenIdCollection);
					}
				}
			}
		}
		if (aITokenIdCollection == null || !aITokenIdCollection.HasToken)
		{
			return null;
		}
		return aITokenIdCollection;
	}

	public List<AITokenInformation> GetAllySideWhenPlaySummonTokenIdList(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_summonTokenList == null || _summonTokenList.Count <= 0)
		{
			return null;
		}
		if (!WhenPlayCommonCheckCondition(tagOwner, field, playPtn, situation))
		{
			return null;
		}
		List<AITokenInformation> list = null;
		for (int i = 0; i < _summonTokenList.Count; i++)
		{
			AIPlayTag aIPlayTag = _summonTokenList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIWhenPlayTokenArgumentBase aIWhenPlayTokenArgumentBase)
			{
				List<AITokenInformation> allyTokenIdList = aIWhenPlayTokenArgumentBase.GetAllyTokenIdList(tagOwner, field, playPtn, situation);
				if (allyTokenIdList != null && allyTokenIdList.Count > 0)
				{
					list = AIParamQuery.AddRangeToList(allyTokenIdList, list);
				}
			}
		}
		return list;
	}

	public AIVirtualTargetSelectInfo GetSelectInfo(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, AIScriptTokenArgType selectGroup = AIScriptTokenArgType.TARGET_SELECT)
	{
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		List<int> bestPlayPtn = field.BestPlayPtn;
		List<AIPlayTag> conditionPassedTargetSelectTagList = GetConditionPassedTargetSelectTagList(owner, field, situation, emptyPlayPtn, selectGroup);
		if (conditionPassedTargetSelectTagList == null || conditionPassedTargetSelectTagList.Count <= 0)
		{
			return null;
		}
		for (int i = 0; i < conditionPassedTargetSelectTagList.Count; i++)
		{
			AIPlayTag aIPlayTag = conditionPassedTargetSelectTagList[i];
			AIWhenPlayTagArgument aIWhenPlayTagArgument = aIPlayTag.ArgumentExpressions as AIWhenPlayTagArgument;
			List<AIVirtualCard> targetSelectCandidates = aIWhenPlayTagArgument.GetTargetSelectCandidates(owner, field, situation);
			int selectCount = aIWhenPlayTagArgument.GetSelectCount(owner, field, emptyPlayPtn, situation);
			if (targetSelectCandidates != null && targetSelectCandidates.Count > 0 && selectCount > 0)
			{
				targetSelectCandidates = AITargetSelectFilteringUtility.ExecuteTargetFilteringTags(owner, targetSelectCandidates, bestPlayPtn, situation, selectCount);
				AIRemovalType removalType = aIWhenPlayTagArgument.GetRemovalType();
				TargetSelectType targetSelectType = aIWhenPlayTagArgument.GetTargetSelectType();
				return new AIVirtualTargetSelectInfo(selectCount, targetSelectCandidates, targetSelectType, aIWhenPlayTagArgument.IsForbiddenSelectedTarget, aIPlayTag, removalType);
			}
		}
		return null;
	}

	public List<AIPlayTag> GetConditionPassedTargetSelectTagList(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, List<int> playPtn, AIScriptTokenArgType selectGroup = AIScriptTokenArgType.TARGET_SELECT)
	{
		if (!base.HasTag || !WhenPlayCommonCheckCondition(owner, field, playPtn, situation))
		{
			return null;
		}
		List<AIPlayTag> list = null;
		switch (selectGroup)
		{
		case AIScriptTokenArgType.TARGET_SELECT:
			list = _targetSelectList;
			break;
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			list = _secondSelectList;
			break;
		}
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		List<AIPlayTag> list2 = null;
		for (int i = 0; i < list.Count; i++)
		{
			AIPlayTag aIPlayTag = list[i];
			if (aIPlayTag.CheckCondition(owner, playPtn, field, situation))
			{
				list2 = AIParamQuery.AddElementToList(aIPlayTag, list2);
			}
		}
		return list2;
	}

	protected virtual bool IsRemovalCheckTagType(AIPlayTagType type)
	{
		return false;
	}

	public bool IsDelayHeal(AIVirtualCard tagOwner, AIVirtualField field, AISituationInfo situation)
	{
		if (_healTags == null || _healTags.Count <= 0)
		{
			return false;
		}
		List<int> bestPlayPtn = field.BestPlayPtn;
		if (!WhenPlayCommonCheckCondition(tagOwner, field, bestPlayPtn, situation))
		{
			return false;
		}
		for (int i = 0; i < _healTags.Count; i++)
		{
			AIPlayTag aIPlayTag = _healTags[i];
			if (aIPlayTag.CheckCondition(tagOwner, bestPlayPtn, field, situation) && (aIPlayTag.ArgumentExpressions as AIWhenPlayHeal).IsDelayHeal(tagOwner, field, situation))
			{
				return true;
			}
		}
		return false;
	}

	public int GetHealCount(AIVirtualCard owner, List<AIVirtualCard> wishHealTargetList, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_healTags == null || _healTags.Count <= 0)
		{
			return 0;
		}
		if (!WhenPlayCommonCheckCondition(owner, field, playPtn, situation))
		{
			return 0;
		}
		int num = 0;
		List<AIVirtualCard> bothClassAndInplayCards = field.CardListSet.BothClassAndInplayCards;
		for (int i = 0; i < _healTags.Count; i++)
		{
			AIPlayTag aIPlayTag = _healTags[i];
			if (!aIPlayTag.CheckCondition(owner, playPtn, field, situation))
			{
				continue;
			}
			bool flag = false;
			AIWhenPlayHeal aIWhenPlayHeal = aIPlayTag.ArgumentExpressions as AIWhenPlayHeal;
			for (int j = 0; j < wishHealTargetList.Count; j++)
			{
				if (AIFilteringUtility.CheckMatchTargetFiltering(wishHealTargetList[j], bothClassAndInplayCards, aIWhenPlayHeal.Filters, playPtn, owner, situation))
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				num++;
			}
		}
		return num;
	}

	protected virtual bool WhenPlayCommonCheckCondition(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		return true;
	}

	public int GetWhenPlayRecoverPp(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		int num = 0;
		if (tagOwner == null || tagOwner.IsDead || _recoverPpTagList == null)
		{
			return num;
		}
		AIVirtualField selfField = tagOwner.SelfField;
		for (int i = 0; i < _recoverPpTagList.Count; i++)
		{
			AIPlayTag aIPlayTag = _recoverPpTagList[i];
			if (aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation) && aIPlayTag.ArgumentExpressions is AIWhenPlayRecoverPp aIWhenPlayRecoverPp)
			{
				int playRecoverPp = aIWhenPlayRecoverPp.GetPlayRecoverPp(tagOwner, playPtn, selfField, situation);
				num += Math.Max(0, playRecoverPp);
			}
		}
		return num;
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		if (IsRemovalCheckTagType(tag.Type))
		{
			_removalTags = AIParamQuery.AddElementToList(tag, _removalTags);
		}
		if (tag.ArgumentExpressions is AIWhenPlayTagArgument { SelectType: var selectType })
		{
			switch (selectType)
			{
			case AIScriptTokenArgType.TARGET_SELECT:
				_targetSelectList = AIParamQuery.AddElementToList(tag, _targetSelectList);
				break;
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				_secondSelectList = AIParamQuery.AddElementToList(tag, _secondSelectList);
				break;
			}
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
		if (_healTags != null)
		{
			_healTags.Clear();
			_healTags = null;
		}
		if (_copyTagTags != null)
		{
			_copyTagTags.Clear();
			_copyTagTags = null;
		}
		if (_targetSelectList != null)
		{
			_targetSelectList.Clear();
			_targetSelectList = null;
		}
		if (_secondSelectList != null)
		{
			_secondSelectList.Clear();
			_secondSelectList = null;
		}
		if (_tokenDrawList != null)
		{
			_tokenDrawList.Clear();
			_tokenDrawList = null;
		}
		if (_summonTokenList != null)
		{
			_summonTokenList.Clear();
			_summonTokenList = null;
		}
		if (_recoverPpTagList != null)
		{
			_recoverPpTagList.Clear();
			_recoverPpTagList = null;
		}
		if (_removeKeywordSkillTagList != null)
		{
			_removeKeywordSkillTagList.Clear();
			_removeKeywordSkillTagList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		RemoveTagFromList(_removalTags, tag);
		RemoveTagFromList(_tokenDrawList, tag);
		RemoveTagFromList(_healTags, tag);
		RemoveTagFromList(_copyTagTags, tag);
		RemoveTagFromList(_summonTokenList, tag);
		if (!RemoveTagFromList(_targetSelectList, tag) && !RemoveTagFromList(_secondSelectList, tag) && !RemoveTagFromList(_recoverPpTagList, tag))
		{
			RemoveTagFromList(_removeKeywordSkillTagList, tag);
		}
	}
}
