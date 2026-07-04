namespace Wizard;

public class AIScriptNumericToken : AIScriptTokenBase
{
	public AIScriptNumericToken(float value)
		: base(AIScriptTokenType.NUMERIC, value)
	{
	}

	public override AIScriptTokenBase Clone()
	{
		return new AIScriptNumericToken(Value);
	}
}
