using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.DeckCardEdit;
using Wizard.Dialog.Setting;

public class DeckCreateMenuUI : MonoBehaviour
{
	private enum DeckCopyCodeType
	{
	}

	[SerializeField]
	private UIButton _btnCamera;

	[SerializeField]
	private UIButton _btnLibrary;

	[SerializeField]
	private ItemToggle _foilPreferred;

	[SerializeField]
	private ItemToggle _isPrizePreferred;

	[SerializeField]
	private UISprite _centerSeparatorLine;

	private DialogBase _parentDialog;

	private Format _format;

	private ConventionDeckList _conventionDeckList;

	private IFormatBehavior _formatBehavior;

	private Action _onStartChangeViewScene;

	public static void ShowDeckCreateMenu(DeckData deck, ConventionDeckList conventionDeckList, Action onStartChangeViewScene = null)
	{
		Format format = deck.Format;
		DeckCardEditUI.SetDeckEditParameter(deck, conventionDeckList);
		DeckCreateMenuUI menu = UnityEngine.Object.Instantiate(UIManager.GetInstance()._deckCreateMenuOriginal);
		UnityEngine.Object.Destroy(menu._btnCamera.gameObject);
		menu._btnLibrary.gameObject.transform.SetSiblingIndex(0);
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(Data.SystemText.Get("Card_0108"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.CloseBtn);
		dialogBase.SetSize(DialogBase.Size.M);
		dialogBase.SetPanelDepth(10);
		if (conventionDeckList == null)
		{
			menu._foilPreferred.gameObject.SetActive(value: true);
			menu._foilPreferred.SetTitleLabel("プレミアムカード優先");
			menu._foilPreferred.SetValue(Data.Load.data._userConfig.IsFoilPreferred);
			menu._foilPreferred.SetActive_SeparatorLine(isActive: true);
			menu._foilPreferred.AddChangeCallback(delegate
			{
				DeckCardEditUI.SendConfigUpdateFoilPreferred(menu._foilPreferred.GetValue());
			});
			menu._isPrizePreferred.gameObject.SetActive(value: true);
			menu._isPrizePreferred.SetTitleLabel("絵違いカード優先");
			menu._isPrizePreferred.SetValue(Data.Load.data._userConfig.IsPrizePreferred);
			menu._isPrizePreferred.SetActive_SeparatorLine(isActive: true);
			menu._isPrizePreferred.AddChangeCallback(delegate
			{
				DeckCardEditUI.SendConfigUpdatePrizePreferred(menu._isPrizePreferred.GetValue());
			});
		}
		dialogBase.SetObj(menu.gameObject);
		if (conventionDeckList != null)
		{
			Vector3 localPosition = menu.transform.localPosition;
			localPosition.y = -70f;
			menu.transform.localPosition = localPosition;
			menu._centerSeparatorLine.gameObject.SetActive(value: false);
		}
		DeckCreateMenuUI component = menu.GetComponent<DeckCreateMenuUI>();
		component.SetParentDialog(dialogBase);
		component._format = format;
		component._conventionDeckList = conventionDeckList;
		component._formatBehavior = FormatBehaviorManager.Create(format, conventionDeckList);
		component._onStartChangeViewScene = onStartChangeViewScene;
	}

	private void SetParentDialog(DialogBase dialog)
	{
		_parentDialog = dialog;
	}
}
