namespace Wizard;

public class AITokenInformation
{
	public int TokenId;

	public AITokenType TokenType;

	public bool IsChoice => TokenType == AITokenType.Choice;

	public AITokenInformation(int tokenId, AITokenType tokenType)
	{
		TokenId = tokenId;
		TokenType = tokenType;
	}
}
