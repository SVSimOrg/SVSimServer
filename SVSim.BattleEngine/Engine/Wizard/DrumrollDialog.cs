using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class DrumrollDialog : UIBase
{
	[SerializeField]
	private UtilityDrumrollScroll _drumroll;

	public int CurrentIndex => _drumroll.CurrentIndex;

	public static DialogBase Create(List<string> textList, int defaultIndex, Action<int> selectCallback, Action createCallback = null, Action<int> decideCallBack = null, string dialogTitle = "")
	{
		return Create(UnityEngine.Object.Instantiate(UIManager.GetInstance().DrumrollDialogPrefab), textList, defaultIndex, selectCallback, createCallback, decideCallBack, dialogTitle);
	}

	public static DialogBase Create(DrumrollDialog drumroll, List<string> textList, int defaultIndex, Action<int> selectCallback, Action createCallback = null, Action<int> decideCallBack = null, string dialogTitle = "")
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetSize(DialogBase.Size.M);
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		if (dialogTitle.IsNotNullOrEmpty())
		{
			dialogBase.SetTitleLabel(dialogTitle);
		}
		dialogBase.SetObj(drumroll.gameObject);
		drumroll.CreateDrumroll(textList, defaultIndex, selectCallback, createCallback);
		dialogBase.onPushButton1 = delegate
		{
			decideCallBack.Call(drumroll.CurrentIndex);
		};
		return dialogBase;
	}

	private void CreateDrumroll(List<string> textList, int defaultIndex, Action<int> selectCallback, Action createCallback = null)
	{
		StartCoroutine(_drumroll.CreateDrumrollScroll_Coroutine(textList, defaultIndex, selectCallback, createCallback));
	}
}
