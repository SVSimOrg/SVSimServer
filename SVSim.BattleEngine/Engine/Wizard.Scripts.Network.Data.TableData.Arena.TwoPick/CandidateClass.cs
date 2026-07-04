using LitJson;

namespace Wizard.Scripts.Network.Data.TableData.Arena.TwoPick;

public class CandidateClass
{
	public int classId1;

	public int classId2;

	public int classId3;

	public int classId4;

	public int selectedClassId;

	public int charaId1;

	public int charaId2;

	public int SelectedChaosId { get; private set; }

	public CandidateClass()
	{
	}

	public CandidateClass(JsonData data)
	{
		if (data.Keys.Contains("class_id_1"))
		{
			classId1 = data["class_id_1"].ToInt();
			classId2 = data["class_id_2"].ToInt();
			classId3 = data["class_id_3"].ToInt();
			if (data.Keys.Contains("class_id_4"))
			{
				classId4 = data["class_id_4"].ToInt();
			}
		}
		else
		{
			classId1 = data["chaos_id_1"].ToInt();
			classId2 = data["chaos_id_2"].ToInt();
			classId3 = data["chaos_id_3"].ToInt();
			if (data.Keys.Contains("chaos_id_4"))
			{
				classId4 = data["chaos_id_4"].ToInt();
			}
		}
		selectedClassId = data["selected_class_id"].ToInt();
		SelectedChaosId = data.GetValueOrDefault("selected_chaos_id", 0);
	}
}
