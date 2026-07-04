using System.Collections.Generic;
using LitJson;

namespace Wizard.Scripts.Network.Data.TableData.Arena.TwoPick;

public class CandidateChaos
{
	public class ChaosCharaData
	{
		public int ChaosId;

		public int ClassId;

		public int CharaId;

		public int[] SourceClassIds;
	}

	public ChaosCharaData[] CharaData;

	public int SelectedChaosId;

	public int SelectedCharaId;

	public int SelectedClassId;

	public CandidateChaos()
	{
	}

	public CandidateChaos(JsonData chaosInfoData, JsonData classInfoData)
	{
		CharaData = new ChaosCharaData[3];
		for (int i = 0; i < 3; i++)
		{
			CharaData[i] = GetChaosCharaData(chaosInfoData, classInfoData[$"chaos_id_{i + 1}"].ToString());
		}
	}

	public CandidateChaos(JsonData chaosInfoData, List<string> idList)
	{
		CharaData = new ChaosCharaData[3];
		for (int i = 0; i < 3; i++)
		{
			CharaData[i] = GetChaosCharaData(chaosInfoData, idList[i]);
		}
	}

	public CandidateChaos(JsonData data)
	{
		SelectedChaosId = data["selected_chaos_id"].ToInt();
		SelectedCharaId = data["selected_leader_skin_id"].ToInt();
		SelectedClassId = data["selected_class_id"].ToInt();
		CharaData = new ChaosCharaData[3];
		JsonData chaosInfoData = data["chaos_info"];
		for (int i = 0; i < 3; i++)
		{
			CharaData[i] = GetChaosCharaData(chaosInfoData, data[$"chaos_id_{i + 1}"].ToString());
		}
	}

	public static ChaosCharaData GetChaosCharaData(JsonData chaosInfoData, string chaosIdString)
	{
		if (!chaosInfoData.TryGetValue(chaosIdString, out var value))
		{
			return null;
		}
		ChaosCharaData chaosCharaData = new ChaosCharaData
		{
			ChaosId = value["chaos_id"].ToInt(),
			ClassId = value["main_class_id"].ToInt(),
			CharaId = value["leader_skin_id"].ToInt()
		};
		JsonData jsonData = value["usable_class_ids"];
		int[] array = new int[jsonData.Count];
		for (int i = 0; i < jsonData.Count; i++)
		{
			array[i] = jsonData[i].ToInt();
		}
		chaosCharaData.SourceClassIds = array;
		return chaosCharaData;
	}
}
