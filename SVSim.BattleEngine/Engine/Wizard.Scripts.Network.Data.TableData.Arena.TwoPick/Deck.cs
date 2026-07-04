using System.Collections.Generic;
using LitJson;

namespace Wizard.Scripts.Network.Data.TableData.Arena.TwoPick;

public class Deck
{
	public int entryId;

	public int classId;

	public int skinId;

	public int[] cardIds;

	public bool isSelectCompleted;

	public int selectTurn;

	public Deck()
	{
	}

	public Deck(JsonData data)
	{
		if (data != null)
		{
			ICollection<string> keys = data.Keys;
			if (keys.Contains("two_pick_entry_id"))
			{
				entryId = data["two_pick_entry_id"].ToInt();
			}
			if (keys.Contains("class_id"))
			{
				classId = data["class_id"].ToInt();
			}
			isSelectCompleted = data["is_select_completed"].ToInt() == 1;
			JsonData jsonData = data["selected_card_ids"];
			cardIds = new int[jsonData.Count];
			for (int i = 0; i < cardIds.Length; i++)
			{
				cardIds[i] = jsonData[i].ToInt();
			}
			selectTurn = data["select_turn"].ToInt();
		}
	}
}
