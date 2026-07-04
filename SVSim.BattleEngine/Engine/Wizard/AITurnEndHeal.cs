using System.Collections.Generic;

namespace Wizard;

public class AITurnEndHeal : AIFiltersAndSelectTypeArgument, IAITurnEndArgument
{
	private readonly int HEAL_AMOUNT_OFFSET = 2;

	private readonly int IS_ALLY_TURN_OFFSET = 1;

	public AIPolishConvertedExpression Heal { get; private set; }

	public bool IsAllyTurn { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 3;

	public AITurnEndHeal(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - IS_ALLY_TURN_OFFSET);
		int count = _exprList.Count;
		Heal = _exprList[count - HEAL_AMOUNT_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int heal = (int)Heal.EvalArg(tagOwner, playPtn, field, situation);
			AIScriptTokenArgType selectType = base.SelectType;
			if (selectType != AIScriptTokenArgType.RANDOM_SELECT && selectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AISkillSimulationUtility.HealAll(targetsFromField, field, heal, playPtn, situation);
			}
		}
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		return 0f;
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}
}
