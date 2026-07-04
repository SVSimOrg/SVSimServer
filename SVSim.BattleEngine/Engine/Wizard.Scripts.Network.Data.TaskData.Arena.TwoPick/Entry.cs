using LitJson;

namespace Wizard.Scripts.Network.Data.TaskData.Arena.TwoPick;

public class Entry : HeaderData
{
	public int[] candidateClassIds;

	public Entry()
	{
	}

	public Entry(JsonData data)
	{
		JsonData jsonData = null;
		jsonData = ((!data.Keys.Contains("candidate_class_ids")) ? data["candidate_chaos_ids"] : data["candidate_class_ids"]);
		candidateClassIds = new int[jsonData.Count];
		for (int i = 0; i < jsonData.Count; i++)
		{
			int num = jsonData[i].ToInt();
			candidateClassIds[i] = num;
		}
	}
}
