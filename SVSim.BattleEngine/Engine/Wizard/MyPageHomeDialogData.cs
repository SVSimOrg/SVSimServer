using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class MyPageHomeDialogData
{
	public class TransitionData
	{
		public string TransitionTarget { get; private set; }

		public string Status { get; private set; }

		public string ButtonText { get; private set; }

		public TransitionData(JsonData data)
		{
			TransitionTarget = data["scene"].ToString();
			ButtonText = Data.SystemText.Get(data["button_text_id"].ToString());
			Status = data["status"].ToString();
		}
	}

	public bool IsEnable { get; private set; }

	public string DialogTitle { get; private set; }

	public string FilePath { get; private set; }

	public List<TransitionData> TransitionList { get; private set; }

	public MyPageHomeDialogData()
	{
	}

	public MyPageHomeDialogData(JsonData data, string key)
	{
		Parse(data, key);
	}

	private void Parse(JsonData data, string key)
	{
		IsEnable = false;
		if (data.TryGetValue(key, out var value) && value.Count != 0)
		{
			IsEnable = true;
			JsonData jsonData = value[0];
			DialogTitle = Data.SystemText.Get(jsonData["title_text_id"].ToString());
			FilePath = jsonData["image"].ToString();
			TransitionList = new List<TransitionData>();
			JsonData jsonData2 = jsonData["button_list"];
			for (int i = 0; i < jsonData2.Count; i++)
			{
				TransitionList.Add(new TransitionData(jsonData2[i]));
			}
		}
	}
}
