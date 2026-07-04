using System.Collections.Generic;

namespace Wizard;

public class AIOtherWhenPlayDamage : AIOtherWhenPlayTagArgument
{

	public AIPolishConvertedExpression Damage { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIOtherWhenPlayDamage(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (IsLegalSelectType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], out var selectType))
		{
			SelectType = selectType;
		}
		else
		{
			AIConsoleUtility.LogError("AIOtherWhenPlayDamage Error!!! SelectType is " + selectType);
		}
		Damage = _exprList[_exprList.Count - 1];
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		int damage = (int)Damage.EvalArg(tagOwner, playPtn, field);
		switch (SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			AIDamageSimulationUtility.DamageAll(targets, tagOwner, field, damage, situation);
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
			AIDamageSimulationUtility.DamageRandom(targets, tagOwner, field, damage, situation);
			break;
		default:
			AIConsoleUtility.LogError($"AIOtherWhenPlayDamage.RunTagMethod() : Unsupported SelectType {SelectType}");
			break;
		}
	}

	public override void TargetLifePrediction(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
		if (SelectType == AIScriptTokenArgType.ALL_SELECT && (target.IsLeader || target.IsUnit) && AIFilteringUtility.CheckMatchTargetFiltering(target, GetCandidateRange(field), base.TargetFilters, playPtn, owner, situation))
		{
			int damageAmount = (int)Damage.EvalArg(owner, playPtn, field, situation);
			int num = target.SimulateDamageAmount(damageAmount, isSkillDamage: true, owner.IsSpell);
			targetLifeRecord.CurrentLife -= num;
		}
	}

	public override void MultipleTargetLifePrediction(List<AIVirtualCard> targetList, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<LifeRecord> lifeList)
	{
		if (SelectType != AIScriptTokenArgType.ALL_SELECT)
		{
			return;
		}
		int damageAmount = (int)Damage.EvalArg(owner, playPtn, field, situation);
		for (int i = 0; i < targetList.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targetList[i];
			LifeRecord lifeRecord = lifeList[i];
			if ((!aIVirtualCard.IsLeader && !aIVirtualCard.IsUnit) || !AIFilteringUtility.CheckMatchTargetFiltering(aIVirtualCard, GetCandidateRange(field), base.TargetFilters, playPtn, owner, situation))
			{
				continue;
			}
			int num = aIVirtualCard.SimulateDamageAmount(damageAmount, isSkillDamage: true, owner.IsSpell);
			lifeRecord.CurrentLife -= num;
			if (lifeRecord.CurrentLife <= 0)
			{
				if (aIVirtualCard.IsDestroyByBanish)
				{
					situation.RegisterOwnBanishedCard(aIVirtualCard);
				}
				else
				{
					situation.RegisterOwnDestroyedCard(aIVirtualCard);
				}
			}
		}
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}
