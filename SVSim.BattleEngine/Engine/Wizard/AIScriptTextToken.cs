namespace Wizard;

public class AIScriptTextToken : AIScriptArgumentToken
{
	public string Text { get; private set; }

	public AIScriptTextToken(string text, bool isNot)
		: base(AIScriptTokenArgType.AI_TRIBE, isNot)
	{
		Text = text;
	}

	public override AIScriptTokenBase Clone()
	{
		return new AIScriptTextToken(Text, base.IsNot);
	}

	public override bool IsEqual(AIScriptTokenBase token)
	{
		if (!(token is AIScriptTextToken aIScriptTextToken))
		{
			return false;
		}
		if (ArgumentType == aIScriptTextToken.ArgumentType && base.IsNot == aIScriptTextToken.IsNot)
		{
			return Text == aIScriptTextToken.Text;
		}
		return false;
	}
}
