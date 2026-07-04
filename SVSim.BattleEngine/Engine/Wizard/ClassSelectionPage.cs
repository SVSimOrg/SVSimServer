using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Convention;
using Cute;
using UnityEngine;
using Wizard.DeckCardEdit;
using Wizard.Story;

namespace Wizard;

public class ClassSelectionPage : UIBase
{
	public enum eMode
	{
		StorySelect,
		PracticeSelect,
		DeckEdit
	}

	[SerializeField]
	private ClassInfoParts _selectClassInfo;

	[SerializeField]
	private UILabel _storyProgressLabel;

	[SerializeField]
	private UILabel _storyAnnotationLabel;

	[SerializeField]
	private UILabel _selectClassDescriptionLabel;

	[SerializeField]
	private UITexture _selectCharaTexture;

	[SerializeField]
	private UISprite _selectMarkSprite;

	[SerializeField]
	private UIGrid _classButtonGrid;

	[SerializeField]
	private ClassSelectionButton _classButtonParts;

	[SerializeField]
	private UIButton _decideButton;

	[SerializeField]
	private GameObject _effectDecideButton;

	[SerializeField]
	private GameObject _selectMarkMainClass;

	[SerializeField]
	private GameObject _selectMarkSubClass;

	[SerializeField]
	private Vector3 _charaMoveStartPos = new Vector3(252f, -50f, 0f);

	[SerializeField]
	private Vector3 _charaMoveEndPos = new Vector3(282f, -50f, 0f);

	[SerializeField]
	private iTween.EaseType _charaMoveEaseType = iTween.EaseType.linear;

	[SerializeField]
	private float _charaMoveTime = 0.1f;

	[SerializeField]
	private GameObject _useSubClassMessageObj;

	private List<ClassCharacterMasterData> _classCharacterMasterDatas = new List<ClassCharacterMasterData>();

	private List<string> _loadPathList = new List<string>();

	private ClassCharacterMasterData _selectCharaMasterData;

	private CardBasePrm.ClanType _selectMainClass = CardBasePrm.ClanType.NONE;

	private CardBasePrm.ClanType _selectSubClass = CardBasePrm.ClanType.NONE;

	private CardBasePrm.ClanType _displayClass = CardBasePrm.ClanType.NONE;

	private List<int> _notificationIconClassList = new List<int>();

	private List<ClassSelectionButton> _classSelectionButtonList = new List<ClassSelectionButton>();

	private string _lastChapterClearTextId;

	private ClassSelectionPageParam SceneParam => UIManager.GetInstance().GetSceneParam<ClassSelectionPageParam>(UIManager.ViewScene.ClassSelectionPage);

	private bool _useSubClass => FormatBehaviorManager.GetDefaultBehaviour(SceneParam.Format).UseSubClass;

	public eMode Mode => SceneParam.Mode;

	public List<int> UsedClassIdList => SceneParam.UsedClassIdList;

