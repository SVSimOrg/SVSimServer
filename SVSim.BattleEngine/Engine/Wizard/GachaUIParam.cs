namespace Wizard;

public class GachaUIParam
{
	public bool EnableOverRideDefaultPackId { get; private set; }

	public int OverrideDefaultPackId { get; private set; }

	public void SetDefaultPackId(int packId)
	{
		EnableOverRideDefaultPackId = true;
		OverrideDefaultPackId = packId;
	}
}
