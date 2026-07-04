using System;

namespace Wizard;

public class ArenaCommonLobbyInitParam
{
	public int ClassId { get; set; }

	public int BattleMaxNum { get; set; }

	public int BattleWinNum { get; set; }

	public bool[] BattleResultList { get; set; }

	public Action BattleButtonClickCallback { get; set; }

	public Action RewardReceiveButtonClickCallback { get; set; }

	public NetworkDefine.MAINTENANCE_TYPE? BattleMaintenanceType { get; set; }
}
