using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AIEmoteFileNameList
{

	private readonly Dictionary<int, string> _dataTable = new Dictionary<int, string>();

	public AIEmoteFileNameList(List<string[]> csvData)
	{
		foreach (string[] csvDatum in csvData)
		{
			int key = int.Parse(csvDatum[0]);
			string value = csvDatum[1];
			_dataTable.Add(key, value);
		}
	}

	public string GetFileName(int emoteId)
	{
		if (_dataTable.ContainsKey(emoteId))
		{
			return _dataTable[emoteId];
		}
		return "";
	}
}
