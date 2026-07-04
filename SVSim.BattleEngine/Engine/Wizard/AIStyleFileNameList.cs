using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AIStyleFileNameList
{

	private readonly Dictionary<int, string> _dataTable = new Dictionary<int, string>();

	public AIStyleFileNameList(List<string[]> csvData)
	{
		foreach (string[] csvDatum in csvData)
		{
			int key = int.Parse(csvDatum[0]);
			string value = csvDatum[1];
			_dataTable.Add(key, value);
		}
	}

	public string GetFileName(int id)
	{
		if (_dataTable.ContainsKey(id))
		{
			return _dataTable[id];
		}
		return "";
	}
}
