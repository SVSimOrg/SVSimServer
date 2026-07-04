namespace Convention;

public class Offline
{

	public static bool IsConventionMode { get; set; }

	public static void OnSoftwareReset()
	{
		IsConventionMode = false;
	}
}
