using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle.Phase;
using Wizard.Battle.Recovery;
using Wizard.Battle.Replay;
using Wizard.Battle.Resource;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;
using Wizard.BattleMgr;

public class PuzzleBattleManager : BattleManagerBase
{
	private class PuzzleBattleMgrContentsCreator : IBattleMgrContentsCreator
	{
		public int RandomSeed { get; private set; }

		public IRecoveryManager RecoveryManager { get; private set; }

		public IRecoveryRecordManager RecoveryRecordManager { get; private set; }

		public IReplayRecordManager ReplayRecordManager { get; private set; }

		public PuzzleBattleMgrContentsCreator()
		{
			RandomSeed = new System.Random().Next();
			RecoveryManager = new NullRecoveryManager();
			RecoveryRecordManager = new NullRecoveryRecordManager();
			ReplayRecordManager = new NullReplayRecordManager();
		}

		public IBattleResourceMgr CreateResourceMgr()
		{
			return new BattleResourceMgr();
		}

		public VfxMgr CreateVfxMgr()
		{
			return new VfxMgr();
		}

		public IPhaseCreator CreatePhaseCreator(BattleManagerBase battleMgr)
		{
			return new PuzzleBattlePhaseCreator(battleMgr);
		}
	}

	public class PuzzleBattlePhaseCreator : PhaseCreatorBase
	{
		public PuzzleBattlePhaseCreator(BattleManagerBase battleMgr)
			: base(battleMgr)
		{
		}

		public override IPhase CreateFirstPhase()
		{
			return new PuzzleLoadingPhase(_battleMgr);
		}

		public override IPhase CreateOpeningPhase()
		{
			CreateBattleLogManager();
			return new PuzzleOpeningPhase(_battleMgr);
		}

		public override IPhase CreateMainPhase()
		{
			return new PuzzleMainPhase(_battleMgr, BattleLogManager.GetInstance());
		}
	}

	public class PuzzleLoadingPhase : LoadingPhase
	{
		public PuzzleLoadingPhase(BattleManagerBase tutorialBattleMgr)
			: base(tutorialBattleMgr)
		{
		}

		public override VfxBase Setup()
		{

			return base.Setup();
		}

		public override VfxBase Teardown()
		{
			return NullVfx.GetInstance();
		}
	}

	public class PuzzleOpeningPhase : OpeningPhase
	{
		public PuzzleOpeningPhase(BattleManagerBase quizBattleMgr)
			: base(quizBattleMgr)
		{
		}

