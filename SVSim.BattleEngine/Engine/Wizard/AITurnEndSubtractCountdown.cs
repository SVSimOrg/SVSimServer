using System.Collections.Generic;

namespace Wizard;

public class AITurnEndSubtractCountdown : AIFiltersAndSelectTypeArgument, IAITurnEndArgument
{
	private readonly int COUNT_ARG_OFFSET = 2;

	private readonly int IS_ALLY_TURN_OFFSET = 1;

	public AIPolishConvertedExpression CountDown { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 3;

	public bool IsAllyTurn { get; private set; }

	public AITurnEndSubtractCountdown(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - IS_ALLY_TURN_OFFSET);
		CountDown = _exprList[_exprList.Count - COUNT_ARG_OFFSET];
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		return 0f;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int value = (int)CountDown.EvalArg(tagOwner, playPtn, tagOwner.SelfField);
			AISubtractCountdownSimulationUtility.SubtractCountdownAll(targetsFromField, value, situation);
		}
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForCountdownAmuletOnly(candidates, tagOwner, base.Filters, playPtn, situation, isBlockDead);
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.AllyInplayCards;
	}
}
