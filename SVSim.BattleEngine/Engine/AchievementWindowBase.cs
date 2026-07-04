using System;
using System.Collections.Generic;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Bingo;
using Wizard.Lottery;
using Wizard.Scripts.Network.Data.TableData;

public class AchievementWindowBase : MonoBehaviour
{
	public enum AchievementType
	{
		None,
		Reward,
		Nonattainment,
		AlreadyReceived,
		PointRunning,
		PointClear,
		PointReceived
	}

	public GameObject goButtonReward;

	public UITexture achievementIconTexture;

	public UILabel labelAchievementTitle;

	public UILabel labelAchievementData;

	public UILabel labelAchievementCount;

	public UILabel _missionWaitLabel;

	public UILabel _labelMissionPeriod;

	public UILabel _labelMissionNotice;

	public UISprite _titleLine;

	public UILabel alreadyReceived;

	[NonSerialized]
	public int type;

	[NonSerialized]
	public int iType = -1;

	[NonSerialized]
	public int level;

	[NonSerialized]
	public string strAchievementData = "3種類のスリーブを使う。";

	[SerializeField]
	private UILabel LabelDetailBtn;

	[SerializeField]
	private UITable StarTable;

	[SerializeField]
	private UISprite StarOriginal;

	[SerializeField]
	private GameObject MailReceive;

	[SerializeField]
	private UIGauge GaugeUI;

	[SerializeField]
	private UILabel GaugeLabel;

	[SerializeField]
	private UISprite _Separator;

	[SerializeField]
	private UIWidget _StarsWidget;

	[SerializeField]
	private UILabel _labelTopRight;

	private ResourceHandler _resourceHandler;

	private int _viewMailId;

	private QuestRewardInfo _questRewardInfo;

	private Action _onReceivceAchievementSuccess;

	private void Awake()
	{
		LabelDetailBtn.text = Data.SystemText.Get("Common_0022");
	}

	public void SetType(AchievementType typeBase)
	{
		labelAchievementCount.gameObject.SetActive(typeBase != AchievementType.AlreadyReceived);
		alreadyReceived.gameObject.SetActive(typeBase == AchievementType.AlreadyReceived || typeBase == AchievementType.PointReceived);
		goButtonReward.SetActive(typeBase != AchievementType.AlreadyReceived && typeBase != AchievementType.PointReceived);
		UIManager.SetObjectToGrey(goButtonReward, typeBase == AchievementType.Nonattainment || typeBase == AchievementType.PointRunning);
	}

	public void OnRewardClick()
	{
		AchievementReceiveRewardTask achievementReceiveRewardTask = new AchievementReceiveRewardTask();
		achievementReceiveRewardTask.SetParameter(type, level);
		StartCoroutine(Toolbox.NetworkManager.Connect(achievementReceiveRewardTask, OnRequestRewardAchievement, BaseTask.OnRequestFailed, BaseTask.OnFailedErrorCode));
	}

	private void OnRequestRewardAchievement(NetworkTask.ResultCode error)
	{
		OnRequestReward(error, Data.MissionInfo.data.total_reward_list);
		_onReceivceAchievementSuccess.Call();
	}

	private void OnRequestReward(NetworkTask.ResultCode error, List<ReceivedReward> rewards)
	{
		base.transform.parent.gameObject.AddMissingComponent<ReceiveReward>().ShowReadDialog(rewards, MailReceive, base.gameObject, _resourceHandler);
		MyPageMenu.Instance.UpdateMissionCount();
	}

