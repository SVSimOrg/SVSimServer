public class NetworkReplayBattleReceiver : NetworkWatchBattleReceiver
{
	public NetworkReplayBattleReceiver(NetworkBattleManagerBase battlemgr)
		: base(battlemgr)
	{
	}

	protected override int ConvertToInt(object intStr)
	{
		return int.Parse(intStr.ToString());
	}
}
