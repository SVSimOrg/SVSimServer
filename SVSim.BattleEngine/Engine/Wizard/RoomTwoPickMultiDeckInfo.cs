using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class RoomTwoPickMultiDeckInfo
{
	public class DeckResultInfo
	{
		public List<int> _cardIds;

		public int _classId;

		public int _skinId;

		public int _opponentClassId;

		public bool _isWin;

		public DeckResultInfo(JsonData data)
		{
			JsonData jsonData = data["card_id_list"];
			int count = jsonData.Count;
			List<int> list = new List<int>(count);
			for (int i = 0; i < count; i++)
			{
				list.Add(ConvertValue.ToInt(jsonData[i]));
			}
			_cardIds = UIManager.GetInstance().getUIBase_CardManager().SortIDList(list, CardMaster.CardMasterId.Default);
			_classId = ConvertValue.ToInt(data["class_id"]);
			_skinId = ConvertValue.ToInt(data["chara_id"]);
			_opponentClassId = ConvertValue.ToInt(data["opponent_class_id"]);
			_isWin = ConvertValue.ToInt(data["battle_result"]) == 1;
		}

		public DeckResultInfo(Dictionary<string, object> data)
		{
			List<object> list = data["cardIds"] as List<object>;
			int count = list.Count;
			List<int> list2 = new List<int>(count);
			for (int i = 0; i < count; i++)
			{
				list2.Add(ConvertValue.ToInt(list[i]));
			}
			_cardIds = UIManager.GetInstance().getUIBase_CardManager().SortIDList(list2, CardMaster.CardMasterId.Default);
			_classId = ConvertValue.ToInt(data["classId"]);
			_skinId = ConvertValue.ToInt(data["skinId"]);
			_opponentClassId = ConvertValue.ToInt(data["oppoClass"]);
			_isWin = ConvertValue.ToInt(data["isWin"]) == 1;
		}
	}
}
