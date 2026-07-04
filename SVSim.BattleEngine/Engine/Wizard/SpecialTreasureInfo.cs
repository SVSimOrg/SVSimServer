using LitJson;

namespace Wizard;

public class SpecialTreasureInfo
{
	public int SpecialTreasureBoxNecessaryNumbers { get; private set; }

	public int ReceivedBoxNumbers { get; private set; }

	public bool IsGotSpecialTreasureBox { get; private set; }

	public SpecialTreasureInfo(JsonData json)
	{
		SpecialTreasureBoxNecessaryNumbers = json["special_treasure_box_necessary_numbers"].ToInt();
		ReceivedBoxNumbers = json["received_box_numbers"].ToInt();
		IsGotSpecialTreasureBox = json["is_got_special_treasure_box"].ToInt() == 1;
	}
}
