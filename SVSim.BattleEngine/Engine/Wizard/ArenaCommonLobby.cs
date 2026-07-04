using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard.ErrorDialog;

namespace Wizard;

public class ArenaCommonLobby : MonoBehaviour
{
	[SerializeField]
	private BoxCollider _clickProtectionCollider;

	[SerializeField]
	private UITexture _charaTexture;

	[SerializeField]
	private GameObject _mainObjectRoot;

	[SerializeField]
	private ArenaCommonLobbyBattleInfo _battleInfo;

	[SerializeField]
	private ArenaCommonLobbyTreasureBoxInfo _treasureBoxInfo;

	[SerializeField]
	private GameObject _buttonsRoot;

	[SerializeField]
	private UIButton _decisionButton;

	[SerializeField]
	private UILabel _decisionButtonLabel;

	private List<string> _unloadAssetList = new List<string>();

	private GameObject _decisionButtonEffect;

	private bool _isDecisionButtonDark;

	private Vector3 _charaInitPos = Vector3.zero;

	private Vector3 _mainObjectInitPos = Vector3.zero;

	private Vector3 _buttonInitPos = Vector3.zero;

	public BoxCollider ClickProtectionCollider => _clickProtectionCollider;

	public ArenaCommonLobbyTreasureBoxInfo TreasureBoxInfo => _treasureBoxInfo;

	public GameObject ButtonsRoot => _buttonsRoot;

	public bool IsReady { get; private set; }

	public void Init(ArenaCommonLobbyInitParam initParam)
	{
		_clickProtectionCollider.gameObject.SetActive(value: false);
		_charaInitPos = _charaTexture.transform.position;
		_mainObjectInitPos = _mainObjectRoot.transform.position;
		_buttonInitPos = _decisionButton.transform.position;
		_charaTexture.transform.position = _charaInitPos + Vector3.right * 4.6875f;
		_mainObjectRoot.transform.position = _mainObjectInitPos + Vector3.left * 2.8125f;
		_decisionButton.transform.position = _buttonInitPos + Vector3.right * 1.5625f;
		SystemText systemText = Data.SystemText;
		int num = initParam.BattleResultList.Length;
		bool battleExists = num < initParam.BattleMaxNum;
		_decisionButtonLabel.text = (battleExists ? systemText.Get("Arena_0051", (num + 1).ToString()) : systemText.Get("Arena_0027"));
		UIEventListener.Get(_decisionButton.gameObject).onClick = delegate
		{
			if (battleExists && initParam.BattleMaintenanceType.HasValue)
			{
				NetworkDefine.MAINTENANCE_TYPE value = initParam.BattleMaintenanceType.Value;
				if (Data.MaintenanceCodeList.Contains(value))
				{
					Wizard.ErrorDialog.Dialog.Create((int)value);
					return;
				}
			}
			(battleExists ? initParam.BattleButtonClickCallback : initParam.RewardReceiveButtonClickCallback)();
		};
		List<string> loadAssetList = new List<string>();
		List<Action> loadEndCallbackList = new List<Action>();
		ResourcesManager resMgr = Toolbox.ResourcesManager;
		string strSkinId = "0"; // Pre-Phase-5b: no chara master headless
		loadAssetList.Add(resMgr.GetAssetTypePath(strSkinId, ResourcesManager.AssetLoadPathType.ClassCharaBase));
		loadAssetList.Add(resMgr.GetAssetTypePath("cmn_ui_btn_1", ResourcesManager.AssetLoadPathType.Effect2D));
		ArenaCommonLobbyLoadRequest arenaCommonLobbyLoadRequest = _battleInfo.Init(initParam, _unloadAssetList);
		loadAssetList.AddRange(arenaCommonLobbyLoadRequest.LoadAssetList);
		loadEndCallbackList.Add(arenaCommonLobbyLoadRequest.LoadEndCallback);
		arenaCommonLobbyLoadRequest = _treasureBoxInfo.Init(initParam, _unloadAssetList);
		loadAssetList.AddRange(arenaCommonLobbyLoadRequest.LoadAssetList);
		loadEndCallbackList.Add(arenaCommonLobbyLoadRequest.LoadEndCallback);
		StartCoroutine(resMgr.LoadAssetGroupAsync(loadAssetList, delegate
		{
			_unloadAssetList.AddRange(loadAssetList);
			for (int i = 0; i < loadEndCallbackList.Count; i++)
			{
				loadEndCallbackList[i]();
			}
			UIManager.GetInstance().AttachAtlas(base.gameObject);
			_charaTexture.mainTexture = resMgr.LoadObject<Texture>(resMgr.GetAssetTypePath(strSkinId, ResourcesManager.AssetLoadPathType.ClassCharaBase, isfetch: true));
			_decisionButtonEffect = EffectUtility.CreateEffect2D(new Effect2dCreateParam
			{
				Parent = _decisionButton.gameObject,
				EffectName = "cmn_ui_btn_1",
				ColorCode = eColorCodeId.DECISION_BTN_2_COLOR,
				InitActive = !_isDecisionButtonDark,
				UnloadAssetList = _unloadAssetList
			});
			AppearAnimation();
			IsReady = true;
		}));
	}

