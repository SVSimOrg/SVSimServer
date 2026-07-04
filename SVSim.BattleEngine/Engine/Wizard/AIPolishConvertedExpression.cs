using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AIPolishConvertedExpression
{
	private readonly AIScriptTokenFuncType[] EvalFuncList = new AIScriptTokenFuncType[22]
	{
		AIScriptTokenFuncType.EVAL_ALL_DAMAGE,
		AIScriptTokenFuncType.EVAL_TARGETING_DAMAGE,
		AIScriptTokenFuncType.EVAL_TARGETING_AND_RANDOM_MULTI_DAMAGE,
		AIScriptTokenFuncType.EVAL_ALL_DESTROY,
		AIScriptTokenFuncType.EVAL_ALL_BOUNCE,
		AIScriptTokenFuncType.EVAL_TARGETING_BOUNCE,
		AIScriptTokenFuncType.EVAL_TARGETING_BANISH,
		AIScriptTokenFuncType.EVAL_RANDOM_BANISH,
		AIScriptTokenFuncType.EVAL_ALL_BANISH,
		AIScriptTokenFuncType.EVAL_ALL_METAMORPHOSE,
		AIScriptTokenFuncType.EVAL_TARGETING_METAMORPHOSE,
		AIScriptTokenFuncType.EVAL_ALL_BUFF,
		AIScriptTokenFuncType.EVAL_RANDOM_BUFF,
		AIScriptTokenFuncType.EVAL_TARGETING_DESTROY,
		AIScriptTokenFuncType.EVAL_TARGETING_HEAL,
		AIScriptTokenFuncType.EVAL_ALL_HEAL,
		AIScriptTokenFuncType.EVAL_RANDOM_BOUNCE,
		AIScriptTokenFuncType.EVAL_RANDOM_DESTROY,
		AIScriptTokenFuncType.EVAL_RANDOM_METAMORPHOSE,
		AIScriptTokenFuncType.EVAL_RANDOM_MULTI_DAMAGE,
		AIScriptTokenFuncType.EVAL_RANDOM_MULTI_SELECT_DAMAGE,
		AIScriptTokenFuncType.EVAL_DIVIDED_DAMAGE
	};

	public List<AIScriptTokenBase> TokenList { get; private set; }

	public int Hash { get; private set; }

	public bool IsMultiplyMarked { get; private set; }

	public AIPolishConvertedExpression(string text, bool isMultiplyMarked = false)
	{
		IsMultiplyMarked = isMultiplyMarked;
		TokenList = CreateExpression(text);
		Hash = text.GetHashCode();
	}

	public int EvalID()
	{
		if (TokenList == null || TokenList.Count != 1)
		{
			return 0;
		}
		if (!(TokenList[0] is AIScriptIDToken))
		{
			return 0;
		}
		return (TokenList[0] as AIScriptIDToken).ID;
	}

	public List<int> GetReferringIDLists()
	{
		List<int> list = null;
		for (int i = 0; i < TokenList.Count; i++)
		{
			AIScriptTokenBase aIScriptTokenBase = TokenList[i];
			if (aIScriptTokenBase is AIScriptIDToken)
			{
				if (list == null)
				{
					list = new List<int>();
				}
				list.Add(((AIScriptIDToken)aIScriptTokenBase).ID);
			}
		}
		return list;
	}

	public string EvalText()
	{
		if (TokenList == null || TokenList.Count != 1)
		{
			return "";
		}
		if (TokenList[0] is AIScriptTextToken aIScriptTextToken)
		{
			return aIScriptTextToken.Text;
		}
		return "";
	}

	public float EvalArg(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, AISituationInfo situation = null)
	{
		if (TokenList == null || TokenList.Count <= 0)
		{
			return 0f;
		}
		return GetFloatValue(tagOwner, situation, playPtn, field);
	}

	public bool CheckCondition(AIVirtualCard tagOwner, List<int> playPtn, AIVirtualField field, AISituationInfo situation)
	{
		if (TokenList == null || TokenList.Count <= 0)
		{
			return true;
		}
		return GetFloatValue(tagOwner, situation, playPtn, field) > 0f;
	}

	public bool IsZeroOrNone()
	{
		if (TokenList == null || TokenList.Count <= 0)
		{
			return true;
		}
		if (TokenList.Count > 1)
		{
			return false;
		}
		AIScriptTokenBase aIScriptTokenBase = TokenList[0];
		if (aIScriptTokenBase is AIScriptArgumentToken { ArgumentType: AIScriptTokenArgType.NONE } || aIScriptTokenBase is AIScriptNumericToken { Value: 0f })
		{
			return true;
		}
		return false;
	}

	public bool IsMathematicExpress()
	{
		if (TokenList == null || TokenList.Count <= 0)
		{
			return false;
		}
		if (TokenList.Count > 1)
		{
			return true;
		}
		AIScriptTokenBase aIScriptTokenBase = TokenList[0];
		if (!(aIScriptTokenBase is AIScriptNumericToken))
		{
			return aIScriptTokenBase is AIScriptVariableToken;
		}
		return true;
	}

	public bool IsCertainArgumentTypeExpress(AIScriptTokenArgType argType)
	{
		if (TokenList == null || TokenList.Count <= 0 || TokenList.Count > 1)
		{
			return false;
		}
		if (TokenList[0] is AIScriptArgumentToken aIScriptArgumentToken && aIScriptArgumentToken.ArgumentType == argType)
		{
			return true;
		}
		return false;
	}

	private float GetFloatValue(AIVirtualCard tagOwner, AISituationInfo situation, List<int> playPtn, AIVirtualField field)
	{
		if (TokenList == null || TokenList.Count <= 0)
		{
			return 0f;
		}
		return AIScriptExpressionCalculator.CalculateExpression(TokenList, playPtn, field, tagOwner, situation);
	}

	private List<AIScriptTokenBase> CreateExpression(string expression)
	{
		if (expression == "")
		{
			return null;
		}
		List<AIScriptTokenBase> list = CreateScriptTokenList(expression);
		if (list == null || list.Count <= 0)
		{
			return null;
		}
		if (list.Count == 1 && list[0] is AIScriptNumericToken)
		{
			return list;
		}
		return _ConvertExprToIPolish(list);
	}

	private List<AIScriptTokenBase> CreateScriptTokenList(string text)
	{
		List<AIScriptTokenBase> list = new List<AIScriptTokenBase>();
		string[] array = text.Split(' ');
		bool flag = false;
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (!(text2 == ""))
			{
				AIScriptTokenBase aIScriptTokenBase = AIScriptParser.ConvertWordToToken(text2);
				if (aIScriptTokenBase != null)
				{
					list.Add(aIScriptTokenBase);
				}
				else
				{
					flag = true;
				}
			}
		}
		return list;
	}

	private List<AIScriptTokenBase> _ConvertExprToIPolish(List<AIScriptTokenBase> expression)
	{
		List<AIScriptTokenBase> list = new List<AIScriptTokenBase>();
		Stack<AIScriptTokenBase> stack = new Stack<AIScriptTokenBase>();
		AIScriptFunctionToken aIScriptFunctionToken = null;
		for (int i = 0; i < expression.Count; i++)
		{
			AIScriptTokenBase aIScriptTokenBase = expression[i];
			if (aIScriptTokenBase is AIScriptNumericToken || aIScriptTokenBase is AIScriptArgumentToken || aIScriptTokenBase is AIScriptVariableToken || aIScriptTokenBase is AIScriptIDToken)
			{
				list.Add(aIScriptTokenBase);
				continue;
			}
			if (aIScriptTokenBase is AIScriptOperatorSymbolToken)
			{
				if (aIScriptTokenBase.Type == AIScriptTokenType.COMMA)
				{
					while (stack.Count > 0)
					{
						AIScriptTokenBase aIScriptTokenBase2 = stack.Pop();
						if (aIScriptTokenBase2 is AIScriptOperatorSymbolToken && aIScriptTokenBase2.Type == AIScriptTokenType.LEFT_BLACKET)
						{
							break;
						}
						if (aIScriptTokenBase2 == aIScriptFunctionToken)
						{
							aIScriptFunctionToken = stack.LastOrDefault((AIScriptTokenBase t) => t is AIScriptFunctionToken) as AIScriptFunctionToken;
						}
						list.Add(aIScriptTokenBase2);
					}
					aIScriptFunctionToken.ArgCount++;
					stack.Push(new AIScriptOperatorSymbolToken(AIScriptTokenType.LEFT_BLACKET));
					continue;
				}
				if (aIScriptTokenBase.Type == AIScriptTokenType.LEFT_BLACKET)
				{
					stack.Push(aIScriptTokenBase);
					continue;
				}
				if (aIScriptTokenBase.Type == AIScriptTokenType.RIGHT_BLACKET)
				{
					while (stack.Count > 0)
					{
						AIScriptTokenBase aIScriptTokenBase3 = stack.Pop();
						if (aIScriptTokenBase3 is AIScriptOperatorSymbolToken && aIScriptTokenBase3.Type == AIScriptTokenType.LEFT_BLACKET)
						{
							break;
						}
						if (aIScriptTokenBase3 == aIScriptFunctionToken)
						{
							aIScriptFunctionToken = stack.LastOrDefault((AIScriptTokenBase t) => t is AIScriptFunctionToken) as AIScriptFunctionToken;
						}
						list.Add(aIScriptTokenBase3);
					}
					continue;
				}
			}
			if (aIScriptTokenBase is AIScriptFunctionToken)
			{
				aIScriptFunctionToken = aIScriptTokenBase as AIScriptFunctionToken;
				aIScriptFunctionToken.ArgCount = 1;
			}
			if (stack.Count <= 0)
			{
				stack.Push(aIScriptTokenBase);
				continue;
			}
			while (stack.Count > 0)
			{
				AIScriptTokenBase aIScriptTokenBase4 = stack.Peek();
				if (aIScriptTokenBase4.Type == AIScriptTokenType.LEFT_BLACKET || aIScriptTokenBase4.Priority < aIScriptTokenBase.Priority)
				{
					break;
				}
				if (aIScriptTokenBase4 == aIScriptFunctionToken)
				{
					aIScriptFunctionToken = stack.LastOrDefault((AIScriptTokenBase t) => t is AIScriptFunctionToken) as AIScriptFunctionToken;
				}
				list.Add(stack.Pop());
			}
			stack.Push(aIScriptTokenBase);
		}
		while (stack.Count > 0)
		{
			list.Add(stack.Pop());
		}
		return list;
	}

	public AIScriptCalculationToken CreateCalculationToken()
	{
		return new AIScriptCalculationToken(TokenList);
	}

	public bool IsHoldingEVAL()
	{
		if (TokenList == null)
		{
			return false;
		}
		for (int i = 0; i < TokenList.Count; i++)
		{
			AIScriptTokenBase aIScriptTokenBase = TokenList[i];
			if (aIScriptTokenBase is AIScriptFunctionToken && EvalFuncList.Contains(((AIScriptFunctionToken)aIScriptTokenBase).FuncType))
			{
				return true;
			}
		}
		return false;
	}
}
