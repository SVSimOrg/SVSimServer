using System.Collections.Generic;
using LitJson;
using Wizard.Scripts.Network.Data.TableData;

namespace Wizard.Scripts.Network.Data.TaskData;

public class ItemAcquireHistoryInfo
{
	public List<ItemAcquireHistory> Histories { get; private set; }

	public ItemAcquireHistoryInfo()
	{
	}

	public ItemAcquireHistoryInfo(JsonData data)
	{
		Histories = new List<ItemAcquireHistory>();
		for (int i = 0; i < data.Count; i++)
		{
			ItemAcquireHistory item = new ItemAcquireHistory(data[i]);
			Histories.Add(item);
		}
	}
}
