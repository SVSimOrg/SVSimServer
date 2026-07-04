namespace Wizard;

public class AIScriptVariableToken : AIScriptTokenBase
{
	public AIScriptTokenVariableType VariableType;

	public AIScriptVariableToken(AIScriptTokenVariableType valType)
		: base(AIScriptTokenType.VARIABLE, 0f)
	{
		VariableType = valType;
	}

	public override AIScriptTokenBase Clone()
	{
		return new AIScriptVariableToken(VariableType);
	}

	public override bool IsEqual(AIScriptTokenBase token)
	{
		if (!(token is AIScriptVariableToken aIScriptVariableToken))
		{
			return false;
		}
		return VariableType == aIScriptVariableToken.VariableType;
	}
}
