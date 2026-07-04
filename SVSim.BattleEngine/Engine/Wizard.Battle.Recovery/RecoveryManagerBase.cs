using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;
using Wizard.Story.ChapterSelection;

namespace Wizard.Battle.Recovery;

public abstract class RecoveryManagerBase : IRecoveryManager
{
	protected readonly RecoveryOperationInfo _operationInfo;

	protected bool _needUpdate;

	private readonly Queue<string> _skillTargetNameQueue;

	protected Action StartRecoveryEvent;

	protected Action EndDataRecoveryEvent;

	protected Action EndRecoveryEvent;

	public DataMgr.BattleType BattleType => _operationInfo.BattleType;

	public bool? DidPlayerGoFirst => _operationInfo.SetupInfo.DidPlayerGoFirst;

	public int RandomSeed => _operationInfo.SetupInfo.RandomSeed;

	public bool HasMulliganInfo => _operationInfo.SetupInfo.HasMulliganInfo;

	public int BackGroundId => _operationInfo.SetupInfo.BackGroundId;

	public string BgmId => _operationInfo.SetupInfo.BgmId;

	public long RecordTime => _operationInfo.RecordTime;

	public int IdxChangeSeed => -1;

	public static bool failedRecoveryFlag { get; private set; }

	public event Action OnStartRecovery
	{
		add
		{
			StartRecoveryEvent = (Action)Delegate.Combine(StartRecoveryEvent, value);
		}
		remove
		{
			StartRecoveryEvent = (Action)Delegate.Remove(StartRecoveryEvent, value);
		}
	}

	public event Action OnEndDataRecovery
	{
		add
		{
			EndDataRecoveryEvent = (Action)Delegate.Combine(EndDataRecoveryEvent, value);
		}
		remove
		{
			EndDataRecoveryEvent = (Action)Delegate.Remove(EndDataRecoveryEvent, value);
		}
	}

	public event Action OnEndRecovery
	{
		add
		{
			EndRecoveryEvent = (Action)Delegate.Combine(EndRecoveryEvent, value);
		}
		remove
		{
			EndRecoveryEvent = (Action)Delegate.Remove(EndRecoveryEvent, value);
		}
	}

	public static void OpenRecoveryFailedDialog()
	{
		AbortSoloPlayRecoveryTask task = new AbortSoloPlayRecoveryTask();
		UIManager.GetInstance().StartCoroutine(Toolbox.NetworkManager.Connect(task, delegate
		{
			DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose(isSystem: true);
			dialogBase.SetSize(DialogBase.Size.M);
			dialogBase.SetTitleLabel(Data.SystemText.Get("ErrorHeader_20001"));
			dialogBase.SetText(Data.SystemText.Get("Error_20001"));
			dialogBase.AddButton(DialogBase.ButtonType.BackToTitle);
			dialogBase.SetPanelDepth(6000);
			dialogBase.SetFadeButtonEnabled(flag: false);
			dialogBase.onPushButton1 = (Action)Delegate.Combine(dialogBase.onPushButton1, (Action)delegate
			{
				failedRecoveryFlag = false;
				/* Pre-Phase-5b: BattleCtrl.BattleEnd stubbed */
			});
			failedRecoveryFlag = true;
			RecoveryRecordManagerBase.DeleteRecoveryFile();
		}));
	}

	public RecoveryManagerBase(string filePath)
	{
		try
		{
			_operationInfo = new RecoveryOperationInfo(filePath);
			_skillTargetNameQueue = new Queue<string>(_operationInfo.SkillTargetCardNames);
		}
		catch (Exception)
		{
			OpenRecoveryFailedDialog();
		}
	}

