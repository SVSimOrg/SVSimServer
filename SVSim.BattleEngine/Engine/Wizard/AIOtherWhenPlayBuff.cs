using System;
using System.Collections.Generic;

namespace Wizard;

public class AIOtherWhenPlayBuff : AIOtherWhenPlayTagArgument
{
	private readonly int TEMP_OR_PERM_ARG_OFFSET = 1;

	private readonly int LIFE_ARG_OFFSET = 2;

	private readonly int ATTACK_ARG_OFFSET = 3;

	public AIPolishConvertedExpression Attack { get; private set; }

	public AIPolishConvertedExpression Life { get; private set; }

	public AIScriptTokenArgType TempOrPerm { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 4;

	public AIOtherWhenPlayBuff(string text)
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

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, Attack, Life);
		bool isTemp = TempOrPerm == AIScriptTokenArgType.TEMP;
		if (SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AIBuffSimulationUtility.BuffAll_old(targets, field, buffExecutingInfo_old, isTemp, playPtn, situation);
		}
		else
		{
			AIConsoleUtility.LogError($"AIOtherWhenPlayBuff.RunTagMethod() : Unsupported SelectType {SelectType}");
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override void TargetLifePrediction(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, LifeRecord targetLifeRecord)
	{
		if (SelectType == AIScriptTokenArgType.ALL_SELECT && !IsTargetNotCandidate(target, owner, field, playPtn, situation))
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
		if (SelectType != AIScriptTokenArgType.ALL_SELECT)
		{
			return;
		}
		int num = (int)Life.EvalArg(owner, playPtn, field, situation);
		for (int i = 0; i < targetList.Count; i++)
		{
			AIVirtualCard target = targetList[i];
			if (!IsTargetNotCandidate(target, owner, field, playPtn, situation))
			{
				LifeRecord lifeRecord = lifeList[i];
				lifeRecord.MaxLife = Math.Max(lifeRecord.MaxLife + num, 0);
				if (lifeRecord.CurrentLife > lifeRecord.MaxLife)
				{
					lifeRecord.CurrentLife = lifeRecord.MaxLife;
				}
			}
		}
	}

	private bool IsTargetNotCandidate(AIVirtualCard target, AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (target.IsLeader || target.IsFollower(playPtn))
		{
			return !AIFilteringUtility.CheckMatchTargetFiltering(target, GetCandidateRange(field), base.TargetFilters, playPtn, owner, situation);
		}
		return true;
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		bool isAttackEffective = !Attack.IsZeroOrNone();
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective, isBlockDead);
	}
}
