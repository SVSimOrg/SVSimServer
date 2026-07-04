using System.Collections.Generic;

namespace Wizard;

public class AISkillProcessInfoCollection
{
	private Queue<AISkillProcessInformation> _processQueue;

	private List<AISkillProcessInformation> _tempPreprocessList;

	public AISkillProcessInfoCollection()
	{
		_processQueue = new Queue<AISkillProcessInformation>();
	}

	public void RegisterProcess(AISkillProcessInformation process)
	{
		_processQueue.Enqueue(process);
	}

	public void RegisterProcess(List<AISkillProcessInformation> processList)
	{
		if (processList == null)
		{
			return;
		}
		for (int i = 0; i < processList.Count; i++)
		{
			if (processList[i] != null)
			{
				_processQueue.Enqueue(processList[i]);
			}
		}
	}

	public void RegisterPreprocessProcessInfo(AISkillProcessInformation process)
	{
		_tempPreprocessList = AIParamQuery.AddElementToList(process, _tempPreprocessList, isBlockDuplicate: true);
	}

	public void CombinePreprocessToProcessQueue()
	{
		if (_tempPreprocessList != null && _tempPreprocessList.Count > 0)
		{
			RegisterProcess(_tempPreprocessList);
			_tempPreprocessList.Clear();
		}
	}

	public void ClearTempPreprocessList()
	{
		if (_tempPreprocessList != null)
		{
			_tempPreprocessList.Clear();
		}
	}

	public void ExecuteAllProcess(AISituationInfo situation)
	{
		while (_processQueue.Count > 0)
		{
			_processQueue.Dequeue().ExecuteAllAction(situation);
		}
	}
}
