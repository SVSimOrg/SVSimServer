using LitJson;

namespace Wizard.Scripts.Network.Data.TableData.Arena.TwoPick;

public class CandidateCard
{
	public int id;

	public int turn;

	public int setNum;

	public int cardId1;

	public int cardId2;

	public bool isSelected;

	public CandidateCard()
	{
	}

	public CandidateCard(JsonData data)
	{
		id = data["id"].ToInt();
		turn = data["turn"].ToInt();
		setNum = data["set_num"].ToInt();
		cardId1 = data["card_id_1"].ToInt();
		cardId2 = data["card_id_2"].ToInt();
		isSelected = data["is_selected"].ToInt() == 1;
	}
}
