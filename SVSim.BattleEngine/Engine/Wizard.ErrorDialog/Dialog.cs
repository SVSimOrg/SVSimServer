using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard.ErrorDialog;

public static class Dialog
{

	private static Dictionary<string, Data> _dataList = new Dictionary<string, Data>();

	private static Data _defaultData;

	public static DialogBase Create(string errorId)
	{
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose(isSystem: true);
		Setup(dialogBase, errorId);
		UIManager.GetInstance().WebViewHelper.CloseWebViewDialog();
		return dialogBase;
	}

	public static DialogBase Create(int errorId)
	{
		return Create(errorId.ToString());
	}

	public static void Setup(DialogBase dialog, string errorId)
	{
		Data data = (_dataList.ContainsKey(errorId) ? _dataList[errorId] : _defaultData);
		dialog.SetSize(DialogBase.Size.M);
		dialog.SetTitleLabel(Wizard.Data.SystemText.Get(data.TitleId));
		dialog.SetText(Wizard.Data.SystemText.Get(data.BodyId, errorId));
		dialog.SetVisibleContactButton(data.IsDisplayContact, errorId);
		dialog.SetButtonLayout(DialogBase.ButtonLayout.NONE);
		AddButtonToDialog(dialog, data.MainButton, data.SubButton == Data.ButtonType._NONE_);
		AddButtonToDialog(dialog, data.SubButton, isReflect: true);
		dialog.SetFadeButtonEnabled(flag: false);
		dialog.SetPanelDepth(data.PanelDepth);
		dialog.SetPanelSortingOrder(2);
	}

	private static void AddButtonToDialog(DialogBase dialog, Data.ButtonType type, bool isReflect)
	{
		switch (type)
		{
		case Data.ButtonType.OK:
			dialog.AddButton(DialogBase.ButtonType.OK, isReflect);
			break;
		case Data.ButtonType.リトライ:
			dialog.AddButton(DialogBase.ButtonType.Retry, isReflect);
			break;
		case Data.ButtonType.タイトルへ戻る:
			dialog.AddButton(DialogBase.ButtonType.BackToTitle, isReflect);
			break;
		case Data.ButtonType.ホームへ戻る:
			dialog.AddButton(DialogBase.ButtonType.BackToHome, isReflect);
			break;
		case Data.ButtonType.アプリ終了:
			dialog.AddButton(DialogBase.ButtonType.QuitApplication, isReflect);
			break;
		case Data.ButtonType.バージョンアップ:
			dialog.AddButton(DialogBase.ButtonType.VersionUp, isReflect);
			break;
		case Data.ButtonType.推奨端末一覧:
			dialog.AddButton(DialogBase.ButtonType.RecommendedList, isReflect);
			break;
		case Data.ButtonType._NONE_:
			break;
		}
	}
}
