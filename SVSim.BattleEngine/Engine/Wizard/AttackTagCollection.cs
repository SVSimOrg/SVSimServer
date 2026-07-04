using System.Collections.Generic;

namespace Wizard;

public class AttackTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[32]
	{
		AIPlayTagType.AttackBuff,
		AIPlayTagType.AttackDamage,
		AIPlayTagType.AttackHeal,
		AIPlayTagType.AttackDestroy,
		AIPlayTagType.AttackBanish,
		AIPlayTagType.AttackToken,
		AIPlayTagType.AttackDiscard,
		AIPlayTagType.AttackAttackableCount,
		AIPlayTagType.AttackAttachTag,
		AIPlayTagType.AttackEvo,
		AIPlayTagType.AttackSubtractCountdown,
		AIPlayTagType.AttackKiller,
		AIPlayTagType.AttackShield,
		AIPlayTagType.AttackDamageClip,
		AIPlayTagType.AttackQuick,
		AIPlayTagType.AttackRemoveTag,
		AIPlayTagType.AttackRemoveSkill,
		AIPlayTagType.AttackSetStatus,
		AIPlayTagType.AttackAddDeck,
		AIPlayTagType.AttackHandBuff,
		AIPlayTagType.ClashDestroy,
		AIPlayTagType.ClashDamage,
		AIPlayTagType.ClashBuff,
		AIPlayTagType.ClashHeal,
		AIPlayTagType.ClashRemoveSkill,
		AIPlayTagType.ClashKiller,
		AIPlayTagType.ClashToken,
		AIPlayTagType.ClashBanish,
		AIPlayTagType.ClashShield,
		AIPlayTagType.ClashDamageClip,
		AIPlayTagType.ClashRemoveTag,
		AIPlayTagType.ClashSpellboost
	};

	private List<AIPlayTag> _attackBuffList;

	private List<AIPlayTag> _attackDamageList;

	private List<AIPlayTag> _attackDestroyList;

	private List<AIPlayTag> _attackTokenList;

	private List<AIPlayTag> _attackDiscardList;

	private List<AIPlayTag> _attackAttachTagList;

	private List<AIPlayTag> _clashTags;

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	private bool HasAttackBuff
	{
		get
		{
			if (_attackBuffList != null)
			{
				return _attackBuffList.Count > 0;
			}
			return false;
		}
	}

	public bool HasAttackDamage
	{
		get
		{
			if (_attackDamageList != null)
			{
				return _attackDamageList.Count > 0;
			}
			return false;
		}
	}

	public bool HasAttackDestroy
	{
		get
		{
			if (_attackDestroyList != null)
			{
				return _attackDestroyList.Count > 0;
			}
			return false;
		}
	}

	public bool HasAttackToken
	{
		get
		{
			if (_attackTokenList != null)
			{
				return _attackTokenList.Count > 0;
			}
			return false;
		}
	}

	public bool HasAttackDiscard
	{
		get
		{
			if (_attackDiscardList != null)
			{
				return _attackDiscardList.Count > 0;
			}
			return false;
		}
	}

	public bool HasAttackAttachTag
	{
		get
		{
			if (_attackAttachTagList != null)
			{
				return _attackAttachTagList.Count > 0;
			}
			return false;
		}
	}

	public bool HasClashTag
	{
		get
		{
			if (_clashTags != null)
			{
				return _clashTags.Count > 0;
			}
			return false;
		}
	}

	public List<AIPlayTag> ClashTags => _clashTags;

	public AttackTagCollection()
		: base(TagCollectionType.WhenAttack)
	{
		_attackBuffList = null;
		_attackDamageList = null;
		_attackDestroyList = null;
		_attackTokenList = null;
		_attackDiscardList = null;
		_attackAttachTagList = null;
		_clashTags = null;
	}

	private AttackTagCollection(AttackTagCollection tagCollection)
		: base(tagCollection)
	{
		if (tagCollection != null)
		{
			_attackBuffList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._attackBuffList);
			_attackDamageList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._attackDamageList);
			_attackDestroyList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._attackDestroyList);
			_attackTokenList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._attackTokenList);
			_attackDiscardList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._attackDiscardList);
			_attackAttachTagList = AIPlayTagInitializingUtility.CloneTagList(tagCollection._attackAttachTagList);
			_clashTags = AIPlayTagInitializingUtility.CloneTagList(tagCollection._clashTags);
		}
	}

	public override TagCollection Clone()
	{
		return new AttackTagCollection(this);
	}

	public void RegisterConditionPassedTagProgress(AIVirtualField field, AIVirtualCard tagOwner, AIVirtualAttackInfo situation)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		if (!base.HasTag || situation.ActionType != AIOperationType.ATTACK || actor == null || attackTarget == null || actor.IsDead || attackTarget.IsDead)
		{
			return;
		}
		List<AIPlayTag> conditionPassedTagList = null;
		List<int> playPtn = field.BestPlayPtn;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (AIAttackSimulationUtility.IsUsePreCheckBuff(aIPlayTag, situation) || CheckTagConditionWhenExecuting(aIPlayTag, field, tagOwner, playPtn, situation))
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
				AIPlayTag tag = conditionPassedTagList[j];
				ExecuteTagArgument(tag, field, tagOwner, playPtn, situation);
			}
		});
	}

	private bool CheckTagConditionWhenExecuting(AIPlayTag tag, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AIVirtualAttackInfo situation)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		if (tag.IsClashTagType() && !attackTarget.IsUnit)
		{
			return false;
		}
		if (tag.IsAttackTagType() && !tagOwner.IsSameCard(actor))
		{
			return false;
		}
		if (!tag.CheckCondition(tagOwner, playPtn, field, situation))
		{
			return false;
		}
		return true;
	}

	private void ExecuteTagArgument(AIPlayTag tag, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AIVirtualAttackInfo situation)
	{
		if (AIAttackSimulationUtility.IsUsePreCheckBuff(tag, situation))
		{
			situation.PreCheckInformation.ApplyBuff(tag.Hash, field, playPtn, situation);
		}
		else if (tag.ArgumentExpressions is IAIRemoveTagArgument iAIRemoveTagArgument)
		{
			iAIRemoveTagArgument.Execute(tagOwner, field, situation, tag);
		}
		else
		{
			tag.ArgumentExpressions.Execute(tagOwner, field, playPtn, situation);
		}
	}

	public AISimulationBuffInfoCollection RegisterBuffInfoToCollection(AIVirtualField field, AIVirtualCard tagOwner, AISituationInfo situation, AISimulationBuffInfoCollection collection)
	{
		if (!HasAttackBuff)
		{
			return collection;
		}
		List<int> bestPlayPtn = field.BestPlayPtn;
		for (int i = 0; i < _attackBuffList.Count; i++)
		{
			AIPlayTag aIPlayTag = _attackBuffList[i];
			AIAttackBuff aIAttackBuff = aIPlayTag.ArgumentExpressions as AIAttackBuff;
			if (_attackBuffList[i].CheckCondition(tagOwner, bestPlayPtn, field, situation))
			{
				aIAttackBuff.RegisterBuffInfo(collection, tagOwner, field, bestPlayPtn, situation, aIPlayTag.Hash);
			}
		}
		return collection;
	}

	public int GetAttackDamageToCertainTarget(AIVirtualCard tagOwner, AIVirtualAttackInfo situation, AIVirtualField field, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		int num = 0;
		bool flag = tagOwner.IsSameCard(actor);
		bool flag2 = tagOwner.IsSameCard(attackTarget);
		if (!flag && !flag2)
		{
			AIConsoleUtility.LogError("AttackTagCollection.WillSingleTargetDestroyByAttackTags errror!! tagOwner is neither attacker nor target!!!!!");
			return num;
		}
		AIVirtualCard owner = simBarrier.Owner;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (!IsSkipWhenCheckCertainTargetDamage(aIPlayTag, flag, flag2) && aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				AIWhenAttackOrWhenFightTagArgument aIWhenAttackOrWhenFightTagArgument = aIPlayTag.ArgumentExpressions as AIWhenAttackOrWhenFightTagArgument;
				num += aIWhenAttackOrWhenFightTagArgument.PseudoSimulateWhenAttackDamageToCertainCard(tagOwner, owner, field, situation, playPtn, simBarrier);
			}
		}
		return num;
	}

	public bool WillSingleTargetDestroyByAttackTags(AIVirtualCard tagOwner, AIVirtualAttackInfo situation, AIVirtualField field, List<int> playPtn, AIBarrierPseudoSimulationInfo simBarrier)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		AIVirtualCard owner = simBarrier.Owner;
		if (actor == null || actor.IsDead || owner == null || owner.IsDead || attackTarget == null || attackTarget.IsDead)
		{
			return false;
		}
		bool flag = tagOwner.IsSameCard(actor);
		bool flag2 = tagOwner.IsSameCard(attackTarget);
		if (!flag && !flag2)
		{
			AIConsoleUtility.LogError("AttackTagCollection.WillSingleTargetDestroyByAttackTags errror!! tagOwner is neither attacker nor target!!!!!");
			return false;
		}
		int totalDamage = 0;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if (!IsSkipWhenCheckDestroyTarget(aIPlayTag, flag, flag2) && aIPlayTag.CheckCondition(actor, playPtn, field, situation) && (aIPlayTag.ArgumentExpressions as AIWhenAttackOrWhenFightTagArgument).CanKillTarget(actor, owner, field, situation, playPtn, simBarrier, ref totalDamage))
			{
				return true;
			}
		}
		return false;
	}

	public bool WillAnyTargetDestroyByAttackTags(AIVirtualCard tagOwner, AIVirtualAttackInfo situation, AIVirtualField field, List<int> playPtn, List<AIVirtualCard> targetList)
	{
		if (!base.HasTag)
		{
			return false;
		}
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		bool flag = tagOwner.IsSameCard(actor);
		bool flag2 = tagOwner.IsSameCard(attackTarget);
		if (!flag && !flag2)
		{
			AIConsoleUtility.LogError("AttackTagCollection.WillSingleTargetDestroyByAttackTags errror!! tagOwner is neither attacker nor target!!!!!");
			return false;
		}
		int count = targetList.Count;
		List<AIBarrierPseudoSimulationInfo> list = new List<AIBarrierPseudoSimulationInfo>();
		int[] array = new int[count];
		for (int i = 0; i < targetList.Count; i++)
		{
			array[i] = 0;
			AIBarrierPseudoSimulationInfo item = new AIBarrierPseudoSimulationInfo(targetList[i]);
			list.Add(item);
		}
		for (int j = 0; j < base.TagList.Count; j++)
		{
			AIPlayTag aIPlayTag = base.TagList[j];
			if (!IsSkipWhenCheckDestroyTarget(aIPlayTag, flag, flag2) && aIPlayTag.CheckCondition(actor, playPtn, field, situation) && (aIPlayTag.ArgumentExpressions as AIWhenAttackOrWhenFightTagArgument).CanKillAnyTarget(actor, targetList, field, situation, playPtn, list, array))
			{
				return true;
			}
		}
		return false;
	}

	private bool IsSkipWhenCheckDestroyTarget(AIPlayTag tag, bool tagOwnerIsAttacker, bool tagOwnerIsTarget)
	{
		if (IsWhenAttackRemovalTag(tag))
		{
			return !tagOwnerIsAttacker;
		}
		if (IsWhenClashRemovalTag(tag))
		{
			return !(tagOwnerIsAttacker || tagOwnerIsTarget);
		}
		return true;
	}

	private bool IsSkipWhenCheckCertainTargetDamage(AIPlayTag tag, bool tagOwnerIsAttacker, bool tagOwnerIsTarget)
	{
		if (tag.Type == AIPlayTagType.AttackShield || tag.Type == AIPlayTagType.AttackDamage || tag.Type == AIPlayTagType.AttackDamageClip)
		{
			return !tagOwnerIsAttacker;
		}
		if (tag.Type == AIPlayTagType.ClashDamage || tag.Type == AIPlayTagType.ClashShield || tag.Type == AIPlayTagType.ClashDamageClip)
		{
			return !(tagOwnerIsAttacker || tagOwnerIsTarget);
		}
		return true;
	}

	private bool IsWhenAttackRemovalTag(AIPlayTag tag)
	{
		if (tag.Type != AIPlayTagType.AttackDestroy && tag.Type != AIPlayTagType.AttackBanish && tag.Type != AIPlayTagType.AttackDamage && tag.Type != AIPlayTagType.AttackShield)
		{
			return tag.Type == AIPlayTagType.AttackDamageClip;
		}
		return true;
	}

	private bool IsWhenClashRemovalTag(AIPlayTag tag)
	{
		if (tag.Type != AIPlayTagType.ClashDestroy && tag.Type != AIPlayTagType.ClashDamage && tag.Type != AIPlayTagType.ClashBanish && tag.Type != AIPlayTagType.ClashShield)
		{
			return tag.Type == AIPlayTagType.ClashDamageClip;
		}
		return true;
	}

	public bool CheckAttackDiscardTargetInPlayPtn(AIVirtualCard tagOwner, List<AIVirtualCard> handCards, List<int> playPtn, AISituationInfo situation)
	{
		for (int i = 0; i < _attackDiscardList.Count; i++)
		{
			AIPlayTag aIPlayTag = _attackDiscardList[i];
			if (aIPlayTag.ArgumentExpressions is AIAttackDiscard aIAttackDiscard && aIPlayTag.CheckCondition(tagOwner, playPtn, tagOwner.SelfField, situation) && aIAttackDiscard.IsAttackDiscardTargetInPlayPtn(tagOwner, handCards, playPtn, situation))
			{
				return true;
			}
		}
		return false;
	}

	public void PseudoExecuteForEvalInstantAttack(AIVirtualCard tagOwner, AIVirtualAttackInfo situation, AIVirtualField field, List<int> playPtn, EvalInstantAttackInformation information)
	{
		if (!base.HasTag)
		{
			return;
		}
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		bool num = tagOwner.IsSameCard(actor);
		bool flag = tagOwner.IsSameCard(attackTarget);
		List<AIPlayTag> list;
		if (num)
		{
			list = base.TagList;
		}
		else
		{
			if (!flag)
			{
				AIConsoleUtility.LogError("AttackTagCollection.PseudoExecuteForEvalInstantAttack errror!! tagOwner is neither attacker nor target!!!!!");
				return;
			}
			list = _clashTags;
		}
		for (int i = 0; i < list.Count; i++)
		{
			AIPlayTag aIPlayTag = list[i];
			AIWhenAttackOrWhenFightTagArgument aIWhenAttackOrWhenFightTagArgument = aIPlayTag.ArgumentExpressions as AIWhenAttackOrWhenFightTagArgument;
			if (aIWhenAttackOrWhenFightTagArgument.IsActivateWhenEvalInstantAttack && aIPlayTag.CheckCondition(tagOwner, playPtn, field, situation))
			{
				aIWhenAttackOrWhenFightTagArgument.PseudoSimulateForEvalInstantAttack(tagOwner, field, situation, playPtn, information);
			}
		}
	}

	public override void AddTag(AIPlayTag tag)
	{
		base.AddTag(tag);
		switch (tag.Type)
		{
		case AIPlayTagType.AttackBuff:
			_attackBuffList = AIParamQuery.AddElementToList(tag, _attackBuffList);
			break;
		case AIPlayTagType.AttackDamage:
			_attackDamageList = AIParamQuery.AddElementToList(tag, _attackDamageList);
			break;
		case AIPlayTagType.AttackDestroy:
			_attackDestroyList = AIParamQuery.AddElementToList(tag, _attackDestroyList);
			break;
		case AIPlayTagType.AttackToken:
			_attackTokenList = AIParamQuery.AddElementToList(tag, _attackTokenList);
			break;
		case AIPlayTagType.AttackDiscard:
			_attackDiscardList = AIParamQuery.AddElementToList(tag, _attackDiscardList);
			break;
		case AIPlayTagType.AttackAttachTag:
			_attackAttachTagList = AIParamQuery.AddElementToList(tag, _attackAttachTagList);
			break;
		case AIPlayTagType.ClashDestroy:
		case AIPlayTagType.ClashHeal:
		case AIPlayTagType.ClashBanish:
		case AIPlayTagType.ClashSpellboost:
		case AIPlayTagType.ClashBuff:
		case AIPlayTagType.ClashDamage:
		case AIPlayTagType.ClashKiller:
		case AIPlayTagType.ClashRemoveSkill:
		case AIPlayTagType.ClashRemoveTag:
		case AIPlayTagType.ClashShield:
		case AIPlayTagType.ClashDamageClip:
			_clashTags = AIParamQuery.AddElementToList(tag, _clashTags);
			break;
		case AIPlayTagType.ClashToken:
			_attackTokenList = AIParamQuery.AddElementToList(tag, _attackTokenList);
			_clashTags = AIParamQuery.AddElementToList(tag, _clashTags);
			break;
		}
	}

	public override void Clear()
	{
		base.Clear();
		if (HasAttackBuff)
		{
			_attackBuffList.Clear();
		}
		if (HasAttackDamage)
		{
			_attackDamageList.Clear();
		}
		if (HasAttackDestroy)
		{
			_attackDestroyList.Clear();
		}
		if (HasAttackToken)
		{
			_attackTokenList.Clear();
		}
		if (HasAttackDiscard)
		{
			_attackDiscardList.Clear();
		}
		if (HasAttackAttachTag)
		{
			_attackAttachTagList.Clear();
		}
		if (HasClashTag)
		{
			_clashTags.Clear();
			_clashTags = null;
		}
	}

	protected override void RemoveTagFromManagedTagList(AIPlayTag tag)
	{
		if (!RemoveTagFromList(_attackBuffList, tag) && !RemoveTagFromList(_attackDamageList, tag) && !RemoveTagFromList(_attackDestroyList, tag) && !RemoveTagFromList(_attackTokenList, tag) && !RemoveTagFromList(_attackDiscardList, tag) && !RemoveTagFromList(_attackAttachTagList, tag))
		{
			RemoveTagFromList(_clashTags, tag);
		}
	}
}
