using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Player.Emotion;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class BattlePlayer : BattlePlayerBase
{
	private readonly Vector3 FIELD_CENTER_POSITION = new Vector3(0f, -0.3f, 0f);

	private BattleUIContainer _battleUIContainer;

	protected VfxBase _canNotTouchCardVfx;

	public bool _isPlayerActive;

	public int PlayCardTouchCount;

	public bool IsTimeOverTurnEndProcessing;

	public bool IsDuringChoiceBrave;

	public override bool IsGameFirst => base.BattleMgr.IsFirst;

	public override bool IsPlayer => true;

	public override IBattlePlayerView BattleView => PlayerBattleView;

	public virtual IPlayerView PlayerBattleView { get; protected set; }

	public override IEmotion Emotion => PlayerEmotion;

	public IPlayerEmotion PlayerEmotion { get; protected set; }

	public bool IsTurnStartEffectNotFinished { get; set; }

	public override bool CanChoiceBraveThisTurn
	{
		get
		{
			if (!base.IsAlreadyChoiceBraveInThisTurn && !IsTimeOverTurnEndProcessing)
			{
				return base.IsChoiceBraveEffectTiming;
			}
			return false;
		}
	}

	public override bool CanChoiceBrave
	{
		get
		{
			if (CanChoiceBraveThisTurn && base.CanPlayAnyChoiceBraveCard && BattleView.IsTouchable() && !BattleView.IsSelecting && !CantPlayChoiceBrave)
			{
				return !PlayerBattleView.IsMoving();
			}
			return false;
		}
	}

	public event Func<VfxBase> OnAfterPlayerTurnStart;

	public event Action OnPlayerActive;

	public event Action<List<BattleCardBase>> OnMulliganEndForReplay;

	public BattlePlayer(BattleManagerBase battleMgr, BattleCamera battleCamera, BackGroundBase backGround, IInnerOptionsBuilder innerOptionsBuilder)
		: base(battleMgr, battleCamera, backGround, innerOptionsBuilder)
	{
	}

	protected override void Initialize()
	{
		PlayerBattleView = new BattlePlayerView(this);
	}

	protected override void CreateSelfBattleCard()
	{
		PlayerClassBattleCard item = new PlayerClassBattleCard(new ClassBattleCardBase.ClassBuildInfo(_isPlayer: true, 20, this, base.BattleMgr.BattleEnemy, base.BattleMgr, base.BattleMgr.BattleResourceMgr));
		base.ClassAndInPlayCardList.Add(item);
	}

	public override void Setup(BattlePlayerBase opponentBattlePlayer)
	{
		if (_battleUIContainer == null && !(this is VirtualBattlePlayer) && (base.IsSelfTurn || IsTurnStartEffectNotFinished))
		{
			_battleUIContainer = base.BattleMgr.BattleUIContainer;
			_battleUIContainer.DisableMenu();
		}
		PlayerEmotion = _innerOptionsBuilder.CreatePlayerEmotion((IClassBattleCardView)base.Class.BattleCardView);
		base.OnTurnEnd += (SkillProcessor skill) => InstantVfx.Create(delegate
		{
			if (!BattleMgr.GameMgr.IsWatchBattle)
			{
				PlayerBattleView.TurnEndButtonUI.ChangeButtonView(base.IsSelfTurn);
			}
		});
		opponentBattlePlayer.OnTurnStartAfterDraw += () => InstantVfx.Create(delegate
		{
			EnableBattleMenu();
			ITurnEndButtonUI turnEndButtonUI = PlayerBattleView.TurnEndButtonUI;
			if (!turnEndButtonUI.GameObject.activeSelf)
			{
				BattleMgr.GameMgr.GetEffectMgr().Start(EffectMgr.EffectType.CMN_UI_TURN_1, turnEndButtonUI.GetBtnPosition());
			}
			turnEndButtonUI.GameObject.SetActive(value: true);
			turnEndButtonUI.EnableButton();
			turnEndButtonUI.HideBtn();
			turnEndButtonUI.ChangeButtonView(base.IsSelfTurn);
		});
		base.Setup(opponentBattlePlayer);
	}

	public override void SetupClone(BattlePlayerBase sourceBattlePlayer, BattlePlayerBase virtualOpponentBattlePlayer, CloneActualFlags cloneFlags)
	{
		sourceBattlePlayer.CopyToVirtualBase(this, virtualOpponentBattlePlayer, cloneFlags);
	}

	public override void SetupCardEvent(BattleCardBase card)
	{
		base.SetupCardEvent(card);
		card.OnPlay += delegate
		{
			foreach (BattleCardBase item in base.HandCardList.Where((BattleCardBase c) => c != card))
			{
				item.BattleCardView.UpdateMovability();
			}
			BattleView.UpdateChoiceBraveButtonPulsateEffectAndSprite();
			return NullVfx.GetInstance();
		};
	}

	public override VfxBase TurnStart()
	{
		VfxBase vfxBase = base.TurnStart();
		if (base.BattleMgr.IsRecovery)
		{
			EnableBattleMenu();
			if (base.IsSelfTurn)
			{
				_isPlayerActive = true;
			}
		}
		return SequentialVfxPlayer.Create(vfxBase, InstantVfx.Create(PlayerBattleView.UpdateTurnEndPulseEffect));
	}

	public void TurnStartEffectEnd()
	{
		IsTurnStartEffectNotFinished = false;
	}

	private void EnableBattleMenu()
	{
		if (_battleUIContainer != null)
		{
			_battleUIContainer.EnableMenu();
		}
	}

	public override VfxBase StartTurnControl(string log = "")
	{
		if (_canNotTouchCardVfx == null)
		{
			_canNotTouchCardVfx = NullVfx.GetInstance();
			BattleMgr.VfxMgr.RegisterImmediateVfx(_canNotTouchCardVfx);
		}
		if (BattleMgr.GameMgr.IsNetworkBattle)
		{
			NetworkBattleManagerBase networkBattleManagerBase = BattleMgr as NetworkBattleManagerBase;
			if (networkBattleManagerBase.turnEndTimeController != null)
			{
				networkBattleManagerBase.turnEndTimeController.AddTurnEndTimerLog("TurnStart" + log);
			}
		}
		PlayerEmotion.ResetPlayCount();
		Turn++;
		SequentialVfxPlayer sequentialVfxPlayer = TurnEvolveControl(PlayerBattleView.EpIcon);
		VfxBase vfx = TurnStart();
		sequentialVfxPlayer.Register(vfx);
		VfxBase allFuncVfxResults = this.OnAfterPlayerTurnStart.GetAllFuncVfxResults();
		this.OnAfterPlayerTurnStart = null;
		sequentialVfxPlayer.Register(allFuncVfxResults);
		VfxBase vfx2 = base.BattleMgr.JudgeBattleResult();
		sequentialVfxPlayer.Register(vfx2);
		return sequentialVfxPlayer;
	}

	public override VfxBase UsePp(int pp, bool isNewReplayMoveTurn = false)
	{
		base.UsePp(pp);
		if (this.BattleMgr.InstanceIsForecast)
		{
			return NullVfx.GetInstance();
		}
		int pp2 = base.Pp;
		Vector3 zero = Vector3.zero;
		zero = BattleView.GetPPLabelPosition();
		return m_vfxCreator.CreateUsePp(pp2, base.PpTotal, zero, isNewReplayMoveTurn);
	}

	protected override VfxBase TurnStartDrawCard(SkillProcessor skillProcessor)
	{
		int drawCount = ((IsGameFirst || Turn != 1) ? 1 : 2);
		VfxWith<IEnumerable<BattleCardBase>> vfxWith = RandomCardDraw(drawCount, skillProcessor);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(vfxWith.Vfx);
		sequentialVfxPlayer.Register(CardDrawVfx(vfxWith.Value));
		BattleLogManager.GetInstance().AddLogOverDrawCards(vfxWith.Value.Where((BattleCardBase s) => !s.IsInHand).ToList());
		return sequentialVfxPlayer;
	}

	public override VfxBase TurnEnd()
	{
		bool flag = false;
		if (BattleMgr.VfxMgr.IsEnd && IsTimeOverTurnEndProcessing)
		{
			base.HandControl.RearrangeHand(0.4f, base.HandCardList.ConvertToViewList());
			flag = true;
		}
		_isPlayerActive = false;
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create(base.TurnEnd());
		foreach (BattleCardBase handCard in base.HandCardList)
		{
			handCard.BattleCardView.HideCanPlayEffect();
		}
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			base.NowTurnEvol = true;
		}));
		if (IsTimeOverTurnEndProcessing && !flag)
		{
			sequentialVfxPlayer.Register(InstantVfx.Create(delegate
			{
				base.HandControl.RearrangeHand(0.4f, base.HandCardList.ConvertToViewList());
			}));
		}
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			IsTimeOverTurnEndProcessing = false;
		}));
		return sequentialVfxPlayer;
	}

	public override void HandCardToField(BattleCardBase targetCard, SkillBase skill = null)
	{
		base.HandCardToField(targetCard, skill);
		if (base.HandCardList.Count <= 0)
		{
			base.BattleMgr.VfxMgr.RegisterImmediateVfx(BattleView.HandUnfocus());
		}
	}

	protected override void SetActive()
	{
		PlayerActive();
		this.OnPlayerActive.Call();
		_isPlayerActive = true;
	}

	protected override void PlayerActive()
	{
		TurnStartEffectEnd();
		EnableBattleMenu();
		if (!BattleMgr.GameMgr.IsWatchBattle && !BattleMgr.GameMgr.IsReplayBattle)
		{
			ITurnEndButtonUI turnEndButtonUI = PlayerBattleView.TurnEndButtonUI;
			turnEndButtonUI.StartTurnEndCountdown();
			turnEndButtonUI.ChangeButtonView(base.IsSelfTurn);
			BattleMgr.GameMgr.GetEffectMgr().Start(EffectMgr.EffectType.CMN_UI_TURN_1, turnEndButtonUI.GetBtnPosition());
		}
		_canNotTouchCardVfx = null;
		if (!IsGameFirst || Turn != 1)
		{
			base.IsChoiceBraveEffectTiming = true;
			PlayerBattleView.UpdateChoiceBraveButtonPulsateEffectAndSprite();
		}
	}

	public override BattlePlayerBase CreateVirtualPlayer()
	{
		return new VirtualBattlePlayer(base.BattleMgr, base.BattleCamera, base.BackGround);
	}

	public override void UpdateHandCardsPlayability(bool areArrowsForcedOff = false)
	{
		foreach (BattleCardBase handCard in base.HandCardList)
		{
			handCard.BattleCardView.areArrowsForcedOff = areArrowsForcedOff;
			handCard.BattleCardView.UpdateMovability();
		}
		if (base.IsSelfTurn && !BattleMgr.GameMgr.IsNewReplayBattle)
		{
			CantPlayChoiceBrave = areArrowsForcedOff;
			BattleView.UpdateChoiceBraveButtonPulsateEffectAndSprite();
		}
	}

	public override VfxBase MoveToHand(List<BattleCardBase> cardsToMoveToHand)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase item in cardsToMoveToHand)
		{
			parallelVfxPlayer.Register(item.CreateMoveToHandVfx());
		}
		return SequentialVfxPlayer.Create(parallelVfxPlayer, InstantVfx.Create(delegate
		{
			UpdateHandCardsPlayability();
		}));
	}

	public override VfxBase CardDrawVfx(IEnumerable<BattleCardBase> cards, bool skipShuffle = false, bool isOpenDrawSkill = false)
	{
		return m_vfxCreator.CreateCardDraw(cards, isOpenDrawSkill);
	}

	public override EffectBattle GetSkillEffect(string skillEffectPath)
	{
		return BattleMgr.GameMgr.GetEffectMgr().GetEffectBattle(skillEffectPath);
	}

	public override Vector3 GetFieldCenterPosition()
	{
		return FIELD_CENTER_POSITION;
	}

	public void CallRecordingMulliganEnd(List<BattleCardBase> cards)
	{
		this.OnMulliganEndForReplay.Call(cards);
	}
}