	private SelectedStoryInfo SelectedStoryInfo => Data.SelectedStoryInfo;

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
		if (SceneParam.Mode != eMode.DeckEdit || !DeckListUI.IsSpecialFormatPeriodError(SceneParam.Format))
		{
			_classButtonParts.gameObject.SetActive(value: false);
			InitTopBar();
			InitFooter();
			InitMessage();
			StartCoroutine(InitClassSelection());
		}
	}

	private void Final()
	{
		Toolbox.ResourcesManager.RemoveAssetGroup(_loadPathList);
		_loadPathList.Clear();
	}

	private void InitTopBar()
	{
		UIManager.ChangeViewSceneParam changeViewSceneParam = new UIManager.ChangeViewSceneParam();
		changeViewSceneParam.MyPageMenuIndex = 1;
		changeViewSceneParam.IsCutCardMotion = true;
		UIManager.GetInstance().RemoveNowSceneBackButtonParameter();
		if (Mode == eMode.PracticeSelect)
		{
			changeViewSceneParam.OnFinishChangeView = delegate
			{
				MyPageMenu.Instance.GoToPracticeTypeSelect();
			};
		}
		UIManager.GetInstance().CreateTopBar(base.gameObject, GetTopBarText(), UIManager.ViewScene.MyPage, MoneyDraw: false, changeViewSceneParam).gameObject.layer = LayerMask.NameToLayer("MyPage");
	}

	private string GetTopBarText()
	{
		SystemText systemText = Data.SystemText;
		switch (Mode)
		{
		case eMode.StorySelect:
			return systemText.Get("Story_0001");
		case eMode.PracticeSelect:
			return systemText.Get("Story_0016");
		case eMode.DeckEdit:
			if (!_useSubClass)
			{
				return systemText.Get("Story_0001");
			}
			return systemText.Get("Story_0078");
		default:
			return string.Empty;
		}
	}

	private void InitFooter()
	{
		UIManager instance = UIManager.GetInstance();
		switch (Mode)
		{
		case eMode.StorySelect:
			instance.setBackScene(base.gameObject, UIManager.ViewScene.StorySelectionWorld);
			instance._Footer.UpdateCurrentIndex(1);
			instance._Footer.UpdateSoloPlayBadgeIcon();
			break;
		case eMode.PracticeSelect:
			instance.setBackScene(base.gameObject, UIManager.ViewScene.MyPage);
			instance._Footer.UpdateCurrentIndex(1);
			break;
		case eMode.DeckEdit:
			if (!Offline.IsConventionMode)
			{
				instance._Footer.UpdateCurrentIndex(4);
			}
			instance.setBackScene(base.gameObject, UIManager.ViewScene.DeckList);
			break;
		}
	}

	private void InitMessage()
	{
		_useSubClassMessageObj.SetActive(_useSubClass);
	}

	private IEnumerator InitClassSelection()
	{
		BaseTask task = null;
		switch (Mode)
		{
		case eMode.StorySelect:
			ResetSelectedStoryInfo();
			task = new StoryLeaderSelectTask(SelectedStoryInfo);
			break;
		case eMode.PracticeSelect:
			task = new PracticeInfoTask();
			break;
		}
		if (task != null)
		{
			StartCoroutine(Toolbox.NetworkManager.Connect(task));
			while (!task.isServerResultCodeOK())
			{
				yield return null;
			}
		}
		if (Mode == eMode.PracticeSelect)
		{
			_notificationIconClassList.AddRange(Data.PracticeDataMgr.CampaignClassIdList);
		}
		if (Mode == eMode.StorySelect)
		{
			_classCharacterMasterDatas = Data.StoryLeaderSelect.LeaderCharaIds.Select((int charaId) => GetCharaPrmByCharaId(charaId)).ToList();
		}
		else
		{
			// Pre-Phase-5b: chara master unavailable headless; leave the master list empty.
		}
		ResourcesManager resourcesManager = Toolbox.ResourcesManager;
		foreach (ClassCharacterMasterData classCharacterMasterData in _classCharacterMasterDatas)
		{
			_loadPathList.Add(resourcesManager.GetAssetTypePath(classCharacterMasterData.skin_id.ToString(), ResourcesManager.AssetLoadPathType.ClassCharaButton));
			_loadPathList.Add(resourcesManager.GetAssetTypePath(classCharacterMasterData.skin_id.ToString(), ResourcesManager.AssetLoadPathType.ClassCharaBase));
		}
		_loadPathList.Add(resourcesManager.GetAssetTypePath("empty", ResourcesManager.AssetLoadPathType.ClassCharaButton));
		yield return StartCoroutine(resourcesManager.LoadAssetGroupAsync(_loadPathList, null));
		CreateClassButton();
		UIEventListener.Get(_decideButton.gameObject).onClick = OnDecideButtonClick;
		SetActiveDecideButton(!_useSubClass);
		UIManager.GetInstance().OnReadyViewScene(isFadein: true, null, OnFinishFadeIn);
	}

	private void SetActiveDecideButton(bool isActive)
	{
		UIManager.SetObjectToGrey(_decideButton.gameObject, !isActive);
		_effectDecideButton.SetActive(isActive);
	}

	private void SetUnselectedButtonGrayout(bool isGrayout)
	{
		List<int> usedClass = new List<int>();
		if (UsedClassIdList != null)
		{
			usedClass.AddRange(UsedClassIdList);
		}
		_classSelectionButtonList.Where((ClassSelectionButton x) => !usedClass.Contains(x.ClassCharacterMasterData.class_id) && x.ClassCharacterMasterData.class_id != (int)_selectMainClass && x.ClassCharacterMasterData.class_id != (int)_selectSubClass).ToList().ForEach(delegate(ClassSelectionButton button)
		{
			UIManager.SetObjectToGrey(button.gameObject, isGrayout);
		});
	}

	private ClassCharacterMasterData GetCharaPrmByCharaId(int charaId)
	{
		return null; // Pre-Phase-5b: headless has no chara master
	}

	private void CreateClassButton()
	{
		ClassSelectionButton classButton = null;
		switch (Mode)
		{
		case eMode.PracticeSelect:
		case eMode.DeckEdit:
			classButton = CreateClassButtonPracticeAndDeckEdit();
			_storyAnnotationLabel.gameObject.SetActive(value: false);
			_storyProgressLabel.gameObject.SetActive(value: false);
			break;
		case eMode.StorySelect:
			classButton = CreateClassButtonStory();
			_storyAnnotationLabel.gameObject.SetActive(Data.StoryLeaderSelect.DataList.Count < Data.StoryLeaderSelect.LeaderCount);
			break;
		}
		_classButtonGrid.Reposition();
		_selectMarkSprite.gameObject.SetActive(!_useSubClass);
		_selectMarkMainClass.SetActive(value: false);
		_selectMarkSubClass.SetActive(value: false);
		SelectClass(classButton);
	}

	private ClassSelectionButton CreateClassButtonPracticeAndDeckEdit()
	{
		ResourcesManager resourcesManager = Toolbox.ResourcesManager;
		ClassSelectionButton classSelectionButton = null;
		foreach (ClassCharacterMasterData classCharacterMasterData in _classCharacterMasterDatas)
		{
			bool flag = UsedClassIdList.Contains(classCharacterMasterData.class_id);
			GameObject gameObject = NGUITools.AddChild(_classButtonGrid.gameObject, _classButtonParts.gameObject);
			_classSelectionButtonList.Add(gameObject.GetComponent<ClassSelectionButton>());
			gameObject.SetActive(value: true);
			ClassSelectionButton component = gameObject.GetComponent<ClassSelectionButton>();
			component.Init(classCharacterMasterData, resourcesManager.LoadObject<Texture>(resourcesManager.GetAssetTypePath(classCharacterMasterData.skin_id.ToString(), ResourcesManager.AssetLoadPathType.ClassCharaButton, isfetch: true)), OnClickClassButton, isShowStoryClearLabel: false, flag, _notificationIconClassList.Contains(classCharacterMasterData.class_id));
			if (classSelectionButton == null && !flag)
			{
				classSelectionButton = component;
			}
		}
		return classSelectionButton;
	}

	private ClassSelectionButton CreateClassButtonStory()
	{
		ResourcesManager resourcesManager = Toolbox.ResourcesManager;
		ClassSelectionButton classSelectionButton = null;
		int i = 0;
		for (int leaderCount = Data.StoryLeaderSelect.LeaderCount; i < leaderCount; i++)
		{
			GameObject obj = NGUITools.AddChild(_classButtonGrid.gameObject, _classButtonParts.gameObject);
			obj.SetActive(value: true);
			ClassSelectionButton component = obj.GetComponent<ClassSelectionButton>();
			if (i < _classCharacterMasterDatas.Count)
			{
				ClassCharacterMasterData classCharacterMasterData = _classCharacterMasterDatas[i];
				StoryLeaderSelectData storyLeaderSelectDataByClassID = GetStoryLeaderSelectDataByClassID(classCharacterMasterData.class_id);
				component.Init(classCharacterMasterData, resourcesManager.LoadObject<Texture>(resourcesManager.GetAssetTypePath(classCharacterMasterData.skin_id.ToString(), ResourcesManager.AssetLoadPathType.ClassCharaButton, isfetch: true)), OnClickClassButton, storyLeaderSelectDataByClassID.IsFinished, isShowUsedLabel: false, showNotificationIcon: false);
				if (classSelectionButton == null)
				{
					classSelectionButton = component;
				}
			}
			else
			{
				component.InitEmpty();
			}
		}
		return classSelectionButton;
	}

	private void OnClickClassButton(ClassSelectionButton classButton)
	{

		if (_useSubClass)
		{
			SelectClassForUseSubClass(classButton);
		}
		else
		{
			SelectClass(classButton);
		}
	}

	private void SelectClass(ClassSelectionButton classButton)
	{
		if (_selectCharaMasterData == classButton.ClassCharacterMasterData)
		{
			return;
		}
		_selectCharaMasterData = classButton.ClassCharacterMasterData;
		_selectMarkSprite.transform.position = classButton.transform.position;
		ViewClassCharaInfo(_selectCharaMasterData);
		if (Mode == eMode.StorySelect && !GetStoryLeaderSelectDataByClassID(_selectCharaMasterData.class_id).IsFinished)
		{
			string currentChapter = GetStoryLeaderSelectDataByClassID(_selectCharaMasterData.class_id).CurrentChapter;
			string text = UIUtil.ExtractStringNumber(currentChapter).ToString();
			string value = UIUtil.ExtractStringAlphabet(currentChapter);
			string text2 = "";
			if (!text.Equals(currentChapter))
			{
				StringBuilder tempStringBuilder = UIUtil.GetTempStringBuilder();
				tempStringBuilder.Append("Route_");
				tempStringBuilder.Append(SelectedStoryInfo.SectionId.ToString()).Append("_");
				tempStringBuilder.Append(_selectCharaMasterData.chara_id.ToString()).Append("_");
				tempStringBuilder.Append(value);
				text2 = Data.SystemText.Get(tempStringBuilder.ToString()) + " " + text;
			}
			else
			{
				text2 = currentChapter;
			}
			_storyProgressLabel.text = Data.SystemText.Get("Story_0050", text2.ToString());
			_storyProgressLabel.gameObject.SetActive(value: true);
		}
		else
		{
			_storyProgressLabel.gameObject.SetActive(value: false);
		}
	}

	private void SelectClassForUseSubClass(ClassSelectionButton classButton)
	{
		CardBasePrm.ClanType clan = classButton.ClassCharacterMasterData.clan;
		if (_selectMainClass == clan)
		{
			_selectMarkMainClass.gameObject.SetActive(value: false);
			_selectMainClass = CardBasePrm.ClanType.NONE;
			SetUnselectedButtonGrayout(isGrayout: false);
			SetActiveDecideButton(isActive: false);
		}
		else if (_selectSubClass == clan)
		{
			_selectMarkSubClass.gameObject.SetActive(value: false);
			_selectSubClass = CardBasePrm.ClanType.NONE;
			SetActiveDecideButton(isActive: false);
			SetUnselectedButtonGrayout(isGrayout: false);
		}
		else if (!IsCompleteSelectClass())
		{
			if (_selectMainClass == CardBasePrm.ClanType.NONE)
			{
				_selectMainClass = clan;
				_selectMarkMainClass.transform.position = classButton.transform.position;
				_selectMarkMainClass.gameObject.SetActive(value: true);
			}
			else if (_selectSubClass == CardBasePrm.ClanType.NONE)
			{
				_selectSubClass = clan;
				_selectMarkSubClass.transform.position = classButton.transform.position;
				_selectMarkSubClass.gameObject.SetActive(value: true);
			}
			ViewClassCharaInfo(classButton.ClassCharacterMasterData);
			SetActiveDecideButton(IsCompleteSelectClass());
			SetUnselectedButtonGrayout(IsCompleteSelectClass());
		}
		bool IsCompleteSelectClass()
		{
			if (_selectMainClass != CardBasePrm.ClanType.NONE)
			{
				return _selectSubClass != CardBasePrm.ClanType.NONE;
			}
			return false;
		}
	}

	private void ViewClassCharaInfo(ClassCharacterMasterData charaData)
	{
		if (charaData.clan != _displayClass)
		{
			_displayClass = charaData.clan;
			_selectClassInfo.InitByCharaPrm(charaData);
			_selectClassDescriptionLabel.SetWrapText(Data.SystemText.Get("Class_Selection_" + charaData.class_id.ToString("D4")));
			ResourcesManager resourcesManager = Toolbox.ResourcesManager;
			_selectCharaTexture.mainTexture = resourcesManager.LoadObject<Texture>(resourcesManager.GetAssetTypePath(charaData.skin_id.ToString(), ResourcesManager.AssetLoadPathType.ClassCharaBase, isfetch: true));
			GameObject obj = _selectCharaTexture.gameObject;
			iTween.Stop(obj);
			obj.transform.localPosition = _charaMoveStartPos;
			iTween.MoveTo(obj, iTween.Hash("position", _charaMoveEndPos, "time", _charaMoveTime, "islocal", true, "easetype", _charaMoveEaseType));
		}
	}

	private void OnDecideButtonClick(GameObject g)
	{
		switch (Mode)
		{
		case eMode.StorySelect:
			SelectedStoryInfo.SetSectionChara(_selectCharaMasterData);

			UIManager.GetInstance().ChangeViewScene(SelectedStoryInfo.ChapterSelectionView);
			break;
		case eMode.PracticeSelect:

			ShowSelectDifficultyDialog();
			break;
		case eMode.DeckEdit:
			OnDecideButtonDeckEditMode();
			break;
		}
	}

	private void OnDecideButtonDeckEditMode()
	{
		if (SceneParam.Format == Format.MyRotation)
		{

			MyRotationPeriodSelectDialog.Create(null, (CardBasePrm.ClanType)_selectCharaMasterData.class_id, delegate(MyRotationInfo myRotationData)
			{
				OnDecideClassDeckEditMode(myRotationData);
			});
		}
		else
		{

			OnDecideClassDeckEditMode(null);
		}
	}

	private void OnDecideClassDeckEditMode(MyRotationInfo myRotationInfo)
	{
		if (_useSubClass)
		{
			DeckCardEditUI.ClassSet = new ClassSet(_selectMainClass, _selectSubClass);
		}
		else
		{
			DeckCardEditUI.ClassSet = new ClassSet((CardBasePrm.ClanType)_selectCharaMasterData.class_id);
		}
		DeckCardEditUI.MyRotationInfo = myRotationInfo;
		UIManager.ChangeViewSceneParam changeViewSceneParam = new UIManager.ChangeViewSceneParam();
		if (!Offline.IsConventionMode)
		{
			changeViewSceneParam.IsUpdateFooterMenuTexture = true;
		}
		changeViewSceneParam.OnChange = delegate
		{
			UIManager.GetInstance().GetUiBaseOfCurrentScene();
		};
		UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.DeckCardEdit, changeViewSceneParam);
	}

	private StoryLeaderSelectData GetStoryLeaderSelectDataByClassID(int classId)
	{
		return Data.StoryLeaderSelect.DataList.Find((StoryLeaderSelectData data) => data.ClassId == classId);
	}

	private void ShowSelectDifficultyDialog()
	{
		int enemyClassId = _selectCharaMasterData.class_id;
		List<PracticeData> practiceDataList = Data.PracticeDataMgr.GetClassDataList(enemyClassId);
		if (practiceDataList.Count <= 0)
		{
			return;
		}
		int num = -1;
		List<string> list = new List<string>();
		for (int i = 0; i < practiceDataList.Count; i++)
		{
			list.Add(practiceDataList[i].Text);
			if (num < 0 && !practiceDataList[i].IsMaintenance)
			{
				num = i;
			}
		}
		if (num < 0)
		{
			num = 0;
		}
		DialogBase dia = null;
		int selectIndex = num;
		Action<int> selectCallback = delegate(int selectIdx)
		{
			selectIndex = selectIdx;
			UIManager.SetObjectToGrey(dia.button1.gameObject, practiceDataList[selectIndex].IsMaintenance);
		};
		dia = DrumrollDialog.Create(list, num, selectCallback);
		dia.SetTitleLabel(Data.SystemText.Get("Story_0022"));
		dia.SetButtonLayout(DialogBase.ButtonLayout.DecisionBtn);
		dia.onPushButton1 = delegate
		{
			// Pre-Phase-5b: kicked off practice-battle setup via DataMgr writes + PracticeStartTask.
			// Headless never runs practice UI; stub to close the dialog and change scene directly.
			dia.CloseWithoutSelect();
			UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.Battle);
		};
		dia.ClickSe_Btn1 = 0;
	}

	private void OnFinishFadeIn()
	{
		if (Mode == eMode.StorySelect && _lastChapterClearTextId != null)
		{
			SystemText systemText = Data.SystemText;
			DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
			dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
			dialogBase.SetTitleLabel(systemText.Get("Common_0021"));
			dialogBase.SetButtonText(systemText.Get("Common_0004"));
			dialogBase.SetText(systemText.Get(_lastChapterClearTextId));
			_lastChapterClearTextId = null;
		}
	}

	private void ResetSelectedStoryInfo()
	{
		_lastChapterClearTextId = SelectedStoryInfo.LastChapterClearTextId;
		SelectedStoryInfo.ClearInfoForLeaderSelectionScene();
	}
}
