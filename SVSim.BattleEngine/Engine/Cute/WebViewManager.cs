using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Wizard;

namespace Cute;

public class WebViewManager : MonoBehaviour
{
	private static WebViewManager instance;

	[SerializeField]
	public WebViewScreen m_WebViewScreen;

	private Action<string> onError;

	private Action<string> onLoaded;

	private Vector4 marginNow;

	private float screenDpi;

	private bool screenDpiChangedWhileInvisible;

	public Action<string> Callback { get; set; }

	public static bool HasInstance => instance != null;

	public Action<string> OnError
	{
		set
		{
			onError = value;
		}
	}

	public Action<string> OnLoaded
	{
		set
		{
			onLoaded = value;
		}
	}

	public bool Visible { get; private set; }

	public Action OnDpiChangedAction { private get; set; }

	public static WebViewManager getInstance()
	{
		return instance;
	}

	private void UpdateScreenDpi()
	{
		screenDpi = Screen.dpi;
	}

	private void Awake()
	{
		instance = this;
		SetMargins(Screen.width / 30, Screen.height / 5, Screen.width / 30, Screen.height / 14);
		UpdateScreenDpi();
	}

	private void OnDpiChange()
	{
		if (OnDpiChangedAction != null)
		{
			OnDpiChangedAction();
		}
		Vector4 marginBackCoroutine = marginNow;
		SetMargins((int)marginBackCoroutine.x, (int)marginBackCoroutine.y, (int)marginBackCoroutine.z - 1, (int)marginBackCoroutine.w - 1);
		StartCoroutine(SetMarginBackCoroutine(marginBackCoroutine));
	}

	private IEnumerator SetMarginBackCoroutine(Vector4 oldMargin)
	{
		yield return new WaitForSecondsRealtime(0.5f);
		SetMargins((int)oldMargin.x, (int)oldMargin.y, (int)oldMargin.z, (int)oldMargin.w);
	}

	public void LoadWeb(string url)
	{
	}

	public void SetMargins(int leftMargin, int topMargin, int rightMargin, int bottomMargin)
	{
		marginNow = new Vector4(leftMargin, topMargin, rightMargin, bottomMargin);
	}

	public void UnloadWebView()
	{
		m_WebViewScreen.CurBoxCollider.enabled = false;
		SetVisible(visible: false);
		LoadWeb(CustomPreference.GetApplicationServerURL() + "information/blank");
		Callback = null;
		OnLoaded = null;
		OnError = null;
	}

	public void SetVisible(bool visible)
	{
		Visible = visible;
		if (visible && screenDpiChangedWhileInvisible)
		{
			screenDpiChangedWhileInvisible = false;
			OnDpiChange();
		}
	}

	public void SetCallback(Action<string> cb)
	{
		Callback = cb;
	}
}