	private void SetAchievementCommon(UserAchievement achi)
	{
		bool num = achi.reward_type == 4;
		strAchievementData = achi.achievement_name;
		SystemText systemText = Data.SystemText;
		labelAchievementTitle.text = systemText.Get("Mission_0023") + strAchievementData;
		labelAchievementTitle.rightAnchor.target = _StarsWidget.transform;
		labelAchievementTitle.rightAnchor.relative = 0f;
		if (num)
		{
			ReceiveReward.SetTicket(achi.RewardUserGoodsId, achi.reward_number, achievementIconTexture, labelAchievementData, _resourceHandler);
		}
		else
		{
			ReceiveReward.SetTexture((UserGoods.Type)achi.reward_type, achievementIconTexture, _resourceHandler);
			labelAchievementData.text = ReceiveReward.getTitle((UserGoods.Type)achi.reward_type, achi.RewardUserGoodsId, achi.reward_number);
		}
		GaugeUI.gameObject.SetActive(value: true);
		int num2 = ((achi.total_count > achi.require_number) ? achi.require_number : achi.total_count);
		int require_number = achi.require_number;
		labelAchievementCount.gameObject.SetActive(value: false);
		GaugeLabel.text = num2 + "/" + require_number;
		if (num2 != 0 && require_number != 0)
		{
			float value = (float)num2 / (float)require_number;
			GaugeUI.Value = value;
		}
		else
		{
			GaugeUI.Value = 0f;
		}
		goButtonReward.GetComponent<UIButton>().GetComponentInChildren<UILabel>().text = systemText.Get("Mail_0023");
	}

	private void SetRunning(UserAchievement achi)
	{
		SetType(AchievementType.Nonattainment);
		SetAchievementCommon(achi);
		SetAchievementStars(achi, cleared: false);
	}

	private void SetAchievementStars(UserAchievement achi, bool cleared)
	{
		int num = achi._maxLevel;
		int num2 = achi.level;
		if (achi._maxLevel > 5)
		{
			num = 5;
			num2 = ((achi.level == achi._maxLevel) ? 5 : ((achi.level <= 0 || achi.level % 5 != 0) ? (achi.level % 5) : 5));
		}
		for (int i = 0; i < num; i++)
		{
			UISprite uISprite = UnityEngine.Object.Instantiate(StarOriginal, StarOriginal.transform.localPosition, StarOriginal.transform.localRotation);
			uISprite.transform.parent = StarTable.transform;
			uISprite.transform.localPosition = Vector3.zero;
			uISprite.transform.localScale = Vector3.one;
			if ((num2 == i + 1 && cleared) || num2 > i + 1)
			{
				uISprite.spriteName = "achievement_star_02";
			}
			else
			{
				uISprite.spriteName = "achievement_star_01";
			}
			uISprite.gameObject.SetActive(value: true);
		}
		StarTable.repositionNow = true;
	}

	private void SetCanReceive(UserAchievement achi)
	{
		SetType(AchievementType.Reward);
		SetAchievementCommon(achi);
		SetAchievementStars(achi, cleared: false);
		UIButton component = goButtonReward.GetComponent<UIButton>();
		component.onClick.Clear();
		component.onClick.Add(new EventDelegate(delegate
		{

			OnRewardClick();
		}));
	}

	private void SetAlreadyReceived(UserAchievement achi)
	{
		SetType(AchievementType.AlreadyReceived);
		SetAchievementCommon(achi);
		SetAchievementStars(achi, cleared: true);
	}

	public void SetAchievement(UserAchievement achi, ResourceHandler resourceHandler, Action onReceivceAchievementSuccess)
	{
		_resourceHandler = resourceHandler;
		_onReceivceAchievementSuccess = onReceivceAchievementSuccess;
		switch (achi.achievement_status)
		{
		case 0:
			SetRunning(achi);
			break;
		case 1:
			SetCanReceive(achi);
			break;
		case 2:
			SetAlreadyReceived(achi);
			break;
		default:
			UnityEngine.Debug.LogError("unkown achievement status");
			break;
		}
	}

