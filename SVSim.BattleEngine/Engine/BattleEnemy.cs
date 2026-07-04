using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.Player.Emotion;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class BattleEnemy : BattlePlayerBase
{

	private IEmotion _emotion;

	private readonly Vector3 FIELD_CENTER_POSITION = new Vector3(0f, 0.25f, 0f);

	public override bool IsGameFirst => !base.BattleMgr.IsFirst;

	public override bool IsPlayer => false;

	public override IBattlePlayerView BattleView => BattleEnemyView;

	public override IEmotion Emotion => _emotion;

	public virtual IBattlePlayerView BattleEnemyView { get; protected set; }

	public bool EnableEnemyAI { get; set; }

	public override int Turn
	{
		get
		{
			if (!base.BattleMgr.IsFirst)
			{
				return base.BattleMgr.FirstTurn;
			}
			return base.BattleMgr.SecondTurn;
		}
		set
		{
			if (base.BattleMgr.IsFirst)
			{
				base.BattleMgr.SecondTurn = value;
			}
			else
			{
				base.BattleMgr.FirstTurn = value;
			}
		}
	}

	public event Action<List<int>> OnMulliganEndForReplay;

	public BattleEnemy(BattleManagerBase battleMgr, BattleCamera battleCamera, BackGroundBase backGround, IInnerOptionsBuilder innerOptionsBuilder)
		: base(battleMgr, battleCamera, backGround, innerOptionsBuilder)
	{
	}

	protected override void Initialize()
	{
		BattleEnemyView = new BattleEnemyView(this);
	}

	protected override void CreateSelfBattleCard()
	{
		EnemyClassBattleCard item = new EnemyClassBattleCard(new ClassBattleCardBase.ClassBuildInfo(_isPlayer: false, 20, this, base.BattleMgr.BattlePlayer, base.BattleMgr, base.BattleMgr.BattleResourceMgr));
		base.ClassAndInPlayCardList.Add(item);
	}

	public override void Setup(BattlePlayerBase opponentBattlePlayer)
	{
		_emotion = _innerOptionsBuilder.CreateEnemyEmotion((IClassBattleCardView)base.Class.BattleCardView);
		base.Setup(opponentBattlePlayer);
	}

	public override void SetupClone(BattlePlayerBase sourceBattlePlayer, BattlePlayerBase virtualOpponentBattlePlayer, CloneActualFlags cloneFlags)
	{
		sourceBattlePlayer.CopyToVirtualBase(this, virtualOpponentBattlePlayer, cloneFlags);
	}

	public override VfxBase StartTurnControl(string log = "")
	{
		if (BattleMgr.GameMgr.IsAdminWatch)
		{
			UpdateHandCardsPlayability();
		}
		Turn++;
		SequentialVfxPlayer sequentialVfxPlayer = TurnEvolveControl(BattleView.EpIcon);
		VfxBase vfx = TurnStart();
		sequentialVfxPlayer.Register(vfx);
		VfxBase vfx2 = BattleMgr.JudgeBattleResult();
		sequentialVfxPlayer.Register(vfx2);
		sequentialVfxPlayer.Register(CreateThinkingVfx(base.BattleMgr));
		return sequentialVfxPlayer;
	}

	public VfxBase CreateThinkingVfx(BattleManagerBase battleMgr)
	{
		if (BattleMgr.GameMgr.IsAdminWatch)
		{
			return NullVfx.GetInstance();
		}
		return NullVfx.GetInstance();
	}

	public override VfxBase UsePp(int pp, bool isNewReplayMoveTurn = false)
	{
		base.UsePp(pp);
		int usedPp = base.Pp;
		int maxPp = base.PpTotal;
		Vector3 labelPosition = default(Vector3);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			Vector3 position = base.BattleCamera.Get3DCamera().WorldToScreenPoint(StatusPanelControl.GetPPPanel().transform.Find("PPIcon/PPLabel").transform.position);
			labelPosition = UIManager.GetInstance().getCamera().ScreenToWorldPoint(position);
		}));
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		return sequentialVfxPlayer;
	}

	protected override VfxBase TurnStartDrawCard(SkillProcessor skillProcessor)
	{
		NullVfx.GetInstance();
		int drawCount = ((IsGameFirst || Turn != 1) ? 1 : 2);
		VfxWith<IEnumerable<BattleCardBase>> vfxWith = RandomCardDraw(drawCount, skillProcessor);
		VfxBase vfxBase = CardDrawVfx(vfxWith.Value);
		SequentialVfxPlayer result = SequentialVfxPlayer.Create(vfxWith.Vfx, vfxBase);
		if (!base.Class.IsDead && EnableEnemyAI)
		{
			base.BattleMgr.EnemyAI.ExecuteEnemyAI(useWait: true);
		}
		_ = base.Class.IsDead;
		return result;
	}

	public override VfxBase CardDrawVfx(IEnumerable<BattleCardBase> DrawList, bool skipShuffle = false, bool isOpenDrawSkill = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (BattleMgr.GameMgr.IsAdminWatch)
		{
			foreach (BattleCardBase card in DrawList)
			{
				if (card.BaseCost != card.Cost)
				{
					List<int> costList = card.BattleCardView.GetUseCostList(card.Cost);
					sequentialVfxPlayer.Register(InstantVfx.Create(delegate
					{
						card.BattleCardView.UpdateCost(costList);
					}));
				}
			}
		}
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		return sequentialVfxPlayer;
	}

	public override VfxBase TurnEnd()
	{
		ParallelVfxPlayer result = ParallelVfxPlayer.Create(base.TurnEnd(), NullVfx.GetInstance());
		if (BattleMgr.GameMgr.IsAdminWatch)
		{
			foreach (BattleCardBase handCard in base.HandCardList)
			{
				handCard.BattleCardView.HideCanPlayEffect();
			}
		}
		return result;
	}

	protected override void SetActive()
	{
		if (BattleMgr.GameMgr.IsAdminWatch)
		{
			UpdateHandCardsPlayability();
		}
		if (!IsGameFirst || Turn != 1)
		{
			base.IsChoiceBraveEffectTiming = true;
			BattleEnemyView.UpdateChoiceBraveButtonPulsateEffectAndSprite();
		}
	}

	public override BattlePlayerBase CreateVirtualPlayer()
	{
		return new VirtualBattleEnemy(base.BattleMgr, base.BattleCamera, base.BackGround);
	}

	public override void UpdateHandCardsPlayability(bool areArrowsForcedOff = false)
	{
		foreach (BattleCardBase handCard in _opponentBattlePlayer.HandCardList)
		{
			handCard.BattleCardView.areArrowsForcedOff = areArrowsForcedOff;
			handCard.BattleCardView.UpdateMovability();
		}
		if (!BattleMgr.GameMgr.IsAdmin)
		{
			return;
		}
		foreach (BattleCardBase handCard2 in base.HandCardList)
		{
			handCard2.BattleCardView.areArrowsForcedOff = areArrowsForcedOff;
			handCard2.BattleCardView.UpdateMovability();
		}
		if (base.IsSelfTurn)
		{
			BattleView.UpdateChoiceBraveButtonPulsateEffectAndSprite();
		}
	}

	public override VfxBase MoveToHand(List<BattleCardBase> cardsToMoveToHand)
	{
		return SequentialVfxPlayer.Create(NullVfx.GetInstance(), InstantVfx.Create(delegate
		{
			UpdateHandCardsPlayability();
		}));
	}

	public override EffectBattle GetSkillEffect(string skillEffectPath)
	{
		return BattleMgr.GameMgr.GetEffectMgr().GetEnemyEffectBattle(skillEffectPath);
	}

	public override Vector3 GetFieldCenterPosition()
	{
		return FIELD_CENTER_POSITION;
	}

	public override VfxBase TurnStartDraw(SkillProcessor skillProcessor)
	{
		return base.TurnStartDraw(skillProcessor);
	}

	public void CallRecordingMulliganEnd(List<int> cardIndexList)
	{
		this.OnMulliganEndForReplay.Call(cardIndexList);
	}
}
