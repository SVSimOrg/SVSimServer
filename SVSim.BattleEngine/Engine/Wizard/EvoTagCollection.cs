using System.Collections.Generic;

namespace Wizard;

public class EvoTagCollection : TagCollection, IAITargetSelectTagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[31]
	{
		AIPlayTagType.EvoBuff,
		AIPlayTagType.EvoHeal,
		AIPlayTagType.EvoDamage,
		AIPlayTagType.EvoBanish,
		AIPlayTagType.EvoSubtractCountdown,
		AIPlayTagType.EvoEvo,
		AIPlayTagType.EvoToken,
		AIPlayTagType.EvoTokenDraw,
		AIPlayTagType.EvoDestroy,
		AIPlayTagType.EvoRecoverPp,
		AIPlayTagType.EvoSetLeaderMaxLife,
		AIPlayTagType.EvoDiscard,
		AIPlayTagType.EvoMetamorphose,
		AIPlayTagType.EvoHandMetamorphose,
		AIPlayTagType.EvoBounce,
		AIPlayTagType.EvoRush,
		AIPlayTagType.EvoQuick,
		AIPlayTagType.EvoGuard,
		AIPlayTagType.EvoAttachTag,
		AIPlayTagType.EvoAttackableCount,
		AIPlayTagType.EvoHandBuff,
		AIPlayTagType.EvoSetStatus,
		AIPlayTagType.EvoChangeCost,
		AIPlayTagType.EvoHandSelect,
		AIPlayTagType.EvoShield,
		AIPlayTagType.EvoDamageCut,
		AIPlayTagType.EvoReanimate,
		AIPlayTagType.EvoAddStack,
		AIPlayTagType.EvoKiller,
		AIPlayTagType.EvoDrain,
		AIPlayTagType.EvoAddDeck
	};

	private List<AIPlayTag> _evoTokenList;

	private List<AIPlayTag> _evoTokenDrawList;

	private List<AIPlayTag> _targetSelectList;

	private List<AIPlayTag> _secondTargetSelectList;

	private List<AIPlayTag> _evoAttachTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public bool HasEvoToken
	{
		get
		{
			if (_evoTokenList != null)
			{
				return _evoTokenList.Count > 0;
			}
			return false;
		}
	}

	public EvoTagCollection()
		: base(TagCollectionType.WhenEvo)
	{
		_evoTokenList = null;
		_evoTokenDrawList = null;
		_targetSelectList = null;
		_secondTargetSelectList = null;
		_evoAttachTagList = null;
	}

	private EvoTagCollection(EvoTagCollection tagCollection)
		: base(tagCollection)
	{
		_evoTokenList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._evoTokenList);
		_evoTokenDrawList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._evoTokenDrawList);
		_targetSelectList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._targetSelectList);
		_secondTargetSelectList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._secondTargetSelectList);
		_evoAttachTagList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._evoAttachTagList);
	}

	public override TagCollection Clone()
	{
		return new EvoTagCollection(this);
	}

	public void Execute(AIVirtualField field, AIVirtualCard tagOwner, AIVirtualCard evolver, AISituationInfo situation)
	{
		if (!base.HasTag || tagOwner.IsDead || evolver.IsDead)
		{
			return;
		}
		List<int> conditionPassedIndexList = new List<int>();
		List<int> playPtn = field.BestPlayPtn;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (!aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				continue;
			}
			if (aIPlayTag.ArgumentExpressions is AIEvoTagArgument aIEvoTagArgument)
			{
				if (aIEvoTagArgument.IsImmediate)
				{
					aIEvoTagArgument.Execute(tagOwner, field, playPtn, situation);
					continue;
				}
			}
			else
			{
				AIConsoleUtility.LogError("EvoTagCollection.Execute() error!! tag " + aIPlayTag.Type.ToString() + " is not AIEvoTagArgument!!!!!");
			}
			conditionPassedIndexList.Add(i);
		}
		if (conditionPassedIndexList.Count <= 0)
		{
			return;
		}
		situation.RegisterNewProcessInfo(evolver, AISituationTriggerInformation.TriggerType.Evolver).AddExecutingAction(delegate
		{
			if (!tagOwner.IsDead)
			{
				for (int j = 0; j < conditionPassedIndexList.Count; j++)
				{
					base.TagList[conditionPassedIndexList[j]].ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
				}
			}
		});
	}

	public bool CheckDestroyByEvolveTags(AIVirtualCard tagOwner, AISituationInfo situation, AIVirtualCard destroyTarget)
	{
		AIVirtualCard actor = situation.Actor;
		if (actor == null || actor.IsDead || destroyTarget == null || destroyTarget.IsDead || !base.HasTag)
		{
			return false;
		}
		AIVirtualField selfField = actor.SelfField;
		if (base.HasTag)
		{
			for (int i = 0; i < base.TagList.Count; i++)
			{
				AIPlayTag aIPlayTag = base.TagList[i];
				if (aIPlayTag.CheckCondition(tagOwner, selfField.BestPlayPtn, selfField, situation) && aIPlayTag.ArgumentExpressions is AIEvoTagArgument aIEvoTagArgument && aIEvoTagArgument.IsTargetGoingToDie(tagOwner, destroyTarget, situation))
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetEvoTokenSummonCount(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		int num = 0;
		if (HasEvoToken)
		{
			AIVirtualField selfField = tagOwner.SelfField;
			for (int i = 0; i < _evoTokenList.Count; i++)
			{
				AIPlayTag aIPlayTag = _evoTokenList[i];
				if (aIPlayTag.ArgumentExpressions is AIEvoToken && aIPlayTag.CheckCondition(tagOwner, playPtn, selfField, situation))
				{
					int tokenCount = (aIPlayTag.ArgumentExpressions as AIEvoToken).GetTokenCount(tagOwner, playPtn, situation);
					if (tokenCount > 0)
					{
						num += tokenCount;
					}
				}
			}
		}
		return num;
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

	public AIVirtualTargetSelectInfo GetSelectInfo(AIVirtualCard owner, AIVirtualField field, AISituationInfo situation, AIScriptTokenArgType selectGroup = AIScriptTokenArgType.TARGET_SELECT)
	{
		if (!base.HasTag)
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
			list = _secondTargetSelectList;
			break;
		}
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		List<int> bestPlayPtn = field.BestPlayPtn;
		for (int i = 0; i < list.Count; i++)
		{
			AIPlayTag aIPlayTag = list[i];
			if (aIPlayTag.CheckCondition(owner, emptyPlayPtn, field, situation) && aIPlayTag.ArgumentExpressions is AIEvoTagArgument aIEvoTagArgument)
			{
				List<AIVirtualCard> targetSelectCandidates = aIEvoTagArgument.GetTargetSelectCandidates(owner, field, situation);
				if (targetSelectCandidates != null && targetSelectCandidates.Count > 0)
				{
					AIRemovalType removalType = aIEvoTagArgument.GetRemovalType();
					targetSelectCandidates = AITargetSelectFilteringUtility.ExecuteTargetFilteringTags(owner, targetSelectCandidates, bestPlayPtn, situation);
					int selectCount = aIEvoTagArgument.GetSelectCount(owner, field, emptyPlayPtn, situation);
					TargetSelectType simulationTargetSelectType = aIEvoTagArgument.SimulationTargetSelectType;
					return new AIVirtualTargetSelectInfo(selectCount, targetSelectCandidates, simulationTargetSelectType, aIEvoTagArgument.IsForbiddenSelectedTarget, aIPlayTag, removalType);
				}
			}
		}
		return null;
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		switch (tag.Type)
		{
		case AIPlayTagType.EvoToken:
			_evoTokenList = AIParamQuery.AddElementToList(tag, _evoTokenList);
			break;
		case AIPlayTagType.EvoTokenDraw:
			_evoTokenDrawList = AIParamQuery.AddElementToList(tag, _evoTokenDrawList);
			break;
		case AIPlayTagType.EvoAttachTag:
			_evoAttachTagList = AIParamQuery.AddElementToList(tag, _evoAttachTagList);
			break;
		}
		if (tag.ArgumentExpressions is AIEvoTagArgument { SelectType: var selectType })
		{
			switch (selectType)
			{
			case AIScriptTokenArgType.TARGET_SELECT:
				_targetSelectList = AIParamQuery.AddElementToList(tag, _targetSelectList);
				break;
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				_secondTargetSelectList = AIParamQuery.AddElementToList(tag, _secondTargetSelectList);
				break;
			}
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (HasEvoToken)
		{
			_evoTokenList.Clear();
			_evoTokenList = null;
		}
		if (_evoTokenDrawList != null)
		{
			_evoTokenDrawList.Clear();
			_evoTokenDrawList = null;
		}
		if (_evoAttachTagList != null)
		{
			_evoAttachTagList.Clear();
			_evoAttachTagList = null;
		}
		if (_targetSelectList != null)
		{
			_targetSelectList.Clear();
			_targetSelectList = null;
		}
		if (_secondTargetSelectList != null)
		{
			_secondTargetSelectList.Clear();
			_secondTargetSelectList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		RemoveTagFromList(_evoTokenList, tag);
		RemoveTagFromList(_evoTokenDrawList, tag);
		RemoveTagFromList(_evoAttachTagList, tag);
		if (!RemoveTagFromList(_targetSelectList, tag))
		{
			RemoveTagFromList(_secondTargetSelectList, tag);
		}
	}
}
