using System.Collections.Generic;

namespace Wizard;

public class SimulationResult
{
	public AIVirtualActionInfo FirstActionInfo;

	public AIVirtualField BestResultField;

	public List<AIVirtualActionInfo> ActionSequence;

	public int MinActionLengthOfLethal;

	public float MaxFieldValue { get; set; }

	public float MaxFieldValueAtSelfTurnEnd { get; set; }

	public float MaxFieldValueFirstSkill { get; set; }

	public AISelectedTargetInfoSet SelectedTargets { get; set; }

	public bool IsLethalPlan { get; set; }

	public bool IsRecentlyUpdated { get; set; }

	public SimulationResult()
	{
		MaxFieldValue = float.MinValue;
		MaxFieldValueAtSelfTurnEnd = float.MinValue;
		FirstActionInfo = null;
		BestResultField = null;
		ActionSequence = null;
		MaxFieldValueFirstSkill = float.MinValue;
		SelectedTargets = null;
		IsLethalPlan = false;
		MinActionLengthOfLethal = int.MaxValue;
		IsRecentlyUpdated = false;
	}
}
