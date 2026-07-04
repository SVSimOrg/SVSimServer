namespace Wizard;

public class AIScriptFunctionToken : AIScriptTokenBase
{
	public AIScriptTokenFuncType FuncType;

	public int ArgCount;

	public AIScriptFunctionToken(AIScriptTokenFuncType funcType, int refNum)
		: base(AIScriptTokenType.FUNC, refNum)
	{
		FuncType = funcType;
		ArgCount = refNum;
	}

	public override AIScriptTokenBase Clone()
	{
		return new AIScriptFunctionToken(FuncType, ArgCount);
	}

	public override bool IsEqual(AIScriptTokenBase token)
	{
		if (!(token is AIScriptFunctionToken aIScriptFunctionToken))
		{
			return false;
		}
		if (FuncType == aIScriptFunctionToken.FuncType)
		{
			return ArgCount == aIScriptFunctionToken.ArgCount;
		}
		return false;
	}
}
