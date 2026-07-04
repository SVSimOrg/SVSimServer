using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;

namespace Wizard;

public class PuzzleUtil
{

	public static void SetPuzzleQuestData(PuzzleQuestData data, int difficulty, DataMgr.BattleType battleType)
	{
		CardMaster.SetBattleCardMasterId(FormatBehaviorManager.GetDefaultBehaviour(Format.Unlimited).CardMasterId);
		// GameMgr.IsPuzzleQuest is const-false in headless (Phase 4); the assignment was a no-op
		// signal for a UI branch headless never runs.
		Data.CurrentFormat = Format.Unlimited;
		DataMgr dataMgr = null; // Pre-Phase-5b: headless has no DataMgr
		dataMgr.m_BattleType = battleType;
		dataMgr.SetPlayerCharaIdBySkinId(data.PlayerSkin);
		dataMgr.SetPlayerSleeveId(3000011L);
		dataMgr.SetSelectDeckFormat(Format.Unlimited);
		dataMgr.Load();
		dataMgr.PuzzleEnemyClass = data.BattleData.EnemyClass;
		dataMgr.SetEnemyCharaId(data.EnemySkin);
		dataMgr.SetEnemySleeveId(3000011L);
		dataMgr.SetCurrentEnemyDeckData(Enumerable.Repeat(100011010, 40).ToList());
		dataMgr.LoadEnemyClassData();
		dataMgr.SetSoroPlay3DFieldID(data.StageId);
		dataMgr.SetStoryBgmID("NONE");
		dataMgr.PuzzleQuestId = data.Id;
		dataMgr.PuzzleDifficulty = difficulty;
	}

	public static void ChangeSceneToPuzzleQuest(PuzzleQuestData data)
	{
		QuestStartTask questStartTask = new QuestStartTask(isPuzzle: true);
		questStartTask.SetParameterForPuzzle(data.Id);
		UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(questStartTask, delegate
		{
			UIManager.ChangeViewSceneParam param = new UIManager.ChangeViewSceneParam
			{
				IsShow_CardIntroduction = true
			};
			UIManager.GetInstance().ChangeViewScene(UIManager.ViewScene.Battle, param);
		}));
	}

	public static IEnumerator OpenPuzzleSelectDialogCoroutine(Action<PuzzleQuestData, int> onDecide, Action<QuestOpenPuzzleDialogTask> onOpen = null, Action<QuestOpenPuzzleDialogTask> onClose = null)
	{
		QuestOpenPuzzleDialogTask task = new QuestOpenPuzzleDialogTask();
		yield return Toolbox.NetworkManager.Connect(task, delegate
		{
			UIManager.GetInstance().createInSceneCenterLoading();
		});
		while (!task.IsResultSuccess)
		{
			yield return null;
		}
		List<string> loadList = PuzzleQuestSelectDialog.CollectResourcePath(task.PuzzleQuestInfo.DisplayDatas);
		yield return Toolbox.ResourcesManager.LoadAssetGroupAsync(loadList, null);
		UIManager.GetInstance().closeInSceneCenterLoading();
		PuzzleQuestSelectDialog.CreateDialog(task.PuzzleQuestInfo, onDecide).OnClose = delegate
		{
			Toolbox.ResourcesManager.RemoveAssetGroup(loadList);
			onClose.Call(task);
		};
		onOpen.Call(task);
	}
}
