using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayChangeCost : AIWhenPlayTagArgument
{
	private AIPolishConvertedExpression _costValue;

	private readonly AIScriptTokenArgType[] _legalCostTypeArgs = new AIScriptTokenArgType[2]
	{
		AIScriptTokenArgType.ADD,
		AIScriptTokenArgType.SET
	};

	protected override int SELECT_TYPE_OFFSET => 3;

	public AIScriptTokenArgType CostVariableType { get; private set; }

	public AIWhenPlayChangeCost(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_costValue = _exprList[_exprList.Count - 1];
		CostVariableType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 2], _legalCostTypeArgs);
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
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
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

	public override void ExecuteForPlayPtnEvaluation(AIVirtualCard owner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		Execute(owner, field, playPtn, situation);
	}

	public override void PseudoExecute(AIVirtualField field, AISinglePlayptnRecord record, PlayedCardInfo playInfo, AIVirtualTargetSelectAction situation)
	{
		Execute(playInfo.Card, field, record.PlayPtn, situation);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			AIConsoleUtility.Log("AIWhenPlayChangeCost: filtered target is nothing");
			return;
		}
		int costValue = GetCostValue(tagOwner, field, playPtn, situation);
		AIChangeCostSimulationUtility.ExecuteCostChange(CostVariableType, base.SelectType, costValue, tagOwner, targetsFromField, situation, field);
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

	public int GetCostValue(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (_costValue == null)
		{
			AIConsoleUtility.LogError("AIWhenPlayChangeCost.GetCostValue() error!! _costValue is null");
			return 0;
		}
		return (int)_costValue.EvalArg(tagOwner, playPtn, field, situation);
	}
}
