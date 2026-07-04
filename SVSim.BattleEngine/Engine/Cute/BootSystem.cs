#define STEAM
using System;
using System.Collections;
using System.Diagnostics;
using RedShellUnity;
using UnityEngine;
using Wizard;

namespace Cute;

public class BootSystem : MonoBehaviour
{

	public static bool isRootBootCamera;

	private string _logMsg = "";

	private void Awake()
	{
		_logMsg += "Awake";
		if (isRootBootCamera)
		{
			UnityEngine.Object.Destroy(GameObject.Find("BootCamera"));
			isRootBootCamera = false;
		}
		VisibleBootCamera(enable: true);
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void VisibleBootCamera(bool enable)
	{
		GameObject gameObject = base.transform.Find("BootCamera").gameObject;
		if (gameObject != null)
		{
			gameObject.SetActive(enable);
		}
	}
}
