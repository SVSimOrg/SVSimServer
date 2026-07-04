using System;
using System.Collections.Generic;
using Cute;

namespace Wizard;

public class AISkillProcessInformation
{
	private List<Action> _executingAction;

	public AISituationTriggerInformation TriggerInfo { get; private set; }

	public AIOwnSkillProcessRecord OwnProcessRecord { get; private set; }

	public AISkillProcessInformation(AISituationTriggerInformation triggerInfo)
	{
		TriggerInfo = triggerInfo;
		OwnProcessRecord = new AIOwnSkillProcessRecord();
	}

	public void AddExecutingAction(Action action)
	{
		_executingAction = AIParamQuery.AddElementToList(action, _executingAction);
	}

	public void ExecuteAllAction(AISituationInfo situation)
	{
		if (_executingAction != null && _executingAction.Count > 0)
		{
			situation.SetExecutingSkillProcess(this);
			for (int i = 0; i < _executingAction.Count; i++)
			{
				_executingAction[i].Call();
			}
		}
	}
}
