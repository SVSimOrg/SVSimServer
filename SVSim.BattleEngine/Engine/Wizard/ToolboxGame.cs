using UnityEngine;

namespace Wizard;

public static class ToolboxGame
{
	public static UIManager UIManager = null;

	public static void SetRealTimeNetworkBattle(RealTimeNetworkAgent agent)
	{
		var mgr = BattleManagerBase.GetIns()
			?? throw new System.InvalidOperationException("SetRealTimeNetworkBattle called with no attached mgr.");
		mgr.InstanceNetworkAgent = agent;
	}
}
