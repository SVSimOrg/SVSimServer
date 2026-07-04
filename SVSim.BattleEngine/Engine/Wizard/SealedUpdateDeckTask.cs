using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class SealedUpdateDeckTask : BaseTask
{
	private enum eArrayIndex
	{
	}

	public class Param : BaseParam
	{
		public int[][] card_id_array;

		public Param(int[][] cardIdArray)
		{
			card_id_array = cardIdArray;
		}
	}

	public SealedUpdateDeckTask(List<int> cardList)
	{
		base.type = ApiType.Type.ArenaSealedUpdateDeck;
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		SealedData sealedData = Data.ArenaData.SealedData;
		for (int i = 0; i < cardList.Count; i++)
		{
			SealedCardInfo sealedCardInfo = sealedData.GetSealedCardInfo(cardList[i]);
			int originalCardId = sealedCardInfo.OriginalCardId;
			if (!sealedCardInfo.IsPhantom)
			{
				list.Add(originalCardId);
			}
			else
			{
				list2.Add(originalCardId);
			}
		}
		base.Params = new Param(new int[2][]
		{
			list.ToArray(),
			list2.ToArray()
		});
	}

	protected override int Parse()
	{
		int num = base.Parse();
		if (num != 1)
		{
			return num;
		}
		JsonData jsonData = base.ResponseData["data"];
		SealedData sealedData = Data.ArenaData.SealedData;
		sealedData.SetSealedCardInfo(jsonData, isRegisterSealedCard: false);
		sealedData.SetDeckCompleted(jsonData);
		return num;
	}
}
