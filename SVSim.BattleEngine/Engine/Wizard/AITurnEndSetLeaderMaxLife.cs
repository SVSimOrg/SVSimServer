using System.Collections.Generic;

namespace Wizard;

public class AITurnEndSetLeaderMaxLife : AIScriptArgumentExpressions, IAITurnEndArgument
{
	private readonly int IS_ALLY_TURN_OFFSET = 1;

	private readonly int VALUE_OFFSET = 2;

	private readonly int SIDE_OFFSET = 3;

	private AIScriptTokenArgType _side;

	private AIPolishConvertedExpression _life;

	public bool IsAllyTurn { get; private set; }

	public AITurnEndSetLeaderMaxLife(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), _exprList.Count - IS_ALLY_TURN_OFFSET);
		_side = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SIDE_OFFSET]);
		_life = _exprList[_exprList.Count - VALUE_OFFSET];
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		int maxLife = (int)_life.EvalArg(tagOwner, playPtn, field, situation);
		AISetStatusSimulationUtility.SetLeaderMaxLife(tagOwner, maxLife, _side, field, situation);
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		AIVirtualField selfField = tagOwner.SelfField;
		(AIVirtualCard, AIVirtualCard) targetSideLeaders = AISetStatusSimulationUtility.GetTargetSideLeaders(tagOwner, selfField);
		if (targetSideLeaders.Item1 == null || targetSideLeaders.Item2 == null)
		{
			AIConsoleUtility.LogError("AITurnEndSetLeaderMaxLife.CalculateThreaten(): Failed to get the leader.");
			return 0f;
		}
		int changeLife = (int)_life.EvalArg(tagOwner, EnemyAI.EmptyPlayPtn, selfField);
		float num = 0f;
		switch (_side)
		{
		case AIScriptTokenArgType.ALLY:
			num += CalculateTargetLeader(tagOwner, targetSideLeaders.Item1, selfField, changeLife, ref allInplayStatusList);
			break;
		case AIScriptTokenArgType.OPPONENT:
			num += CalculateTargetLeader(tagOwner, targetSideLeaders.Item2, selfField, changeLife, ref allInplayStatusList);
			break;
		case AIScriptTokenArgType.BOTH:
			num += CalculateTargetLeader(tagOwner, targetSideLeaders.Item1, selfField, changeLife, ref allInplayStatusList);
			num += CalculateTargetLeader(tagOwner, targetSideLeaders.Item2, selfField, changeLife, ref allInplayStatusList);
			break;
		default:
			AIConsoleUtility.LogError($"AITurnEndSetLeaderMaxLife.CalculateThreaten(): Unexpected side type. type:{_side}");
			break;
		}
		return num;
	}

	private float CalculateTargetLeader(AIVirtualCard tagOwner, AIVirtualCard leader, AIVirtualField field, int changeLife, ref Tuple<int, int>[] allInplayStatusList)
	{
		int num = field.CardListSet.BothClassAndInplayCards.IndexOf(leader);
		int second = allInplayStatusList[num].second;
		if (second <= 0)
		{
			return 0f;
		}
		if (changeLife < 0)
		{
			changeLife = 0;
		}
		float result = 0f;
		if (changeLife < second)
		{
			result = AILeaderLifeEvaluationUtility.Evaluate(changeLife, second, leader.IsAlly, tagOwner.IsAlly);
			allInplayStatusList[num].second = changeLife;
		}
		return result;
	}
}
