using System.Collections.Generic;

namespace Wizard;

public class AILethalPlan
{
	public bool IsSuccess;

	public List<AISituationInfo> ActionSequence;

	public AILethalPlan()
	{
		IsSuccess = false;
		ActionSequence = null;
	}

	public void AddAction(AISituationInfo action)
	{
		ActionSequence = AIParamQuery.AddElementToList(action, ActionSequence);
	}

	public AILethalPlan Clone()
	{
		AILethalPlan aILethalPlan = new AILethalPlan();
		aILethalPlan.IsSuccess = IsSuccess;
		aILethalPlan.ActionSequence = AIParamQuery.AddRangeToList(ActionSequence, aILethalPlan.ActionSequence);
		return aILethalPlan;
	}
}
