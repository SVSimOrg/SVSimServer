namespace Wizard;

public class AIScriptTokenBase
{
	public AIScriptTokenType Type { get; private set; }

	public virtual float Value { get; private set; }

	public int Priority => Type switch
	{
		AIScriptTokenType.OR => -3, 
		AIScriptTokenType.AND => -2, 
		AIScriptTokenType.PLUS => 0, 
		AIScriptTokenType.MINUS => 0, 
		AIScriptTokenType.MULTI => 2, 
		AIScriptTokenType.DIV => 2, 
		AIScriptTokenType.REMAIN => 2, 
		AIScriptTokenType.MORE_THAN => -1, 
		AIScriptTokenType.LESS_THAN => -1, 
		AIScriptTokenType.MORE_EQUAL => -1, 
		AIScriptTokenType.LESS_EQUAL => -1, 
		AIScriptTokenType.EQUAL => -1, 
		AIScriptTokenType.MAX => 3, 
		AIScriptTokenType.MIN => 3, 
		AIScriptTokenType.FUNC => 6, 
		_ => 0, 
	};

	protected AIScriptTokenBase(AIScriptTokenType type, float value)
	{
		Type = type;
		Value = value;
	}

	public virtual AIScriptTokenBase Clone()
	{
		return new AIScriptTokenBase(Type, Value);
	}

	public virtual bool IsEqual(AIScriptTokenBase token)
	{
		if (Type == token.Type)
		{
			return Value == token.Value;
		}
		return false;
	}
}
