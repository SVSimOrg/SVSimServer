using System.Collections.Generic;

namespace Wizard;

public class AITurnEndDestroy : AIFiltersAndSelectTypeArgument, IAITurnEndArgument
{
	private AIPolishConvertedExpression _count;

	private readonly int IS_ALLY_TURN_OFFSET = 1;

	private readonly int COUNT_ARG_OFFSET = 2;

	public bool IsAllyTurn { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 3;

	public AITurnEndDestroy(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - IS_ALLY_TURN_OFFSET);
		_count = _exprList[_exprList.Count - COUNT_ARG_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField == null || targetsFromField.Count <= 0)
		{
			return;
		}
		int num = (int)_count.EvalArg(tagOwner, playPtn, field, situation);
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
			if (num > 0)
			{
				AISkillSimulationUtility.DestroyAll(targetsFromField, field, situation);
			}
			break;
		case AIScriptTokenArgType.RANDOM_SELECT:
		{
			for (int i = 0; i < num; i++)
			{
				AISkillSimulationUtility.DestroyRandom(targetsFromField, tagOwner, field, playPtn, situation);
			}
			break;
		}
		case AIScriptTokenArgType.RANDOM_MULTI_SELECT:
			AISkillSimulationUtility.DestroyRandom(targetsFromField, tagOwner, field, playPtn, situation, num);
			break;
		case AIScriptTokenArgType.TARGET_SELECT:
			break;
		}
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		return 0f;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[3]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT,
			AIScriptTokenArgType.RANDOM_MULTI_SELECT
		};
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
