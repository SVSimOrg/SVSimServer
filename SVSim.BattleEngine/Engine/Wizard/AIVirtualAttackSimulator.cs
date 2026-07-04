using System.Collections.Generic;

namespace Wizard;

public static class AIVirtualAttackSimulator
{
	public static void Attack(AIVirtualAttackInfo situation, AIVirtualField field)
	{
		situation.IsAttackSuccessed = false;
		if (situation.ActionType == AIOperationType.ATTACK && AIAttackSimulationUtility.IsAttackPossible(field, situation))
		{
			AIVirtualCard actor = situation.Actor;
			_ = situation.AttackTarget;
			actor.AttackableCount--;
			field.ActionLength++;
			situation.IsAttackSuccessed = true;
			field.ApplyAttackBonus(situation);
			field.ApplyClashBonus(situation);
			AIPreprocessSimulationUtility.SimulatePreprocess(actor, situation, field, AIScriptTokenArgType.WHEN_ATTACK, isPseudo: false);
			SimulateSkillsBeforeAttackCalculation(situation, field);
			field.AllActivateCountHolderIncrement(situation, AIPlayTagType.AttackActivateCount, actor);
			situation.ProcessCollection.CombinePreprocessToProcessQueue();
			situation.ExecuteAllSkillProcess();
			if (field.AllyClass.Life > 0 && field.EnemyClass.Life > 0)
			{
				ApplyAttackActionToTargetAndAttacker(situation, field);
			}
		}
	}

	private static void ApplyAttackActionToTargetAndAttacker(AIVirtualAttackInfo situation, AIVirtualField field)
	{
		if (situation.AttackTarget.IsLeader)
		{
			AttackLeaderDamageCalculation(situation, field);
		}
		else
		{
			AttackFollowerDamageCalculation(situation, field);
		}
	}

	private static void AttackFollowerDamageCalculation(AIVirtualAttackInfo situation, AIVirtualField field)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		_ = field.BestPlayPtn;
		if (!actor.IsDead && !attackTarget.IsDead)
		{
			int attackerAtk = actor.Attack;
			int targetAtk = attackTarget.Attack;
			AIAttackTagSimulator.ExecuteAttackByLife(field, actor, attackTarget, ref attackerAtk, ref targetAtk);
			attackerAtk = actor.SimulateAttackAmount(situation);
			targetAtk = attackTarget.SimulateAttackAmount(situation);
			int attackDamage = attackTarget.AddDamage(situation, attackerAtk, isSkillDamage: false);
			int defendDamage = actor.AddDamage(situation, targetAtk, isSkillDamage: false);
			situation.ExecuteAllSkillProcess();
			if (attackTarget.IsDead)
			{
				attackTarget.RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
			}
			if (actor.IsDead)
			{
				actor.RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
			}
			ExecAfterClashSkills(situation, field, attackDamage, defendDamage);
			situation.ExecuteAllSkillProcess();
		}
	}

	private static void AttackLeaderDamageCalculation(AIVirtualAttackInfo situation, AIVirtualField field)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		List<int> bestPlayPtn = field.BestPlayPtn;
		if (actor.Life <= 0)
		{
			return;
		}
		int baseDamage = actor.SimulateAttackAmount(situation);
		actor.DepriveSneakWithGiveSneakTag();
		int damage = attackTarget.AddDamage(situation, baseDamage, isSkillDamage: false);
		if (attackTarget.IsDead)
		{
			attackTarget.RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
		}
		if (field.AllyClass.Life > 0 && field.EnemyClass.Life > 0)
		{
			actor.DealDamageDrain(damage, bestPlayPtn, situation);
			situation.ExecuteAllSkillProcess();
			if (actor.Life <= 0)
			{
				actor.RemoveCard(situation, AIRemovalType.Destroy, isFromSkill: false);
			}
			if (!actor.IsDead)
			{
				AIAttackTagSimulator.ApplyOtherAfterAttackOrClashTags(field, situation);
				situation.ExecuteAllSkillProcess();
			}
		}
	}

	private static void SimulateWhenAttackAndWhenFightSkills(AIVirtualAttackInfo situation, AIVirtualField field)
	{
		AIVirtualCard actor = situation.Actor;
		actor.IsNotAttackYet = false;
		if (actor.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack))
		{
			actor.TagCollectionContainer.AttackTags.RegisterConditionPassedTagProgress(field, actor, situation);
		}
		AIVirtualCard attackTarget = situation.AttackTarget;
		if (attackTarget.IsUnit && attackTarget.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenAttack))
		{
			AttackTagCollection attackTags = attackTarget.TagCollectionContainer.AttackTags;
			if (attackTags.HasClashTag)
			{
				attackTags.RegisterConditionPassedTagProgress(field, attackTarget, situation);
			}
		}
	}

	private static void SimulateWhenAttackSelfAndOtherSkills(AIVirtualAttackInfo situation, AIVirtualField field)
	{
		field.RegisterOtherCardAttackTags(situation);
	}

	public static void SimulateSkillsBeforeAttackCalculation(AIVirtualAttackInfo situation, AIVirtualField field)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		if (!actor.IsDead && !attackTarget.IsDead && situation.ActionType == AIOperationType.ATTACK)
		{
			SimulateWhenAttackAndWhenFightSkills(situation, field);
			SimulateWhenAttackSelfAndOtherSkills(situation, field);
			if (attackTarget.IsUnit && actor.SelfField.AllyClass.Life > 0 && actor.SelfField.EnemyClass.Life > 0)
			{
				_ = attackTarget.IsDead;
			}
		}
	}

	private static void ExecAfterClashSkills(AIVirtualAttackInfo situation, AIVirtualField field, int attackDamage, int defendDamage)
	{
		AIVirtualCard actor = situation.Actor;
		AIVirtualCard attackTarget = situation.AttackTarget;
		List<int> bestPlayPtn = field.BestPlayPtn;
		actor.AfterClash(attackTarget, attackDamage, isAttacker: true, bestPlayPtn, situation);
		attackTarget.AfterClash(actor, defendDamage, isAttacker: false, bestPlayPtn, situation);
		AIAttackTagSimulator.ApplyOtherAfterAttackOrClashTags(field, situation);
	}
}
