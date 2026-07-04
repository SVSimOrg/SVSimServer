using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.ErrorDialog;

public class Mail : UIBase
{
	private enum TAB_TYPE
	{
		NONE,
		GIFT,
		HISTORY
	}

	private enum MAIL_ACTION_TYPE
	{
		NONE,
		READ,
		READALL
	}

	[SerializeField]
	private NguiObjs HistoryButton;

	[SerializeField]
	private NguiObjs GiftButton;

	[SerializeField]
	private UILabel GiftInfoLabel;

	[SerializeField]
	private UIButton _giftTabButton;

	[SerializeField]
	private UIButton _historyTabButton;

	[SerializeField]
	private UIButton _allReceiveButton;

	[SerializeField]
	private UILabel MailEmptyLabel;

	[SerializeField]
	private UIWrapContentWizard WrapContent;

	[SerializeField]
	private GameObject MailTemplate;

	[SerializeField]
	private NguiObjs ReadAllMailButton;

	[SerializeField]
	private GameObject MailReceive;

	[SerializeField]
	private UIScrollBarWrapContent ScrollBar;

	[SerializeField]
	private WrapContentsScrollBarSize WrapScrollbar;

	[SerializeField]
	private UIScrollView ScrollView;

	private List<GameObject> _scrollItems = new List<GameObject>();

	private int _readMailID;

	private List<MailData> _currentList;

	private TopBar _topBar;

	private TAB_TYPE _tabType;

	private MAIL_ACTION_TYPE _mailActionType = MAIL_ACTION_TYPE.READ;

	private int _lastHistoryCount;

	private List<string> _assetList = new List<string>();

	private ResourceHandler _resourceHandler;

	private bool IsTutorial => Wizard.Data.Load.data._userTutorial.TutorialStep != 100;

	private void SetLanguage()
	{
		SystemText systemText = Wizard.Data.SystemText;
		HistoryButton.labels[0].text = systemText.Get("Mail_0024");
		GiftButton.labels[0].text = systemText.Get("Mail_0002");
		ReadAllMailButton.labels[0].text = systemText.Get("Mail_0004");
		MailEmptyLabel.text = systemText.Get("Mail_0006");
	}

	public override void onFirstStart()
	{
		base.IsShowFooterMenu = true;
		base.onFirstStart();
		_topBar = UIManager.GetInstance().CreateTopBar(base.gameObject, Wizard.Data.SystemText.Get("Mail_0002"), UIManager.ViewScene.MyPage);
		SetLanguage();
		WrapContent.EnableNoLimit = false;
		WrapContent.onInitializeItem = InitScrollItem;
		ScrollBar.m_WrapContents = WrapContent;
		HistoryButton.buttons[0].onClick.Clear();
		HistoryButton.buttons[0].onClick.Add(new EventDelegate(delegate
		{

			GiftInfoLabel.text = Wizard.Data.SystemText.Get("Mail_0028", 100.ToString());
			ChangeHistory();
		}));
		GiftButton.buttons[0].onClick.Clear();
		GiftButton.buttons[0].onClick.Add(new EventDelegate(delegate
		{

			GiftInfoLabel.text = Wizard.Data.SystemText.Get("Mail_0027", 100.ToString());
			ChangeGift();
		}));
		UIEventListener.Get(ReadAllMailButton.gameObject).onClick = OnReadAllMail;
		ChangeGift();
		UIManager.GetInstance().SetLayerRecursive(base.transform, LayerMask.NameToLayer("MyPage"));
		_resourceHandler = base.gameObject.AddMissingComponent<ResourceHandler>();
		if (IsTutorial)
		{
			SetTutorialMode();
			ShowTutorialDialog();
			LoadTutorialResource();
		}
	}

	private void LoadTutorialResource()
	{
		// Pre-Phase-5b: async-loaded EffectTutorialData JSON via EffectMgr and paired the
		// UIManager view-change lock. Headless has no EffectMgr; the increment/decrement
		// pair collapses cleanly since headless never triggers view changes here.
		UIManager uiManager = UIManager.GetInstance();
		uiManager.Force_Increment_LockCountChangeView();
		uiManager.Force_Decrement_LockCountChangeView();
	}

