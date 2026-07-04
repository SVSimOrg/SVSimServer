using System.Collections.Generic;

namespace Wizard;

public class AISimulationPreprocessRecorder
{
	public List<AISinglePreprocessRecord> RecordList;

	public int TotalBurialCount { get; private set; }

	public bool HasRecord
	{
		get
		{
			if (RecordList != null)
			{
				return RecordList.Count > 0;
			}
			return false;
		}
	}

	public AISimulationPreprocessRecorder()
	{
		TotalBurialCount = 0;
	}

	public void AddRecord(AISinglePreprocessRecord record)
	{
		RecordList = AIParamQuery.AddElementToList(record, RecordList);
		if (record.BurialRiteCount > 0)
		{
			TotalBurialCount += record.BurialRiteCount;
		}
	}

	public void RemoveAll()
	{
		if (RecordList != null)
		{
			RecordList.Clear();
			RecordList = null;
		}
	}

	public void ClearBurialCount()
	{
		TotalBurialCount = 0;
	}
}
