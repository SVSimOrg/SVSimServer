using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard;

public class Mission : UIBase
{
	public enum MissionViewTab
	{
		NormalMission,
		Achievement,
		BattlePassMission
	}

	private static readonly Vector3 POS_SOLO_PLAY_MISSION_LABEL_CHANGE_ENABLE = new Vector3(990f, -31f, 0f);

	private static readonly Vector3 POS_SOLO_PLAY_MISSION_LABEL_CHANGE_DISABLE = new Vector3(990f, -21f, 0f);

	public static readonly int PERIOD_TEXT_SIZE_BIG = 21;

	[SerializeField]
	private UIButton MissionButton;

	[SerializeField]
	private UIButton AchievementButton;

	[SerializeField]
	private UIButton BattlePassMissionButton;

	[SerializeField]
	private UIButton ReceiveButton;

	[SerializeField]
	private UILabel _labelSoloPlayMssion;

	[SerializeField]
	private UIToggle _soloPlayMissionToggleUI;

	[SerializeField]
	private UIEventListener _soloPlayMissionEventListener;

	[SerializeField]
	private UILabel _soloPlayMssionChangeWaitTime;

	public GameObject goAchievementBaseTop;

	public GameObject goAchievementWindowBase;

	private GameObject[] scrollItems;

	private MissionViewTab _currentViewTab;

	private static MissionViewTab _transitionViewTab = MissionViewTab.NormalMission;

	[SerializeField]
	private UILabel LabelMission;

	[SerializeField]
	private UILabel LabelAchievement;

	[SerializeField]
	private UILabel AchievementTopText;

	[SerializeField]
	private UILabel _labelBgCenter;

	[SerializeField]
	private GameObject _topObjMission;

	[SerializeField]
	private UILabel _labelMissionChangeText;

	[SerializeField]
	private GameObject _mailReceive;

	[SerializeField]
	private UILabel AchievementCanReceiveCount;

	[SerializeField]
	private GameObject _battlePassMontlyMissionRoot;

	[SerializeField]
	private GameObject _missionAndAchievementRoot;

	[SerializeField]
	private UILabel _battlePassMontlyMissionPeriodLabel;

	[SerializeField]
	private SimpleScrollViewUI _battlePassMontlyMissionScrollView;

	[SerializeField]
	private UIButton _btnTransitionBattlePass;

	[SerializeField]
	private UILabel _labelNoBattlePassMission;

	public ResourceHandler Handler { get; private set; }

