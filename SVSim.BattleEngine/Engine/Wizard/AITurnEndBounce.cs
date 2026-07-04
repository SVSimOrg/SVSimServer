using System.Collections.Generic;

namespace Wizard;

public class AITurnEndBounce : AIFiltersAndSelectTypeArgument, IAITurnEndArgument
{
	private readonly int IS_ALLY_TURN_OFFSET = 1;

	public bool IsAllyTurn { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 2;

	public AITurnEndBounce(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - IS_ALLY_TURN_OFFSET);
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
				AIBounceSimulationUtility.BounceAll(targetsFromField, situation);
				break;
			case AIScriptTokenArgType.RANDOM_SELECT:
				AIBounceSimulationUtility.BounceRandom(targetsFromField, tagOwner, field, playPtn, situation);
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
