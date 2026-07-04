using System.Collections.Generic;

namespace Wizard;

public class AITurnEndMetamorphose : AIFiltersAndSelectTypeArgument, IAITurnEndArgument
{
	private readonly int METAMORPHOSE_ID_OFFSET = 2;

	private readonly int IS_ALLY_TURN_OFFSET = 1;

	public int MetamorphoseCardId { get; private set; }

	public bool IsAllyTurn { get; private set; }

	protected override int SELECT_TYPE_OFFSET => 3;

	public AITurnEndMetamorphose(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - IS_ALLY_TURN_OFFSET);
		MetamorphoseCardId = _exprList[_exprList.Count - METAMORPHOSE_ID_OFFSET].EvalID();
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
			{
				AIMetamorphoseSimulationUtility.MetamorphoseAll(field, targetsFromField, MetamorphoseCardId, tagOwner, situation);
			}
			else if (base.SelectType == AIScriptTokenArgType.RANDOM_SELECT)
			{
				AIMetamorphoseSimulationUtility.MetamorphoseRandom(field, targetsFromField, MetamorphoseCardId, tagOwner, playPtn, situation);
			}
		}
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		return 0f;
	}

	protected override List<AIVirtualCard> GetCandidateRange(AIVirtualField field)
	{
		return field.CardListSet.BothInplayCards;
	}
}
