using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.DeckSelect.FirstDisplayPageIndexGetter;

namespace Wizard;

public class QuestSelectionPage : UIBase
{
	public enum FirstSelectType
	{
		NONE}

	[SerializeField]
	private UISpriteAtlasOverwriter _spriteAtlasOverwriter;

	[SerializeField]
	private UITexture _selectCharaTexture;

	[SerializeField]
	private QuestSelectionButtonBase[] _classButtonParts;

	[SerializeField]
	private UIButton _decideButton;

	[SerializeField]
	private UILabel _decideButtonTextLabel;

	[SerializeField]
	private GameObject _decisionButtonEffect;

	[SerializeField]
	private UIButton _pointConfirmButton;

	[SerializeField]
	private UIButton _questConfirmButton;

	[SerializeField]
	private UIButton _questBonusDetailButton;

	[SerializeField]
	private UILabel _rewardRecieveNumberLabel;

	[SerializeField]
	private UITexture _bgTexture;

	[SerializeField]
	private UILabel _periodLabel;

	[SerializeField]
	private SimpleScrollViewUI _questButtonScrollView;

	[SerializeField]
	private Vector3 _charaMoveStartPos = new Vector3(245f, 15f, 0f);

	[SerializeField]
	private Vector3 _charaMoveEndPos = new Vector3(275f, 15f, 0f);

	[SerializeField]
	private iTween.EaseType _charaMoveEaseType = iTween.EaseType.linear;

	[SerializeField]
	private float _charaMoveTime = 0.1f;

	[SerializeField]
	private QuestPointConfirmDialog _questPointConfirmDialogOriginal;

	[SerializeField]
	private QuestAllConfirmDialog _questAllConfirmDialogOriginal;

	[SerializeField]
	private GameObject _winBonusRoot;

	[SerializeField]
	private GameObject _bossRushTurnDisplayRoot;

	[SerializeField]
	private UIButton _tweetBannerButton;

	[SerializeField]
	private UITexture _tweetBannerTexture;

	private bool _isTweetFinish;

	private List<string> _loadPathList = new List<string>();

	private List<QuestOpponentData> _questDataList;

	private List<QuestSelectionButtonBase> _selectionButtonList;

	private int _currentIndex = -1;

	private string _currentTextureId = "";

	private bool _isOpenExtra;

	private bool _isLastDay;

	private QuestInfoTask _questInfoTask;

	private string _announceId = string.Empty;

	private PuzzleQuestInfo _puzzleQuestInfo;

	private EventStoryQuestInfo _eventStoryQuestInfo;

	private BossRushInfo _bossRushInfo;

	private SecretBossInfo _secretBossInfo;

	private List<QuestSelectionButtonData> _buttonData;

	public override bool IsUseCommonBackground()
	{
		return false;
	}

	public override void onFirstStart()
	{
		base.IsShowFooterMenu = true;
		base.onFirstStart();
	}

	protected override void onOpen()
	{
		base.onOpen();
		Init();
	}

	protected override void onClose()
	{
		Final();
		base.onClose();
	}

	private void Init()
	{
		QuestSelectionButtonBase[] classButtonParts = _classButtonParts;
		for (int i = 0; i < classButtonParts.Length; i++)
		{
			classButtonParts[i].gameObject.SetActive(value: false);
		}
		CreateTopBar();
		InitFooter();
		InitTask();
		InitSpriteAtlasOverwriter();
	}

	private void Final()
	{
		Toolbox.ResourcesManager.RemoveAssetGroup(_loadPathList);
		_loadPathList.Clear();
		UIManager.GetInstance()._Footer.CancelOverwriteLabelColors();
	}

