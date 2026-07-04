using System.Collections.Generic;

namespace Wizard;

public class AIEvoHandBuff : AIEvoTagArgument
{
	private AIPolishConvertedExpression _attackBuffArgument;

	private AIPolishConvertedExpression _lifeBuffArgument;

	private readonly int ATTACK_BUFF_ARG_OFFSET = 2;

	private readonly int LIFE_BUFF_ARG_OFFSET = 1;

	protected override int SELECT_TYPE_OFFSET => 3;

	public override TargetSelectType SimulationTargetSelectType => TargetSelectType.NormalRuleBase;

	public AIEvoHandBuff(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_attackBuffArgument = _exprList[_exprList.Count - ATTACK_BUFF_ARG_OFFSET];
		_lifeBuffArgument = _exprList[_exprList.Count - LIFE_BUFF_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			AIBuffExecutingInfo_old buffExecutingInfo_old = AIBuffSimulationUtility.GetBuffExecutingInfo_old(tagOwner, field, situation, playPtn, _attackBuffArgument, _lifeBuffArgument);
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

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
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

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[3]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}

	protected override List<AIVirtualCard> GetBaseFilteringCards(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, bool isBlockDead)
	{
		List<AIVirtualCard> baseFilteringCards = base.GetBaseFilteringCards(candidates, tagOwner, field, playPtn, situation, isBlockDead);
		baseFilteringCards?.RemoveAll((AIVirtualCard c) => !c.IsUnit);
		return baseFilteringCards;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}
}
