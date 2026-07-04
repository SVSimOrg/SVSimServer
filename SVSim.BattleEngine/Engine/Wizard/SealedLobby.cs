using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard;

public class SealedLobby : MonoBehaviour
{
	[SerializeField]
	private ArenaCommonLobby _commonLobbyPrefab;

	[SerializeField]
	private GameObject _buttonsRoot;

	[SerializeField]
	private UIButton _deckEditButton;

	[SerializeField]
	private UIButton _deckViewButton;

	[SerializeField]
	private UIButton _deckCodeGenerateButton;

	[SerializeField]
	private UIButton _stageSelectButton;

	[SerializeField]
	private UIButton _retireButton;

	[SerializeField]
	private UISprite _incompleteDeckIcon;

	[SerializeField]
	private CardSelectDialog _cardSelectDialogPrefab;

	private List<string> _unloadAssetList = new List<string>();

	private ArenaCommonLobby _commonLobby;

	private UICardList _deckViewerDialog;

	private SealedData SealedData => Data.ArenaData.SealedData;

	public bool IsReady => _commonLobby.IsReady;

	public void Init()
	{
		_commonLobby = NGUITools.AddChild(base.gameObject, _commonLobbyPrefab.gameObject).GetComponent<ArenaCommonLobby>();
		_commonLobby.Init(new ArenaCommonLobbyInitParam
		{
			ClassId = SealedData.SelectedClassId.Value,
			BattleMaxNum = 5,
			BattleWinNum = SealedData.BattleWinNum,
			BattleResultList = SealedData.BattleResultList,
			BattleButtonClickCallback = OnClickBattleButton,
			RewardReceiveButtonClickCallback = OnClickRewardReceiveButton,
			BattleMaintenanceType = NetworkDefine.MAINTENANCE_TYPE.ARENA_SEALED_BATTLE_MAINTENANCE
		});
		_buttonsRoot.transform.SetParent(_commonLobby.ButtonsRoot.transform);
		_buttonsRoot.transform.localPosition = Vector3.zero;
		UIEventListener.Get(_deckEditButton.gameObject).onClick = OnClickDeckEditButton;
		UIEventListener.Get(_deckViewButton.gameObject).onClick = OnClickDeckViewButton;
		UIEventListener.Get(_deckCodeGenerateButton.gameObject).onClick = OnClickDeckCodeGenerateButton;
		UIEventListener.Get(_stageSelectButton.gameObject).onClick = OnClickStageSelectButton;
		UIEventListener.Get(_retireButton.gameObject).onClick = OnClickRetireButton;
		bool flag = SealedData.BattleResultList.Length >= 5;
		UIManager.SetObjectToGrey(_deckEditButton.gameObject, flag);
		UIManager.SetObjectToGrey(_stageSelectButton.gameObject, flag);
		UIManager.SetObjectToGrey(_retireButton.gameObject, flag);
		bool flag2 = !SealedData.IsCompletedDeck.Value && !flag;
		_commonLobby.SetDecisionButtonToDark(flag2);
		UIManager.SetObjectToGrey(_deckCodeGenerateButton.gameObject, flag2);
		_incompleteDeckIcon.gameObject.SetActive(flag2 || false /* Pre-Phase-5b: headless has no maintenance card list */);
	}

	public void Final()
	{
		Toolbox.ResourcesManager.RemoveAssetGroup(_unloadAssetList);
		_unloadAssetList.Clear();
		_deckViewerDialog?.RemoveData();
		_commonLobby.Final();
		UnityEngine.Object.Destroy(_commonLobby.gameObject);
	}

	private void OnClickDeckEditButton(GameObject g)
	{

		SealedController.GoToSealedDeckEdit();
	}

	private void OnClickDeckViewButton(GameObject g)
	{

		StartCoroutine(OpenDeckViewerDialogCoroutine());
	}

	private IEnumerator OpenDeckViewerDialogCoroutine()
	{
		if (_deckViewerDialog == null)
		{
			_deckViewerDialog = DialogCreator.CreateDeckViewerDialog(base.gameObject);
			DeckData deckData = SealedData.DeckData;
			List<string> loadAssetList = _deckViewerDialog.GetLoadFileList(deckData.GetCardIdList());
			UIManager uiMgr = UIManager.GetInstance();
			uiMgr.createInSceneCenterLoading();
			yield return StartCoroutine(Toolbox.ResourcesManager.LoadAssetGroupAsync(loadAssetList, delegate
			{
				_unloadAssetList.AddRange(loadAssetList);
			}));
			uiMgr.closeInSceneCenterLoading();
			_deckViewerDialog.SetDeck(deckData, null);
			_deckViewerDialog.SetShareButtonUse(isUse: true);
			_deckViewerDialog.SubmitDeckType = GenerateDeckCodeTask.SubmitDeckType.SEALED;
			CardDetailUI cardDetail = _deckViewerDialog.CardDetail;
			cardDetail.IsShowFlavorTextButton = true;
			cardDetail.IsShowVoiceButton = true;
			cardDetail.IsShowEvolutionButton = true;
		}
		_deckViewerDialog.SetActive(in_Active: true);
		_deckViewerDialog.ResetScroll();
	}

