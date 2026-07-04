namespace Wizard;

public class AIScriptIDToken : AIScriptTokenBase
{
	public int ID;

	public bool IsNot { get; private set; }

	public AIScriptIDToken(int id, bool isNot)
		: base(AIScriptTokenType.ID, 0f)
	{
		ID = id;
		IsNot = isNot;
	}

	public override AIScriptTokenBase Clone()
	{
		return new AIScriptIDToken(ID, IsNot);
	}

	public override bool IsEqual(AIScriptTokenBase token)
	{
		if (!(token is AIScriptIDToken aIScriptIDToken))
		{
			return false;
		}
		if (ID == aIScriptIDToken.ID)
		{
			return IsNot == aIScriptIDToken.IsNot;
		}
		return false;
	}
}
