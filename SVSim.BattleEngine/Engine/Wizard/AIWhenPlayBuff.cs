using System;
using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayBuff : AIWhenPlayTagArgument
{
	private readonly int TEMP_OR_PERM_ARG_OFFSET = 1;

	private readonly int LIFE_ARG_OFFSET = 2;

	private readonly int ATTACK_ARG_OFFSET = 3;

	public AIPolishConvertedExpression Attack { get; private set; }

	public AIPolishConvertedExpression Life { get; private set; }

	public AIScriptTokenArgType TempOrPerm { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 4;

	public AIWhenPlayBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		Attack = _exprList[_exprList.Count - ATTACK_ARG_OFFSET];
		Life = _exprList[_exprList.Count - LIFE_ARG_OFFSET];
		TempOrPerm = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - TEMP_OR_PERM_ARG_OFFSET], AIBuffEvaluationUtility.LEGAL_TEMP_OR_PERM_ARGUMENTS);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, Attack, Life);
			bool isTemp = TempOrPerm == AIScriptTokenArgType.TEMP;
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIBuffSimulationUtility.BuffAll_old(targetsFromField, field, buffExecutingInfo_old, isTemp, playPtn, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				ExecuteRandomSelectBuff(tagOwner, targetsFromField, situation, field, playPtn, buffExecutingInfo_old, isTemp);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
			case AIScriptTokenArgType.SECOND_TARGET_SELECT:
				ExecuteTargetSelectBuff(targetsFromField, situation, field, playPtn, buffExecutingInfo_old, isTemp);
				break;
			default:
				AIConsoleUtility.LogError("AIWhenPlayBuff.Execute() Error!! SelectType=" + base.SelectType);
				break;
			}
		}
	}

	private void ExecuteTargetSelectBuff(List<AIVirtualCard> targets, AISituationInfo situation, AIVirtualField field, List<int> playPtn, AIBuffExecutingInfo_old buffInfo, bool isTemp)
	{
		if (situation == null)
		{
			AIConsoleUtility.LogError("AIWhenPlayBuff.ExecuteTargetSelectBuff() Error!! situation is null!!!!!");
			return;
		}
		if (situation.IsTargetExists(base.SelectType))
		{
			AIBuffSimulationUtility.BuffTarget_old(situation, targets, field, playPtn, base.SelectType, buffInfo, isTemp);
			return;
		}
		AIVirtualCard aIVirtualCard = AISimulationRemovalUtility.SelectBestTargetForBuff(targets, buffInfo);
		if (aIVirtualCard != null)
		{
			AIBuffSimulationUtility.BuffSingle_old(aIVirtualCard, field, buffInfo, isTemp, playPtn, situation);
		}
	}

	private void ExecuteRandomSelectBuff(AIVirtualCard tagOwner, List<AIVirtualCard> targets, AISituationInfo situation, AIVirtualField field, List<int> playPtn, AIBuffExecutingInfo_old buffInfo, bool isTemp)
	{
		AIVirtualCardRealTargetInformation aIVirtualCardRealTargetInformation = situation.DequeueRealTargetInfo(tagOwner, field);
		if (aIVirtualCardRealTargetInformation == null || aIVirtualCardRealTargetInformation.TargetList.Count <= 0)
		{
			AIBuffSimulationUtility.BuffRandom_old(targets, field, playPtn, situation, buffInfo, isTemp);
			return;
		}
		AIVirtualCard aIVirtualCard = aIVirtualCardRealTargetInformation.TargetList[0];
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i].IsSameCard(aIVirtualCard))
			{
				AIBuffSimulationUtility.BuffSingle_old(aIVirtualCard, field, buffInfo, isTemp, playPtn, situation);
				break;
			}
		}
	}

	public override void TargetLifePrediction(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
		if (base.SelectType == AIScriptTokenArgType.ALL_SELECT && AIFilteringUtility.CheckMatchTargetFiltering(target, field.CardListSet.BothClassAndInplayCards, base.Filters, playPtn, owner, situation))
		{
			int num = (int)Life.EvalArg(owner, playPtn, field, situation);
			targetLifeRecord.MaxLife = Math.Max(targetLifeRecord.MaxLife + num, 0);
			if (targetLifeRecord.CurrentLife > targetLifeRecord.MaxLife)
			{
				targetLifeRecord.CurrentLife = targetLifeRecord.MaxLife;
			}
		}
	}

	public override void MultipleTargetLifePrediction(List<AIVirtualCard> targetList, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, List<LifeRecord> lifeList)
	{
		if (base.SelectType != AIScriptTokenArgType.ALL_SELECT)
		{
			return;
		}
		int num = (int)Life.EvalArg(owner, playPtn, field, situation);
		for (int i = 0; i < targetList.Count; i++)
		{
			AIVirtualCard targetCard = targetList[i];
			LifeRecord lifeRecord = lifeList[i];
			if (AIFilteringUtility.CheckMatchTargetFiltering(targetCard, field.CardListSet.BothClassAndInplayCards, base.Filters, playPtn, owner, situation))
			{
				lifeRecord.MaxLife = Math.Max(lifeRecord.MaxLife + num, 0);
				if (lifeRecord.CurrentLife > lifeRecord.MaxLife)
				{
					lifeRecord.CurrentLife = lifeRecord.MaxLife;
				}
			}
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[4]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		bool isAttackEffective = !Attack.IsZeroOrNone();
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
