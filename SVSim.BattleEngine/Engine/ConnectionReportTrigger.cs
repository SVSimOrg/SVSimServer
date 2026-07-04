using Wizard;

public class ConnectionReportTrigger
{
	public static void ConnectionReport(NetworkBattleManagerBase networkBattleManager)
	{
		LocalLog.AddGungnirLog("ConnectionReport");
		networkBattleManager.disconnectToDispChecker.StartChecker();
		networkBattleManager.disconnectToDispChecker.EraseDisp();
		networkBattleManager.disconnectToLoseChecker.StartChecker();
	}
}
