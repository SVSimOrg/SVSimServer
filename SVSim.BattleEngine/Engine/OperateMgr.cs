using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using LitJson;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Replay;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class OperateMgr
{
	private readonly BattleManagerBase _battleMgr;

	private readonly IPlayerView _PlayerBattleView;

	private TouchControl _TouchControl;

	public BattleLogManager BattleLogManager { get; private set; }

	public event Action<IEnumerable<BattleCardBase>> OnSkillCardSelect;

	public event Func<VfxBase> OnSkillCardSelectSuccess;

	public event Func<VfxBase> OnPlayerSetCard;

	public event Func<BattleCardBase, BattleCardBase, VfxBase> OnPlayerAttack;

	public event Func<VfxBase> OnPlayerBattleCardSelect;

	public event Func<VfxBase> OnPlayerEvolve;

	public event Func<VfxBase> OnPlayerFusion;

	public event Action<BattleCardBase> OnBeforeSetCard;

	public event Action<BattleCardBase> OnSetCard;

	public event Action<BattleCardBase, BattleCardBase, IEnumerable<BattleCardBase>> OnSetCardSuccess;

	public event Func<BattleCardBase, VfxBase> OnSetCardComplete;

	public event Func<BattleCardBase, VfxBase> OnSetCardExecuted;

	public event Func<BattleCardBase, BattleCardBase, SkillProcessor, VfxBase> OnBeforeAttack;

	public event Func<BattleCardBase, BattleCardBase, VfxBase> OnAttackAfter;

	public event Action<BattleCardBase, bool> OnBattleCardSelect;

	public event Func<BattleCardBase, BattleCardBase, bool, VfxBase> OnAttackExecuted;

	public event Action<BattleCardBase, BattleCardBase, int, int> OnAttackDamageExecuted;

	public event Action<BattleCardBase> OnAttackProcessComplete;

	public event Action<BattleCardBase, BattleCardBase> OnAttackStart;

	public event Action<BattleCardBase, BattleCardBase, IEnumerable<BattleCardBase>> OnEvolveSuccess;

	public event Action<BattleCardBase> OnJustBeforeEvolve;

	public event Action<BattleCardBase> OnRightAfterEvolve;

	public event Action<BattleCardBase, IEnumerable<BattleCardBase>> OnBeforeFusion;

	public event Func<BattleCardBase, VfxBase> OnEvoleComplete;

	public event Func<BattleCardBase, VfxBase> OnAfterFusion;

	public event Action OnTurnEnd;

	public event Action OnBeforePlayerTurnEnd;

	public event Action OnTurnEnd_ButtonPush;

	public event Action<BattleCardBase, bool, List<BattleCardBase>, bool> OnStartSelect;

	public event Action<BattleCardBase, bool, BattleCardBase, bool, bool> OnSelect;

	public event Action<BattleCardBase, bool, List<BattleCardBase>, bool> OnStartMultipleSelect;

	public event Action<BattleCardBase, bool, BattleCardBase, bool, bool> OnCompleteSelect;

	public event Action<BattleCardBase, bool, List<BattleCardBase>, bool> OnStartChoice;

	public event Action<BattleCardBase, bool, List<BattleCardBase>, BattleCardBase, List<int>, bool, bool> OnCompleteChoice;

	public event Action<BattleCardBase, bool, bool> OnCancelSelect;

	public event Action<BattleCardBase, bool, bool> OnCancelChoice;

	public event Action<BattleCardBase, List<BattleCardBase>> OnStartFusion;

	public event Action<int, bool, int, bool> OnSelectFusion;

	public event Action<BattleCardBase> OnSelectFusionForRecovery;

	public event Action<BattleCardBase> OnCancelFusion;

	public event Action OnSkillProcessStart;

	public event Action OnSkillProcessEnd;

	public event Action OnSkillVfxStart;

	public event Action OnSkillVfxEnd;

	public event Func<BattleCardBase, SkillBase, bool, bool, JsonData> OnCreateSideLogCardData;

	public event Action<BattleCardBase, SkillBase, bool, bool, bool, bool, JsonData> OnCreateSideLog;

	public event Action<bool> OnClearSideLog;

	public event Action<SkillCreator.SkillBuildInfo, List<BattleCardBase>, BattleCardBase> OnAttachSkill;

	public event Action<SkillCreator.SkillBuildInfo, bool, bool, bool, bool> OnCreateEffect;

	public event Action<BattleCardBase, List<BattleCardBase>> OnShowSkillEffect;

	public event Action<SkillBase, bool> OnSkillInductionEffect;

	public event Action<List<BattleCardBase>> OnShowIndependentEffect;

	public event Action<BattleCardBase, List<BattleCardBase>, CardBasePrm.ClanType, CardBasePrm.TribeInfo> OnChangeAffiliation;

	public event Action<List<BattleCardBase>, List<BattleCardBase>> OnUpdateAttackableEffect;

	public event Action<List<BattleCardBase>, bool, bool, bool> OnUpdateSkillEffect;

	public event Action<List<BattleCardBase>> OnChangeUnionBurstAndSkyboundArt;

	public event Action<bool> OnShowRepeatSkillEffect;

	public event Action<BattleCardBase, List<BattleCardBase>> OnGiveCantActivateFanfare;

	public event Action<BattleCardBase, List<BattleCardBase>> OnDepriveCantActivateFanfare;

	public event Action<BattleCardBase, List<BattleCardBase>> OnLoseSkill;

	public event Action<BattleCardBase> OnAttachShortageDeckWin;

	public event Action<BattleCardBase> OnSpecialWin;

	public event Action<BattleCardBase> OnSpecialLose;

	public event Action<bool, bool> OnTurnEndFinish;

	public OperateMgr(BattleManagerBase battleMgr, TouchControl touchControl)
	{
		_battleMgr = battleMgr;
		_TouchControl = touchControl;
		_PlayerBattleView = _battleMgr.BattlePlayer.PlayerBattleView;
		_battleMgr.SetUpOperateEvent(this);
		BattleLogManager = BattleLogManager.GetInstance();
	}

	public void SetTouchControl(TouchControl touchControl)
	{
		_TouchControl = touchControl;
	}

	private ActionProcessor CreateActionProcessor(bool isPlayer)
	{
		ActionProcessor actionProcessor = new ActionProcessor(_battleMgr.GetBattlePlayerPair(isPlayer));
		_battleMgr.SetupActionProcessorEvent(actionProcessor, isPlayer);
		return actionProcessor;
	}

	public virtual VfxBase InitSetCard(BattleCardBase card, bool isPlayer, bool isSelect = false, bool isRecovery = false, bool isChoiceSelect = false, bool isAccelerateSelect = false, bool registerDirectlyToVfxManager = true, bool isFusionWait = false, bool isChoiceBrave = false)
	{
		this.OnBeforeSetCard.Call(card);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		bool isSelectTarget = _battleMgr.GameMgr.IsAdminWatch && (isAccelerateSelect || card.Skills.CheckWhenPlaySelectTargetSkillCondition);
		if (isPlayer || _battleMgr.GameMgr.IsAdminWatch)
		{
			PlayQueueViewBase playQueueView = _battleMgr.GetBattlePlayer(isPlayer).BattleView.PlayQueueView;
			_battleMgr.VfxMgr.RegisterImmediateVfx(card.StopSpellCharge());
			VfxBase vfx = playQueueView.AddCardToViewVfx(forceCardIntoPlayQueue: !isRecovery && (isAccelerateSelect || card.Skills.CheckWhenPlaySelectTargetSkillCondition) && !isFusionWait, playedCardView: card.BattleCardView, isSelectTarget: isSelectTarget, isChoice: isChoiceSelect, isChoiceBrave: isChoiceBrave);
			if (registerDirectlyToVfxManager)
			{
				_battleMgr.VfxMgr.RegisterImmediateVfx(vfx);
				if (!isPlayer && _battleMgr.GameMgr.IsAdminWatch && !isSelect && !isAccelerateSelect && !isFusionWait)
				{
					sequentialVfxPlayer.Register(WaitVfx.Create(0.5f));
				}
			}
			else
			{
				sequentialVfxPlayer.Register(vfx);
			}
		}
		else
		{
			PlayQueueViewBase playQueueView2 = _battleMgr.BattleEnemy.BattleView.PlayQueueView;
			bool forceCardIntoPlayQueue = !isRecovery && card.IsSpell;
			sequentialVfxPlayer.Register(playQueueView2.AddCardToViewVfx(card.BattleCardView, forceCardIntoPlayQueue, isSelectTarget, isChoiceSelect));
		}
		VfxBase vfxBase = NullVfx.GetInstance();
		if (isPlayer && isSelect)
		{
			vfxBase = this.OnPlayerSetCard.GetAllFuncVfxResults();
		}
		return SequentialVfxPlayer.Create(vfxBase, sequentialVfxPlayer);
	}

	protected ActionProcessor CreateSetCardActionProcessor(bool isPlayer)
	{
		ActionProcessor actionProcessor = CreateActionProcessor(isPlayer);
		actionProcessor.OnBeforePlayCard += this.OnSetCardSuccess;
		actionProcessor.OnAfterPlayCard += this.OnSetCardComplete;
		if (!_battleMgr.IsVirtualBattle)
		{
			actionProcessor.OnBeforePlayCard += delegate(BattleCardBase originalCard, BattleCardBase _card, IEnumerable<BattleCardBase> _)
			{
				if (originalCard != _card && originalCard.Skills.Any((SkillBase s) => s.OnWhenAccelerate != 0))
				{
					BattleLogManager.BeginLogAccelerate(_card);
				}
				else if (originalCard != _card && originalCard.Skills.Any((SkillBase s) => s.OnWhenCrystallize != 0))
				{
					BattleLogManager.BeginLogCrystallize(_card);
				}
				else if (originalCard == _card || !originalCard.Skills.HaveBeforeChoiceSkill())
				{
					BattleLogManager.BeginLogBlockPlay(_card);
				}
			};
			actionProcessor.OnAfterPlayCard += (BattleCardBase _card) => BattleLogManager.EndLogBlockPlay();
		}
		return actionProcessor;
	}

	public virtual VfxBase PlayCard(BattleCardBase card, bool isPlayer, List<BattleCardBase> selectCards, bool isRecovery = false, List<int> selectChoiceId = null, bool isChoiceBrave = false)
	{
		if (isPlayer)
		{
			_battleMgr.BattlePlayer.PlayCardTouchCount++;
		}
		ActionProcessor actionProcessor = CreateSetCardActionProcessor(isPlayer);
		bool flag = selectCards.IsNotNullOrEmpty();
		actionProcessor.OnAfterPlayCard += this.OnSetCardExecuted;
		actionProcessor.OnAfterPlayCard += (BattleCardBase c) => c.SelfBattlePlayer.UpdateHandCardsCost();
		VfxBase vfxBase = NullVfx.GetInstance();
		if (isPlayer)
		{
			vfxBase = (flag ? this.OnPlayerBattleCardSelect.GetAllFuncVfxResults() : this.OnPlayerSetCard.GetAllFuncVfxResults());
		}
		this.OnSetCard.Call(card);
		return SequentialVfxPlayer.Create(actionProcessor.PlayCard(card, selectCards, selectChoiceId, isChoiceBrave), vfxBase, InstantVfx.Create(delegate
		{
			if (!_PlayerBattleView.IsMoving())
			{
				_PlayerBattleView.UpdateTurnEndPulseEffect();
			}
		}));
	}

	public virtual VfxBase Attack(BattleCardBase attackCard, BattleCardBase targetCard, bool isPlayer)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (attackCard.SkillApplyInformation.RandomAttackCount > 0)
		{
			IBattlePlayerView battleView = targetCard.SelfBattlePlayer.BattleView;
			sequentialVfxPlayer.Register(battleView.CreateStopAttackFloatVfx(targetCard.BattleCardView));
			sequentialVfxPlayer.Register(battleView.AttackSelectControl.ResetCardAfterAttack(targetCard.BattleCardView));
			List<BattleCardBase> list = new List<BattleCardBase>();
			list.AddRange(attackCard.SelfBattlePlayer.ClassAndInPlayCardList.Where((BattleCardBase c) => c != attackCard));
			list.AddRange(attackCard.OpponentBattlePlayer.ClassAndInPlayCardList);
			list = list.Where((BattleCardBase c) => (c.IsUnit || c.IsClass) && !c.CantBeFocusedAttack(attackCard)).ToList();
			targetCard = list[attackCard.SelfBattlePlayer.BattleMgr.StableRandom(list.Count)];
		}
		SequentialVfxPlayer sequentialVfxPlayer2 = SequentialVfxPlayer.Create();
		if (_battleMgr.IsBattleEnd)
		{
			return NullVfx.GetInstance();
		}
		SkillProcessor skillProcessor = new SkillProcessor();
		sequentialVfxPlayer.Register(this.OnBeforeAttack.GetAllFuncVfxResults(attackCard, targetCard, skillProcessor));
		sequentialVfxPlayer.Register(skillProcessor.Process(new BattlePlayerPair(attackCard.SelfBattlePlayer, attackCard.OpponentBattlePlayer)));
		ActionProcessor actionProcessor = CreateAttackActionProcessor(attackCard, targetCard, isPlayer);
		sequentialVfxPlayer.Register(actionProcessor.Attack(attackCard, targetCard));
		if (isPlayer)
		{
			sequentialVfxPlayer2.Register(this.OnPlayerAttack.GetAllFuncVfxResults(attackCard, targetCard));
		}
		sequentialVfxPlayer2.Register(this.OnAttackAfter.GetAllFuncVfxResults(attackCard, targetCard));
		return SequentialVfxPlayer.Create(sequentialVfxPlayer, sequentialVfxPlayer2);
	}

	protected ActionProcessor CreateAttackActionProcessor(BattleCardBase attackCard, BattleCardBase targetCard, bool isPlayer)
	{
		ActionProcessor actionProcessor = CreateActionProcessor(isPlayer);
		actionProcessor.OnBeforeAttack += () => BattleLogManager.SetupWarActionLog();
		actionProcessor.OnBeforeAttackSkillComplete += () => BattleLogManager.BeginLogBlockWar(attackCard, targetCard);
		actionProcessor.OnAfterAttack += BattleLogManager.EndLogBlockWar;
		actionProcessor.OnAfterAttack += this.OnAttackExecuted;
		actionProcessor.OnAttackDamageComplete = (Action<BattleCardBase, BattleCardBase, int, int>)Delegate.Combine(actionProcessor.OnAttackDamageComplete, this.OnAttackDamageExecuted);
		actionProcessor.OnAttackProcessComplete = (Action<BattleCardBase>)Delegate.Combine(actionProcessor.OnAttackProcessComplete, this.OnAttackProcessComplete);
		actionProcessor.OnAttackStart = (Action<BattleCardBase, BattleCardBase>)Delegate.Combine(actionProcessor.OnAttackStart, this.OnAttackStart);
		return actionProcessor;
	}

	public virtual VfxBase EvolutionCard(BattleCardBase card, bool isPlayer, List<BattleCardBase> selectCards, List<int> selectChoiceId = null)
	{
		if (selectCards != null)
		{
			for (int i = 0; i < selectCards.Count; i++)
			{
				if (selectCards[i] != null)
				{
					selectCards[i].SelfBattlePlayer.AddLastTargetCardsList(selectCards[i]);
				}
			}
		}
		ActionProcessor actionProcessor = CreateEvolutionActionProcessor(isPlayer);
		bool flag = selectCards.IsNotNullOrEmpty();
		VfxBase vfxBase = actionProcessor.Evolution(card, selectCards, selectChoiceId);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (isPlayer)
		{
			parallelVfxPlayer.Register(this.OnPlayerEvolve.GetAllFuncVfxResults());
			if (flag)
			{
				parallelVfxPlayer.Register(this.OnPlayerBattleCardSelect.GetAllFuncVfxResults());
			}
		}
		return SequentialVfxPlayer.Create(vfxBase, parallelVfxPlayer);
	}

	protected ActionProcessor CreateEvolutionActionProcessor(bool isPlayer)
	{
		ActionProcessor actionProcessor = CreateActionProcessor(isPlayer);
		actionProcessor.OnBeforeEvolution += this.OnEvolveSuccess;
		actionProcessor.OnJustBeforeEvolution += this.OnJustBeforeEvolve;
		actionProcessor.OnRightAfterEvolution += this.OnRightAfterEvolve;
		actionProcessor.OnAfterEvolution += this.OnEvoleComplete;
		if (!_battleMgr.IsVirtualBattle)
		{
			actionProcessor.OnBeforeEvolution += delegate(BattleCardBase _originalcard, BattleCardBase _card, IEnumerable<BattleCardBase> _)
			{
				BattleLogManager.BeginLogBlockEvolution(_card);
			};
			actionProcessor.OnAfterEvolution += (BattleCardBase _card) => BattleLogManager.EndLogBlockEvolution();
		}
		return actionProcessor;
	}

	public virtual VfxBase FusionCard(BattleCardBase card, bool isPlayer, List<BattleCardBase> selectCards)
	{
		if (selectCards != null)
		{
			for (int i = 0; i < selectCards.Count; i++)
			{
				if (selectCards[i] != null)
				{
					selectCards[i].SelfBattlePlayer.AddLastTargetCardsList(selectCards[i]);
				}
			}
		}
		ActionProcessor actionProcessor = CreateFusionActionProcessor(isPlayer);
		bool flag = selectCards.IsNotNullOrEmpty();
		VfxBase vfxBase = actionProcessor.Fusion(card, selectCards);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (isPlayer)
		{
			parallelVfxPlayer.Register(this.OnPlayerFusion.GetAllFuncVfxResults());
			if (flag)
			{
				parallelVfxPlayer.Register(this.OnPlayerBattleCardSelect.GetAllFuncVfxResults());
			}
		}
		return SequentialVfxPlayer.Create(vfxBase, parallelVfxPlayer, InstantVfx.Create(delegate
		{
			if (!_PlayerBattleView.IsMoving())
			{
				_PlayerBattleView.UpdateTurnEndPulseEffect();
			}
			if (_battleMgr.GameMgr.IsWatchBattle)
			{
				_battleMgr.GetBattlePlayer(isPlayer).BattleView.ClearSelectCardList();
			}
		}));
	}

	protected ActionProcessor CreateFusionActionProcessor(bool isPlayer)
	{
		ActionProcessor actionProcessor = CreateActionProcessor(isPlayer);
		actionProcessor.OnBeforeFusion += this.OnBeforeFusion;
		actionProcessor.OnAfterFusion += this.OnAfterFusion;
		if (!_battleMgr.IsVirtualBattle)
		{
			actionProcessor.OnBeforeFusion += delegate(BattleCardBase _card, IEnumerable<BattleCardBase> _ingredientCards)
			{
				BattleLogManager.AddLogFusion(_card, _ingredientCards.ToList());
			};
			actionProcessor.OnAfterFusion += (BattleCardBase _card) => BattleLogManager.EndLogBlockFusion();
		}
		return actionProcessor;
	}

	public virtual VfxBase BattleCardSelect(BattleCardBase actCard, BattleCardBase target, bool isPlayer, bool registerEffectsDirectlyToVfxMgr = true, bool isTransformskill = false, bool isBurialRiteSkill = false, bool isComplete = true)
	{
		return BattleCardSelect(actCard, new List<BattleCardBase> { target }, isPlayer, registerEffectsDirectlyToVfxMgr, isTransformskill, isBurialRiteSkill, isComplete);
	}

	public virtual VfxBase BattleCardSelect(BattleCardBase actCard, List<BattleCardBase> targets, bool isPlayer, bool registerEffectsDirectlyToVfxMgr = true, bool isTransformskill = false, bool isBurialRiteSkill = false, bool isComplete = true)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (isPlayer || _battleMgr.GameMgr.IsAdminWatch || _battleMgr.GameMgr.IsReplayBattle)
		{
			foreach (BattleCardBase target in targets)
			{
				if (target.BattleCardView.GameObject != null)
				{
					_TouchControl._hitCard = null;
					if (isComplete)
					{
						_battleMgr.GetBattlePlayer(isPlayer).BattleView.OnCancelSkillTargetSelect = null;
					}
					if (!registerEffectsDirectlyToVfxMgr)
					{
						sequentialVfxPlayer.Register(CreateCardSelectEffectAndSoundVfx(target));
					}
					else
					{
						_battleMgr.VfxMgr.RegisterImmediateVfx(CreateCardSelectEffectAndSoundVfx(target));
					}
				}
			}
		}
		if (targets.IsNotNullOrEmpty())
		{
			sequentialVfxPlayer.Register(SelectCard(actCard, targets[0], isPlayer, registerEffectsDirectlyToVfxMgr, isTransformskill, isBurialRiteSkill));
		}
		return sequentialVfxPlayer;
	}

	private VfxBase CreateCardSelectEffectAndSoundVfx(BattleCardBase targetCard)
	{
		return InstantVfx.Create(delegate
		{
			_battleMgr.GameMgr.GetEffectMgr().Start(EffectMgr.EffectType.CMN_CARD_SELECT_3, targetCard.BattleCardView.GameObject.transform.position);

		});
	}

	protected VfxBase SelectCard(BattleCardBase actCard, BattleCardBase targetCard, bool isPlayer, bool registerSelectStopVfxDirectlyToVfxMgr = true, bool isTransformskill = false, bool isBurialRiteSkill = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		targetCard.IsSelectedDuringSelectingBurialRiteTarget = isBurialRiteSkill;
		if (isPlayer || _battleMgr.GameMgr.IsAdminWatch || _battleMgr.GameMgr.IsReplayBattle)
		{
			if (registerSelectStopVfxDirectlyToVfxMgr)
			{
				_battleMgr.GetBattlePlayer(isPlayer).BattleView.StopShowSelect(actCard, isAct: true, isTransformskill);
			}
			else
			{
				sequentialVfxPlayer.Register(_battleMgr.GetBattlePlayer(isPlayer).BattleView.CreateStopShowSelectVfx(actCard, isAct: true, stopChoiceSelectUiImmediately: false));
			}
		}
		if (this.OnSkillCardSelect != null)
		{
			this.OnSkillCardSelect(new BattleCardBase[1] { targetCard });
			this.OnSkillCardSelect = null;
		}
		if (this.OnSkillCardSelectSuccess != null)
		{
			sequentialVfxPlayer.Register(this.OnSkillCardSelectSuccess.GetAllFuncVfxResults());
			this.OnSkillCardSelectSuccess = null;
		}
		this.OnBattleCardSelect.Call(targetCard, isPlayer);
		return sequentialVfxPlayer;
	}

	public void SelectCancel(BattleCardBase actCard, bool isPlayer = true, bool isPlay = true, bool isTransformedSkill = false, bool isResetDetail = true)
	{
		BattlePlayerBase battlePlayer = _battleMgr.GetBattlePlayer(isPlayer);
		battlePlayer.BattleView.CancelPlayCard(actCard, isPlay);
		for (int i = 0; i < battlePlayer.HandCardList.Count(); i++)
		{
			battlePlayer.HandCardList[i].IsSelectedDuringSelectingBurialRiteTarget = false;
		}
		if (battlePlayer.HandCardList.Contains(actCard))
		{
			battlePlayer.HandControl.AttachCardView(actCard.BattleCardView);
			actCard.BattleCardView.GameObject.SetActive(value: true);
		}
		_TouchControl._hitCard = null;
		battlePlayer.BattleView.DisableSettingFlag();
		battlePlayer.BattleView.AllClear(popUpClose: false, isRemoveSideLog: true, isStopDrag: false, isResetDetail);
		battlePlayer.BattleView.StopShowSelect(actCard, isAct: false);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		parallelVfxPlayer.Register(actCard.StartHandEffect());
		parallelVfxPlayer.Register(actCard.IsInHand ? actCard.BattleCardView.ShowHandCardInfo() : NullVfx.GetInstance());
		_battleMgr.VfxMgr.RegisterImmediateVfx(parallelVfxPlayer);
		if (actCard.IsInHand)
		{
			_battleMgr.VfxMgr.RegisterImmediateVfx(InstantVfx.Create(delegate
			{
				battlePlayer.HandControl.RearrangeHand(0.3f, battlePlayer.HandCardList.ConvertToViewList());
			}));
		}
		if (_battleMgr.GameMgr.IsWatchBattle)
		{
			_battleMgr.VfxMgr.RegisterImmediateVfx(InstantVfx.Create(delegate
			{
				_battleMgr.GetBattlePlayer(isPlayer).ClassInformationUIController.SetIsSelect(isSelect: false);
			}));
			battlePlayer.BattleView.ClearSelectCardList();
		}
		this.OnSkillCardSelectSuccess = null;
	}

	public void StartSelectCard(BattleCardBase card, bool isEvolve, List<BattleCardBase> selectableCards, bool isChoiceBrave)
	{
		this.OnStartSelect.Call(card, isEvolve, selectableCards, isChoiceBrave);
	}

	public void StartMultipleSelectCard(BattleCardBase card, bool isEvolve, List<BattleCardBase> selectableCards, bool isChoiceBrave)
	{
		this.OnStartMultipleSelect.Call(card, isEvolve, selectableCards, isChoiceBrave);
	}

	public void SelectCard(BattleCardBase card, bool isEvolve, BattleCardBase actCard, bool isChoiceBrave, bool isBurialRiteSkill = false)
	{
		this.OnSelect.Call(card, isEvolve, actCard, isBurialRiteSkill, isChoiceBrave);
	}

	public void CompleteSelectCard(BattleCardBase selectedCard, bool isEvolve, BattleCardBase actCard, bool isChoiceBrave, bool isBurialRiteSkill = false)
	{
		this.OnCompleteSelect.Call(selectedCard, isEvolve, actCard, isChoiceBrave, isBurialRiteSkill);
	}

	public void StartChoiceCard(BattleCardBase card, bool isEvolve, List<BattleCardBase> choiceCards, bool isChoiceBrave)
	{
		this.OnStartChoice.Call(card, isEvolve, choiceCards, isChoiceBrave);
	}

	public void CompleteChoiceCard(BattleCardBase card, bool isEvolve, List<BattleCardBase> cardList, BattleCardBase actCard, List<int> chosenCardIndexList, bool hasSelectionSkill, bool isChoiceBrave)
	{
		this.OnCompleteChoice.Call(card, isEvolve, cardList, actCard, chosenCardIndexList, hasSelectionSkill, isChoiceBrave);
	}

	public void CancelSelect(BattleCardBase card, bool isEvolve, bool isChoiceBrave)
	{
		this.OnCancelSelect.Call(card, isEvolve, isChoiceBrave);
	}

	public void CancelChoice(BattleCardBase card, bool isEvolve, bool isChoiceBrave)
	{
		this.OnCancelChoice.Call(card, isEvolve, isChoiceBrave);
	}

	public void StartFusionSelect(BattleCardBase card, List<BattleCardBase> selectableCards)
	{
		this.OnStartFusion.Call(card, selectableCards);
	}

	public void SelectFusion(int index, bool isActive, bool canFusionMetamorphose, int maxSelectCount, BattleCardBase selectedCard)
	{
		this.OnSelectFusion.Call(index, isActive, maxSelectCount, canFusionMetamorphose);
		this.OnSelectFusionForRecovery.Call(selectedCard);
	}

	public void CancelFusion(BattleCardBase card)
	{
		this.OnCancelFusion.Call(card);
	}

	public void CallOnSkillProcessStart()
	{
		this.OnSkillProcessStart.Call();
	}

	public void CallOnSkillProcessEnd()
	{
		this.OnSkillProcessEnd.Call();
	}

	public void CallOnSkillVfxStart()
	{
		this.OnSkillVfxStart.Call();
	}

	public void CallOnSkillVfxEnd()
	{
		this.OnSkillVfxEnd.Call();
	}

	public JsonData CallOnCreateSideLogCardData(BattleCardBase card, SkillBase skill, bool isDeckSelf, bool isInHand)
	{
		return this.OnCreateSideLogCardData.Call(card, skill, isDeckSelf, isInHand);
	}

	public void CallOnCreateSideLog(BattleCardBase card, SkillBase skill, bool isEvol, bool isOnSummonOrSkill, bool isDeckSelf, bool isInHand, JsonData sideLogCardData)
	{
		this.OnCreateSideLog.Call(card, skill, isEvol, isOnSummonOrSkill, isDeckSelf, isInHand, sideLogCardData);
	}

	public void CallOnClearSideLog(bool isSelf)
	{
		this.OnClearSideLog.Call(isSelf);
	}

	public void CallOnAttachSkill(SkillCreator.SkillBuildInfo buildInfo, List<BattleCardBase> targetCards, BattleCardBase ownerCard)
	{
		this.OnAttachSkill.Call(buildInfo, targetCards, ownerCard);
	}

	public void CallOnEffect(SkillCreator.SkillBuildInfo buildInfo, bool isFollowInHand = false, bool isTargetPosition = false, bool addToLastOperation = false, bool isWhenFusioned = false)
	{
		this.OnCreateEffect.Call(buildInfo, isFollowInHand, isTargetPosition, addToLastOperation, isWhenFusioned);
	}

	public void CallOnShowSkillEffect(BattleCardBase card, List<BattleCardBase> targetCards)
	{
		this.OnShowSkillEffect.Call(card, targetCards);
	}

	public void CallOnSkillInductionEffect(SkillBase skill, bool isIgnoreVoice = false)
	{
		this.OnSkillInductionEffect.Call(skill, isIgnoreVoice);
	}

	public void CallOnShowIndependentEffect(List<BattleCardBase> targetCards)
	{
		this.OnShowIndependentEffect.Call(targetCards);
	}

	public void CallOnChangeAffiliation(BattleCardBase card, List<BattleCardBase> targetCards, CardBasePrm.ClanType clan, CardBasePrm.TribeInfo tribe)
	{
		this.OnChangeAffiliation.Call(card, targetCards, clan, tribe);
	}

	public void CallOnUpdateAttackableEffect(List<BattleCardBase> playerInplayCards, List<BattleCardBase> enemyInplayCards)
	{
		this.OnUpdateAttackableEffect.Call(playerInplayCards, enemyInplayCards);
	}

	public void CallOnUpdateSkillEffect(List<BattleCardBase> cards, bool updateAttackEffect = false, bool useRecordAttackEffect = false, bool isCantAttackSkill = false)
	{
		this.OnUpdateSkillEffect.Call(cards, updateAttackEffect, useRecordAttackEffect, isCantAttackSkill);
	}

	public void CallOnChangeUnionBurstAndSkyboundArt(List<BattleCardBase> targetCards)
	{
		this.OnChangeUnionBurstAndSkyboundArt.Call(targetCards);
	}

	public void CallOnShowRepeatSkillEffect(bool isSelf)
	{
		this.OnShowRepeatSkillEffect.Call(isSelf);
	}

	public void CallOnGiveCantActivateFanfare(BattleCardBase ownerCard, List<BattleCardBase> targetCards)
	{
		this.OnGiveCantActivateFanfare.Call(ownerCard, targetCards);
	}

	public void CallOnDepriveCantActivateFanfare(BattleCardBase ownerCard, List<BattleCardBase> targetCards)
	{
		this.OnDepriveCantActivateFanfare.Call(ownerCard, targetCards);
	}

	public void CallOnLoseSkill(BattleCardBase ownerCard, List<BattleCardBase> targetCards)
	{
		this.OnLoseSkill.Call(ownerCard, targetCards);
	}

	public void CallOnAttachShortageDeckWin(BattleCardBase card)
	{
		this.OnAttachShortageDeckWin.Call(card);
	}

	public void CallOnSpecialWin(BattleCardBase card)
	{
		this.OnSpecialWin.Call(card);
	}

	public void CallOnSpecialLose(BattleCardBase card)
	{
		this.OnSpecialLose.Call(card);
	}

	public void CallOnTurnEndFinish()
	{
		this.OnTurnEndFinish.Call(arg1: true, arg2: true);
	}

	public virtual VfxBase PlayerTurnEnd(bool isAuto = false)
	{
		if (!isAuto)
		{
			this.OnTurnEnd_ButtonPush.Call();
		}
		if (!_battleMgr.BattlePlayer.IsSelfTurn)
		{
			return NullVfx.GetInstance();
		}
		BattleCardBase hitCard = _TouchControl._hitCard;
		if (hitCard != null && hitCard.IsOnMove)
		{
			_TouchControl.StopMovingHandCard(hitCard);
			_TouchControl.Exit();
			EmitHandUtility.SendSelectObject(_battleMgr, null);
		}
		List<BattlePlayerViewBase.BattleDialogItem> list = new List<BattlePlayerViewBase.BattleDialogItem>();
		list.Add(BattlePlayerViewBase.BattleDialogItem.Menu);
		list.Add(BattlePlayerViewBase.BattleDialogItem.Retire);
		_PlayerBattleView.ClearDifferentiatePopUp(list);
		_battleMgr.BattlePlayer.IsChoiceBraveEffectTiming = false;
		_PlayerBattleView.AllClear();
		_PlayerBattleView.ShowPlayerTurnEnd(isAuto);
		EmitHandUtility.SendSelectObject(_battleMgr, null);
		if (isAuto)
		{
			return NullVfx.GetInstance();
		}
		return TurnEndOperation(isPlayer: true);
	}

	public virtual VfxBase TurnEndOperation(bool isPlayer)
	{
		if (isPlayer)
		{
			this.OnBeforePlayerTurnEnd.Call();
			if (_battleMgr.GameMgr.IsWatchBattle)
			{
				_battleMgr.BattlePlayer.IsChoiceBraveEffectTiming = false;
				_battleMgr.BattlePlayer.BattleView.UpdateChoiceBraveButtonPulsateEffectAndSprite();
			}
		}
		else
		{
			_battleMgr.BattleUIContainer.DisableMenu();
			_battleMgr.BattlePlayer.IsTurnStartEffectNotFinished = true;
			if (_battleMgr is NetworkBattleManagerBase)
			{
				((NetworkBattleManagerBase)_battleMgr).SetTimeDecrementFlag(isDecrement: true);
			}
			if (_battleMgr.GameMgr.IsAdminWatch)
			{
				_battleMgr.BattleEnemy.UpdateHandCardsPlayability(areArrowsForcedOff: true);
			}
			_battleMgr.BattleEnemy.IsChoiceBraveEffectTiming = false;
			_battleMgr.BattleEnemy.BattleEnemyView.UpdateChoiceBraveButtonPulsateEffectAndSprite();
		}
		_battleMgr.BattlePlayer.UpdateHandCardsPlayability(areArrowsForcedOff: true);
		VfxBase result = _battleMgr.TurnEnd(isPlayer);
		this.OnTurnEnd.Call();
		return result;
	}

	public void AllClearBattleView()
	{
		_PlayerBattleView.AllClear(popUpClose: true);
	}
}