	public void SetQuestPoint(QuestRewardInfo reward, AchievementType type, ResourceHandler resourceHandler, Action onRequestRewardPointCallBack)
	{
		_questRewardInfo = reward;
		strAchievementData = reward.Point.ToString();
		labelAchievementTitle.text = Data.SystemText.Get("Quest_0019", strAchievementData);
		_resourceHandler = resourceHandler;
		SetType(type);
		string texName = UserGoods.GetUserGoodsImageName((UserGoods.Type)reward.RewardType, reward.RewardDetailId);
		string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(texName, ResourcesManager.AssetLoadPathType.Item);
		_resourceHandler.Add(assetTypePath, delegate
		{
			if (reward.RewardDetailId == _questRewardInfo.RewardDetailId)
			{
				string assetTypePath2 = Toolbox.ResourcesManager.GetAssetTypePath(texName, ResourcesManager.AssetLoadPathType.Item, isfetch: true);
				achievementIconTexture.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(assetTypePath2);
			}
		});
		labelAchievementData.text = ReceiveReward.getTitle((UserGoods.Type)reward.RewardType, reward.RewardDetailId, reward.RewardCount);
		GaugeUI.gameObject.SetActive(value: false);
		UIButton component = goButtonReward.GetComponent<UIButton>();
		component.GetComponentInChildren<UILabel>().text = Data.SystemText.Get("Mail_0023");
		goButtonReward.gameObject.SetActive(type != AchievementType.PointReceived);
		UIManager.SetObjectToGrey(goButtonReward, type != AchievementType.PointClear);
		component.onClick.Clear();
		component.onClick.Add(new EventDelegate(delegate
		{

			OnQuestPointReceive(reward.Id, onRequestRewardPointCallBack);
		}));
		GetComponent<UISprite>().spriteName = string.Empty;
		CopyAnchor(_labelTopRight.rightAnchor, labelAchievementTitle.rightAnchor);
	}

	public void SetMission(UserMission mission, ResourceHandler resourceHandler, bool canChangeMissions, bool enableSeparator, bool displayChange, Action onChangeMissionSuccess = null)
	{
		_resourceHandler = resourceHandler;
		_Separator.gameObject.SetActive(enableSeparator);
		if (mission.mission_status == 0 && SetMissionWait(mission))
		{
			return;
		}
		SystemText systemText = Data.SystemText;
		if (mission.reward_type == 4)
		{
			ReceiveReward.SetTicket(mission.RewardUserGoodsId, mission.reward_number, achievementIconTexture, labelAchievementData, _resourceHandler);
		}
		else
		{
			ReceiveReward.SetTexture((UserGoods.Type)mission.reward_type, mission.RewardUserGoodsId, achievementIconTexture, _resourceHandler);
			labelAchievementData.text = ReceiveReward.getTitle((UserGoods.Type)mission.reward_type, mission.RewardUserGoodsId, mission.reward_number);
		}
		labelAchievementTitle.text = mission.mission_name;
		int require_number = mission.require_number;
		bool flag = require_number > 0;
		GaugeUI.gameObject.SetActive(flag);
		if (flag)
		{
			int num = ((mission.total_count > mission.require_number) ? mission.require_number : mission.total_count);
			GaugeLabel.text = num + "/" + require_number;
			if (num != 0)
			{
				float value = (float)num / (float)require_number;
				GaugeUI.Value = value;
			}
			else
			{
				GaugeUI.Value = 0f;
			}
		}
		UIButton component = goButtonReward.GetComponent<UIButton>();
		component.normalSprite = "btn_common_02_s_off";
		component.pressedSprite = "btn_common_02_s_on";
		component.GetComponentInChildren<UILabel>().text = systemText.Get("Mission_0029");
		component.onClick.Add(new EventDelegate(delegate
		{

			ChangeMission(mission.id, mission.mission_name, onChangeMissionSuccess);
		}));
		goButtonReward.SetActive(displayChange && !mission.default_flag);
		UIManager.SetObjectToGrey(goButtonReward, !canChangeMissions);
		labelAchievementCount.gameObject.SetActive(value: false);
		SetMissionPeriodLabel(mission);
		CopyAnchor(_labelTopRight.rightAnchor, labelAchievementTitle.rightAnchor);
	}