	private void ShowTutorialDialog()
	{
		DialogBase dialogBase = MyPageMenu.CreateDialogForTutorial();
		dialogBase.SetText(Wizard.Data.SystemText.Get("Tutorial_0011"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		MyPageMenu.Instance.SetGuideToOkOnlyDialog(dialogBase);
		dialogBase.OnClose = delegate
		{
			MyPageMenu.Instance.SetGuideEffect(_allReceiveButton.transform, Vector3.zero, 0f);
		};
	}

	private void SetTutorialMode()
	{
		UIManager.SetObjectToGrey(_historyTabButton.gameObject, b: true);
		UIManager.SetObjectToGrey(_giftTabButton.gameObject, b: true);
		_topBar.SetBackButtonEnable(enable: false);
		UIManager.SetObjectToGrey(_topBar.BuyCrystalButton.gameObject, b: true);
		_topBar.BuyCrystalButton.isEnabled = false;
	}

	protected override void onOpen()
	{
		base.onOpen();
		_currentList = Wizard.Data.MailTop.data.mail_data_list;
		ResetScrollWrap();
		_tabType = TAB_TYPE.NONE;
		ChangeGift();
		GiftInfoLabel.text = Wizard.Data.SystemText.Get("Mail_0027", 100.ToString());
		if (Wizard.Data.MailTop.data.limitOverPresentDeleted)
		{
			DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
			dialogBase.SetTitleLabel(Wizard.Data.SystemText.Get("Mail_0066"));
			dialogBase.SetText(Wizard.Data.SystemText.Get("Mail_0067"));
			dialogBase.SetSize(DialogBase.Size.S);
			dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		}
		UIManager.GetInstance().OnReadyViewScene(isFadein: true);
	}

	private void ResetScrollWrap()
	{
		CreateItems(_currentList.Count);
		WrapContent.minIndex = -(_currentList.Count - 1);
		WrapContent.maxIndex = 0;
		WrapScrollbar.ContentUpdate();
		WrapContent.SortBasedOnScrollMovement();
		ScrollView.ResetPosition();
		ScrollBar.gameObject.SetActive(value: true);
		ScrollView.UpdateScrollbars();
	}

	private void InitScrollItem(GameObject obj, int wrapIndex, int realIndex)
	{
		GameObject gameObject = obj.transform.GetChild(0).gameObject;
		if (-realIndex < 0 || -realIndex >= _currentList.Count)
		{
			gameObject.SetActive(value: false);
			return;
		}
		gameObject.SetActive(value: true);
		int num = -realIndex;
		MailData mailData = _currentList[num];
		if (_tabType == TAB_TYPE.GIFT)
		{
			SetMailData(gameObject, mailData);
		}
		else if (_tabType == TAB_TYPE.HISTORY)
		{
			SetHistoryData(gameObject, mailData);
		}
		if (num < _currentList.Count - 1)
		{
			return;
		}
		if (_tabType == TAB_TYPE.GIFT)
		{
			// Pre-Phase-5b: reached MailTopTask.LastPageRead to decide whether to fetch the
			// next mail page. Headless has no MailTopTask; skip the mail-fetch branch entirely.
		}
		else if (_tabType == TAB_TYPE.HISTORY)
		{
			int count = Wizard.Data.MailTop.data.mail_history_list.Count;
			if (count >= 100 && count < 500 && _lastHistoryCount < 500)
			{
				LoadNextPage();
			}
		}
	}

	private void SetMailData(GameObject item, MailData mailData)
	{
		item.name = mailData.mail_id.ToString();
		AchievementWindowBase component = item.GetComponent<AchievementWindowBase>();
		component.SetMail(mailData, OnReadMail, _resourceHandler);
		if (IsTutorial)
		{
			component.SetGetButtonToGreyOut();
		}
	}

	public override bool IsUseCommonBackground()
	{
		return true;
	}

	private void ChangeHistory()
	{
		if (_tabType != TAB_TYPE.HISTORY)
		{
			_tabType = TAB_TYPE.HISTORY;
			_currentList = Wizard.Data.MailTop.data.mail_history_list;
			_lastHistoryCount = _currentList.Count;
			ReadAllMailButton.gameObject.SetActive(value: false);
			MailEmptyLabel.gameObject.SetActive(_currentList.Count == 0);
			MailEmptyLabel.text = Wizard.Data.SystemText.Get("Mail_0029");
			HistoryButton.buttons[0].isEnabled = false;
			GiftButton.buttons[0].isEnabled = true;
			ResetScrollWrap();
		}
	}

	private void ChangeGift()
	{
		if (_tabType != TAB_TYPE.GIFT)
		{
			_tabType = TAB_TYPE.GIFT;
			_currentList = Wizard.Data.MailTop.data.mail_data_list;
			ReadAllMailButton.gameObject.SetActive(value: true);
			bool flag = _currentList.Count == 0;
			UIManager.SetObjectToGrey(ReadAllMailButton.gameObject, flag);
			MailEmptyLabel.gameObject.SetActive(flag);
			MailEmptyLabel.text = Wizard.Data.SystemText.Get("Mail_0006");
			HistoryButton.buttons[0].isEnabled = true;
			GiftButton.buttons[0].isEnabled = false;
			ResetScrollWrap();
		}
	}

	private void CreateItems(int requiredCount)
	{
		for (int i = 0; i < _scrollItems.Count; i++)
		{
			UnityEngine.Object.DestroyImmediate(_scrollItems[i]);
		}
		_scrollItems.Clear();
		requiredCount = Mathf.Min(requiredCount, 5);
		for (int j = 0; j < requiredCount; j++)
		{
			GameObject gameObject = NGUITools.AddChild(WrapContent.gameObject);
			NGUITools.AddChild(gameObject, MailTemplate);
			gameObject.SetActive(value: true);
			_scrollItems.Add(gameObject);
		}
	}

	private void OpenReadAllDialog(GameObject g)
	{
		SystemText systemText = Wizard.Data.SystemText;
		DialogBase dialogBase = (IsTutorial ? MyPageMenu.CreateDialogForTutorial() : UIManager.GetInstance().CreateDialogClose());
		dialogBase.SetTitleLabel(systemText.Get("Mail_0011"));
		dialogBase.SetText(systemText.Get("Mail_0017"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueBtn_CancelBtn);
		dialogBase.SetButtonText(systemText.Get("Dia_Gift_001_Button"));
		dialogBase.onPushButton1 = StartReadRequest;
		if (IsTutorial)
		{
			dialogBase.Button2Grey = true;
			dialogBase.SetDialogNoClose();
			MyPageMenu.Instance.SetGuideToOkOnlyDialog(dialogBase);
		}
	}

	private void PrepareReceiveSingleMail(int mail_index, int mail_id)
	{
		_readMailID = mail_id;
	}

	private void StartReadRequest()
	{
		switch (_mailActionType)
		{
		case MAIL_ACTION_TYPE.READ:
		{
			UIManager.GetInstance().createInSceneCenterLoading();
			MailReadTask mailReadTask2 = new MailReadTask(1);
			mailReadTask2.SetParameter(new string[1] { _readMailID.ToString() }, 1, IsTutorial);
			StartCoroutine(Toolbox.NetworkManager.Connect(mailReadTask2, OnRequestMailRead, delegate(NetworkTask.ResultCode error)
			{
				UIManager.GetInstance().closeInSceneCenterLoading();
				BaseTask.OnRequestFailed(error);
			}, delegate(int error)
			{
				UIManager.GetInstance().closeInSceneCenterLoading();
				BaseTask.OnFailedErrorCode(error);
				CheckAndRemoveExpiredMail();
			}));
			break;
		}
		case MAIL_ACTION_TYPE.READALL:
		{
			UIManager.GetInstance().createInSceneCenterLoading();
			MailReadTask mailReadTask = new MailReadTask(1);
			int num = ((_currentList.Count > 100) ? 100 : _currentList.Count);
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				MailData mailData = _currentList[i];
				array[i] = mailData.mail_id.ToString();
			}
			mailReadTask.SetParameter(array, 1, IsTutorial);
			StartCoroutine(Toolbox.NetworkManager.Connect(mailReadTask, OnRequestMailRead, delegate(NetworkTask.ResultCode error)
			{
				UIManager.GetInstance().closeInSceneCenterLoading();
				BaseTask.OnRequestFailed(error);
			}, delegate(int error)
			{
				UIManager.GetInstance().closeInSceneCenterLoading();
				BaseTask.OnFailedErrorCode(error);
			}));
			break;
		}
		}
	}

	private void OnReadMail(int mail_index, int mail_id)
	{

		_mailActionType = MAIL_ACTION_TYPE.READ;
		PrepareReceiveSingleMail(mail_index, mail_id);
		StartReadRequest();
	}

	private void OnReadAllMail(GameObject g)
	{

		_mailActionType = MAIL_ACTION_TYPE.READALL;
		OpenReadAllDialog(g);
	}

	private void ShowReadDialog()
	{
		ReceiveReward receiveReward = base.gameObject.AddMissingComponent<ReceiveReward>();
		DialogBase dialogBase = receiveReward.ShowReadDialog(Wizard.Data.ReadMail.data.total_recieve_count_list, MailReceive, base.gameObject, _resourceHandler);
		if (IsTutorial)
		{
			MyPageMenu.Instance.SetGuideToOkOnlyDialog(dialogBase);
			receiveReward.SetAllButtonDisable();
			dialogBase.OnClose = delegate
			{
				ShowMoveToCardPackDialog();
			};
		}
	}

	private void ShowMoveToCardPackDialog()
	{
		SystemText systemText = Wizard.Data.SystemText;
		DialogBase dialogBase = MyPageMenu.CreateDialogForTutorial();
		dialogBase.SetText(systemText.Get("Tutorial_0012"));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		MyPageMenu.Instance.SetGuideToOkOnlyDialog(dialogBase);
		dialogBase.OnClose = delegate
		{
			Footer footer = UIManager.GetInstance()._Footer;
			for (int i = 0; i < footer._underButtons.Length; i++)
			{
				footer.SetButtonEnableColorChange(i, i == 5);
			}
			MyPageMenu.Instance.SetGuideEffect(footer._underButtons[5].transform, MyPageItemHome.TUTORIAL_OFFSET_FOOTER, 180f);
		};
	}

	private void OnRequestMailRead(NetworkTask.ResultCode error)
	{
		MyPageMenu.Instance.OnReadGift();
		_currentList = Wizard.Data.MailTop.data.mail_data_list;
		ResetScrollWrap();
		UIManager.GetInstance().closeInSceneCenterLoading();
		bool flag = _currentList.Count == 0;
		UIManager.SetObjectToGrey(ReadAllMailButton.gameObject, flag);
		MailEmptyLabel.gameObject.SetActive(flag);
		if (Wizard.Data.ReadMail.data.is_unreceived_present)
		{
			DialogBase dialogBase = Dialog.Create(1601);
			if (_mailActionType != MAIL_ACTION_TYPE.READ)
			{
				dialogBase.SetText(Wizard.Data.SystemText.Get("Mail_0049"));
			}
			if (Wizard.Data.ReadMail.data.total_recieve_count_list.Count > 0)
			{
				dialogBase.OnClose = ShowReadDialog;
			}
		}
		else if (Wizard.Data.ReadMail.data.total_recieve_count_list.Count == 0)
		{
			SystemText systemText = Wizard.Data.SystemText;
			DialogBase dialogBase2 = UIManager.GetInstance().CreateDialogClose();
			dialogBase2.CloseOnOff(flag: false);
			dialogBase2.SetSize(DialogBase.Size.M);
			dialogBase2.SetTitleLabel(systemText.Get("ErrorHeader_1601"));
			dialogBase2.SetText(systemText.Get("Mail_0053"));
			dialogBase2.SetButtonLayout(DialogBase.ButtonLayout.OkBtn);
		}
		else
		{
			ShowReadDialog();
		}
	}

	private void SetHistoryData(GameObject item, MailData mailData)
	{
		item.name = mailData.mail_id.ToString();
		item.GetComponent<AchievementWindowBase>().SetHistoryMail(mailData, _resourceHandler);
	}

	public static string GetTimeLeft(long seconds_since_unix)
	{
		// Pre-Phase-5b: computed remaining time using GameMgr's MailTopTask server-time delta.
		// Headless has no MailTopTask; return the "expires in N minutes" default with 0 minutes
		// to indicate no known time-remaining. TimeLeftUpdate is the only external caller and
		// only reads the returned string for UI label text.
		SystemText systemText = Wizard.Data.SystemText;
		return systemText.Get("Mail_0048", "0");
	}

	private void LoadNextPage()
	{
		// Pre-Phase-5b: fetched next-page mail via MailTopTask. Headless has no mail service;
		// method becomes a no-op (the scroll wrap paths above handle empty results cleanly).
	}

	protected override void onClose()
	{
		base.onClose();
		Toolbox.ResourcesManager.RemoveAssetGroup(_assetList);
		_assetList.Clear();
		_resourceHandler.UnloadAll();
	}

	private void CheckAndRemoveExpiredMail()
	{
		bool flag = false;
		for (int num = _currentList.Count - 1; num >= 0; num--)
		{
			MailData mailData = _currentList[num];
			if (mailData.limit_type == 1 && IsExpired(mailData.reward_limit_time))
			{
				_currentList.RemoveAt(num);
				flag = true;
			}
		}
		if (flag)
		{
			ResetScrollWrap();
			bool flag2 = _currentList.Count == 0;
			UIManager.SetObjectToGrey(ReadAllMailButton.gameObject, flag2);
			MailEmptyLabel.gameObject.SetActive(flag2);
		}
	}

	private static bool IsExpired(long seconds_since_unix)
	{
		// Pre-Phase-5b: computed expiration via MailTopTask server-time delta. Headless
		// has no MailTopTask; treat everything as not-yet-expired.
		return false;
	}
}
