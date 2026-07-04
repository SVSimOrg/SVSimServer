using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AfterAttackTagCollection : TagCollection
{
	private readonly AIPlayTagType[] _managedTagTypes = new AIPlayTagType[8]
	{
		AIPlayTagType.AttackBreakDamage,
		AIPlayTagType.AttackBreakEvo,
		AIPlayTagType.AttackBreakRecoverPp,
		AIPlayTagType.AttackBreakAttackTwice,
		AIPlayTagType.AfterAttackEvo,
		AIPlayTagType.AfterAttackHeal,
		AIPlayTagType.AfterAttackDraw,
		AIPlayTagType.AfterAttackBanish
	};

	private readonly AIPlayTagType[] AttackBreakTagTypes = new AIPlayTagType[4]
	{
		AIPlayTagType.AttackBreakDamage,
		AIPlayTagType.AttackBreakEvo,
		AIPlayTagType.AttackBreakRecoverPp,
		AIPlayTagType.AttackBreakAttackTwice
	};

	protected override AIPlayTagType[] MANAGED_TAG_TYPES => _managedTagTypes;

	public AfterAttackTagCollection()
		: base(TagCollectionType.WhenAfterAttack)
	{
	}

	private AfterAttackTagCollection(AfterAttackTagCollection tagCollection)
		: base(tagCollection)
	{
	}

	public override TagCollection Clone()
	{
		return new AfterAttackTagCollection(this);
	}

	public void RegisterConditionPassedTags(AIVirtualCard tagOwner, AIVirtualField field, AIVirtualAttackInfo situation)
	{
		if (situation == null)
		{
			AIConsoleUtility.LogError("AfterAttackTagCollection.RegisterConditionPassedTags() error!! situation is null");
			return;
		}
		AIVirtualCard attacker = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		if (!base.HasTag || situation.ActionType != AIOperationType.ATTACK || tagOwner.IsDead || attacker == null || attacker.IsDead || attackTarget == null)
		{
			return;
		}
		List<AIPlayTag> conditionPassedTagList = null;
		List<int> bestPlayPtn = field.BestPlayPtn;
		for (int i = 0; i < base.TagList.Count; i++)
		{
			AIPlayTag aIPlayTag = base.TagList[i];
			if ((!AttackBreakTagTypes.Contains(aIPlayTag.Type) || attackTarget.IsDead) && aIPlayTag.CheckCondition(tagOwner, bestPlayPtn, field, situation))
			{
				conditionPassedTagList = AIParamQuery.AddElementToList(aIPlayTag, conditionPassedTagList);
			}
		}
		if (conditionPassedTagList == null || conditionPassedTagList.Count <= 0)
		{
			return;
		}
		situation.RegisterNewProcessInfo(null, AISituationTriggerInformation.TriggerType.Undefined).AddExecutingAction(delegate
		{
			for (int j = 0; j < conditionPassedTagList.Count; j++)
			{
				if (tagOwner.IsDead)
				{
					break;
				}
				if (attacker.IsDead)
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
				AIPlayTag aIPlayTag2 = conditionPassedTagList[j];
				if (aIPlayTag2.ArgumentExpressions is AITriggerAndTargetFiltersTagBase aITriggerAndTargetFiltersTagBase)
				{
					aITriggerAndTargetFiltersTagBase.Execute(tagOwner, field, field.BestPlayPtn, situation.Actor, situation);
				}
				else
				{
					aIPlayTag2.ArgumentExpressions.Execute(tagOwner, field, field.BestPlayPtn, situation);
				}
			}
		});
	}
}
