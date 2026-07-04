namespace Wizard;

public class AIFirstMoveBonus : AIScriptArgumentExpressions
{
	private AIPolishConvertedExpression valueArg;

	private readonly int BONUS_VALUE_START_INDEX = 1;

	public AIScriptTokenArgType ActionArgType { get; private set; }

	public AIFirstMoveBonus(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		if (_exprList.Count <= BONUS_VALUE_START_INDEX)
		{
			AIConsoleUtility.LogError($"AIFirstMoveBonus Argument Error!! Arg count is not enough !! [count:{_exprList.Count}]");
			return;
		}
		for (int i = 0; i < _exprList.Count - BONUS_VALUE_START_INDEX; i++)
		{
			if (IsLegalArgType(_exprList[i], out var argType))
			{
				ActionArgType = argType;
			}
			else
			{
				AIConsoleUtility.LogError($"AIFirstMoveBonus ArgType Expression Error!! ArgType [{argType}] is Ilegal!");
			}
		}
		valueArg = _exprList[_exprList.Count - BONUS_VALUE_START_INDEX];
	}

	public float GetEvaluateValue(AIVirtualCard tagOwner, AISituationInfo situation)
	{
		return valueArg.EvalArg(tagOwner, EnemyAI.EmptyPlayPtn, tagOwner.SelfField, situation);
	}

	private bool IsLegalArgType(AIPolishConvertedExpression arg, out AIScriptTokenArgType argType)
	{
		argType = GetFirstTokenArgType(arg);
		AIScriptTokenArgType aIScriptTokenArgType = argType;
		if (aIScriptTokenArgType == AIScriptTokenArgType.ALL || (uint)(aIScriptTokenArgType - 161) <= 1u)
		{
			return true;
		}
		return false;
	}
}
