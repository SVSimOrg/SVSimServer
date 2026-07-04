namespace Wizard;

public class AIScriptArgumentToken : AIScriptTokenBase
{
	public AIScriptTokenArgType ArgumentType;

	public bool IsNot { get; private set; }

	public AIScriptArgumentToken(AIScriptTokenArgType arg, bool isNot)
		: base(AIScriptTokenType.ARG, (float)arg)
	{
		ArgumentType = arg;
		IsNot = isNot;
	}

	public override AIScriptTokenBase Clone()
	{
		return new AIScriptArgumentToken(ArgumentType, IsNot);
	}

	public override bool IsEqual(AIScriptTokenBase token)
	{
		if (!(token is AIScriptArgumentToken aIScriptArgumentToken))
		{
			return false;
		}
		if (ArgumentType == aIScriptArgumentToken.ArgumentType)
		{
			return IsNot == aIScriptArgumentToken.IsNot;
		}
		return false;
	}

	public bool IsSideArgType(bool includeBoth = true)
	{
		switch (ArgumentType)
		{
		case AIScriptTokenArgType.ALLY:
		case AIScriptTokenArgType.OPPONENT:
			return true;
		case AIScriptTokenArgType.BOTH:
			if (!includeBoth)
			{
				return false;
			}
			return true;
		default:
			return false;
		}
	}
}
