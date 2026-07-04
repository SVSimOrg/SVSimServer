using LitJson;

namespace Wizard;

public class SealedCardInfo
{
	public int OriginalCardId { get; private set; }

	public int SealedCardId { get; private set; }

	public bool IsPhantom { get; private set; }

	public int OwnNum { get; private set; }

	public int DeckUsingNum { get; private set; }

	public SealedCardInfo(JsonData rootData)
		: this(rootData["card_id"].ToInt(), rootData["is_phantom"].ToInt() == 1, rootData["num"].ToInt(), rootData["deck_using_num"].ToInt())
	{
	}

	public SealedCardInfo(int originalCardId, bool isPhantom, int ownNum, int deckUsingNum)
	{
		OriginalCardId = originalCardId;
		IsPhantom = isPhantom;
		OwnNum = ownNum;
		DeckUsingNum = deckUsingNum;
		SealedCardId = SealedData.ConvertToSealedCardId(originalCardId, isPhantom);
	}
}
