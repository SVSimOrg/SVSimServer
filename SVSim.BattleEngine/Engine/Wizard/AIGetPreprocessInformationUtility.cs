namespace Wizard;

public static class AIGetPreprocessInformationUtility
{
	public static int GetEarthRiteCount(AIVirtualCard owner, AISituationInfo situation)
	{
		if (situation == null)
		{
			return 0;
		}
		if (situation.PreprocessRecorder == null || !situation.PreprocessRecorder.HasRecord)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < situation.PreprocessRecorder.RecordList.Count; i++)
		{
			AISinglePreprocessRecord aISinglePreprocessRecord = situation.PreprocessRecorder.RecordList[i];
			if (aISinglePreprocessRecord.RealActor.IsSameCard(owner) || aISinglePreprocessRecord.OriginalCard.IsSameCard(owner))
			{
				num += aISinglePreprocessRecord.EarthRiteCount;
			}
		}
		return num;
	}

	public static int GetNecromanceCount(AIVirtualCard owner, AISituationInfo situation)
	{
		if (situation == null)
		{
			return 0;
		}
		if (situation.PreprocessRecorder == null || !situation.PreprocessRecorder.HasRecord)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < situation.PreprocessRecorder.RecordList.Count; i++)
		{
			AISinglePreprocessRecord aISinglePreprocessRecord = situation.PreprocessRecorder.RecordList[i];
			if (aISinglePreprocessRecord.RealActor.IsSameCard(owner) || aISinglePreprocessRecord.OriginalCard.IsSameCard(owner))
			{
				num += aISinglePreprocessRecord.NecromanceCount;
			}
		}
		return num;
	}
}