	public override void onFirstStart()
	{
		Handler = base.gameObject.AddMissingComponent<ResourceHandler>();
		base.IsShowFooterMenu = true;
		base.onFirstStart();
		SystemText systemText = Data.SystemText;
		UIManager.GetInstance().CreateTopBar(base.gameObject, systemText.Get("Mission_0001"), UIManager.ViewScene.MyPage);
		LabelMission.text = systemText.Get("Mission_0001");
		LabelAchievement.text = systemText.Get("Mission_0002");
		MissionButton.onClick.Clear();
		MissionButton.onClick.Add(new EventDelegate(delegate
		{
			OnPushTabButton(MissionViewTab.NormalMission);
		}));
		AchievementButton.onClick.Clear();
		AchievementButton.onClick.Add(new EventDelegate(delegate
		{
			OnPushTabButton(MissionViewTab.Achievement);
		}));
		BattlePassMissionButton.onClick.Clear();
		BattlePassMissionButton.onClick.Add(new EventDelegate(delegate
		{
			OnPushTabButton(MissionViewTab.BattlePassMission);
		}));
		_btnTransitionBattlePass.onClick.Clear();
		_btnTransitionBattlePass.onClick.Add(new EventDelegate(delegate
		{
			UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.BattlePass);

		}));
		UIEventListener uIEventListener = UIEventListener.Get(_soloPlayMissionEventListener.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			CreateChangeReceiveTypeConfirmDialog();
		});
		UIManager.GetInstance().SetLayerRecursive(base.transform, LayerMask.NameToLayer("MyPage"));
	}

	private void OnPushTabButton(MissionViewTab viewTab)
	{
		if (_currentViewTab != viewTab)
		{
			UpdateMissionView(viewTab);

		}
	}

	private void CreateChangeReceiveTypeConfirmDialog()
	{
		MissionInfoDetail.eMissionReceiveType receiveType;
		string text;
		if (Data.MissionInfo.data._missionReceiveType == MissionInfoDetail.eMissionReceiveType.normal)
		{

			receiveType = MissionInfoDetail.eMissionReceiveType.solo;
			text = Data.SystemText.Get("Mission_0083");
		}
		else
		{

			receiveType = MissionInfoDetail.eMissionReceiveType.normal;
			text = Data.SystemText.Get("Mission_0084");
		}
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(Data.SystemText.Get("Mission_0082"));
		dialogBase.SetText(text);
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.DecisionBtn);
		dialogBase.SetButtonText(Data.SystemText.Get("Mission_0085"));
		dialogBase.onPushButton1 = delegate
		{
			MissionChangeReceiveSettingTask missionChangeReceiveSettingTask = new MissionChangeReceiveSettingTask();
			missionChangeReceiveSettingTask.SetParameter(receiveType);
			StartCoroutine(Toolbox.NetworkManager.Connect(missionChangeReceiveSettingTask, delegate
			{
				UpdateMissionView(MissionViewTab.NormalMission);
			}));
		};
	}

	private void SetMissionTypeToggleBtn(MissionInfoDetail.eMissionReceiveType type)
	{
		_soloPlayMissionToggleUI.value = type == MissionInfoDetail.eMissionReceiveType.solo;
		bool flag = Data.MissionInfo.data.CanChangeReceiveType;
		TimeSpan timeSpan = default(TimeSpan);
		if (!flag)
		{
			timeSpan = TimeSpan.FromSeconds(Data.MissionInfo.data.WaitTimeCanChangeReceiveType - 0L /* headless UI stub: no mission time */);
			flag = timeSpan.TotalSeconds < -10.0;
		}
		_soloPlayMssionChangeWaitTime.gameObject.SetActive(!flag);
		UIManager.SetObjectToGrey(_soloPlayMissionEventListener.gameObject, !flag);
		_soloPlayMissionToggleUI.activeSprite.alpha = (_soloPlayMissionToggleUI.value ? 1f : 0f);
		_labelSoloPlayMssion.transform.localPosition = (flag ? POS_SOLO_PLAY_MISSION_LABEL_CHANGE_ENABLE : POS_SOLO_PLAY_MISSION_LABEL_CHANGE_DISABLE);
		if (!flag)
		{
			int num = timeSpan.Hours;
			int num2 = timeSpan.Minutes;
			if (num <= 0 && num2 <= 0)
			{
				num = 0;
				num2 = 1;
			}
			_soloPlayMssionChangeWaitTime.text = Data.SystemText.Get("Mission_0081", num.ToString("00"), num2.ToString("00"));
		}
	}

	public static void ChangeViewSceneTransitionMissionViewTab(MissionViewTab viewTab)
	{
		_transitionViewTab = viewTab;
		UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.Mission);
	}

	protected override void onOpen()
	{
		base.onOpen();
		if (_currentViewTab != _transitionViewTab)
		{
			_currentViewTab = _transitionViewTab;
			ResetTransitionViewTab();
		}
		bool flag = Data.MaintenanceCodeList.Contains(NetworkDefine.MAINTENANCE_TYPE.BATTLE_PASS);
		UIManager.SetObjectToGrey(BattlePassMissionButton.gameObject, flag);
		if (flag && _currentViewTab == MissionViewTab.BattlePassMission)
		{
			_currentViewTab = MissionViewTab.NormalMission;
		}
		UpdateMissionView(_currentViewTab);
		UIManager.GetInstance().OnReadyViewScene(isFadein: true);
		int canReceiveAchievementRewards = GetCanReceiveAchievementRewards();
		AchievementCanReceiveCount.gameObject.SetActive(canReceiveAchievementRewards > 0);
		AchievementCanReceiveCount.text = canReceiveAchievementRewards.ToString();
	}

	private void ResetTransitionViewTab()
	{
		_transitionViewTab = MissionViewTab.NormalMission;
	}

	private void UpdateMissionView(MissionViewTab viewTab)
	{
		switch (viewTab)
		{
		case MissionViewTab.NormalMission:
			changeMission();
			break;
		case MissionViewTab.Achievement:
			changeAchievement();
			break;
		case MissionViewTab.BattlePassMission:
			ChangeBattlePassMission();
			break;
		}
	}

	public override bool IsUseCommonBackground()
	{
		return true;
	}

	public void changeMission()
	{
		setScene(MissionViewTab.NormalMission);
		ReceiveButton.gameObject.SetActive(value: false);
		_soloPlayMissionToggleUI.gameObject.SetActive(value: true);
		SetMissionTypeToggleBtn(Data.MissionInfo.data._missionReceiveType);
		bool flag = Data.MissionInfo.data._isChangeMission;
		long num = 0L /* headless UI stub: no mission time */;
		TimeSpan timeSpan = TimeSpan.FromSeconds(Data.MissionInfo.data._canChangeMissionTime - num);
		if (!flag)
		{
			flag = timeSpan.TotalSeconds < -10.0;
		}
		List<UserMission> user_mission_list = Data.MissionInfo.data.user_mission_list;
		scrollItems = new GameObject[user_mission_list.Count];
		int num2 = 0;
		foreach (UserMission item in user_mission_list)
		{
			if (item.end_time <= 0 || item.GetMissionPeriodSec(num) > 0)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(goAchievementWindowBase);
				scrollItems[num2] = gameObject;
				scrollItems[num2].transform.parent = goAchievementBaseTop.transform;
				scrollItems[num2].transform.localPosition = Vector3.zero;
				scrollItems[num2].transform.localScale = Vector3.one;
				scrollItems[num2].SetActive(value: true);
				gameObject.GetComponent<AchievementWindowBase>().SetMission(item, Handler, flag, enableSeparator: false, displayChange: true, delegate
				{
					UpdateMissionView(MissionViewTab.NormalMission);
				});
				num2++;
			}
		}
		if (flag)
		{
			_labelMissionChangeText.text = Data.SystemText.Get("Mission_0036");
		}
		else
		{
			int num3 = timeSpan.Hours;
			int num4 = timeSpan.Minutes;
			if (timeSpan.TotalHours >= 24.0)
			{
				num3 = 24;
				num4 = 0;
			}
			else if (num3 <= 0 && num4 <= 0)
			{
				num3 = 0;
				num4 = 1;
			}
			_labelMissionChangeText.text = Data.SystemText.Get("Mission_0031", num3.ToString("00"), num4.ToString("00"));
		}
		_topObjMission.gameObject.SetActive(value: true);
		DisplayScrollItems();
		_labelBgCenter.gameObject.SetActive(value: false);
		AchievementTopText.gameObject.SetActive(value: false);
	}

	public void changeAchievement()
	{
		setScene(MissionViewTab.Achievement);
		scrollItems = new GameObject[Data.MissionInfo.data.user_achievement_list.Count];
		int num = 0;
		int num2 = 0;
		foreach (UserAchievement item in Data.MissionInfo.data.user_achievement_list)
		{
			scrollItems[num] = UnityEngine.Object.Instantiate(goAchievementWindowBase);
			scrollItems[num].transform.parent = goAchievementBaseTop.transform;
			scrollItems[num].transform.localPosition = Vector3.zero;
			scrollItems[num].transform.localScale = Vector3.one;
			scrollItems[num].SetActive(value: true);
			AchievementWindowBase component = scrollItems[num].GetComponent<AchievementWindowBase>();
			component.SetAchievement(item, Handler, delegate
			{
				UpdateMissionView(MissionViewTab.Achievement);
			});
			if (item.achievement_status == 1)
			{
				num2++;
			}
			component.type = item.achievement_type;
			component.iType = num;
			component.level = item.level;
			num++;
		}
		AchievementCanReceiveCount.gameObject.SetActive(num2 > 0);
		AchievementCanReceiveCount.text = num2.ToString();
		AchievementTopText.text = Data.SystemText.Get("Mission_0021", num2.ToString());
		AchievementTopText.gameObject.SetActive(value: true);
		_topObjMission.SetActive(value: false);
		_soloPlayMissionToggleUI.gameObject.SetActive(value: false);
		ReceiveButton.gameObject.SetActive(value: true);
		UIManager.SetObjectToGrey(ReceiveButton.gameObject, num2 == 0);
		ReceiveButton.onClick.Clear();
		ReceiveButton.onClick.Add(new EventDelegate(delegate
		{

			ReceiveAllAchievementRewards();
		}));
		goAchievementWindowBase.SetActive(value: false);
		DisplayScrollItems();
		_labelBgCenter.gameObject.SetActive(value: false);
	}

	private void ChangeBattlePassMission()
	{
		setScene(MissionViewTab.BattlePassMission);
		BattlePassMonthlyMission monthlyMission = Data.MissionInfo.data.BattlePassMonthlyMissionData;
		bool flag = monthlyMission != null;
		_battlePassMontlyMissionScrollView.gameObject.SetActive(flag);
		_battlePassMontlyMissionPeriodLabel.gameObject.SetActive(flag);
		_btnTransitionBattlePass.gameObject.SetActive(flag);
		_labelNoBattlePassMission.gameObject.SetActive(!flag);
		if (flag)
		{
			_battlePassMontlyMissionScrollView.CreateScrollView(monthlyMission.MissionList.Count, delegate(int index, GameObject plate)
			{
				plate.GetComponent<AchievementWindowBase>().SetBttlePassMonthlyMission(monthlyMission.MissionList[index], Handler);
			});
			DateTime? dateTime = ConvertTime.GetDateTime(monthlyMission.EndDate);
			if (dateTime.HasValue)
			{
				string remainingTime = ConvertTime.GetRemainingTime(ConvertTime.GetTimeSpan(0L /* headless UI stub: no mission time */, dateTime.Value));
				_battlePassMontlyMissionPeriodLabel.text = remainingTime;
				_battlePassMontlyMissionPeriodLabel.fontSize = PERIOD_TEXT_SIZE_BIG;
				_battlePassMontlyMissionPeriodLabel.color = LabelDefine.TEXT_COLOR_ORANGE;
			}
			else
			{
				_battlePassMontlyMissionPeriodLabel.text = Data.SystemText.Get("BattlePass_0007", ConvertTime.GetLocalPeriod(monthlyMission.StartDate, monthlyMission.EndDate));
			}
		}
	}

	private void setScene(MissionViewTab next_scene)
	{
		if (scrollItems != null)
		{
			for (int i = 0; i < scrollItems.Length; i++)
			{
				if ((bool)scrollItems[i])
				{
					UnityEngine.Object.Destroy(scrollItems[i]);
				}
			}
			scrollItems = null;
		}
		goAchievementBaseTop.GetComponent<UITable>().repositionNow = true;
		goAchievementBaseTop.GetComponent<UIScrollView>().ResetPosition();
		UpdateTabSprite(next_scene);
		_battlePassMontlyMissionRoot.SetActive(next_scene == MissionViewTab.BattlePassMission);
		_missionAndAchievementRoot.SetActive(next_scene != MissionViewTab.BattlePassMission);
		AchievementTopText.gameObject.SetActive(next_scene == MissionViewTab.Achievement);
		_topObjMission.SetActive(next_scene == MissionViewTab.NormalMission);
		_currentViewTab = next_scene;
	}

	private void UpdateTabSprite(MissionViewTab scene)
	{
		switch (scene)
		{
		case MissionViewTab.NormalMission:
			MissionButton.normalSprite = MissionButton.pressedSprite;
			AchievementButton.normalSprite = AchievementButton.disabledSprite;
			BattlePassMissionButton.normalSprite = BattlePassMissionButton.disabledSprite;
			break;
		case MissionViewTab.Achievement:
			MissionButton.normalSprite = MissionButton.disabledSprite;
			AchievementButton.normalSprite = AchievementButton.pressedSprite;
			BattlePassMissionButton.normalSprite = BattlePassMissionButton.disabledSprite;
			break;
		case MissionViewTab.BattlePassMission:
			MissionButton.normalSprite = MissionButton.disabledSprite;
			AchievementButton.normalSprite = AchievementButton.disabledSprite;
			BattlePassMissionButton.normalSprite = BattlePassMissionButton.pressedSprite;
			break;
		}
	}

	private void DisplayScrollItems()
	{
		goAchievementBaseTop.GetComponent<UITable>().repositionNow = true;
		goAchievementBaseTop.GetComponent<UIScrollView>().ResetPosition();
	}

	public void ReceiveAndDisplayRewards(GameObject obj, GameObject prefab, List<ReceivedReward> rewards)
	{
		obj.AddMissingComponent<ReceiveReward>().ShowReadDialog(rewards, prefab, obj, Handler);
		MyPageMenu.Instance.UpdateMissionCount();
	}

	private void ReceiveAllAchievementRewards()
	{
		AchievementReceiveRewardTask achievementReceiveRewardTask = new AchievementReceiveRewardTask();
		achievementReceiveRewardTask.SetParameter(0, 0);
		StartCoroutine(Toolbox.NetworkManager.Connect(achievementReceiveRewardTask, OnRequestAllAchievementReward, BaseTask.OnRequestFailed, BaseTask.OnFailedErrorCode));
	}

	private void OnRequestAllAchievementReward(NetworkTask.ResultCode error)
	{
		ReceiveAndDisplayRewards(base.gameObject, _mailReceive, Data.MissionInfo.data.total_reward_list);
		UpdateMissionView(MissionViewTab.Achievement);
	}

	protected override void onClose()
	{
		base.onClose();
		Handler.UnloadAll();
		_currentViewTab = MissionViewTab.NormalMission;
		ResetTransitionViewTab();
	}

	private int GetCanReceiveAchievementRewards()
	{
		int num = 0;
		foreach (UserAchievement item in Data.MissionInfo.data.user_achievement_list)
		{
			if (item.achievement_status == 1)
			{
				num++;
			}
		}
		return num;
	}
}
