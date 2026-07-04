using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cute;

public static class SoftwareResetBase
{
	public static bool isSoftwareReset;

	private static Action resetAction;

	public static void setSoftwareResetAction(Action _action)
	{
		resetAction = _action;
	}

	public static void SoftwareReset(string sceneName, Action _resetAction)
	{
		isSoftwareReset = true;
		if (_resetAction != null)
		{
			resetAction = _resetAction;
		}
		resetAction.Call();
		GameObject gameObject = GameObject.Find("OmotePlugin");
		if (gameObject != null)
		{
			UnityEngine.Object.Destroy(gameObject);
		}
		Toolbox.BootSystem.VisibleBootCamera(enable: true);
		GameObject gameObject2 = GameObject.Find("BootCamera");
		if (gameObject2 != null)
		{
			gameObject2.transform.parent = null;
			UnityEngine.Object.DontDestroyOnLoad(gameObject2);
			BootSystem.isRootBootCamera = true;
		}
		Screen.sleepTimeout = -2;
		BootApp.BootScene = sceneName;
		UnityEngine.SceneManagement.SceneManager.LoadScene("_SoftwareReset");
	}
}
