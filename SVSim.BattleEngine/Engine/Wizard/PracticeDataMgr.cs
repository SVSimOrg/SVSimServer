using System.Collections.Generic;
using System.Linq;
using LitJson;

namespace Wizard;

public class PracticeDataMgr
{
	private List<PracticeData> _dataList = new List<PracticeData>();

	public List<int> CampaignClassIdList { get; private set; } = new List<int>();

	public PracticeDataMgr(JsonData data)
	{
		if (data == null)
		{
			return;
		}
		for (int i = 0; i < data.Count; i++)
		{
			_dataList.Add(new PracticeData(data[i]));
		}
		foreach (PracticeData data2 in _dataList)
		{
			if (data2.IsCampaign && !CampaignClassIdList.Contains(data2.ClassId))
			{
				CampaignClassIdList.Add(data2.ClassId);
			}
		}
	}

	public List<PracticeData> GetClassDataList(int classId)
	{
		return _dataList.Where((PracticeData data) => data.ClassId == classId).ToList();
	}
}
