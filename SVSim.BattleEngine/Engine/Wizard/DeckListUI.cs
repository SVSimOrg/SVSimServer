using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.DeckCardEdit;
using Wizard.ErrorDialog;

namespace Wizard;

public class DeckListUI : UIBase
{

	[SerializeField]
	private DeckListMenuUI _deckListMenuPrefab;

	private DeckListMenuUI _deckListMenu;

	[SerializeField]
	private GameObject _rootDeckListMenuUI;

	[SerializeField]
	private DeckDetailDialog _deckDetailDialogPrefab;

	private DeckDetailDialog _deckDetailDialog;

	[SerializeField]
	private UICardList _deckPreviewPrefab;

	private UICardList _deckPreview;

	[SerializeField]
	private CardDetailUI _cardDetailPrefab;

	private CardDetailUI _cardDetail;

	private List<string> _loadCardList;

	private IFormatBehavior _formatBehavior;

	private DeckData _deleteDefaultSelectDeck;

	private ConventionDeckList _conventionDeckList;

	private DeckGroup _deckGroup;

	private TopBar _topBar;

	private readonly List<string> _loadedVoiceList = new List<string>();

	private static readonly Dictionary<Format, int> SPECIAL_FORMAT_PERIOD_ERROR = new Dictionary<Format, int>
	{
		{
			Format.Crossover,
			5801
		},
		{
			Format.MyRotation,
			5802
		}
	};

	private DeckListUIParam SceneParam => UIManager.GetInstance().GetSceneParam<DeckListUIParam>(UIManager.ViewScene.DeckList);

