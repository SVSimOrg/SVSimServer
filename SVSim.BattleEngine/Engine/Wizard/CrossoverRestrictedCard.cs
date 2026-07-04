using System.Collections.Generic;
using LitJson;
// TODO(engine-cleanup-pass2): 1 of 2 methods unrun in baseline
//   Type: Wizard.CrossoverRestrictedCard
//   See data_dumps/reports/engine-cleanup/live-methods.baseline.txt


namespace Wizard;

public class CrossoverRestrictedCard
{
	public class Data
	{
		public readonly int BaseCardId;

		public readonly int Count;

		public Data(int baseCardId, int count)
		{
			BaseCardId = baseCardId;
			Count = count;
		}
	}

	private readonly List<Data> _mainClassDataList = new List<Data>();

	private readonly List<Data> _subClassDataList = new List<Data>();

	public int GetRestrictedCountOrDefault(int baseCardId, ClassType classType, int defaultCount)
	{
		List<Data> list = ((classType == ClassType.MainClass) ? _mainClassDataList : _subClassDataList);
		int num = list.FindIndex((Data data) => data.BaseCardId == baseCardId);
		if (num >= 0)
		{
			return list[num].Count;
		}
		return defaultCount;
	}
}