	private void CreateTopBar()
	{
		UIManager.ChangeViewSceneParam changeViewSceneParam = new UIManager.ChangeViewSceneParam();
		changeViewSceneParam.MyPageMenuIndex = 1;
		changeViewSceneParam.IsCutCardMotion = true;
		TopBar topBar = UIManager.GetInstance().CreateTopBar(base.gameObject, Data.SystemText.Get("Quest_0003"), UIManager.ViewScene.MyPage, MoneyDraw: false, changeViewSceneParam);
		topBar.gameObject.layer = LayerMask.NameToLayer("MyPage");
		topBar.OverwriteBackLabelColors(eColorCodeId.QuestBackButtonGradientTop, eColorCodeId.QuestBackButtonGradientBottom);
	}

	private void InitFooter()
	{
		UIManager instance = UIManager.GetInstance();
		instance.setBackScene(base.gameObject, UIManager.ViewScene.MyPage);
		instance._Footer.UpdateCurrentIndex(1);
		instance._Footer.OverwriteLabelColors(eColorCodeId.QuestFooterGradientTop, eColorCodeId.QuestFooterGradientBottom, eColorCodeId.QuestFooterOutline);
	}

	private void InitSpriteAtlasOverwriter()
	{
		UIAtlas component = Toolbox.ResourcesManager.LoadObject<GameObject>(Toolbox.ResourcesManager.GetAssetTypePath("dummy", ResourcesManager.AssetLoadPathType.QuestAtlas, isfetch: true)).GetComponent<UIAtlas>();
		UISpriteAtlasOverwriter.TargetObject[] targetObjects = new UISpriteAtlasOverwriter.TargetObject[2]
		{
			new UISpriteAtlasOverwriter.TargetObject(UIManager.GetInstance().UIManagerRoot.gameObject, includeChildren: true),
			new UISpriteAtlasOverwriter.TargetObject(UIManager.GetInstance().UIRootSystem.gameObject, includeChildren: true)
		};
		_spriteAtlasOverwriter.Init(component, targetObjects);
	}

	public void UpdateTweetButtonVisible(bool isSelectBeginner)
	{
		if (_isTweetFinish)
		{
			isSelectBeginner = false;
		}
		_tweetBannerButton.gameObject.SetActive(value: false);
	}

