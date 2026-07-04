using System.Collections.Generic;

namespace Wizard;

public class AITurnEndDraw : AIScriptArgumentExpressions, IAITurnEndArgument
{
	private AIScriptTokenArgType _drawSide;

	private AIPolishConvertedExpression _drawCount;

	private readonly AIScriptTokenArgType[] _legalDrawSideArgs = new AIScriptTokenArgType[3]
	{
		AIScriptTokenArgType.ALLY,
		AIScriptTokenArgType.OPPONENT,
		AIScriptTokenArgType.BOTH
	};

	public bool IsAllyTurn { get; private set; }

	public AITurnEndDraw(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_drawSide = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[0], _legalDrawSideArgs);
		_drawCount = _exprList[1];
		IsAllyTurn = TurnEndTagCollection.IsAllyTurn(_exprList, GetType(), 2);
	}

	public float CalculateThreaten(AIVirtualCard tagOwner, ref Tuple<int, int>[] allInplayStatusList)
	{
		return 0f;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		base.Execute(tagOwner, field, playPtn, situation);
		int num = (int)_drawCount.EvalArg(tagOwner, playPtn, field, situation);
		if (num > 0)
		{
			bool flag = false;
			bool flag2 = false;
			switch (_drawSide)
			{
			case AIScriptTokenArgType.ALLY:
				flag = tagOwner.IsAlly;
				flag2 = !tagOwner.IsAlly;
				break;
			case AIScriptTokenArgType.OPPONENT:
				flag2 = tagOwner.IsAlly;
				flag = !tagOwner.IsAlly;
				break;
			case AIScriptTokenArgType.BOTH:
				flag = true;
				flag2 = true;
				break;
			}
			if (flag)
			{
				field.DrawCard(isAlly: true, num, playPtn, situation);
			}
			if (flag2)
			{
				field.DrawCard(isAlly: false, num, playPtn, situation);
			}
		}
	}
}
