using System;
using System.Collections.Generic;

namespace Wizard;

public class AIScriptCalculationToken : AIScriptTokenBase
{
	public Func<AIVirtualCard, AIVirtualCard, AIVirtualField, List<int>, AISituationInfo, float> Expression;

	public AIScriptCalculationToken(List<AIScriptTokenBase> tokens)
		: base(AIScriptTokenType.FUNC, 0f)
	{
		CreateExpression(tokens);
	}

	public AIScriptCalculationToken(Func<AIVirtualCard, AIVirtualCard, AIVirtualField, List<int>, AISituationInfo, float> expression)
		: base(AIScriptTokenType.FUNC, 0f)
	{
		Expression = expression;
	}

	private void CreateExpression(List<AIScriptTokenBase> tokens)
	{
		Stack<AIScriptTokenBase> stack = new Stack<AIScriptTokenBase>();
		for (int i = 0; i < tokens.Count; i++)
		{
			AIScriptTokenBase token = tokens[i];
			if (token is AIScriptNumericToken || token is AIScriptIDToken)
			{
				stack.Push(token);
				continue;
			}
			AIScriptArgumentToken argument = token as AIScriptArgumentToken;
			if (argument != null)
			{
				AIScriptCalculationToken item = new AIScriptCalculationToken(delegate(AIVirtualCard owner, AIVirtualCard candidate, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
				{
					AIFilteringUtility.FilteringParameter filter = new AIFilteringUtility.FilteringParameter
					{
						Type = argument.ArgumentType,
						IsNot = argument.IsNot,
						IsSkipNextToken = false,
						FilteringThreshold = -1
					};
					if (argument is AIScriptTextToken aIScriptTextToken)
					{
						filter.SubParameter = aIScriptTextToken.Text;
					}
					situation.SetCurrentCheckCard(candidate);
					float result = (AIFilteringUtility.CheckFilterPassOrNot(candidate, filter, owner, playPtn, situation) ? 1f : 0f);
					situation.ResetCurrentCheckCard();
					return result;
				});
				stack.Push(item);
			}
			if (token is AIScriptVariableToken)
			{
				AIScriptCalculationToken item2 = new AIScriptCalculationToken(delegate(AIVirtualCard owner, AIVirtualCard candidate, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
				{
					situation.SetCurrentCheckCard(candidate);
					AIScriptTokenBase aIScriptTokenBase = AIScriptExpressionCalculator.CalculateVariableToken(token, playPtn, field, owner, situation);
					situation.ResetCurrentCheckCard();
					return aIScriptTokenBase.Value;
				});
				stack.Push(item2);
			}
			AIScriptFunctionToken funcToken = token as AIScriptFunctionToken;
			if (funcToken != null)
			{
				int argCount = ((AIScriptFunctionToken)token).ArgCount;
				List<AIScriptTokenBase> argList = new List<AIScriptTokenBase>();
				for (int num = 0; num < argCount; num++)
				{
					AIScriptTokenBase item3 = stack.Pop();
					argList.Add(item3);
				}
				AIScriptCalculationToken item4 = new AIScriptCalculationToken(delegate(AIVirtualCard owner, AIVirtualCard candidate, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
				{
					situation.SetCurrentCheckCard(candidate);
					AIScriptTokenBase aIScriptTokenBase = AIScriptExpressionCalculator.CalculateFunctionToken(argList, funcToken, field, playPtn, owner, situation);
					situation.ResetCurrentCheckCard();
					return aIScriptTokenBase.Value;
				});
				stack.Push(item4);
			}
			if (token is AIScriptOperatorSymbolToken)
			{
				AIScriptTokenBase right = stack.Pop();
				AIScriptTokenBase left = stack.Pop();
				AIScriptCalculationToken item5 = new AIScriptCalculationToken(delegate(AIVirtualCard owner, AIVirtualCard candidate, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
				{
					float tokenValue = GetTokenValue(left, owner, candidate, field, playPtn, situation);
					float tokenValue2 = GetTokenValue(right, owner, candidate, field, playPtn, situation);
					return AIScriptExpressionCalculator.CalculateByOperatorSymbol(token.Type, tokenValue, tokenValue2);
				});
				stack.Push(item5);
			}
		}
		if (stack.Count <= 1 && stack.Count > 0 && stack.Pop() is AIScriptCalculationToken aIScriptCalculationToken)
		{
			Expression = aIScriptCalculationToken.Expression;
		}
	}

	public float GetTokenValue(AIScriptTokenBase token, AIVirtualCard owner, AIVirtualCard candidate, AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		if (token is AIScriptCalculationToken aIScriptCalculationToken)
		{
			return aIScriptCalculationToken.Expression(owner, candidate, field, playPtn, situation);
		}
		if (token is AIScriptIDToken aIScriptIDToken)
		{
			if (!((aIScriptIDToken.ID == candidate.BaseId) ^ aIScriptIDToken.IsNot))
			{
				return 0f;
			}
			return 1f;
		}
		return token.Value;
	}
}
