using System.Collections.Generic;

namespace Wizard;

public class OtherAttackTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[6]
	{
		AIPlayTagType.OtherAttackBuff,
		AIPlayTagType.OtherAttackDamage,
		AIPlayTagType.OtherAttackHeal,
		AIPlayTagType.OtherAttackToken,
		AIPlayTagType.OtherAttackAttachTag,
		AIPlayTagType.OtherAttackRemoveTag
	};

	private List<AIPlayTag> _otherAttackBuffList;

	private List<AIPlayTag> _otherAttackDamageList;

	private List<AIPlayTag> _otherAttackTokenList;

	private List<AIPlayTag> _otherAttackAttachTagList;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public OtherAttackTagCollection()
		: base(TagCollectionType.WhenOtherAttack)
	{
		_otherAttackBuffList = null;
		_otherAttackDamageList = null;
		_otherAttackTokenList = null;
		_otherAttackAttachTagList = null;
	}

	private OtherAttackTagCollection(OtherAttackTagCollection tagCollection)
		: base(tagCollection)
	{
		_otherAttackBuffList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._otherAttackBuffList);
		_otherAttackDamageList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._otherAttackDamageList);
		_otherAttackTokenList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._otherAttackTokenList);
		_otherAttackAttachTagList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._otherAttackAttachTagList);
	}

	public override TagCollection Clone()
	{
		return new OtherAttackTagCollection(this);
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		switch (tag.Type)
		{
		case AIPlayTagType.OtherAttackBuff:
			_otherAttackBuffList = AIParamQuery.AddElementToList(tag, _otherAttackBuffList);
			break;
		case AIPlayTagType.OtherAttackDamage:
			_otherAttackDamageList = AIParamQuery.AddElementToList(tag, _otherAttackDamageList);
			break;
		case AIPlayTagType.OtherAttackToken:
			_otherAttackTokenList = AIParamQuery.AddElementToList(tag, _otherAttackTokenList);
			break;
		case AIPlayTagType.OtherAttackAttachTag:
			_otherAttackAttachTagList = AIParamQuery.AddElementToList(tag, _otherAttackAttachTagList);
			break;
		case AIPlayTagType.OtherAttackHeal:
			break;
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (_otherAttackBuffList != null && _otherAttackBuffList.Count > 0)
		{
			_otherAttackBuffList.Clear();
			_otherAttackBuffList = null;
		}
		if (_otherAttackDamageList != null && _otherAttackDamageList.Count > 0)
		{
			_otherAttackDamageList.Clear();
			_otherAttackDamageList = null;
		}
		if (_otherAttackTokenList != null && _otherAttackTokenList.Count > 0)
		{
			_otherAttackTokenList.Clear();
			_otherAttackTokenList = null;
		}
		if (_otherAttackAttachTagList != null && _otherAttackAttachTagList.Count > 0)
		{
			_otherAttackAttachTagList.Clear();
			_otherAttackAttachTagList = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		if (!RemoveTagFromList(_otherAttackBuffList, tag) && !RemoveTagFromList(_otherAttackDamageList, tag) && !RemoveTagFromList(_otherAttackTokenList, tag))
		{
			RemoveTagFromList(_otherAttackAttachTagList, tag);
		}
	}

	public void RegisterConditionPassedTagProgress(AIVirtualField field, AIVirtualCard tagOwner, AIVirtualAttackInfo situation)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		if (!base.HasTag || actor == null || attackTarget == null || actor.IsDead)
		{
			return;
		}
		List<AIPlayTag> conditionPassedTagList = null;
		List<int> playPtn = field.BestPlayPtn;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (AIAttackSimulationUtility.IsUsePreCheckBuff(aIPlayTag, situation) || aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				conditionPassedTagList = AIParamQuery.AddElementToList(aIPlayTag, conditionPassedTagList);
			}
		}
		if (conditionPassedTagList == null || conditionPassedTagList.Count <= 0)
		{
			return;
		}
		situation.RegisterNewProcessInfo(actor, AISituationTriggerInformation.TriggerType.Undefined).AddExecutingAction(delegate
		{
			for (int j = 0; j < conditionPassedTagList.Count; j++)
			{
				if (tagOwner.IsDead)
				{
					break;
				}
				if (field.AllyClass.Life <= 0)
				{
					break;
				}
				if (field.EnemyClass.Life <= 0)
				{
					break;
				}
				ExecuteTagArgument(conditionPassedTagList[j], field, tagOwner, playPtn, situation);
			}
		});
	}

	private void ExecuteTagArgument(AIPlayTag tag, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AIVirtualAttackInfo situation)
	{
		if (AIAttackSimulationUtility.IsUsePreCheckBuff(tag, situation))
		{
			situation.PreCheckInformation.ApplyBuff(tag.Hash, field, playPtn, situation);
		}
		else if (tag.ArgumentExpressions is AITriggerAndTargetFiltersTagBase aITriggerAndTargetFiltersTagBase)
		{
			if (aITriggerAndTargetFiltersTagBase is IAIRemoveTagArgument iAIRemoveTagArgument)
			{
				iAIRemoveTagArgument.Execute(tagOwner, field, situation, tag, playPtn);
			}
			else
			{
				aITriggerAndTargetFiltersTagBase.Execute(tagOwner, field, playPtn, situation.Actor, situation);
			}
		}
	}

	public AISimulationBuffInfoCollection RegisterBuffInfoToCollection(AIVirtualField field, AIVirtualCard tagOwner, AISituationInfo situation, AISimulationBuffInfoCollection collection)
	{
		if (_otherAttackBuffList == null || _otherAttackBuffList.Count <= 0)
		{
			return collection;
		}
		List<int> bestPlayPtn = field.BestPlayPtn;
		for (int i = 0; i < _otherAttackBuffList.Count; i++)
		{
			AIPlayTag aIPlayTag = _otherAttackBuffList[i];
			AIOtherAttackBuff aIOtherAttackBuff = aIPlayTag.ArgumentExpressions as AIOtherAttackBuff;
			if (aIPlayTag.CheckCondition(tagOwner, bestPlayPtn, field, situation))
			{
				aIOtherAttackBuff.RegisterBuffInfo(collection, tagOwner, field, bestPlayPtn, situation, aIPlayTag.Hash);
			}
		}
		return collection;
	}

	public int GetAttackDamageToCertainTarget(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AIBarrierPseudoSimulationInfo simBarrier)
	{
		if (_otherAttackDamageList == null || _otherAttackDamageList.Count <= 0)
		{
			return 0;
		}
		int num = 0;
		AIVirtualCard owner = simBarrier.Owner;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (aIPlayTag.Type == AIPlayTagType.OtherAttackDamage && aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				AIOtherAttackDamage aIOtherAttackDamage = aIPlayTag.ArgumentExpressions as AIOtherAttackDamage;
				num += aIOtherAttackDamage.PseudoSimulateDamageToTarget(tagOwner, field, playPtn, situation, owner, simBarrier);
			}
		}
		return num;
	}

	public bool CheckDestroyByAttackTags(AIVirtualCard tagOwner, AIVirtualAttackInfo situation, AIVirtualField field, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		AIVirtualCard owner = simBarrier.Owner;
		if (actor == null || actor.IsDead || owner == null || owner.IsDead || attackTarget == null || attackTarget.IsDead)
		{
			return false;
		}
		if (GetAttackDamageToCertainTarget(tagOwner, field, playPtn, situation, simBarrier) >= owner.Life)
		{
			return true;
		}
		return false;
	}

	public void PseudoExecuteForEvalInstantAttack(AIVirtualCard tagOwner, AIVirtualAttackInfo situation, AIVirtualField field, List<int> playPtn, EvalInstantAttackInformation information)
	{
		if (!base.HasTag)
		{
			return;
		}
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			AIWhenAttackSelfAndOtherTagArgument aIWhenAttackSelfAndOtherTagArgument = aIPlayTag.ArgumentExpressions as AIWhenAttackSelfAndOtherTagArgument;
			if (aIWhenAttackSelfAndOtherTagArgument.IsActivateWhenEvalInstantAttack && aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				aIWhenAttackSelfAndOtherTagArgument.PseudoSimulateForEvalInstantAttack(tagOwner, field, situation, playPtn, information);
			}
		}
	}
}
