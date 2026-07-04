using Cute;
using UnityEngine;

namespace Wizard;

public class QuestAllConfirmDialog : MonoBehaviour
{
	[SerializeField]
	private QuestItemTitle _questItemTitleOriginal;

	[SerializeField]
	private QuestItem _questItemOriginal;

	[SerializeField]
	private QuestGaugeItem _questGaugeItemOriginal;

	[SerializeField]
	private FlexibleGrid _grid;

	[SerializeField]
	private UIScrollView _scrollView;

	public void CreateQuestAllConfirmDialog()
	{
		RequestQuestInfoTask();
	}

	private void RequestQuestInfoTask()
	{
		QuestMissionInfoTask task = new QuestMissionInfoTask();
		StartCoroutine(Toolbox.NetworkManager.Connect(task, OnRequestQuestInfoTaskFinish, BaseTask.OnRequestFailed, BaseTask.OnFailedErrorCode));
	}

	private void OnRequestQuestInfoTaskFinish(NetworkTask.ResultCode error)
	{
		CreateQuestList();
	}

	private void CreateQuestList()
	{
		for (int i = 0; i < Data.QuestMissionInfo.MissionDataList.Count; i++)
		{
			AddQuest(Data.QuestMissionInfo.MissionDataList[i]);
		}
		_grid.Reposition();
		_scrollView.ResetPosition();
	}

	private void AddQuest(QuestMissionData missionData)
	{
		QuestItemTitle component = NGUITools.AddChild(_grid.gameObject, _questItemTitleOriginal.gameObject).GetComponent<QuestItemTitle>();
		component.name = "QuestItemTitle_" + missionData.MissionTitle;
		component.gameObject.SetActive(value: true);
		component.Initialize(missionData.MissionTitle, (CardBasePrm.ClanType)missionData.MissionClassId, missionData.GetTotalPoint(), missionData.GetMaxPoint());
		for (int i = 0; i < missionData.MissionDetailList.Count; i++)
		{
			QuestMissionDetail questMissionDetail = missionData.MissionDetailList[i];
			QuestItemBase component2;
			if (questMissionDetail.ShowGauge)
			{
				component2 = Object.Instantiate(_questGaugeItemOriginal.gameObject).GetComponent<QuestGaugeItem>();
				component2.Initialize(questMissionDetail.Text, questMissionDetail.Point, questMissionDetail.IsClear, questMissionDetail.CurrentGaugeValue, questMissionDetail.MaxGaugeValue);
			}
			else
			{
				component2 = Object.Instantiate(_questItemOriginal.gameObject).GetComponent<QuestItem>();
				component2.Initialize(questMissionDetail.Text, questMissionDetail.Point, questMissionDetail.IsClear);
			}
			component2.gameObject.SetActive(value: true);
			component.AddChild(component2.gameObject);
		}
	}
}
