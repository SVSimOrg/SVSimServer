using System.Linq;

namespace Wizard;

public class CardPrm
{
	public int Id { get; private set; }

	public bool IsEvolve { get; private set; }

	public int ChantCount { get; private set; } = -1;

	public CardPrm(string cardPrmText)
	{
		if (cardPrmText.Contains('_'))
		{
			string[] array = cardPrmText.Split('_');
			Id = int.Parse(array[0]);
			IsEvolve = array[1] == "1";
		}
		else if (cardPrmText.Contains('|'))
		{
			string[] array2 = cardPrmText.Split('|');
			Id = int.Parse(array2[0]);
			ChantCount = int.Parse(array2[1]);
		}
		else
		{
			Id = int.Parse(cardPrmText);
		}
	}
}
