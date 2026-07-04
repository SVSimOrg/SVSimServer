using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

namespace Cute;

public class SavedataManager : MonoBehaviour, IManager
{

	public int GetInt(string key, int defaultValue = 0)
	{
		return ObscuredPrefs.GetInt(key, defaultValue);
	}

	public void SetInt(string key, int value)
	{
		ObscuredPrefs.SetInt(key, value);
	}

	public string GetString(string key, string defaultValue = "")
	{
		return ObscuredPrefs.GetString(key, defaultValue);
	}

	public void SetString(string key, string value)
	{
		ObscuredPrefs.SetString(key, value);
	}

	public void SetResourceVersion(string version)
	{
		SetString("RES_VER", version);
	}

	public string GetResourceVersion()
	{
		return GetString("RES_VER", "00000000");
	}
}
