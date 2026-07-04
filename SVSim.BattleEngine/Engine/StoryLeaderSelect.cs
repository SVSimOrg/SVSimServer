using System.Collections.Generic;
using System.Linq;

public class StoryLeaderSelect : HeaderData
{
	private List<StoryLeaderSelectData> _dataList = new List<StoryLeaderSelectData>();

	public List<StoryLeaderSelectData> DataList => _dataList;

	public int LeaderCount { get; set; }

	public IEnumerable<int> LeaderCharaIds => _dataList.Select((StoryLeaderSelectData x) => x.CharaId);
}
