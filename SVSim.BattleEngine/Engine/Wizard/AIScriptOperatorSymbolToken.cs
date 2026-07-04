namespace Wizard;

public class AIScriptOperatorSymbolToken : AIScriptTokenBase
{
	public AIScriptOperatorSymbolToken(AIScriptTokenType type)
		: base(type, 0f)
	{
	}

	public override AIScriptTokenBase Clone()
	{
		return new AIScriptOperatorSymbolToken(base.Type);
	}
}
