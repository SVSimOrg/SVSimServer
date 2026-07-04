using System.Collections.Generic;

namespace Wizard;

public class PlayerPrefsCache
{

	private Dictionary<string, int> _intCache = new Dictionary<string, int>();

	private static PlayerPrefsCache _instance;

	public static PlayerPrefsCache Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new PlayerPrefsCache();
			}
			return _instance;
		}
	}

	public static void OnSoftwareReset()
	{
		_instance = null;
	}

	public int GetValue(KeyValuePair<string, int> id)
	{
		if (_intCache.TryGetValue(id.Key, out var value))
		{
			return value;
		}
		value = PlayerPrefsWrapper.GetValue(id);
		_intCache.Add(id.Key, value);
		return value;
	}
}