	private void OnClickDeckCodeGenerateButton(GameObject g)
	{

		ArenaCommonLobby.GenerateDeckCode(GenerateDeckCodeTask.SubmitDeckType.SEALED, SealedData.SelectedClassId.Value, SealedData.DeckOriginalExcludedPhantomCardList, SealedData.DeckOriginalPhantomCardList);
	}

	private void OnClickStageSelectButton(GameObject g)
	{

		DialogCreator.CreateStageSelectDialog();
	}

	private void OnClickRetireButton(GameObject g)
	{

		ArenaCommonLobby.OpenRetireConfirmDialog().onPushButton1 = delegate
		{
			StartCoroutine(Toolbox.NetworkManager.Connect(new SealedRetireTask(), delegate
			{
				ReceiveReward();
			}));
		};
	}

	private void OnClickBattleButton()
	{
		// Pre-Phase-5b: DataMgr writes for sealed-mode deck selection (battle type, deck id,
		// chara/deck data). Purely UI-driven pre-battle setup; headless never runs this
		// coroutine, and Data.CurrentFormat / UIManager view change are the only observable
		// outputs the surviving caller cares about.
		Data.CurrentFormat = Format.Sealed;
		UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.RankMatch);
	}

	private void OnClickRewardReceiveButton()
	{

		StartCoroutine(Toolbox.NetworkManager.Connect(new SealedFinishTask(), delegate
		{
			ReceiveReward();
		}));
	}

	private void ReceiveReward()
	{
		_commonLobby.ClickProtectionCollider.gameObject.SetActive(value: true);
		// Pre-Phase-5b: headless has no InputMgr
		_commonLobby.TreasureBoxInfo.OpenBox(delegate
		{
			_commonLobby.ClickProtectionCollider.gameObject.SetActive(value: false);
			// Pre-Phase-5b: headless has no InputMgr
			if (SealedData.RewardCardCandidates.Count <= 0)
			{
				OpenRewardReceiveDialog();
			}
			else
			{
				OpenCardSelectDialog();
			}
		});
	}

	private void OpenRewardReceiveDialog()
	{
		DialogBase dialogBase = DialogCreator.CreateRewardReceiveDialog(SealedData.RewardList);
		dialogBase.SetLayer("Loading");
		dialogBase.ClickSe_Btn1 = 0;
		dialogBase.OnClose = (Action)Delegate.Combine(dialogBase.OnClose, (Action)delegate
		{
			UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.MyPage);
		});
	}

	private void OpenCardSelectDialog()
	{
		StartCoroutine(OpenCardSelectDialogCoroutine());
	}

	private IEnumerator OpenCardSelectDialogCoroutine()
	{
		UIManager uiMgr = UIManager.GetInstance();
		uiMgr.createInSceneCenterLoading();
		SystemText systemText = Data.SystemText;
		DialogBase dialog = uiMgr.CreateDialogClose();
		dialog.SetLayer("Loading");
		dialog.SetSize(DialogBase.Size.M);
		dialog.SetTitleLabel(systemText.Get("Sealed_RewardCardSelect_0001"));
		dialog.SetButtonLayout(DialogBase.ButtonLayout.CloseBtn);
		dialog.onPushButton1 = (dialog.onCloseWithoutSelect = _commonLobby.TreasureBoxInfo.Reset);
		CardSelectDialog dialogObject = NGUITools.AddChild(dialog.gameObject, _cardSelectDialogPrefab.gameObject).GetComponent<CardSelectDialog>();
		dialogObject.Init(SealedData.RewardCardCandidates, delegate(int cardId)
		{

			OpenCardSelectConfirmDialog(dialog, cardId);
		}, systemText.Get("Sealed_RewardCardSelect_0002"));
		while (!dialogObject.IsReady)
		{
			yield return null;
		}
		uiMgr.closeInSceneCenterLoading();
	}

	private void OpenCardSelectConfirmDialog(DialogBase cardSelectDialog, int cardId)
	{
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetSize(DialogBase.Size.S);
		dialogBase.SetTitleLabel(systemText.Get("Sealed_RewardCardSelect_0003"));
		dialogBase.SetText(systemText.Get("Sealed_RewardCardSelect_0004", CardMaster.GetInstance(FormatBehaviorManager.GetDefaultBehaviour(Format.Sealed).CardMasterId).GetCardParameterFromId(cardId).CardName));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
		dialogBase.SetButtonText(systemText.Get("Common_0004"));
		dialogBase.onPushButton1 = delegate
		{
			cardSelectDialog.Close();
			StartCoroutine(Toolbox.NetworkManager.Connect(new SealedSelectPhantomCardTask(cardId), delegate
			{
				OpenRewardReceiveDialog();
			}));
		};
	}
}
