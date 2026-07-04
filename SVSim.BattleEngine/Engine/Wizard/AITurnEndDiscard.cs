using System.Collections.Generic;

namespace Wizard;

public class AITurnEndDiscard : AIFiltersAndSelectTypeArgument, IAITurnEndArgument
{
	private AIPolishConvertedExpression _discardCountArg;

	private readonly int IS_ALLY_TURN_OFFSET = 1;

	private readonly int DISCARD_COUNT_ARG_OFFSET = 2;

	public bool IsAllyTurn { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 3;

	public AITurnEndDiscard(string text)
		: base(text)
	{
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - IS_ALLY_TURN_OFFSET);
		_discardCountArg = _exprList[_exprList.Count - DISCARD_COUNT_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null)
		{
			int discardCount = (int)_discardCountArg.EvalArg(tagOwner, playPtn, field, situation);
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.DiscardAll(tagOwner, targetsFromField, field, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AISkillSimulationUtility.DiscardRandom(tagOwner, field, targetsFromField, discardCount, situation);
				break;
			}
		}
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		return 0f;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.GetSimulationHandCards();
	}
}
