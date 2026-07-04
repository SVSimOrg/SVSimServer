using System.Collections.Generic;

namespace Wizard;

public class AITurnEndKeywordSkill : AIFiltersAndSelectTypeArgument, IAITurnEndArgument
{
	private readonly AIScriptTokenArgType _skillType;

	private AIPolishConvertedExpression _count;

	private readonly int IS_ALLY_TURN_OFFSET = 1;

	private readonly int COUNT_ARG_OFFSET = 2;

	public bool IsAllyTurn { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 3;

	public AITurnEndKeywordSkill(string text, AIScriptTokenArgType skill)
		: base(text)
	{
		_skillType = skill;
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
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AISkillSimulationUtility.GiveSkillToAll(targetsFromField, field, _skillType);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
			{
				int selectCount = (int)_count.EvalArg(tagOwner, playPtn, field, situation);
				AISkillSimulationUtility.GiveSkillRandom(targetsFromField, selectCount, field, _skillType);
				break;
			}
			default:
				AIConsoleUtility.LogError("AITurnEndKeywordSkill.Execute(): Unexcepted select type.");
				break;
			}
		}
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		return 0f;
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.RANDOM_SELECT
		};
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
