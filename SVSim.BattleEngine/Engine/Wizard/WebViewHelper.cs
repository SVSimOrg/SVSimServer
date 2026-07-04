using System;
using Cute;
using UnityEngine;

namespace Wizard;

public class WebViewHelper
{
	public enum WebViewType
	{
		NONE,
		INFO,
		HELP,
		COPYRIGHT,
		TOS,
		PRIVACY_POLICY,
		KOR_AUTHORITY,
		GACHARATE,
		LEGALTEXT,
		FUND_SETTLEMENT,
		TELECOMMUNICATION_POLICY,
		KOREA_CRYSTAL_PAGE,
		ANNOUNCE,
		SEALED_GACHARATE,
		LIMITED_INFO
	}

	public delegate void ActionWebviewOpen(DialogBase webviewDialog);

	public enum UrlType
	{
	}

	private bool _isDestroyedWebView;

	private DialogBase _webViewDialog;

	public DialogBase WebViewDialog => _webViewDialog;

	public void OpenAnnounceWebView(string announceId, Action<DialogBase> onDialogOpening = null)
	{
		_OpenWebView(WebViewType.ANNOUNCE, null, null, 0, 0, announceId, "", null, null, onDialogOpening);
	}

	private void _OpenWebView(WebViewType webviewtype, ActionWebviewOpen cbWebviewOpen = null, Action cbWebviewClose = null, int ratePackId = 0, int packCategory = 0, string announceId = null, string directOpenUrl = "", Action onButton1 = null, Action onCancel = null, Action<DialogBase> onDialogOpening = null)
	{
		VideoHostingUtil.CheckVideoHostingAndExecAction(delegate
		{
			if (webviewtype == WebViewType.ANNOUNCE)
			{
				OpenBrowserInsteadOfWebView(webviewtype, onDialogOpening, announceId);
			}
			else if (webviewtype == WebViewType.SEALED_GACHARATE)
			{
				OpenBrowserInsteadOfWebView(webviewtype, onDialogOpening, Data.ArenaData.SealedMyPageResponseData.ScheduleId.ToString());
			}
			else
			{
				string text = ratePackId.ToString();
				string text2 = packCategory.ToString();
				OpenBrowserInsteadOfWebView(webviewtype, onDialogOpening, text, text2);
			}
		});
	}

	private string GetWebViewDialogTitleId(WebViewType webviewtype)
	{
		string result = null;
		switch (webviewtype)
		{
		case WebViewType.INFO:
		case WebViewType.ANNOUNCE:
			result = "Common_0036";
			break;
		case WebViewType.LIMITED_INFO:
			result = "Mission_0048";
			break;
		case WebViewType.HELP:
			result = "OtherTop_0006";
			break;
		case WebViewType.TOS:
			result = ((PlayerStaticData._tosAgreementState == PlayerStaticData.AgreementState.Reset) ? "OtherTop_0010_Update" : "OtherTop_0010");
			break;
		case WebViewType.PRIVACY_POLICY:
			result = ((PlayerStaticData._privacyPolicyAgreementState == PlayerStaticData.AgreementState.Reset) ? "OtherTop_0009_Update" : "OtherTop_0009");
			break;
		case WebViewType.TELECOMMUNICATION_POLICY:
			result = "OtherTop_0069";
			break;
		case WebViewType.KOR_AUTHORITY:
			result = ((PlayerStaticData.KorAuthorityAgreementState == PlayerStaticData.AgreementState.Reset) ? "OtherTop_0067_Update" : "OtherTop_0067");
			break;
		case WebViewType.COPYRIGHT:
			result = "OtherTop_0022";
			break;
		case WebViewType.GACHARATE:
			result = "Shop_0004";
			break;
		case WebViewType.LEGALTEXT:
			result = "Shop_0005";
			break;
		case WebViewType.FUND_SETTLEMENT:
			result = "Shop_0077";
			break;
		case WebViewType.KOREA_CRYSTAL_PAGE:
			result = "Shop_0126";
			break;
		case WebViewType.SEALED_GACHARATE:
			result = "Sealed_GachaRateWebViewTitle";
			break;
		}
		return result;
	}

	private string GetBrowserUrl(WebViewType webviewtype)
	{
		string text = null;
		switch (webviewtype)
		{
		case WebViewType.INFO:
		case WebViewType.LIMITED_INFO:
			text = "URL_0004";
			break;
		case WebViewType.HELP:
			text = "URL_0011";
			break;
		case WebViewType.TOS:
			text = "URL_0005";
			break;
		case WebViewType.PRIVACY_POLICY:
			text = "URL_0007";
			break;
		case WebViewType.TELECOMMUNICATION_POLICY:
			text = "URL_0018";
			break;
		case WebViewType.COPYRIGHT:
			text = "URL_0006";
			break;
		case WebViewType.GACHARATE:
			text = "URL_0010";
			break;
		case WebViewType.LEGALTEXT:
			text = "URL_0009";
			break;
		case WebViewType.FUND_SETTLEMENT:
			text = "URL_0008";
			break;
		case WebViewType.KOREA_CRYSTAL_PAGE:
			text = "URL_0015";
			break;
		case WebViewType.ANNOUNCE:
			text = "URL_0016";
			break;
		case WebViewType.SEALED_GACHARATE:
			text = "URL_0017";
			break;
		}
		return text + "_steam";
	}

	private void OpenBrowserInsteadOfWebView(WebViewType webviewtype, Action<DialogBase> onDialogOpening, params string[] param)
	{
		string webViewDialogTitleId = GetWebViewDialogTitleId(webviewtype);
		string browserUrl = GetBrowserUrl(webviewtype);
		SystemText systemText = Data.SystemText;
		UIManager.ShowDialogUrl(systemText.Get(webViewDialogTitleId), systemText.Get(browserUrl, param), onDialogOpening);
	}

	public void DestroyWebView()
	{
		if (!_isDestroyedWebView && WebViewManager.HasInstance)
		{
			_isDestroyedWebView = true;
			WebViewManager instance = WebViewManager.getInstance();
			instance.UnloadWebView();
			instance.SetCallback(null);
		}
	}

	public void CloseWebViewDialog()
	{
		if (IsOpenWebViewDialog())
		{
			_webViewDialog.Close();
			_webViewDialog = null;
			DestroyWebView();
		}
	}

	public bool IsOpenWebViewDialog()
	{
		if (_webViewDialog != null)
		{
			return _webViewDialog.IsOpen();
		}
		return false;
	}
}