	public static void ChangeSceneToDeckList(Format format, UIManager.ChangeViewSceneParam param = null, ConventionInfo conventionInfo = null)
	{
		UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.DeckList, param, new DeckListUIParam(format, conventionInfo));
	}

	public static bool IsSpecialFormatPeriodError(Format format)
	{
		if (!CheckSpecialFormatPeriod(format))
		{
			Wizard.ErrorDialog.Dialog.Create(SPECIAL_FORMAT_PERIOD_ERROR[format]);
			return true;
		}
		return false;
	}

	public static bool CheckSpecialFormatPeriod(Format format)
	{
		return format switch
		{
			Format.MyRotation => Data.MyRotationAllInfo.IsMyRotationEnable, 
			Format.Crossover => Data.Crossover.IsWithinAnyPeriod, 
			_ => true, 
		};
	}

	public override void onFirstStart()
	{
		if (!CheckSpecialFormatPeriod(SceneParam.Format))
		{
			Wizard.ErrorDialog.Dialog.Create(SPECIAL_FORMAT_PERIOD_ERROR[SceneParam.Format]);
			return;
		}
		base.IsShowFooterMenu = true;
		UIManager.GetInstance()._Footer.SetAllButtonEnableColorChange(isEnable: true);
		_ = Data.SystemText;
		base.onFirstStart();
		UIManager.ChangeViewSceneParam changeViewSceneParam = new UIManager.ChangeViewSceneParam();
		changeViewSceneParam.MyPageMenuIndex = 4;
		changeViewSceneParam.IsCutCardMotion = true;
		DeckListUIParam sceneParam = SceneParam;
		if (sceneParam.ConventionInfo == null)
		{
			changeViewSceneParam.OnFinishChangeView = delegate
			{
				MyPageMenu.Instance.GoToCardDeck();
			};
		}
		UIManager.GetInstance().RemoveNowSceneBackButtonParameter();
		if (sceneParam.ConventionInfo != null)
		{
			changeViewSceneParam.MyPageMenuIndex = 3;
			if (sceneParam.ConventionInfo.Status == ConventionInfo.ConventionStatus.GameStart)
			{
				changeViewSceneParam.OnFinishChangeView = delegate
				{
					MyPageMenu.Instance.GoToConventionActionMenu(sceneParam.ConventionInfo);
				};
			}
			else
			{
				changeViewSceneParam.OnFinishChangeView = delegate
				{
					MyPageMenu.Instance.GoToConventionListMenu();
				};
			}
			_topBar = UIManager.GetInstance().CreateTopBar(base.gameObject, string.Empty, UIManager.ViewScene.MyPage, MoneyDraw: true, changeViewSceneParam);
		}
		else
		{
			_ = string.Empty;
			if (sceneParam.Format == Format.MyRotation)
			{
				UIManager.GetInstance().CheckFirstTips(FirstTips.TipsType.MyRotationDeck);
			}
			_topBar = UIManager.GetInstance().CreateTopBar(base.gameObject, string.Empty, UIManager.ViewScene.MyPage, MoneyDraw: true, changeViewSceneParam);
		}
		UIManager.GetInstance().SetLayerRecursive(_topBar.transform, LayerMask.NameToLayer("MyPage"));
	}

	private void UpdateTopBarText()
	{
		SystemText systemText = Data.SystemText;
		int num = 0;
		foreach (DeckData deckData in _deckGroup.DeckDataList)
		{
			if (!deckData.IsNoCard())
			{
				num++;
			}
		}
		if (SceneParam.ConventionInfo != null)
		{
			string text = systemText.Get("Arena_0058") + " " + UIUtil.GetFormatName(SceneParam.Format);
			if (SceneParam.Format == Format.Rotation && num <= 1)
			{
				_topBar.SetTitleLabel(text, isWideMode: true);
				return;
			}
			_topBar.SetTitleLabel(text, isWideMode: false);
			_topBar.SetTitleLabelWidth(400);
			return;
		}
		string text2 = string.Empty;
		switch (SceneParam.Format)
		{
		case Format.Rotation:
			text2 = systemText.Get("Card_0001");
			break;
		case Format.Unlimited:
			text2 = systemText.Get("Card_0187");
			break;
		case Format.PreRotation:
			text2 = systemText.Get("Card_0233");
			break;
		case Format.Crossover:
			text2 = systemText.Get("Card_0292");
			break;
		case Format.MyRotation:
			text2 = systemText.Get("Card_0297");
			break;
		}
		if (SceneParam.Format == Format.Rotation && num <= 1)
		{
			_topBar.SetTitleLabel(text2, isWideMode: true);
			return;
		}
		_topBar.SetTitleLabel(text2, isWideMode: false);
		_topBar.SetTitleLabelWidth(420);
	}

	protected override void onOpen()
	{
		base.onOpen();
		InitializeDeckList(delegate
		{
			UpdateTopBarText();
			StartCoroutine(WaitForCommonBackGround(delegate
			{
				UIManager.GetInstance().OnReadyViewScene(isFadein: true);
			}));
		});
	}

	protected override void onClose()
	{
		_deckListMenu.OnSelectDeck -= OnSelectDeck;
		if (_loadedVoiceList.Count > 0)
		{

			Toolbox.ResourcesManager.RemoveAssetGroup(_loadedVoiceList);
			_loadedVoiceList.Clear();
		}
		base.onClose();
	}

	private IEnumerator WaitForCommonBackGround(Action onComplete)
	{
		while (!CommonBackGround.Instance.IsFinishLod)
		{
			yield return null;
		}
		while (!CommonBackGround.Instance.IsFinishEffectLoading())
		{
			yield return null;
		}
		onComplete?.Invoke();
	}

	public override bool IsUseCommonBackground()
	{
		return true;
	}

	private void InitializeDeckList(Action callback)
	{
		DisplayEditDeckListLoad(delegate(DeckGroup deckGroup)
		{
			_deckGroup = deckGroup;
			bool enableFirstViewLastUseDeck = SceneParam.ConventionInfo == null;
			_deckListMenu = NGUITools.AddChild(_rootDeckListMenuUI, _deckListMenuPrefab.gameObject).GetComponent<DeckListMenuUI>();
			_deckListMenu.Initialize(_deckGroup, GetEditState(), OnSelectDeck, OnMultiDeckDelete, OnLongPressMultiDeckDelete, CreateDeckDeleteTask, CreateSaveDeckOrderTask, IsVisibleCreateNewButton(), enableFirstViewLastUseDeck, callback);
			if (SceneParam.ConventionInfo == null)
			{
				_formatBehavior = FormatBehaviorManager.GetDefaultBehaviour(SceneParam.Format);
			}
		}, isNeedDeckListUpdateAPI: true);
	}

	private void ReloadDeckList(bool isNeedDeckListUpdateAPI)
	{
		DisplayEditDeckListLoad(delegate(DeckGroup deckGroup)
		{
			_deckGroup = deckGroup;
			_deckListMenu.UpdateDeckList(_deckGroup, null);
			if (_deckDetailDialog != null)
			{
				_deckDetailDialog.SetDeck(GetDeckById(_deckDetailDialog.GetDeckId()));
			}
			UpdateTopBarText();
		}, isNeedDeckListUpdateAPI);
	}

	private DeckData GetDeckById(int deckId)
	{
		if (SceneParam.ConventionInfo != null)
		{
			return _conventionDeckList.DeckList[deckId];
		}
		return _deckGroup.DeckDataList.FirstOrDefault((DeckData deck) => deck.GetDeckID() == deckId);
	}

	private BaseTask CreateDeckDeleteTask(List<int> deleteDeckNoList)
	{
		if (SceneParam.ConventionInfo != null)
		{
			ConventionDeckDeleteTask conventionDeckDeleteTask = new ConventionDeckDeleteTask();
			conventionDeckDeleteTask.SetParameter(SceneParam.ConventionInfo.Id, deleteDeckNoList.ToArray(), _deckGroup.DeckFormat);
			return conventionDeckDeleteTask;
		}
		DeckDeleteTask deckDeleteTask = new DeckDeleteTask();
		deckDeleteTask.SetParameter(deleteDeckNoList.ToArray(), _deckGroup.DeckFormat);
		return deckDeleteTask;
	}

	private BaseTask CreateSaveDeckOrderTask(List<int> deckOrderList)
	{
		if (SceneParam.ConventionInfo != null)
		{
			ConventionDeckOrderTask conventionDeckOrderTask = new ConventionDeckOrderTask();
			conventionDeckOrderTask.SetParameter(SceneParam.ConventionInfo.Id, deckOrderList.ToArray(), _deckGroup.DeckFormat);
			return conventionDeckOrderTask;
		}
		DeckOrderTask deckOrderTask = new DeckOrderTask();
		deckOrderTask.SetParameter(deckOrderList.ToArray(), _deckGroup.DeckFormat);
		return deckOrderTask;
	}

	private bool IsVisibleCreateNewButton()
	{
		if (SceneParam.ConventionInfo != null && SceneParam.ConventionInfo.Status == ConventionInfo.ConventionStatus.GameStart)
		{
			return false;
		}
		if (_deckGroup.DeckFormat == Format.PreRotation && Prerelease.Status == Prerelease.eStatus.DISPLAY_DECK_ONLY)
		{
			return false;
		}
		return true;
	}

	private DeckListMenuUI.eEditState GetEditState()
	{
		DeckListMenuUI.eEditState result = DeckListMenuUI.eEditState.CanEdit;
		if (SceneParam.ConventionInfo != null && SceneParam.ConventionInfo.Status == ConventionInfo.ConventionStatus.GameStart)
		{
			result = DeckListMenuUI.eEditState.Lock;
		}
		else if (SceneParam.Format == Format.PreRotation && Prerelease.Status == Prerelease.eStatus.DISPLAY_DECK_ONLY)
		{
			result = DeckListMenuUI.eEditState.DeleteOnly;
		}
		return result;
	}

	private void DisplayEditDeckListLoad(Action<DeckGroup> onSuccessWithDeckGroup, bool isNeedDeckListUpdateAPI)
	{
		if (SceneParam.ConventionInfo != null)
		{
			DeckConventionInfoTask conventionDeckTask = new DeckConventionInfoTask();
			conventionDeckTask.SetParameter(0, SceneParam.ConventionInfo);
			UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(conventionDeckTask, delegate
			{
				_conventionDeckList = conventionDeckTask.DeckList;
				_formatBehavior = FormatBehaviorManager.Create(SceneParam.Format, _conventionDeckList);
				DeckGroup arg = new DeckGroup(_conventionDeckList.DeckList.Values.ToList(), SceneParam.ConventionInfo.BattleParameterInstance.DeckFormat, DeckAttributeType.CustomDeck);
				onSuccessWithDeckGroup.Call(arg);
			}));
		}
		else if (isNeedDeckListUpdateAPI)
		{
			DeckMyListTask deckMyListTask = new DeckMyListTask();
			deckMyListTask.SetParameter(SceneParam.Format);
			UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(deckMyListTask, delegate
			{
				onSuccessWithDeckGroup.Call(GetCustomDeckGroup());
			}));
		}
		else
		{
			onSuccessWithDeckGroup.Call(GetCustomDeckGroup());
		}
		DeckGroup GetCustomDeckGroup()
		{
			return (from deckGroup in DeckListUtility.DeckGroupDataBaseClone()
				where deckGroup.AttributeType == DeckAttributeType.CustomDeck
				select deckGroup).FirstOrDefault((DeckGroup deckGroup) => deckGroup.DeckFormat == SceneParam.Format);
		}
	}

	private void OnSelectDeck(DeckData deck)
	{
		if (deck != null)
		{
			_deleteDefaultSelectDeck = _deckListMenu.GetDeckDataSamePage(deck);
			if (deck.IsNoCard())
			{
				DeckCreateMenuUI.ShowDeckCreateMenu(deck, _conventionDeckList);
			}
			else
			{
				CreateDeckDetailDialog(deck, _deckListMenu.EditState);
			}
		}
	}

	private void CreateDeckDetailDialog(DeckData deck, DeckListMenuUI.eEditState editState)
	{
		_deckDetailDialog = UnityEngine.Object.Instantiate(_deckDetailDialogPrefab);
		_deckDetailDialog.gameObject.SetActive(value: true);
		_deckDetailDialog.Initialize(deck, delegate
		{
			ReloadDeckList(isNeedDeckListUpdateAPI: false);
		}, _loadedVoiceList, _conventionDeckList);
		DialogBase dialog = UIManager.GetInstance().CreateDialogClose();
		dialog.SetSize(DialogBase.Size.M);
		dialog.SetPanelDepth(100);
		dialog.SetTitleLabel("");
		dialog.SetObj(_deckDetailDialog.gameObject);
		DialogBase dialogBase = dialog;
		dialogBase.OnClose = (Action)Delegate.Combine(dialogBase.OnClose, (Action)delegate
		{
			_deckDetailDialog.Final();
		});
		SystemText text = Data.SystemText;
		Action onPushButton = delegate
		{
			DeckCardEditUI.SetDeckEditParameter(_deckDetailDialog.Deck, _conventionDeckList);
			UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.DeckCardEdit);
		};
		Action onPushButton2 = delegate
		{
			dialog.CloseWithoutSelect();
			DialogBase dialogBase2 = UIManager.GetInstance().CreateDialogClose();
			dialogBase2.SetTitleLabel(text.Get("Dia_DeckEdit_001_Title"));
			dialogBase2.SetText(text.Get("Card_0009"));
			dialogBase2.SetButtonLayout(DialogBase.ButtonLayout.RedBtn_CancelBtn);
			dialogBase2.SetButtonText(text.Get("Card_0104"));
			dialogBase2.onPushButton1 = delegate
			{
				DeleteDeck(deck);
			};
		};
		Action action = delegate
		{
			ShowDeckViewer(deck);
		};
		if (deck.IsDefaultDeck())
		{
			dialog.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_RedBtn_GrayBtn);
			dialog.SetButtonText(text.Get("Card_0007"), text.Get("Card_0008"), text.Get("Card_0083"));
			dialog.SetButtonDisable(isEnableOK: true, isEnableCancel: true);
			dialog.ClickSe_Btn3 = 0;
			dialog.onPushButton3 = action;
			return;
		}
		switch (editState)
		{
		case DeckListMenuUI.eEditState.CanEdit:
			dialog.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_RedBtn_GrayBtn);
			dialog.SetButtonText(text.Get("Card_0007"), text.Get("Card_0008"), text.Get("Card_0083"));
			dialog.onPushButton1 = onPushButton;
			dialog.onPushButton2 = onPushButton2;
			dialog.ClickSe_Btn3 = 0;
			dialog.onPushButton3 = action;
			break;
		case DeckListMenuUI.eEditState.DeleteOnly:
			dialog.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_RedBtn_GrayBtn);
			dialog.SetButtonText(text.Get("Card_0007"), text.Get("Card_0008"), text.Get("Card_0083"));
			dialog.SetButtonDisable(isEnableOK: true);
			dialog.onPushButton2 = onPushButton2;
			dialog.ClickSe_Btn3 = 0;
			dialog.onPushButton3 = action;
			break;
		case DeckListMenuUI.eEditState.Lock:
			dialog.SetButtonLayout(DialogBase.ButtonLayout.GrayBtn);
			dialog.SetButtonText(text.Get("Card_0083"));
			dialog.ClickSe_Btn1 = 0;
			dialog.onPushButton1 = action;
			break;
		}
	}

	private void DeleteDeck(DeckData deck)
	{
		MyPageMenu.SetEnableReloadCard();
		int deckID = deck.GetDeckID();
		int deckClassID = deck.GetDeckClassID();
		if (SceneParam.ConventionInfo != null)
		{
			DeckConventionUpdateTask deckConventionUpdateTask = new DeckConventionUpdateTask();
			deckConventionUpdateTask.SetParameter(deckID, deckClassID, 0, isRandomLeaderSkin: false, null, 3000011L, "", is_delete: true, null, deck.MyRotationId, _conventionDeckList);
			StartCoroutine(Toolbox.NetworkManager.Connect(deckConventionUpdateTask, OnSuccessDeckDelete));
			return;
		}
		DeckUpdateTask deckUpdateTask = new DeckUpdateTask();
		deckUpdateTask.SetParameter(deckID, deckClassID, 0, isRandomLeaderSkin: false, null, 3000011L, "", is_delete: true, null, SceneParam.Format, deck.MyRotationId);
		StartCoroutine(Toolbox.NetworkManager.Connect(deckUpdateTask, delegate
		{
			DeckInfoTask deckInfoTask = new DeckInfoTask();
			deckInfoTask.SetParameter(Format.All);
			UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(deckInfoTask, OnSuccessDeckDelete));
		}));
	}

	private void ShowDeleteSuccessDialog()
	{
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(systemText.Get("Dia_DeckEdit_002_Title"));
		dialogBase.SetText(systemText.Get("Card_0010"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
	}

	private void OnSuccessDeckDelete(NetworkTask.ResultCode code)
	{
		ShowDeleteSuccessDialog();
		if (_deleteDefaultSelectDeck != null)
		{
			DeckListUtility.SaveLastSelectDeck(_deleteDefaultSelectDeck.GetDeckID(), _deleteDefaultSelectDeck.IsDefaultDeck(), _deleteDefaultSelectDeck.IsDeckAttributeMatch(DeckAttributeType.TrialDeck), _deleteDefaultSelectDeck.Format);
		}
		else
		{
			DeckListUtility.ClearLastSelectDeck(SceneParam.Format);
		}
		ReloadDeckList(isNeedDeckListUpdateAPI: true);
	}

	private void ShowDeckViewer(DeckData deck)
	{
		CheckTimeSlipRotationPeriodTask task = new CheckTimeSlipRotationPeriodTask();
		StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
		{
			StartCoroutine(ShowDeckViewerBody(deck));
		}));
	}

	private IEnumerator ShowDeckViewerBody(DeckData deck)
	{
		UIManager.GetInstance().createInSceneCenterLoading();
		if (_deckPreview == null)
		{
			_deckPreview = UnityEngine.Object.Instantiate(_deckPreviewPrefab);
			_deckPreview.transform.parent = base.transform;
			_deckPreview.transform.localPosition = Vector3.zero;
			_deckPreview.transform.localScale = Vector3.one;
			_cardDetail = UnityEngine.Object.Instantiate(_cardDetailPrefab);
			_cardDetail.transform.parent = _deckPreview.transform;
			_cardDetail.transform.localPosition = Vector3.zero;
			_cardDetail.transform.localScale = Vector3.one;
			_cardDetail.gameObject.SetActive(value: false);
			_cardDetail.Initialize(LayerMask.NameToLayer("Detail"), _formatBehavior.CardMasterId, _formatBehavior);
			_cardDetail.IsShowFlavorTextButton = true;
			_cardDetail.IsShowVoiceButton = true;
			_cardDetail.IsShowEvolutionButton = true;
			yield return null;
		}
		_deckPreview.gameObject.SetActive(value: false);
		List<int> cardIdList = deck.GetCardIdList();
		_loadCardList = _deckPreview.GetLoadFileList(cardIdList);
		yield return StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(_loadCardList, null));
		UIManager.GetInstance().closeInSceneCenterLoading();
		_deckPreview.gameObject.SetActive(value: true);
		_deckPreview.SetActive(in_Active: false);
		string deckName = deck.GetDeckName();
		_deckPreview.Init(null, _cardDetail, deckName, HideDeckViewer, "Detail", in_DetailCameraUse: true, (CardBasePrm.ClanType)deck.GetDeckClassID(), 40);
		_deckPreview.SetShareButtonUse(isUse: true);
		_deckPreview.SetDeck(deck, _conventionDeckList);
		if (_formatBehavior.CanShowQRCode)
		{
			_deckPreview.SetQRCodeButtonToGray();
		}
		yield return null;
		_deckPreview.SetActive(in_Active: true);
	}

	private void HideDeckViewer()
	{
		Toolbox.ResourcesManager.RemoveAssetGroup(_loadCardList);
		_deckPreview.RemoveData();
		_deckPreview.gameObject.SetActive(value: false);
		_deckListMenu.EnableDrag = true;
	}

	private void OnMultiDeckDelete()
	{
		ReloadDeckList(isNeedDeckListUpdateAPI: true);
	}

	private void OnLongPressMultiDeckDelete(DeckData deck)
	{

		ShowDeckViewer(deck);
		_deckListMenu.EnableDrag = false;
	}
}