	public void Setup()
	{
		try
		{
			DataMgr dataMgr = null; // Pre-Phase-5b: DataMgr not reachable headless
			dataMgr.m_BattleType = _operationInfo.BattleType;
			SetupConditionInfo setupInfo = _operationInfo.SetupInfo;
			BattleConditionPlayerInfo playerInfo = setupInfo.PlayerInfo;
			BattleConditionEnemyInfo enemyInfo = setupInfo.EnemyInfo;
			int enemyAiID = -1;
			List<int> currentDeckData = playerInfo.DeckCardInfos.Select((DeckCardInfo i) => i.CardId.Value).ToList();
			List<int> specialAbilityIdList = null;
			dataMgr.SetCurrentDeckData(currentDeckData);
			dataMgr.SetPlayerCharaId(playerInfo.CharaId);
			dataMgr.SetPlayerSubClassID(playerInfo.SubClassId);
			dataMgr.SetPlayerMyRotationInfo(playerInfo.MyRotationId);
			dataMgr.SetPlayerSleeveId(playerInfo.SleeveId);
			dataMgr.PracticeDifficultyDegreeId = setupInfo.PracticeDifficultyDegreeId;
			dataMgr.MissionNecessaryInformation = setupInfo.MissionNecessaryInformation;
			if (setupInfo.IsPrebuildDeck)
			{
				dataMgr.LastSelectDeckAttributeType = DeckAttributeType.BuildDeck;
			}
			if (setupInfo.IsTrialDeck)
			{
				dataMgr.LastSelectDeckAttributeType = DeckAttributeType.TrialDeck;
			}
			if (dataMgr.m_BattleType == DataMgr.BattleType.Quest)
			{
				dataMgr.SetSoroPlay3DFieldID(setupInfo.BackGroundId);
				dataMgr.SetQuestBattleData(new QuestBattleData(setupInfo));
				enemyAiID = setupInfo.QuestEnemyAiId;
			}
			if (dataMgr.m_BattleType == DataMgr.BattleType.BossRushQuest || dataMgr.m_BattleType == DataMgr.BattleType.SecretBossQuest)
			{
				dataMgr.SetSoroPlay3DFieldID(setupInfo.BackGroundId);
				dataMgr.SetBossRushBattleData(new BossRushBattleData(setupInfo));
				specialAbilityIdList = dataMgr.BossRushBattleData.PlayerSkillList.Select((BossRushSpecialSkill s) => s.OriginalCardId).ToList();
				enemyAiID = setupInfo.QuestEnemyAiId;
			}
			else if (dataMgr.m_BattleType == DataMgr.BattleType.Practice)
			{
				dataMgr.SetSoroPlay3DFieldID(BackGroundId);
			}
			else if (dataMgr.m_BattleType == DataMgr.BattleType.Story)
			{
				UIManager.GetInstance().OverrideSceneParam(UIManager.ViewScene.ClassSelectionPage, ClassSelectionPageParam.CreateStorySelect());
				AreaSelectUI.SetRecoveryData(setupInfo);
				if (setupInfo.IsDefaultDeck)
				{
					dataMgr.LastSelectDeckAttributeType = DeckAttributeType.DefaultDeck;
				}
			}
			dataMgr.SetEnemyCharaId(enemyInfo.CharaId);
			dataMgr.SetEnemySubClassID(enemyInfo.SubClassId);
			dataMgr.SetEnemyMyRotationInfo(enemyInfo.MyRotationId);
			dataMgr.SetEnemySleeveId(enemyInfo.SleeveId);
			dataMgr.SetSelectDeckFormat((Format)PlayerPrefsWrapper.GetValue(PlayerPrefsWrapper.LAST_BATTLE_DECK_FORMAT_FOR_SINGLE_RECOVER));
			Data.CurrentFormat = dataMgr.GetSelectDeckFormat();
			CardMaster.SetBattleCardMasterId(FormatBehaviorManager.GetDefaultBehaviour(dataMgr.GetSelectDeckFormat()).CardMasterId);
			if (enemyInfo.AIDeckId >= 0)
			{
				if (dataMgr.m_BattleType == DataMgr.BattleType.Story)
				{
					StoryChapterSelectionUtility.RegisterStoryBattleData(Data.SelectedStoryInfo.ChapterData.FindBattleSettingDataByPlayerCharaId(setupInfo.StoryRecoveryData.ChapterCharaId));
				}
				else
				{
					dataMgr.SetCurrentEnemyDeckDataFromAIDeck(enemyInfo.ClassId, enemyInfo.AIDifficulty.Value, enemyInfo.AILevel.Value, enemyInfo.AIMaxLife.Value, enemyInfo.AIDeckId.Value, enemyInfo.AIStyleId.Value, enemyInfo.AIEmoteId.Value, enemyInfo.AIUseInnerEmote.Value, enemyAiID, specialAbilityIdList);
				}
			}
			else
			{
				List<int> deck = enemyInfo.DeckCardInfos.Select((DeckCardInfo i) => i.CardId.Value).ToList();
				dataMgr.SetEnemyAIDeckFromCustomDeck(enemyInfo.ClassId, deck, enemyInfo.AIDifficulty.Value, enemyInfo.AILevel.Value, enemyInfo.AIMaxLife.Value, enemyInfo.AIStyleId.Value, enemyInfo.AIEmoteId.Value, enemyInfo.AIUseInnerEmote.Value);
			}
			dataMgr.Load();
			dataMgr.LoadEnemy();
		}
		catch (Exception)
		{
			OpenRecoveryFailedDialog();
		}
	}

	public abstract VfxBase Recovery(BattlePlayer battlePlayer, BattleEnemy battleEnemy, Func<IEnumerator, Coroutine> startCoroutine);

	public abstract VfxBase UpdateRecovery();

	public virtual void RecoveryBeforeMulligan()
	{
	}

	public virtual VfxBase RecoveryMulligan(BattlePlayer battlePlayer, BattleEnemy battleEnemy)
	{
		EndDataRecovery();
		return SequentialVfxPlayer.Create(InstantVfx.Create(delegate
		{
			/* Pre-Phase-5b: FontChanger UI-only */
		}), ParallelVfxPlayer.Create(battlePlayer.BattleView.RecoveryMulligan(), battleEnemy.BattleView.RecoveryMulligan()), InstantVfx.Create(delegate
		{
			EndRecoveryEvent.Call();
		}));
	}

	public virtual string RecoveryPopSkillTargetCardName()
	{
		return _skillTargetNameQueue.Dequeue();
	}

	protected void EndDataRecovery()
	{
		_needUpdate = false;
		EndDataRecoveryEvent.Call();
	}
}
