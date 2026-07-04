namespace Wizard;

public static class ChaosUtil
{
	public static string GetDeckName(int chaosId, int chaosNum)
	{
		return Data.SystemText.Get($"Chaos_DeckName_{chaosId:0000}_{chaosNum}");
	}
}