	public void SetBttlePassMonthlyMission(BattlePassMonthlyMission.MissionDetail mission, ResourceHandler resourceHandler)
	{
		_Separator.gameObject.SetActive(value: false);
		_labelMissionPeriod.gameObject.SetActive(value: false);
		labelAchievementCount.gameObject.SetActive(value: false);
		goButtonReward.gameObject.SetActive(value: false);
		_resourceHandler = resourceHandler;
		labelAchievementTitle.text = mission.Name;
		alreadyReceived.gameObject.SetActive(mission.IsCleared);
		BattlePassMonthlyMission.MissionDetail.RewardInfo reward = mission.Reward;
		if (reward == null)
		{
			_resourceHandler.Add(Toolbox.ResourcesManager.GetAssetTypePath("thumbnail_battle_pass_point", ResourcesManager.AssetLoadPathType.BattlePass), delegate
			{
				string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath("thumbnail_battle_pass_point", ResourcesManager.AssetLoadPathType.BattlePass, isfetch: true);
				achievementIconTexture.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(assetTypePath);
			});
			labelAchievementData.text = string.Empty;
		}
		else if (reward.UserGoods.GoodsType == UserGoods.Type.Item)
		{
			ReceiveReward.SetTicket(reward.UserGoods.Id, reward.Number, achievementIconTexture, labelAchievementData, _resourceHandler);
		}
		else
		{
			ReceiveReward.SetTexture(reward.UserGoods.GoodsType, achievementIconTexture, _resourceHandler);
			labelAchievementData.text = ReceiveReward.getTitle(reward.UserGoods.GoodsType, reward.UserGoods.Id, reward.Number);
		}
		SetViewBattlePassPointText(mission.BattlePassPoint);
		int requireNumber = mission.RequireNumber;
		bool flag = requireNumber > 0;
		GaugeUI.gameObject.SetActive(flag);
		if (flag)
		{
			int num = ((mission.DoneNumber > mission.RequireNumber) ? mission.RequireNumber : mission.DoneNumber);
			GaugeLabel.text = num + "/" + requireNumber;
			if (num != 0)
			{
				float value = (float)num / (float)requireNumber;
				GaugeUI.Value = value;
			}
			else
			{
				GaugeUI.Value = 0f;
			}
		}
	}

	private void SetViewBattlePassPointText(int point)
	{
		string text = "  ";
		if (labelAchievementData.text == string.Empty)
		{
			labelAchievementData.text = Data.SystemText.Get("BattlePass_0010", point.ToString());
		}
		else
		{
			UILabel uILabel = labelAchievementData;
			uILabel.text = uILabel.text + text + Data.SystemText.Get("BattlePass_0010", point.ToString());
		}
	}

	private bool SetMissionWait(UserMission mission)
	{
		long num = 0L; long num2 = 0L; // Pre-Phase-5b: MissionInfoTask headless-unreachable
		TimeSpan timeSpan = TimeSpan.FromSeconds(mission.start_time - num2).Add(new TimeSpan(0, 1, 0));
		int num3 = timeSpan.Hours;
		int num4 = timeSpan.Minutes;
		if (timeSpan.TotalHours >= 24.0)
		{
			num3 = 24;
			num4 = 0;
		}
		else if (num3 <= 0 && num4 <= 0)
		{
			return false;
		}
		if (mission.IsGemMission())
		{
			_missionWaitLabel.text = Data.SystemText.Get("Mission_0073", num3.ToString("00"), num4.ToString("00"));
			_labelMissionNotice.gameObject.SetActive(value: true);
			_labelMissionNotice.text = Data.SystemText.Get("Mission_0074");
		}
		else
		{
			_missionWaitLabel.text = Data.SystemText.Get("Mission_0041", num3.ToString("00"), num4.ToString("00"));
		}
		labelAchievementTitle.gameObject.SetActive(value: false);
		labelAchievementCount.gameObject.SetActive(value: false);
		labelAchievementData.gameObject.SetActive(value: false);
		labelAchievementData.gameObject.SetActive(value: false);
		goButtonReward.gameObject.SetActive(value: false);
		GaugeUI.gameObject.SetActive(value: false);
		_titleLine.gameObject.SetActive(value: false);
		_missionWaitLabel.gameObject.SetActive(value: true);
		return true;
	}

	private void SetMissionPeriodLabel(UserMission mission)
	{
		if (mission.end_time <= 0 || mission.IsGemMission())
		{
			_labelMissionPeriod.gameObject.SetActive(value: false);
			return;
		}
		long nowUnixTime = 0L; // Pre-Phase-5b: headless has no MissionInfoTask
		string remainingTime = ConvertTime.GetRemainingTime(TimeSpan.FromSeconds(mission.GetMissionPeriodSec(nowUnixTime)));
		goButtonReward.gameObject.SetActive(value: false);
		_labelMissionPeriod.gameObject.SetActive(value: true);
		_labelMissionPeriod.text = remainingTime;
	}

