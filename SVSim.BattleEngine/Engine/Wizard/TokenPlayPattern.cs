using System.Collections.Generic;

namespace Wizard;

public class TokenPlayPattern
{
	public int IndexOfHandCardList { get; private set; }

	public float tokenValue { get; private set; }

	public List<AITokenInformation> TokenInfoPairList { get; private set; }

	public TokenPlayPattern(int index, float value, List<AITokenInformation> tokenIdList)
	{
		IndexOfHandCardList = index;
		tokenValue = value;
		TokenInfoPairList = tokenIdList;
	}
}
