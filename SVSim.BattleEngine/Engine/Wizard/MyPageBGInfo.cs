using System.Collections.Generic;
using LitJson;

namespace Wizard;

public class MyPageBGInfo
{
	public MyPageDetail.BGType BGType { get; set; }

	public string Id { get; set; }

	public List<string> RandomIdList { get; set; }

	public MyPageBGInfo()
	{
	}

	public MyPageBGInfo(JsonData json)
	{
		BGType = (MyPageDetail.BGType)json["select_type"].ToInt();
		Id = json["mypage_id"].ToString();
		RandomIdList = new List<string>();
		if (json.TryGetValue("mypage_id_list", out var value))
		{
			for (int i = 0; i < value.Count; i++)
			{
				RandomIdList.Add(value[i].ToString());
			}
		}
	}
}
