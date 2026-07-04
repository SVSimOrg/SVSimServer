using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayHandBuff : AIWhenPlayTagArgument
{
	private AIPolishConvertedExpression _attackBuff;

	private AIPolishConvertedExpression _lifeBuff;

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIWhenPlayHandBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_attackBuff = _exprList[_exprList.Count - 2];
		_lifeBuff = _exprList[_exprList.Count - 1];
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[3]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.TARGET_SELECT
		};
	}

	public override TargetSelectType GetTargetSelectType()
	{
		return TargetSelectType.NormalRuleBase;
	}

	public override List<AIVirtualCard> GetTargetsFromField(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		List<AIVirtualCard> candidateRange = GetCandidateRange(field);
		return GetFilteredTargets(candidateRange, owner, playPtn, situation, isBlockDead);
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		List<AIVirtualCard> filteredTargets = base.GetFilteredTargets(candidates, tagOwner, playPtn, situation, isBlockDead);
		filteredTargets?.RemoveAll((AIVirtualCard c) => !c.IsInHand || c.IsSameCard(tagOwner) || !c.IsUnit);
		return filteredTargets;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, _attackBuff, _lifeBuff);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIHandBuffSimulationUtility.ExecuteHandBuffAll(targetsFromField, buffExecutingInfo_old, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIHandBuffSimulationUtility.ExecuteHandBuffRandom(targetsFromField, buffExecutingInfo_old, playPtn, situation);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
				ExecuteTargetSelectHandBuff(targetsFromField, buffExecutingInfo_old, situation);
				break;
			}
		}
	}

	public override void RegisterRuleBaseTargets(List<AIVirtualCard> candidates, AIVirtualCard actor, AIVirtualField field, AIVirtualTargetSelectAction situation, ref List<AIVirtualCard> targetList)
	{
		base.RegisterRuleBaseTargets(candidates, actor, field, situation, ref targetList);
		AIVirtualCard aIVirtualCard = AIHandBuffSimulationUtility.SelectHandBuffTarget(candidates, AISelectTargetPattern.Best, EnemyAI.EmptyPlayPtn, situation);
		if (aIVirtualCard != null)
		{
			targetList = AIParamQuery.AddElementToList(aIVirtualCard, targetList);
		}
	}

	private void ExecuteTargetSelectHandBuff(List<AIVirtualCard> candidates, AIBuffExecutingInfo_old buffInfo, AISituationInfo situation)
	{
		if (situation != null && situation.IsTargetExists(base.SelectType))
		{
			AIHandBuffSimulationUtility.ExecuteHandBuffTargetSelect(buffInfo, base.SelectType, situation);
		}
		else
		{
			AIHandBuffSimulationUtility.ExecuteHandBuffBestTarget(candidates, buffInfo, situation);
		}
	}
}