	private void ChangeMission(int id, string content, Action onChangeMissionSuccess)
	{
		SystemText systemText = Data.SystemText;
		DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
		dialogBase.SetTitleLabel(systemText.Get("Mission_0033"));
		dialogBase.SetText(systemText.Get("Mission_0030", content));
		dialogBase.SetButtonLayout(DialogBase.ButtonLayout.DecisionBtn);
		dialogBase.onPushButton1 = delegate
		{
			MissionRetireTask missionRetireTask = new MissionRetireTask();
			missionRetireTask.SetParameter(id);
			StartCoroutine(Toolbox.NetworkManager.Connect(missionRetireTask, delegate
			{
				onChangeMissionSuccess.Call();
			}, BaseTask.OnRequestFailed, BaseTask.OnFailedErrorCode));
		};
	}

	private void OnQuestPointReceive(int rewardId, Action onRequestRewardPointCallBack)
	{
		QuestRewardReceiveTask task = new QuestRewardReceiveTask();
		task.SetParameter(rewardId);
		StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
		{
			onRequestRewardPointCallBack.Call();
			DialogCreator.CreateRewardReceiveDialog(task.ReceiveRewardList);
		}, BaseTask.OnRequestFailed, BaseTask.OnFailedErrorCode));
	}

	public void SetMail(MailData mail, Action<int, int> OnReadMail, ResourceHandler handler)
	{
		_resourceHandler = handler;
		_viewMailId = mail.mail_id;
		SetCommonMail(mail);
		TimeLeftUpdate timeLeftUpdate = base.gameObject.AddMissingComponent<TimeLeftUpdate>();
		timeLeftUpdate.mailData = mail;
		_labelTopRight.gameObject.SetActive(value: true);
		timeLeftUpdate.timeLeft = _labelTopRight;
		timeLeftUpdate.UpdateTime();
		SystemText systemText = Data.SystemText;
		labelAchievementCount.text = systemText.Get("Mail_0043", mail.create_time);
		goButtonReward.SetActive(value: true);
		goButtonReward.transform.Find("RewardLabel").GetComponent<UILabel>().text = systemText.Get("Mail_0023");
		UIButton component = goButtonReward.GetComponent<UIButton>();
		component.normalSprite = "btn_common_02_s_off";
		component.hoverSprite = "btn_common_02_s_off";
		component.pressedSprite = "btn_common_02_s_on";
		UIEventListener.Get(goButtonReward).onClick = delegate
		{
			OnReadMail(mail.mail_id, mail.mail_id);
		};
	}

	public void SetHistoryMail(MailData mail, ResourceHandler handler)
	{
		_resourceHandler = handler;
		_viewMailId = mail.mail_id;
		SetCommonMail(mail);
		TimeLeftUpdate component = base.gameObject.GetComponent<TimeLeftUpdate>();
		if ((bool)component)
		{
			component.mailData = null;
		}
		_labelTopRight.gameObject.SetActive(value: false);
		labelAchievementCount.text = Data.SystemText.Get("Mail_0044", mail.create_time);
		goButtonReward.SetActive(value: false);
	}

	private void SetCommonMail(MailData mailData)
	{
		GaugeUI.gameObject.SetActive(value: false);
		labelAchievementData.text = mailData.message;
		labelAchievementTitle.text = ReceiveReward.getTitle(mailData);
		string textureName = ReceiveReward.GetThumbnailName((UserGoods.Type)mailData.reward_type, mailData.RewardUserGoodsId);
		string assetTypePath = Toolbox.ResourcesManager.GetAssetTypePath(textureName, ResourcesManager.AssetLoadPathType.Item);
		_resourceHandler.Add(assetTypePath, delegate
		{
			if (mailData.mail_id == _viewMailId)
			{
				string assetTypePath2 = Toolbox.ResourcesManager.GetAssetTypePath(textureName, ResourcesManager.AssetLoadPathType.Item, isfetch: true);
				achievementIconTexture.mainTexture = Toolbox.ResourcesManager.LoadObject<Texture>(assetTypePath2);
			}
		});
	}

	private void CopyAnchor(UIRect.AnchorPoint original, UIRect.AnchorPoint destination)
	{
		destination.target = original.target;
		destination.relative = original.relative;
		destination.absolute = original.absolute;
	}

	public void SetGetButtonToGreyOut()
	{
		UIManager.SetObjectToGrey(goButtonReward, b: true);
	}
}