	private void InitTask()
	{
		QuestInfoTask task = new QuestInfoTask();
		_questInfoTask = task;
		StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
		{
			_questDataList = task.QuestDataList;
			_isOpenExtra = task.IsOpenExtra;
			_isLastDay = task.IsLastDay;
			_announceId = task.AnnounceId;
			ShowUnreceivedRewardCount(task.UnreceivedRewardCount);
			_periodLabel.text = Data.SystemText.Get("Quest_0007", ConvertTime.ToLocal(task.StartTime, task.EndTime));
			UIManager.GetInstance()._Footer.UpdateQuestBadgeIcon(task.IsDisplayBadge);
			_puzzleQuestInfo = task.PuzzleQuestInfo;
			_eventStoryQuestInfo = task.EventStoryQuestInfo;
			_bossRushInfo = task.BossRushInfo;
			_secretBossInfo = task.SecretBossInfo;
			SetupLayout(task.QuestId);
			InitLastUsedDeckSaveData(task.QuestId);
		}));
	}

	private void SetupLayout(int questId)
	{
		StartCoroutine(LoadResources(delegate
		{
			SetBackGround();
			_tweetBannerTexture.mainTexture = Toolbox.ResourcesManager.LoadObject(GetTweetBannerPath(isFetch: true)) as Texture;
			CreateCharaButton();
			SetButtonCallback();
			DisplayFirstTips(questId);
			UIManager.GetInstance().OnReadyViewScene(isFadein: true);
		}));
	}

	private string GetTweetBannerPath(bool isFetch)
	{
		return Toolbox.ResourcesManager.GetAssetTypePath("quest_banner_0013", ResourcesManager.AssetLoadPathType.UiOtherTexture, isFetch);
	}

	private IEnumerator LoadResources(Action onFinish)
	{
		ResourcesManager resourcesManager = Toolbox.ResourcesManager;
		if (_puzzleQuestInfo.Status != PuzzleQuestStatus.None)
		{
			_loadPathList.AddRange(CollectPuzzleResourcePaths());
		}
		if (_eventStoryQuestInfo.EventStoryExist)
		{
			_loadPathList.AddRange(CollectEventStoryResourcePaths());
		}
		if (_bossRushInfo.BossRushInfoExist)
		{
			_loadPathList.AddRange(CollectBossRushResourcePaths());
		}
		if (_secretBossInfo.IsEnable)
		{
			_loadPathList.AddRange(CollectSecretBossPath());
		}
		if (_questInfoTask.IsDisplayTweetBanner)
		{
			_loadPathList.Add(GetTweetBannerPath(isFetch: false));
		}
		for (int i = 0; i < _questDataList.Count; i++)
		{
			string path = _questDataList[i].BattleData.CharaId.ToString();
			_loadPathList.Add(resourcesManager.GetAssetTypePath(path, ResourcesManager.AssetLoadPathType.ClassCharaWideThumbnail));
			_loadPathList.Add(resourcesManager.GetAssetTypePath(path, ResourcesManager.AssetLoadPathType.ClassCharaBase));
		}
		_loadPathList.Add(Toolbox.ResourcesManager.GetAssetTypePath("bg_quest", ResourcesManager.AssetLoadPathType.Background));
		yield return StartCoroutine(resourcesManager.LoadAssetGroupAsync(_loadPathList, null));
		onFinish.Call();
	}

	private List<string> CollectPuzzleResourcePaths()
	{
		string path = _puzzleQuestInfo.CharaId.ToString();
		return new List<string>
		{
			Toolbox.ResourcesManager.GetAssetTypePath(path, ResourcesManager.AssetLoadPathType.ClassCharaWideThumbnail),
			Toolbox.ResourcesManager.GetAssetTypePath(path, ResourcesManager.AssetLoadPathType.ClassCharaBase)
		};
	}

	private List<string> CollectEventStoryResourcePaths()
	{
		return new List<string>
		{
			Toolbox.ResourcesManager.GetAssetTypePath("event_story", ResourcesManager.AssetLoadPathType.ClassCharaWideThumbnail),
			Toolbox.ResourcesManager.GetAssetTypePath("event_story", ResourcesManager.AssetLoadPathType.ClassCharaBase)
		};
	}

	private List<string> CollectBossRushResourcePaths()
	{
		return new List<string>
		{
			Toolbox.ResourcesManager.GetAssetTypePath("boss_rush", ResourcesManager.AssetLoadPathType.ClassCharaWideThumbnail),
			Toolbox.ResourcesManager.GetAssetTypePath("boss_rush", ResourcesManager.AssetLoadPathType.ClassCharaBase)
		};
	}

	private List<string> CollectSecretBossPath()
	{
		string path = _secretBossInfo.CharaId.ToString();
		return new List<string>
		{
			Toolbox.ResourcesManager.GetAssetTypePath(path, ResourcesManager.AssetLoadPathType.ClassCharaWideThumbnail),
			Toolbox.ResourcesManager.GetAssetTypePath(path, ResourcesManager.AssetLoadPathType.ClassCharaBase)
		};
	}

	private void SetBackGround()
	{
		_bgTexture.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(Toolbox.ResourcesManager.GetAssetTypePath("bg_quest", ResourcesManager.AssetLoadPathType.Background, isfetch: true));
	}

	private void DisplayFirstTips(int questId)
	{
		bool item = _puzzleQuestInfo.DisplayDatas.Any((PuzzleQuestSelectDialog.DisplayData x) => x.IsAdditional && !x.IsCleared);
		bool bossRushInfoExist = _bossRushInfo.BossRushInfoExist;
		(FirstTips.TipsType, KeyValuePair<string, int>, bool)[] source = new(FirstTips.TipsType, KeyValuePair<string, int>, bool)[3]
		{
			(FirstTips.TipsType.Quest, PlayerPrefsWrapper.FIRST_TIPS_QUEST_ID, true),
			(FirstTips.TipsType.AdditionalPuzzle, PlayerPrefsWrapper.FIRST_TIPS_ADDITIONAL_PUZZLE_QUEST_ID, item),
			(FirstTips.TipsType.BossRush, PlayerPrefsWrapper.FIRST_TIPS_BOSSRUSH_QUEST_ID, bossRushInfoExist)
		};
		List<(FirstTips.TipsType TipsType, KeyValuePair<string, int> PrefsId, bool IsDisplay)> displayInfos = source.Where(((FirstTips.TipsType TipsType, KeyValuePair<string, int> PrefsId, bool IsDisplay) x) => x.IsDisplay && questId != PlayerPrefsWrapper.GetValue(x.PrefsId)).ToList();
		if (displayInfos.Count <= 0)
		{
			return;
		}
		IEnumerable<FirstTips.TipsType> tipsTypes = displayInfos.Select(((FirstTips.TipsType TipsType, KeyValuePair<string, int> PrefsId, bool IsDisplay) x) => x.TipsType);
		Action onFinish = delegate
		{
			displayInfos.ForEach(delegate((FirstTips.TipsType TipsType, KeyValuePair<string, int> PrefsId, bool IsDisplay) x)
			{
				PlayerPrefsWrapper.SetValue(x.PrefsId, questId);
			});
		};
		UIManager.GetInstance().StartFirstTips(tipsTypes, onFinish);
	}

	private void CreateCharaButton()
	{
		_buttonData = GenerateSelectListData();
		QuestSelectionButtonData defaultSelectData = GetDefaultSelectData();
		int num = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < _buttonData.Count; i++)
		{
			QuestSelectionButtonData questSelectionButtonData = _buttonData[i];
			list.Add((int)questSelectionButtonData.GetPlateType());
			if (questSelectionButtonData == defaultSelectData)
			{
				num = i;
			}
		}
		_currentIndex = num;
		_questButtonScrollView.CreateScrollView(list, InitializePlate);
		_selectionButtonList = _questButtonScrollView.ActivePlateList.Select((GameObject p) => p.GetComponent<QuestSelectionButtonBase>()).ToList();
		_selectionButtonList[num].SelectChara();
		_questButtonScrollView.MovePlateByIndex(num, SimpleScrollViewUI.VerticalMovement.Center);
	}

	private List<QuestSelectionButtonData> GenerateSelectListData()
	{
		List<QuestSelectionButtonData> list = new List<QuestSelectionButtonData>();
		for (int i = 0; i < _questDataList.Count; i++)
		{
			list.Add(new QuestSelectionButtonData(_questDataList[i], _questDataList.Count - i));
		}
		if (_puzzleQuestInfo.Status != PuzzleQuestStatus.None)
		{
			list.Add(new QuestSelectionButtonData(_puzzleQuestInfo));
		}
		if (_eventStoryQuestInfo.EventStoryExist)
		{
			list.Add(new QuestSelectionButtonData(_eventStoryQuestInfo));
		}
		if (_bossRushInfo.BossRushInfoExist)
		{
			list.Add(new QuestSelectionButtonData(_bossRushInfo));
		}
		if (_secretBossInfo.IsEnable)
		{
			list.Add(new QuestSelectionButtonData(_secretBossInfo));
		}
		list.Sort((QuestSelectionButtonData a, QuestSelectionButtonData b) => b.SortValue() - a.SortValue());
		return list;
	}

	private QuestSelectionButtonData GetDefaultSelectData()
	{
		QuestBattleData questBattleData = null; // Pre-Phase-5b: headless has no QuestBattleData
		if (questBattleData != null)
		{
			QuestSelectionButtonData questSelectionButtonData = null;
			{
				foreach (QuestSelectionButtonData buttonDatum in _buttonData)
				{
					if (buttonDatum.QuestData == null)
					{
						continue;
					}
					if (questSelectionButtonData == null)
					{
						questSelectionButtonData = buttonDatum;
					}
					if (_isOpenExtra)
					{
						if (buttonDatum.QuestData.BattleData.QuestStageId == questBattleData.QuestStageId)
						{
							return buttonDatum;
						}
					}
					else if (buttonDatum.QuestData.BattleData.QuestStageId == questBattleData.QuestStageId && !buttonDatum.QuestData.BattleData.IsExtra)
					{
						return buttonDatum;
					}
				}
				return questSelectionButtonData;
			}
		}
		if (false /* Pre-Phase-5b: headless has no QuestFirstSelectType */)
		{
			// Pre-Phase-5b: headless has no QuestFirstSelectType write
			if (_puzzleQuestInfo.Status == PuzzleQuestStatus.InProgress)
			{
				foreach (QuestSelectionButtonData buttonDatum2 in _buttonData)
				{
					if (buttonDatum2.PuzzleData != null)
					{
						return buttonDatum2;
					}
				}
			}
		}
		bool flag = Data.MaintenanceCodeList.Contains(NetworkDefine.MAINTENANCE_TYPE.BOSS_RUSH);
		if (false && !flag /* Pre-Phase-5b: headless has no QuestFirstSelectType */)
		{
			foreach (QuestSelectionButtonData buttonDatum3 in _buttonData)
			{
				if (buttonDatum3.BossRushData != null && !buttonDatum3.BossRushData.IsAllChallengeFinished)
				{
					return buttonDatum3;
				}
			}
		}
		foreach (QuestSelectionButtonData buttonDatum4 in _buttonData)
		{
			if ((buttonDatum4.QuestData == null || buttonDatum4.QuestData.IsPlayable) && (buttonDatum4.BossRushData == null || !(!buttonDatum4.BossRushData.IsBossRushUnlocked || flag)))
			{
				return buttonDatum4;
			}
		}
		Debug.LogError("プレイ可能なクエストが見つかりませんでした");
		return _buttonData[0];
	}

	private void SetButtonCallback()
	{
		UIEventListener.Get(_decideButton.gameObject).onClick = OnDecideButtonClick;
		UIEventListener.Get(_pointConfirmButton.gameObject).onClick = OnPointConfirmButtonClick;
		UIEventListener.Get(_questConfirmButton.gameObject).onClick = OnQuestConfirmButtonClick;
		UIEventListener.Get(_questBonusDetailButton.gameObject).onClick = OnClickBonusDetailButton;
		UIEventListener.Get(_tweetBannerButton.gameObject).onClick = delegate
		{
			OnClickTweetBanner();
		};
	}

	private void InitializePlate(int index, GameObject obj)
	{
		QuestSelectionButtonBase questSelectionButton = obj.GetComponent<QuestSelectionButtonBase>();
		questSelectionButton.SetQuestSelectionPage(this);
		_ = questSelectionButton;
		questSelectionButton.Initialize(_buttonData[index], _isOpenExtra, _isLastDay, delegate
		{
			OnClickClassButton(index);
			SetSelectSprite(questSelectionButton);
		});
		questSelectionButton.SetActiveSelectSprite(index == _currentIndex);
	}

	private void SetSelectSprite(QuestSelectionButtonBase selectButton)
	{
		for (int i = 0; i < _selectionButtonList.Count; i++)
		{
			QuestSelectionButtonBase questSelectionButtonBase = _selectionButtonList[i];
			questSelectionButtonBase.SetActiveSelectSprite(questSelectionButtonBase == selectButton);
		}
	}

	private void OnClickClassButton(int index)
	{

		if (_currentIndex != index)
		{
			_currentIndex = index;
			_selectionButtonList[_currentIndex].SelectChara();
		}
	}

	public void SelectCharaPuzzleButton(PuzzleQuestInfo buttonData)
	{
		UpdateTweetButtonVisible(isSelectBeginner: false);
		ChangeChara(buttonData.CharaId);
		_winBonusRoot.SetActive(value: false);
		UIManager.SetObjectToGrey(_decideButton.gameObject, b: false, ColorCode.Get(eColorCodeId.QuestSelectButtonTextColor));
		_decisionButtonEffect.SetActive(value: true);
		_decideButtonTextLabel.text = Data.SystemText.Get("Puzzle_QuestSelect_0001");
		_bossRushTurnDisplayRoot.SetActive(value: false);
	}

	private void ChangeChara(int charaId, bool isPlayChangeAnimation = true)
	{
		if (!(_currentTextureId == charaId.ToString()))
		{
			_currentTextureId = charaId.ToString();
			ResourcesManager resourcesManager = Toolbox.ResourcesManager;
			_selectCharaTexture.mainTexture = resourcesManager.LoadObject<Texture>(resourcesManager.GetAssetTypePath(charaId.ToString(), ResourcesManager.AssetLoadPathType.ClassCharaBase, isfetch: true));
			if (isPlayChangeAnimation)
			{
				PlayCharaChangeAnimation();
			}
		}
	}

	private void PlayCharaChangeAnimation()
	{
		GameObject obj = _selectCharaTexture.gameObject;
		iTween.Stop(obj);
		obj.transform.localPosition = _charaMoveStartPos;
		iTween.MoveTo(obj, iTween.Hash("position", _charaMoveEndPos, "time", _charaMoveTime, "islocal", true, "easetype", _charaMoveEaseType));
	}

	private void OnDecideButtonClick(GameObject g)
	{
		_selectionButtonList[_currentIndex].OnDecideButtonClick();
	}

	public void OnPuzzleDecideButtonClick()
	{

		CreatePuzzleQuestSelectDialog();
		// Pre-Phase-5b: headless has no QuestBattleData
	}

	private void OnQuestConfirmButtonClick(GameObject g)
	{

		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetSize(DialogBase.Size.M);
		dialogBase.SetTitleLabel(Data.SystemText.Get("Quest_0005"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.CloseBtn);
		QuestAllConfirmDialog questAllConfirmDialog = UnityEngine.Object.Instantiate(_questAllConfirmDialogOriginal);
		questAllConfirmDialog.CreateQuestAllConfirmDialog();
		dialogBase.SetObj(questAllConfirmDialog.gameObject);
	}

	private void OnPointConfirmButtonClick(GameObject g)
	{

		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetSize(DialogBase.Size.M);
		dialogBase.SetTitleLabel(Data.SystemText.Get("Quest_0006"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.CloseBtn);
		QuestPointConfirmDialog questPointConfirmDialog = UnityEngine.Object.Instantiate(_questPointConfirmDialogOriginal);
		questPointConfirmDialog.CreateQuestConfirmDialog(ShowUnreceivedRewardCount);
		dialogBase.SetObj(questPointConfirmDialog.gameObject);
		dialogBase.SetLayer("Loading");
		dialogBase.OnClose = delegate
		{
			questPointConfirmDialog.OnCloseQuestConfirmnDialog();
		};
		dialogBase.SetPanelDepth(1);
	}

	private void OnClickBonusDetailButton(GameObject g)
	{

		if (string.IsNullOrEmpty(_announceId))
		{
			SystemText systemText = Data.SystemText;
			DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
			dialogBase.SetSize(DialogBase.Size.M);
			dialogBase.SetTitleLabel(systemText.Get("Quest_0033"));
			dialogBase.SetText(systemText.Get("Quest_0034"));
			dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		}
		else
		{
			UIManager.GetInstance().WebViewHelper.OpenAnnounceWebView(_announceId);
			DialogBase webViewDialog = UIManager.GetInstance().WebViewHelper.WebViewDialog;
			if (webViewDialog != null)
			{
				_spriteAtlasOverwriter.AddExceptionObjects(new List<UISpriteAtlasOverwriter.TargetObject>
				{
					new UISpriteAtlasOverwriter.TargetObject(webViewDialog.gameObject, includeChildren: true)
				});
			}
		}
	}

	private void ShowUnreceivedRewardCount(int count)
	{
		_rewardRecieveNumberLabel.gameObject.SetActive(count > 0);
		_rewardRecieveNumberLabel.text = count.ToString();
		if (count > 99)
		{
			_rewardRecieveNumberLabel.text = 99 + "+";
		}
		else
		{
			_rewardRecieveNumberLabel.text = count.ToString();
		}
	}

	private void InitLastUsedDeckSaveData(int questId)
	{
		QuestLastUsedDeckSaveDataManager questLastUsedDeckSaveDataManager = new QuestLastUsedDeckSaveDataManager();
		if (questLastUsedDeckSaveDataManager.QuestId != questId)
		{
			questLastUsedDeckSaveDataManager.DeleteAll();
			PlayerPrefsWrapper.SetValue(PlayerPrefsWrapper.BOSSRUSH_LAST_USED_DECK_INFO, string.Empty);
			questLastUsedDeckSaveDataManager.SaveQuestId(questId);
		}
	}

	private void CreatePuzzleQuestSelectDialog()
	{
		StartCoroutine(PuzzleUtil.OpenPuzzleSelectDialogCoroutine(OnDecidePuzzleQuest, OnOpenPuzzleSelectDialog, OnClosePuzzleSelectDialog));
	}

	private void OnDecidePuzzleQuest(PuzzleQuestData data, int difficulty)
	{
		// Pre-Phase-5b: headless has no QuestFirstSelectType write
		PuzzleUtil.SetPuzzleQuestData(data, difficulty, DataMgr.BattleType.Quest);
		PuzzleUtil.ChangeSceneToPuzzleQuest(data);
	}

	private void OnOpenPuzzleSelectDialog(QuestOpenPuzzleDialogTask task)
	{
		_puzzleQuestInfo = task.PuzzleQuestInfo;
		StartCoroutine(LoadPuzzleResourcesCoroutine());
	}

	private void OnClosePuzzleSelectDialog(QuestOpenPuzzleDialogTask task)
	{
		UIManager.GetInstance()._Footer.UpdateQuestBadgeIcon(task.IsDisplayBadge);
		UpdatePuzzleQuestUI();
	}

	private IEnumerator LoadPuzzleResourcesCoroutine()
	{
		UIManager.GetInstance().createInSceneCenterLoading();
		List<string> list = CollectPuzzleResourcePaths().Except(_loadPathList).ToList();
		_loadPathList.AddRange(list);
		yield return Toolbox.ResourcesManager.LoadAssetGroupAsync(list, null);
		UIManager.GetInstance().closeInSceneCenterLoading();
	}

	private void UpdatePuzzleQuestUI()
	{
		foreach (QuestSelectionButtonBase selectionButton in _selectionButtonList)
		{
			QuestPuzzleSelectionButton questPuzzleSelectionButton = selectionButton as QuestPuzzleSelectionButton;
			if (questPuzzleSelectionButton != null)
			{
				questPuzzleSelectionButton.InitializePuzzleQuest(_puzzleQuestInfo);
			}
		}
		ChangeChara(_puzzleQuestInfo.CharaId, isPlayChangeAnimation: false);
	}

	private void OnClickTweetBanner()
	{

		QuestCampaignDialog.Create(null, OnTweet);
	}

	private void OnTweet()
	{
		_isTweetFinish = true;
		UpdateTweetButtonVisible(isSelectBeginner: false);
	}
}
