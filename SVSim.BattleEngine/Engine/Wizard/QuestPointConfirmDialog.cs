using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;

namespace Wizard;

public class QuestPointConfirmDialog : MonoBehaviour
{
	[SerializeField]
	private SimpleScrollViewUI _questPointScrollView;

	[SerializeField]
	private UILabel _pointTitleLabel;

	[SerializeField]
	private UILabel _pointLabel;

	[SerializeField]
	private UILabel _pointCompletedLabel;

	[SerializeField]
	private GameObject _pointAmountGaugeRoot;

	[SerializeField]
	private UIGauge _pointAmountGauge;

	[SerializeField]
	private UIButton _receiveButton;

	[SerializeField]
	private ResourceHandler _resourceHandler;

	private Action<int> _updateRewardRecieveNumber;

	private List<QuestRewardInfo> _rewardInfoList;

	private int UnreceiveRewardCount => _rewardInfoList.Count((QuestRewardInfo d) => d.Status == QuestRewardInfo.QuestRewardStatusList.NotReceived);

	public void CreateQuestConfirmDialog(Action<int> updateRewardRecieveNumber)
	{
		_updateRewardRecieveNumber = updateRewardRecieveNumber;
		RequestQuestPointInfoTask();
	}

	public void OnCloseQuestConfirmnDialog()
	{
		_resourceHandler.UnloadAll();
	}

	private void RequestQuestPointInfoTask()
	{
		QuestPointInfoTask task = new QuestPointInfoTask();
		StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
		{
			_rewardInfoList = task.RewardInfoList;
			_questPointScrollView.CreateScrollView(_rewardInfoList.Count, InitializePlate);
			SetPointCollectionPeriod(task.StartTime, task.EndTime);
			SetPoint(task.TotalPoint, task.MaxPoint);
			SetReceiveAllButton();
			_updateRewardRecieveNumber.Call(UnreceiveRewardCount);
		}));
	}

	private void SetPointCollectionPeriod(DateTime start, DateTime end)
	{
		_pointTitleLabel.text = Data.SystemText.Get("Quest_0008", ConvertTime.ToLocal(start, end));
		_pointTitleLabel.gameObject.SetActive(value: true);
	}

	private void SetPoint(int totalPoint, int maxPoint)
	{
		bool flag = totalPoint >= maxPoint;
		_pointCompletedLabel.gameObject.SetActive(flag);
		_pointLabel.gameObject.SetActive(!flag);
		if (!flag)
		{
			_pointLabel.text = Data.SystemText.Get("Quest_0010", totalPoint.ToString(), maxPoint.ToString());
		}
		_pointAmountGaugeRoot.gameObject.SetActive(value: true);
		if (totalPoint != 0 && maxPoint != 0)
		{
			float num = (float)totalPoint / (float)maxPoint;
			if (num > 1f)
			{
				num = 1f;
			}
			_pointAmountGauge.Value = num;
		}
		else
		{
			_pointAmountGauge.Value = 0f;
		}
	}

	private void SetReceiveAllButton()
	{
		_receiveButton.gameObject.SetActive(value: true);
		UIManager.SetObjectToGrey(_receiveButton.gameObject, UnreceiveRewardCount == 0);
		_receiveButton.onClick.Clear();
		_receiveButton.onClick.Add(new EventDelegate(delegate
		{

			ReceiveAllPointRewards();
		}));
	}

	private void InitializePlate(int index, GameObject plate)
	{
		if (index < _rewardInfoList.Count)
		{
			SetRewardItem(plate, _rewardInfoList[index]);
		}
	}

	private void SetRewardItem(GameObject item, QuestRewardInfo reward)
	{
		AchievementWindowBase component = item.GetComponent<AchievementWindowBase>();
		switch (reward.Status)
		{
		case QuestRewardInfo.QuestRewardStatusList.NotAcheived:
			component.SetQuestPoint(reward, AchievementWindowBase.AchievementType.PointRunning, _resourceHandler, RequestQuestPointInfoTask);
			break;
		case QuestRewardInfo.QuestRewardStatusList.NotReceived:
			component.SetQuestPoint(reward, AchievementWindowBase.AchievementType.PointClear, _resourceHandler, RequestQuestPointInfoTask);
			break;
		case QuestRewardInfo.QuestRewardStatusList.Received:
			component.SetQuestPoint(reward, AchievementWindowBase.AchievementType.PointReceived, _resourceHandler, RequestQuestPointInfoTask);
			break;
		}
	}

	private void ReceiveAllPointRewards()
	{
		QuestRewardReceiveAllTask task = new QuestRewardReceiveAllTask();
		StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
		{
			DialogCreator.CreateRewardReceiveDialog(task.ReceiveRewardList);
			RequestQuestPointInfoTask();
		}));
	}
}