		public override VfxBase Setup()
		{
			PuzzleGenerator puzzleGenerator = new PuzzleGenerator(_battleMgr as PuzzleBattleManager);
			PuzzleQuestData puzzleQuestData = (_battleMgr as PuzzleBattleManager).PuzzleQuestData;
			return SequentialVfxPlayer.Create(new PuzzleOpeningVfx(_battleMgr.BackGround), InstantVfx.Create(delegate
			{
				PuzzleBattleManager puzzleBattleManager = _battleMgr as PuzzleBattleManager;
				puzzleBattleManager.CreateButton();
				puzzleBattleManager.ChangeResultLogo();
				puzzleBattleManager.SetUpResetAndFailedAnimation();
				puzzleBattleManager.SetReaperCard();
				_battleMgr.VfxMgr.RegisterSequentialVfx(puzzleGenerator.Generate(puzzleQuestData));
				_battleMgr.VfxMgr.RegisterSequentialVfx(NullVfx.GetInstance());
				_battleMgr.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
				{
					if (!PlayerPrefsWrapper.GetBool(PlayerPrefsWrapper.SOUND_MUTE))
					{

					}
					_battleMgr.BackGround.PlayBgm();
				}));
				_battleMgr.VfxMgr.RegisterSequentialVfx(WaitVfx.Create(0.3f));
				_battleMgr.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
				{
					CreateWinConditionDisplay(delegate
					{
						if (puzzleBattleManager.IsClearDialogWaiting)
						{
							puzzleBattleManager.IsClearDialogWaiting = false;
							VfxBase vfx = _battleMgr.StartBattle();
							_battleMgr.VfxMgr.RegisterSequentialVfx(vfx);
						}
					});
				}));
			}));
		}

		public override VfxWith<IPhase> Update(float dt)
		{
			return new VfxWith<IPhase>(NullVfx.GetInstance(), null);
		}

		private void CreateWinConditionDisplay(Action onComplete)
		{
			PuzzleBattleManager puzzleBattleManager = _battleMgr as PuzzleBattleManager;
			GameObject winConditionDisplayObj = NGUITools.AddChild(puzzleBattleManager.BattleUIContainer.gameObject, LoadPrefab("Prefab/UI/PuzzleWinConditionDisplay"));
			winConditionDisplayObj.GetComponent<PuzzleWinConditionDisplay>().Setup(Data.SystemText.Get(puzzleBattleManager.PuzzleQuestData.BattleData.WinConditionTextId), delegate
			{
				onComplete();
				UnityEngine.Object.Destroy(winConditionDisplayObj);
			});
		}
	}

	public class PuzzleOpeningVfx : OpeningVfx
	{
		public PuzzleOpeningVfx(BackGroundBase backGround)
			: base(backGround)
		{
		}

		public override void RegisterOpeningVfx(ClassBattleCardBase playerClass, ClassBattleCardBase enemyClass)
		{
			Register(new OpeningShowCharacterPanelVfx());
			Register(InstantVfx.Create(delegate
			{
				playerClass.SelfBattlePlayer.HandControl.SetHandPosition();
				enemyClass.SelfBattlePlayer.HandControl.SetHandPosition();
			}));
			// Nested PuzzleOpeningVfx: no mgr in scope on the nested class; the outer mgr's leader-skin
			// lookup routes through the players' BattleMgr chain instead of the static GameMgr.GetIns.
			string path = playerClass.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr().GetPlayerSkinId()
				.ToString("00");
			string path2 = enemyClass.SelfBattlePlayer.BattleMgr.GameMgr.GetDataMgr().GetEnemySkinId()
				.ToString("00");
			Register(NullVfx.GetInstance());
			Register(NullVfx.GetInstance());
		}
	}

	public class PuzzleMainPhase : MainPhase
	{
		private readonly PuzzleBattleManager _puzzleBtlMgr;

		public PuzzleMainPhase(BattleManagerBase battleManager, BattleLogManager logManager)
			: base(battleManager, logManager)
		{
			_puzzleBtlMgr = battleManager as PuzzleBattleManager;
		}

		public override VfxBase Setup()
		{
			return SequentialVfxPlayer.Create(CreateUpdateBattlePlayersVfx(), InstantVfx.Create(delegate
			{
				if (!_battleManager.IsBattleEnd && _menuButton != null)
				{
					_menuButton.SetActive(value: true);
					_puzzleBtlMgr.ShowResetAndHintButton();
				}
			}), InstantVfx.Create(delegate
			{
				_enableTouch = true;
			}));
		}

		public override VfxBase Teardown()
		{
			_puzzleBtlMgr.HideResetAndHintButton();
			return base.Teardown();
		}
	}

	private bool _isCleared;

	public bool IsClearDialogWaiting = true;

	private PuzzleAnimation _puzzleResetAnimatinon;

	private PuzzleAnimation _puzzleFaledAnimatinon;

	private bool _isReseting;

	public override bool IsBattleEnd
	{
		get
		{
			if (BattlePlayer != null && BattleEnemy != null)
			{
				return _isCleared;
			}
			return true;
		}
	}

	public UIButton HintButton { get; private set; }

	public UIButton ResetButton { get; private set; }

	public PuzzleQuestData PuzzleQuestData { get; private set; }

	public int PuzzleDifficulty { get; private set; }

	public int RetryCount { get; private set; }

	public GameObject ReaperCard { get; private set; }

	public PuzzleBattleManager()
		: base(new PuzzleBattleMgrContentsCreator())
	{
		PuzzleQuestData = Data.Master.PuzzleQuestDataList.First((PuzzleQuestData data) => data.Id == this.GameMgr.GetDataMgr().PuzzleQuestId);
		PuzzleDifficulty = this.GameMgr.GetDataMgr().PuzzleDifficulty;
	}

	protected override void SetupEvent()
	{
		base.SetupEvent();
		BattlePlayer.OnTurnEndFinish += delegate
		{
			if (!BattlePlayer.IsExtraTurn)
			{
				BattlePlayer.PlayerBattleView.TurnEndButtonUI.HideBtn();
				base.BattleUIContainer.ForceDisableMenu();
				BattlePlayer.PlayerBattleView.HideDetailPanel();
				SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
				VfxBase canNotTouchCardVfx = NullVfx.GetInstance();
				base.VfxMgr.RegisterImmediateVfx(canNotTouchCardVfx);
				sequentialVfxPlayer.Register(InstantVfx.Create(delegate
				{
					ForceReset(canNotTouchCardVfx);
				}));
				return sequentialVfxPlayer;
			}
			return ControlTurnStartOpponent();
		};
		BattlePlayer.OnShortageDeck += delegate
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			sequentialVfxPlayer.Register(OnShortageDeck(BattlePlayer));
			sequentialVfxPlayer.Register(JudgeBattleResult());
			return sequentialVfxPlayer;
		};
		BattleEnemy.OnShortageDeck += delegate
		{
			SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
			sequentialVfxPlayer.Register(OnShortageDeck(BattleEnemy));
			sequentialVfxPlayer.Register(JudgeBattleResult());
			return sequentialVfxPlayer;
		};
	}

	private void ForceReset(VfxBase canNotTouchCardVfx)
	{
		if (_isReseting || IsBattleEnd)
		{
			return;
		}
		RetryCount++;
		_isReseting = true;
		PuzzleGenerator puzzleGenerator = new PuzzleGenerator(this);
		base.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
		{
			_puzzleFaledAnimatinon.Run(isReset: false);
		}));
		base.VfxMgr.RegisterSequentialVfx(WaitVfx.Create(_puzzleFaledAnimatinon.FadeOutDuration));
		base.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
		{

		}));
		base.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
		{
			base.VfxMgr.RegisterSequentialVfx(SequentialVfxPlayer.Create(puzzleGenerator.Generate(PuzzleQuestData), ControlTurnStartPlayer(), InstantVfx.Create(delegate
			{
				_puzzleFaledAnimatinon.End();
			}), WaitVfx.Create(_puzzleFaledAnimatinon.FadeInDuration), InstantVfx.Create(delegate
			{

				MenuButtonObject.gameObject.SetActive(value: true);
				base.BattleUIContainer.ForceEnableMenu();
				_isReseting = false;
				base.BackGround.PlayBgm();
			})));
		}));
	}

	private void Reset()
	{
		if (!CanReset())
		{
			return;
		}
		RetryCount++;
		_isReseting = true;
		PuzzleGenerator puzzleGenerator = new PuzzleGenerator(this);
		VfxBase canNotTouchCardVfx = NullVfx.GetInstance();
		base.VfxMgr.RegisterImmediateVfx(canNotTouchCardVfx);
		base.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
		{
			_puzzleResetAnimatinon.Run(isReset: true);
		}));
		base.VfxMgr.RegisterSequentialVfx(WaitVfx.Create(_puzzleResetAnimatinon.FadeOutDuration));
		base.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
		{

		}));
		base.VfxMgr.RegisterSequentialVfx(InstantVfx.Create(delegate
		{
			base.VfxMgr.RegisterSequentialVfx(SequentialVfxPlayer.Create(puzzleGenerator.Generate(PuzzleQuestData), ControlTurnStartPlayer(), InstantVfx.Create(delegate
			{
				_puzzleResetAnimatinon.End();
			}), WaitVfx.Create(_puzzleResetAnimatinon.FadeInDuration), InstantVfx.Create(delegate
			{

				_isReseting = false;
			})));
		}));
	}

	private bool CanReset()
	{
		if (!_isReseting && !IsBattleEnd && base.VfxMgr.IsEnd)
		{
			return !TouchControl.HasTouchProcessor;
		}
		return false;
	}

	private bool IsResetButtanEnable()
	{
		if (!_isReseting && !IsBattleEnd && !BattlePlayer.Class.IsDead && !BattleEnemy.Class.IsDead && base.VfxMgr.IsEnd)
		{
			return !TouchControl.HasTouchProcessor;
		}
		return false;
	}

	public override void Update(float dt)
	{
		if (ResetButton != null)
		{
			ResetButton.enabled = IsResetButtanEnable();
		}
		base.Update(dt);
	}

	public override VfxBase JudgeBattleResult()
	{
		if (CheckWinCondition())
		{
			_isCleared = true;
			base.VfxMgr.RegisterImmediateVfx(InstantVfx.Create(delegate
			{
				BattlePlayer.PlayerBattleView.HideTurnEndButton();
				BattlePlayer.BattleMgr.MenuButtonObject.gameObject.SetActive(value: false);
				HideResetAndHintButton();
			}));
			return InstantVfx.Create(delegate
			{
				InitiateGameEndSequence(hasWon: true);
			});
		}
		if (!CheckWinCondition() && (BattlePlayer.Class.IsDead || BattleEnemy.Class.IsDead))
		{
			SequentialVfxPlayer.Create();
			VfxBase canNotTouchCardVfx = NullVfx.GetInstance();
			base.VfxMgr.RegisterImmediateVfx(canNotTouchCardVfx);
			BattlePlayer.PlayerBattleView.TurnEndButtonUI.HideBtn();
			base.BattleUIContainer.ForceDisableMenu();
			BattlePlayer.PlayerBattleView.HideDetailPanel();
			return InstantVfx.Create(delegate
			{
				ForceReset(canNotTouchCardVfx);
			});
		}
		return NullVfx.GetInstance();
	}

	private bool CheckWinCondition()
	{
		string winCondition = PuzzleQuestData.BattleData.WinCondition;
		if (string.IsNullOrEmpty(winCondition))
		{
			return false;
		}
		BattlePlayerReadOnlyInfoPair battlePlayerInfoPair = GetBattlePlayerInfoPair(isPlayer: true);
		BattleCardBase battleCardBase = GetBattlePlayer(isPlayer: true).Class;
		List<SkillFilterCreator.ContentInfo> retOldInfos = new List<SkillFilterCreator.ContentInfo>();
		List<string> retNewInfos = new List<string>();
		SkillCreator.ParseCondition(winCondition, ref retOldInfos, ref retNewInfos);
		ConditionSkillFilterCollection conditionSkillFilterCollection = new ConditionSkillFilterCollection();
		foreach (string item in retNewInfos)
		{
			SkillFilterCreator.SetupCondition(conditionSkillFilterCollection, item, battleCardBase, null);
		}
		SkillOptionValue skillOptionValue = new SkillOptionValue("");
		skillOptionValue.SetupFilterVariable(battlePlayerInfoPair, battleCardBase, isPrePlay: false, null);
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return conditionSkillFilterCollection.Filtering(battlePlayerInfoPair, battleCardBase, checkerOption, skillOptionValue, isPrePlay: false, null);
	}

	public override void PlayRetire()
	{
		HideResetAndHintButton();
		base.PlayRetire();
	}

	protected override int GetFirstAttack(int FirstAttack)
	{
		return 0;
	}

	private void ChangeResultLogo()
	{
		BattleResultControl.TitleWin.spriteName = "result_text_clear";
		BattleResultControl.TitleLose.spriteName = "result_text_failed";
	}

	private void CreateButton()
	{
		GameObject gameObject = NGUITools.AddChild(base.BattleUIContainer.gameObject, LoadPrefab("Prefab/UI/HintBtn"));
		gameObject.GetComponent<UIAnchor>().uiCamera = this.GameMgr.GetGameObjMgr().GetUIContainerCam();
		UISprite componentInChildren = gameObject.GetComponentInChildren<UISprite>();
		componentInChildren.atlas = UIManager.GetInstance().GetAtlasList().FirstOrDefault((UIAtlas s) => s.name == "Battle");
		componentInChildren.spriteName = "battle_btn_hint_off";
		HintButton = gameObject.GetComponentInChildren<UIButton>();
		HintButton.normalSprite = "battle_btn_hint_off";
		HintButton.pressedSprite = "battle_btn_hint_on";
		HintButton.onClick.Clear();
		HintButton.onClick.Add(new EventDelegate(delegate
		{

			DialogBase dialogBase = UIManager.GetInstance().CreateDialogClose();
			dialogBase.SetTitleLabel(Data.SystemText.Get("Puzzle_Hint_Title"));
			dialogBase.SetSize(DialogBase.Size.M);
			dialogBase.SetButtonLayout(DialogBase.ButtonLayout.BlueButton);
			dialogBase.SetButtonText(Data.SystemText.Get("Common_0004"));
			GameObject gameObject3 = UnityEngine.Object.Instantiate(LoadPrefab("Prefab/UI/PuzzleHintDialog"));
			gameObject3.GetComponent<PuzzleHintDialog>().Setup(Data.SystemText.Get(PuzzleQuestData.BattleData.WinConditionTextId), Data.SystemText.Get(PuzzleQuestData.BattleData.HintTextId));
			dialogBase.SetObj(gameObject3);
		}));
		GameObject gameObject2 = NGUITools.AddChild(base.BattleUIContainer.gameObject, LoadPrefab("Prefab/UI/PuzzleResetBtn"));
		gameObject2.GetComponent<UIAnchor>().uiCamera = this.GameMgr.GetGameObjMgr().GetUIContainerCam();
		ResetButton = gameObject2.GetComponentInChildren<UIButton>();
		ResetButton.onClick.Clear();
		ResetButton.onClick.Add(new EventDelegate(delegate
		{

			BattlePlayer.PlayerBattleView.TurnEndButtonUI.HideBtn();
			base.BattleUIContainer.SetEnableReset(isEnable: false);
			BattlePlayer.PlayerBattleView.HideDetailPanel();
			Reset();
		}));
		ResetButton.GetComponentInChildren<UILabel>().text = Data.SystemText.Get("Puzzle_Reset_Button");
		HideResetAndHintButton();
	}

	public void SetUpResetAndFailedAnimation()
	{
		GameObject gameObject = NGUITools.AddChild(base.BattleUIContainer.gameObject, LoadPrefab("Prefab/UI/PuzzleAnimation"));
		_puzzleResetAnimatinon = gameObject.GetComponent<PuzzleAnimation>();
		_puzzleResetAnimatinon.SetUp();
		GameObject gameObject2 = NGUITools.AddChild(base.BattleUIContainer.gameObject, LoadPrefab("Prefab/UI/PuzzleAnimation"));
		_puzzleFaledAnimatinon = gameObject2.GetComponent<PuzzleAnimation>();
		_puzzleFaledAnimatinon.SetUp();
	}

	public void SetReaperCard()
	{
		ReaperCard = CardHolder.transform.GetChild(GetMaxDeckCount(isSelf: true)).gameObject;
	}

	// Static helper with no mgr in scope; must fall back to GameMgr.GetIns(). A per-instance
	// LoadPrefab would need a mgr ref threaded through every static caller — deferred to a
	// larger cleanup pass.
	private static GameObject LoadPrefab(string path)
	{
		// Pre-Phase-5b: PrefabMgr is a UI-only shim headless
		return null;
	}

	public void ShowResetAndHintButton()
	{
		HintButton.gameObject.SetActive(value: true);
		ResetButton.gameObject.SetActive(value: true);
	}

	public void HideResetAndHintButton()
	{
		HintButton.gameObject.SetActive(value: false);
		ResetButton.gameObject.SetActive(value: false);
	}

	public override void DisposeBattleGameObj()
	{
		// GameMgr.IsPuzzleQuest is const-false in headless (Phase 4) — write dropped.
		base.DisposeBattleGameObj();
	}
}
