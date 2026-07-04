using System.Collections.Generic;

namespace Wizard;

public static class AIAttackSimulationUtility
{
	public static bool IsUsePreCheckBuff(AIPlayTag tag, AIVirtualAttackInfo situation)
	{
		if (!situation.IsUsePreCheck || tag.Type != AIPlayTagType.AttackBuff)
		{
			return false;
		}
		AIAttackPreCheckInformation preCheckInformation = situation.PreCheckInformation;
		if (preCheckInformation.HasBuffInfo)
		{
			return preCheckInformation.HasPreCheckBuffInfo(tag);
		}
		return false;
	}

	public static void SimulateAttackIfValuable(AIVirtualAttackInfo situation, AIVirtualField field, bool useAttackLeaderPreCheck, ref bool isAttackerUsed)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		situation.IsAttackSuccessed = false;
		AIVirtualCard allyClass = field.AllyClass;
		int attackDamageToCertainCard = AIAttackTagSimulator.GetAttackDamageToCertainCard(field, situation, allyClass);
		if (field.AllyClass.SimulateDamageAmount(attackDamageToCertainCard, isSkillDamage: true) >= allyClass.Life)
		{
			isAttackerUsed = true;
			return;
		}
		if (AIDiscardUtility.CheckAttackDiscardTargetInPlayPtn(actor.SelfField, field.BestPlayPtn, situation))
		{
			isAttackerUsed = true;
			return;
		}
		if (attackTarget.IsLeader)
		{
			int attackDamageToCertainCard2 = AIAttackTagSimulator.GetAttackDamageToCertainCard(field, situation, actor);
			AISimulationBuffInfoCollection buffInfoListWhenCertainAttack = AIAttackTagSimulator.GetBuffInfoListWhenCertainAttack(field, situation);
			float attackBonus = actor.GetAttackBonus(field.BestPlayPtn, situation);
			AISimulationBuffInfo buffInfo = buffInfoListWhenCertainAttack?.GetBuffInfoToCertainCard(actor);
			if (useAttackLeaderPreCheck && !IsExecuteAttackValuable(field, actor, actor.Attack, actor.Life, attackDamageToCertainCard2, buffInfo, attackBonus))
			{
				isAttackerUsed = true;
				return;
			}
			AIAttackPreCheckInformation preCheckInformation = new AIAttackPreCheckInformation(attackBonus, buffInfoListWhenCertainAttack);
			situation.SetPreCheckInformation(preCheckInformation);
		}
		AIVirtualAttackSimulator.Attack(situation, field);
		if (actor.IsDead || !actor.IsAttackable(EnemyAI.EmptyPlayPtn))
		{
			isAttackerUsed = true;
		}
		if (!field.CardListSet.HasAttackableClassHolder)
		{
			return;
		}
		for (int i = 0; i < field.CardListSet.AttackableClassHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.AttackableClassHolders[i];
			if (aIVirtualCard.IsUnit && !aIVirtualCard.IsDead)
			{
				aIVirtualCard.TagCollectionContainer.AttackableClassTags.ChangeAttackableClassStatus(aIVirtualCard, field, field.BestPlayPtn);
			}
		}
	}

	public static bool IsAttackPossible(AIVirtualField field, AIVirtualAttackInfo attackSituation, List<AIVirtualCard> replacedEnemyInplayCards = null)
	{
		if (attackSituation == null || attackSituation.ActionType != AIOperationType.ATTACK)
		{
			return false;
		}
		AIVirtualCard actor = attackSituation.Actor;
		AIVirtualCard attackTarget = attackSituation.AttackTarget;
		if (actor.IsDead || attackTarget.IsDead || !actor.IsAttackable(EnemyAI.EmptyPlayPtn))
		{
			return false;
		}
		bool flag = attackTarget == field.EnemyClass;
		if (actor.IsSkillCantAttackUnit)
		{
			if (flag)
			{
				return !actor.IsCantAttackClass();
			}
			return false;
		}
		if (flag && actor.IsCantAttackClass())
		{
			return false;
		}
		if (!(actor.IsAlly ? field.AllyInplayCards : field.EnemyInplayCards).Contains(actor))
		{
			return false;
		}
		if (attackTarget.IsAmulet || attackTarget.IsCantUnderAttack(field.ParamQuery, actor, null, field))
		{
			return false;
		}
		if (actor.IsCannotAttackByTag(attackSituation))
		{
			return false;
		}
		List<AIVirtualCard> list = ((!actor.IsAlly) ? field.AllyInplayCards : ((replacedEnemyInplayCards == null) ? field.EnemyInplayCards : replacedEnemyInplayCards));
		bool flag2 = false;
		if (list != null)
		{
			for (int i = 0; i < list.Count; i++)
			{
				AIVirtualCard aIVirtualCard = list[i];
				if (aIVirtualCard.IsGuard && !aIVirtualCard.IsDead && !aIVirtualCard.IsCantUnderAttack(field.ParamQuery, actor, null, field))
				{
					flag2 = true;
					break;
				}
			}
		}
		bool isGuard = attackTarget.IsGuard;
		if (flag2 && !actor.IsIgnoreGuard && !isGuard)
		{
			return false;
		}
		if (!flag && actor.IsSkillCantAtkUnitNotHasGuard && !isGuard)
		{
			return false;
		}
		return true;
	}

	public static bool IsExecuteAttackValuable(AIVirtualField field, AIVirtualCard attacker, int attack, int life, int preDamage, AISimulationBuffInfo buffInfo, float attackBonus)
	{
		int num = attack;
		int num2 = life;
		num2 -= preDamage;
		if (buffInfo != null)
		{
			num += buffInfo.TotalAttackBuff;
			num2 += buffInfo.TotalLifeBuff;
		}
		if (num2 > 0 && num > 0)
		{
			return true;
		}
		float num3 = attackBonus;
		if (num2 <= 0)
		{
			float num4 = attacker.EvaluateBreakValue(field.BestPlayPtn, useIgnoreBreak: true) + attacker.EvaluateLeaveValue(field.BestPlayPtn, useIgnoreInBattle: true);
			num3 += num4 - attacker.Value;
		}
		return num3 > 0f;
	}
}
