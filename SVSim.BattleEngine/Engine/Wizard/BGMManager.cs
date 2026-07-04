namespace Wizard;

public class BGMManager
{

	private static BGMManager _instance;

	public static void Dispose()
	{
		if (_instance != null)
		{
			_instance = null;
		}
	}

	private BGMManager()
	{
	}
}
