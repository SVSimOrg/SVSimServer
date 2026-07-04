using System;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.ErrorDialog;

public class NtDataTranslateManager
{
	private static NtDataTranslateManager instance;

	public NtDataTranslateInfo TranslateInfo;

	public string curBindEmail = "";

	public const string FIRST_LOOK = "FIRST_LOOK";

	private NtDataTranslateManager()
	{
		TranslateInfo = NtDataTranslateInfo.Init();
	}

	public static NtDataTranslateManager GetInstance()
	{
		if (instance == null)
		{
			instance = new NtDataTranslateManager();
		}
		return instance;
	}

	public void ShowRejectLogin(Action callback = null)
	{
		DialogBase dialogBase = Dialog.Create(330);
		dialogBase.SetTitleLabel(TranslateInfo.titleTip);
		dialogBase.SetText(string.Format(TranslateInfo.containTip, curBindEmail));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		dialogBase.SetSize(DialogBase.Size.XL);
		dialogBase.SetButtonText(TranslateInfo.button_id7);
		dialogBase.SetVisibleContactButton(isVisible: false, 330.ToString());
		dialogBase.onPushButton1 = delegate
		{
			callback.Call();
		};
	}
}
