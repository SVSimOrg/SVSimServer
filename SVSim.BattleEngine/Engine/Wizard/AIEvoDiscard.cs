using System.Collections.Generic;

namespace Wizard;

public class AIEvoDiscard : AIEvoTagArgument
{
	private AIPolishConvertedExpression _discardCountArg;

	protected override int SELECT_TYPE_OFFSET => 2;

	public override TargetSelectType SimulationTargetSelectType => TargetSelectType.NormalRuleBase;

	public AIEvoDiscard(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_discardCountArg = _exprList[_exprList.Count - 1];
	}

	public int GetDiscardCount(AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (_discardCountArg == null)
		{
			return 0;
		}
		return (int)_discardCountArg.EvalArg(tagOwner, playPtn, tagOwner.SelfField, situation);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int discardCount = GetDiscardCount(tagOwner, playPtn, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.TARGET_SELECT:
				AISkillSimulationUtility.ExecuteTargetSelectDiscard(targetsFromField, discardCount, base.SelectType, tagOwner, field, playPtn, situation);
				break;
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.DiscardAll(tagOwner, targetsFromField, field, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AISkillSimulationUtility.DiscardRandom(tagOwner, field, targetsFromField, discardCount, situation);
				break;
			}
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	public override void RegisterRuleBaseTargets(List<AIVirtualCard> candidates, AIVirtualCard actor, AIVirtualField field, AIVirtualTargetSelectAction situation, ref List<AIVirtualCard> targetList)
	{
		base.RegisterRuleBaseTargets(candidates, actor, field, situation, ref targetList);
		List<int> emptyPlayPtn = EnemyAI.EmptyPlayPtn;
		int discardCount = GetDiscardCount(actor, emptyPlayPtn, situation);
		List<AIVirtualCard> list = AIDiscardUtility.SelectBestDiscardTarget(actor, field, candidates, discardCount, emptyPlayPtn, situation);
		if (list != null && list.Count > 0)
		{
			targetList = AIParamQuery.AddRangeToList(list, targetList);
		}
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}
}