	public void Final()
	{
		Toolbox.ResourcesManager.RemoveAssetGroup(_unloadAssetList);
		_unloadAssetList.Clear();
	}

	private void AppearAnimation()
	{
		StartCoroutine(AppearAnimationCoroutine());
	}

	private IEnumerator AppearAnimationCoroutine()
	{
		while (UIManager.GetInstance().isFading())
		{
			yield return null;
		}

		iTween.MoveTo(_charaTexture.gameObject, iTween.Hash("position", _charaInitPos, "time", 0.5f, "islocal", false, "easetype", iTween.EaseType.easeOutExpo));
		iTween.MoveTo(_mainObjectRoot, iTween.Hash("position", _mainObjectInitPos, "time", 0.5f, "delay", 0.2f, "islocal", false, "easetype", iTween.EaseType.easeOutExpo));
		iTween.MoveTo(_decisionButton.gameObject, iTween.Hash("position", _buttonInitPos, "time", 0.5f, "delay", 0.5f, "islocal", false, "easetype", iTween.EaseType.easeOutExpo));
	}

	public void SetDecisionButtonToDark(bool isDark)
	{
		UIManager.SetObjectToGrey(_decisionButton.gameObject, isDark);
		_decisionButtonEffect?.SetActive(!isDark);
		_isDecisionButtonDark = isDark;
	}

	public static void GenerateDeckCode(GenerateDeckCodeTask.SubmitDeckType deckType, int classId, int[] cardIdList, int[] phantomCardIdList = null)
	{
		GenerateDeckCodeTask generateDeckCodeTask = new GenerateDeckCodeTask();
		generateDeckCodeTask.SetParameter(classId, deckType, cardIdList, phantomCardIdList);
		UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(generateDeckCodeTask, delegate
		{
			OpenDeckCodeGenerateCompleteDialog();
		}, null, null, encrypt: false));
	}

	private static DialogBase OpenDeckCodeGenerateCompleteDialog()
	{
		string deckCode = Data.GenerateDeckCode.deck_code;
		SystemText text = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetSize(DialogBase.Size.S);
		dialogBase.SetTitleLabel(text.Get("Card_0120"));
		dialogBase.SetText(text.Get("Card_0128", deckCode));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_GrayBtn);
		dialogBase.SetButtonText(text.Get("Card_0133"), text.Get("Common_0008"));
		dialogBase.onPushButton1 = delegate
		{
			NativePluginWrapper.SetStringToClipboard(deckCode);
			UIManager.GetInstance().CreateConfirmationDialog(text.Get("Card_0132", deckCode));
		};
		return dialogBase;
	}

	public static DialogBase OpenRetireConfirmDialog()
	{
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(systemText.Get("Common_0051"));
		dialogBase.SetText(systemText.Get("Arena_0026"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.RedBtn_CancelBtn);
		dialogBase.SetButtonText(systemText.Get("Dia_Arena_006_Button"));
		return dialogBase;
	}
}
