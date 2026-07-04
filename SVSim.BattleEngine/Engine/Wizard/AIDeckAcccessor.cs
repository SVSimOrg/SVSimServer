namespace Wizard;

public class AIDeckAcccessor
{
	private AIDeckData commonDic;

	private AIDeckData allyCommonDic;

	private AIDeckData curDic;

	public AIDeckAcccessor(AIDeckData _curDic, AIDeckData _commonDic, AIDeckData _allyCommonDic)
	{
		curDic = _curDic;
		commonDic = _commonDic;
		allyCommonDic = _allyCommonDic;
	}

	private AICardData SearchAllyCardData(int card_id)
	{
		AICardData aICardData = null;
		if (curDic != null)
		{
			aICardData = curDic.SearchCardData(card_id);
		}
		if (aICardData == null)
		{
			aICardData = allyCommonDic.SearchCardData(card_id);
		}
		return aICardData;
	}

	public AICardData SearchCardData(int cardId, bool isAlly)
	{
		AICardData aICardData = null;
		if (isAlly)
		{
			return SearchAllyCardData(cardId);
		}
		return commonDic.SearchCardData(cardId);
	}
}
