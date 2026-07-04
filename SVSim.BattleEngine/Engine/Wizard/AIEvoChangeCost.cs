using System.Collections.Generic;

namespace Wizard;

public class AIEvoChangeCost : AIEvoTagArgument
{
	private readonly AIScriptTokenArgType[] _legalCostTypeArgs = new AIScriptTokenArgType[2]
	{
		AIScriptTokenArgType.ADD,
		AIScriptTokenArgType.SET
	};

	private AIPolishConvertedExpression _costValue;

	public AIScriptTokenArgType CostVariableType { get; private set; }

	public override TargetSelectType SimulationTargetSelectType => TargetSelectType.NormalRuleBase;

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIEvoChangeCost(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_costValue = _exprList[_exprList.Count - 1];
		CostVariableType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], _legalCostTypeArgs);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int costValue = GetCostValue(tagOwner, field, playPtn, situation);
			AIChangeCostSimulationUtility.ExecuteCostChange(CostVariableType, base.SelectType, costValue, tagOwner, targetsFromField, situation, field);
		}
	}

	public override void RegisterRuleBaseTargets(List<AIVirtualCard> candidates, AIVirtualCard actor, AIVirtualField field, AIVirtualTargetSelectAction situation, ref List<AIVirtualCard> targetList)
	{
		base.RegisterRuleBaseTargets(candidates, actor, field, situation, ref targetList);
		AIVirtualCard aIVirtualCard = AIChangeCostSimulationUtility.SelectTargetForChangeCost(actor, candidates, AISelectTargetPattern.Best);
		if (aIVirtualCard != null)
		{
			targetList = AIParamQuery.AddElementToList(aIVirtualCard, targetList);
		}
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
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

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}

	private int GetCostValue(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_costValue == null)
		{
			AIConsoleUtility.LogError("AIEvoChangeCost.GetCostValue() error!! _costValue is null");
			return 0;
		}
		return (int)_costValue.EvalArg(tagOwner, playPtn, field, situation);
	}
}
