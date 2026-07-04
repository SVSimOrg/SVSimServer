using System;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard;
using Wizard.Battle.Card;
using Wizard.Battle.Card.InnerOptions;
using Wizard.Battle.Resource;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public abstract class ClassBattleCardBase : BattleCardBase
{
	public class ClassBuildInfo
	{
		public int charaId;

		public bool isPlayer;

		public int life;

		public BattlePlayerBase selfBattlePlayer;

		public BattlePlayerBase opponentBattlePlayer;

		public BattleManagerBase battleMgr;

		public IBattleResourceMgr resourceMgr;

		public ClassBuildInfo(bool _isPlayer, int _life, BattlePlayerBase _selfBattlePlayer, BattlePlayerBase _opponentBattlePlayer, BattleManagerBase _battleMgr, IBattleResourceMgr _resourceMgr)
		{
			isPlayer = _isPlayer;
			life = _life;
			selfBattlePlayer = _selfBattlePlayer;
			opponentBattlePlayer = _opponentBattlePlayer;
			battleMgr = _battleMgr;
			resourceMgr = _resourceMgr;
		}

		public ClassBuildInfo VirtualClone(BattlePlayerBase virtualSelfBattlePlayer, BattlePlayerBase virtualOpponentBattlePlayer)
		{
			return new ClassBuildInfo(isPlayer, life, virtualSelfBattlePlayer, virtualOpponentBattlePlayer, battleMgr, resourceMgr);
		}
	}

	protected readonly ClassBuildInfo _classBuildInfo;

	protected int _baseMaxLife;

	public int BossRushStartLife;

	private static SkillCreator.CardSkillsBuildInfo _sharedEmptySkillInfo;

	public override int BaseMaxLife => _baseMaxLife;

	public override bool IsDead
	{
		get
		{
			if (base.IsDead)
			{
				return true;
			}
			if (base.SelfBattlePlayer.IsShortageDeck && !base.SelfBattlePlayer.IsShortageDeckWin)
			{
				return true;
			}
			if (base.OpponentBattlePlayer.IsShortageDeck && base.OpponentBattlePlayer.IsShortageDeckWin)
			{
				return true;
			}
			return false;
		}
	}

	public override bool IsLifeZeroDead
	{
		get
		{
			if (base.Life <= 0)
			{
				return !base.SkillApplyInformation.IsLifeZeroActivateLeonSkill;
			}
			return false;
		}
	}

	public override bool Attackable => false;

	public override bool IsClass => true;

	public override bool IsOnDraw => false;

	public override bool IsCantAttackClass => base.SkillApplyInformation.IsSkillCantAtkClass;

	public IClassBattleCardView ClassBattleCardView { get; private set; }

	public event Action OnDamageDestroy;

	public event Action<BattlePlayerBase, int> OnForceBerserkChange;

	public event Func<bool, VfxBase> OnBerserkCheck;

	public event Action<BattlePlayerBase, int> OnForceAvariceChange;

	public event Func<bool, VfxBase> OnAvariceCheck;

	public event Action<BattlePlayerBase, int> OnForceWrathChange;

	public event Func<bool, VfxBase> OnWrathCheck;

	public event Func<BattleCardBase, SkillProcessor, VfxBase> OnRetire;

	public VfxBase GetOnBerserkCheck(bool flg)
	{
		return this.OnBerserkCheck.GetAllFuncVfxResults(flg);
	}

	public void CallOnForceBerserkChange(int num)
	{
		this.OnForceBerserkChange.Call(base.SelfBattlePlayer, num);
	}

	public VfxBase GetOnAvariceCheck(bool flag)
	{
		return this.OnAvariceCheck.GetAllFuncVfxResults(flag);
	}

	public void CallOnForceAvariceChange(int number)
	{
		this.OnForceAvariceChange.Call(base.SelfBattlePlayer, number);
	}

	public VfxBase GetOnWrathCheck(bool flag)
	{
		return this.OnWrathCheck.GetAllFuncVfxResults(flag);
	}

	public void CallOnForceWrathChange(int number)
	{
		this.OnForceWrathChange.Call(base.SelfBattlePlayer, number);
	}

	protected ClassBattleCardBase(ClassBuildInfo classBuildInfo)
		: base(CreateBaseBuildInfo(classBuildInfo))
	{
		_classBuildInfo = classBuildInfo;
	}

	public override void Setup(bool createNullView = false, bool isRecreate = false)
	{
		base.Setup();
		_CacheBattlePlayer();
		ClassBattleCardView = (IClassBattleCardView)base.BattleCardView;
	}

	public void InitBaseMaxLife(int baseMaxLife)
	{
		_baseMaxLife = baseMaxLife;
	}

	protected virtual void _CacheBattlePlayer()
	{
		base.SelfBattlePlayer = (base.IsPlayer ? ((BattlePlayerBase)_classBuildInfo.battleMgr.BattlePlayer) : ((BattlePlayerBase)_classBuildInfo.battleMgr.BattleEnemy));
		base.OpponentBattlePlayer = ((!base.IsPlayer) ? ((BattlePlayerBase)_classBuildInfo.battleMgr.BattlePlayer) : ((BattlePlayerBase)_classBuildInfo.battleMgr.BattleEnemy));
	}

	public override DamageResult ApplyDamage(SkillBase skill, DamageParam damageParam, bool doesAttackerPossessKiller, bool isReflectedDamage, SkillProcessor skillProcessor, BattleCardBase reflectCard)
	{
		int damage = damageParam.Damage;
		bool isSkillDamage = skill != null;
		bool isSpellDamage = skill?.SkillPrm.ownerCard.IsSpell ?? false;
		BattleCardBase damageReflectionTarget = GetDamageReflectionTarget(isSkillDamage);
		if (damageReflectionTarget != this)
		{
			return damageReflectionTarget.ApplyDamage(skill, damageParam, doesAttackerPossessKiller: false, isReflectedDamage: true, skillProcessor, this);
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		damageParam.Damage = CalculateFinalDamageAmount(damageParam.Damage, isSkillDamage, isSpellDamage, parallelVfxPlayer);
		int damage2 = damageParam.Damage;
		new SkillConditionCheckerOption
		{
			DefaultDamage = new DamageInfo(skill, damage),
			FixedDamage = new DamageInfo(skill, damage2)
		};
		BattleManagerBase ins = _buildInfo.BattleMgr;
		base.SkillApplyInformation.DamageLife(damageParam.Damage, ins.CurrentTurn, ins.BattlePlayer.IsSelfTurn);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(ParallelVfxPlayer.Create(CreateVfxWithCardPlayabilityRefresh(NullVfx.GetInstance()), parallelVfxPlayer));
		SequentialVfxPlayer sequentialVfxPlayer2 = SequentialVfxPlayer.Create();
		if (IsDead)
		{
			sequentialVfxPlayer2.Register(CreatePullHandInVfx());
			this.OnDamageDestroy.Call();
		}
		base.SelfBattlePlayer.BattleMgr.VfxMgr.RegisterImmediateVfx(this.OnBerserkCheck.GetAllFuncVfxResults(arg1: false));
		skillProcessor?.Register(base.Skills.CreateWhenDamageInfo(skill, skillProcessor, new BattlePlayerReadOnlyInfoPair(base.SelfBattlePlayer, base.OpponentBattlePlayer), damage, damageParam.Damage));
		sequentialVfxPlayer.Register(base.ApplyDamage(skill, damageParam, doesAttackerPossessKiller, isReflectedDamage, skillProcessor, reflectCard).Vfx);
		sequentialVfxPlayer.Register(base.SelfBattlePlayer.StartSkillWhenChangeClassLife(skillProcessor));
		return new DamageResult(sequentialVfxPlayer, damageParam.Damage, damage2, sequentialVfxPlayer2, null, isReflectedDamage);
	}

	public override HealResult ApplyHealing(HealParam healParam, SkillProcessor skillProcessor)
	{
		BattleManagerBase ins = _buildInfo.BattleMgr;
		int num = HealLife(healParam.HealAmount, ins.CurrentTurn, ins.BattlePlayer.IsSelfTurn);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(CreateVfxWithCardPlayabilityRefresh(NullVfx.GetInstance()));
		new SkillConditionCheckerOption().HealingCardAndValue = new List<BattlePlayerBase.CardAndValue>
		{
			new BattlePlayerBase.CardAndValue(this, num)
		};
		base.SelfBattlePlayer.BattleMgr.VfxMgr.RegisterImmediateVfx(this.OnBerserkCheck.GetAllFuncVfxResults(arg1: false));
		if (skillProcessor != null)
		{
			sequentialVfxPlayer.Register(base.SelfBattlePlayer.StartSkillWhenChangeClassLife(skillProcessor));
		}
		return new HealResult(num, sequentialVfxPlayer, CreatePullHandInVfx());
	}

	private VfxBase CreatePullHandInVfx()
	{
		return base.SelfBattlePlayer.BattleView.HandView.HandUnfocus();
	}

	public VfxBase DestroyBySpecialWin()
	{
		return NullVfx.GetInstance();
	}

	public VfxBase Retire()
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(this.OnRetire.GetAllFuncVfxResults(this, new SkillProcessor()));
		return sequentialVfxPlayer;
	}

	public VfxBase LifeZeroActivateLeonSkill()
	{
		if (base.IsDestroyedBySkill || base.SelfBattlePlayer.IsShortageDeck || base.OpponentBattlePlayer.Class.IsDead || (base.OpponentBattlePlayer.IsShortageDeck && base.OpponentBattlePlayer.IsShortageDeckWin))
		{
			base.SkillApplyInformation.DepriveLifeZeroActivateLeonSkill();
			return NullVfx.GetInstance();
		}
		int num = 10;
		int cardId = 104741020;
		string fileName = "stt_quest_leon_1";
		string criSeName = "se_stt_quest_leon_1";
		float waitTime = 0.6f;
		string fileName2 = "stt_quest_leon_2";
		string criSeName2 = "se_stt_quest_leon_2";
		float waitTime2 = 0.5f;
		float num2 = 2f;
		float num3 = 3.5f;
		float waitTime3 = 0f;
		float waitTime4 = 1.5f;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		list.AddRange(base.SelfBattlePlayer.InPlayCards);
		list.AddRange(base.OpponentBattlePlayer.InPlayCards);
		VfxBase vfxBase = null;
		vfxBase = ((!base.OpponentBattlePlayer.IsSelfTurn) ? WaitVfx.Create(waitTime3) : WaitVfx.Create(waitTime4));
		parallelVfxPlayer.Register(SequentialVfxPlayer.Create(vfxBase, base.SelfBattlePlayer.Emotion.PlayEmotion(ClassCharaPrm.EmotionType.PROVOCATION, 0f)));
		sequentialVfxPlayer.Register(WaitVfx.Create(base.OpponentBattlePlayer.IsSelfTurn ? num3 : num2));
		MaxLifeSetModifier lifeModifier = new MaxLifeSetModifier(num);
		sequentialVfxPlayer.Register(base.SkillApplyInformation.GiveCombatValueModifier(null, lifeModifier, new SkillProcessor()));
		int num4 = ((base.Life < 0) ? (base.Life * -1) : 0);
		if (num4 != 0)
		{
			HealLife(num4, base.SelfBattlePlayer.Turn, base.SelfBattlePlayer.IsSelfTurn);
		}
		sequentialVfxPlayer.Register(VfxWithLoadingSequential.Create());
		HealParam healParam = new HealParam(num, this, this, applyModifier: false);
		HealResult healResult = ApplyHealing(healParam, new SkillProcessor());
		sequentialVfxPlayer.Register(healResult.PrehealVfxVfx);
		sequentialVfxPlayer.Register(healResult.HealVfx);
		sequentialVfxPlayer.Register(healResult.PosthealVfxVfx);
		base.SkillApplyInformation.DepriveLifeZeroActivateLeonSkill();
		sequentialVfxPlayer.Register(VfxWithLoadingSequential.Create());
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].SkillApplyInformation.IsIndependent)
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
				continue;
			}
			list[i].FlagCardAsDestroyedBySkill();
			parallelVfxPlayer2.Register(list[i].SelfBattlePlayer.CardManagement(list[i], new SkillProcessor(), BattlePlayerBase.CARD_MANAGEMENT.BANISH, isRandom: false));
		}
		sequentialVfxPlayer.Register(parallelVfxPlayer2);
		SkillBaseSummon.SummonedCardsList summonedCardsList = new SkillBaseSummon.SummonedCardsList();
		summonedCardsList.AddCardToSummonedCards(base.SelfBattlePlayer.CreateNextIndexCard(cardId));
		BattlePlayerBase.SummonInfo summonInfo = new BattlePlayerBase.SummonInfo(base.SelfBattlePlayer.IsPlayer, summonedCardsList, SkillBaseSummon.SUMMON_TYPE.TOKEN);
		VfxWithLoadingSequential vfxWithLoadingToRegister = base.SelfBattlePlayer.CardManagement(null, new SkillProcessor(), BattlePlayerBase.CARD_MANAGEMENT.SUMMON, isRandom: false, null, null, null, summonInfo) as VfxWithLoadingSequential;
		base.SelfBattlePlayer.UpdateHandCardsPlayability();
		StartPickMultiCardVfx vfxToRegister = new StartPickMultiCardVfx(summonedCardsList, _buildInfo.BattleMgr.BattleResourceMgr, base.SelfBattlePlayer.IsPlayer, isToken: true, isIgnoreVoice: false, isRandomVoice: false, isGetoff: false, isEvoVoice: false, 0f);
		if (!base.SelfBattlePlayer.BattleMgr.IsVirtualBattle)
		{
			BattleLogManager.GetInstance().BeginLogBlockTurnChangeReactive();
			BattleLogManager.GetInstance().AddLogSkillBuffSetLife(this, Wizard.Battle.UI.LogType.WhenDestroy, new List<BattleCardBase> { this }, num, isTargetInOpponentHand: false);
			BattleLogManager.GetInstance().AddLogSkillHeal(new List<BattleCardBase> { this }, new List<HealResult> { healResult });
			BattleLogManager.GetInstance().AddLogSkillDeath(list.Where((BattleCardBase c) => !c.SkillApplyInformation.IsIndependent).ToList());
			BattleLogManager.GetInstance().AddLogSkillSummon(summonedCardsList.summonedCards.ToList());
			BattleLogManager.GetInstance().EndLogBlockTurnChangeReactive();
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterToMainVfx(vfxToRegister);
		vfxWithLoadingSequential.RegisterVfxWithLoading(vfxWithLoadingToRegister);
		sequentialVfxPlayer.Register(vfxWithLoadingSequential);
		parallelVfxPlayer.Register(sequentialVfxPlayer);
		return parallelVfxPlayer;
	}

	public override VfxBase LoadResource(bool isLogging = false)
	{
		return ClassBattleCardView.LoadResource();
	}

	public override VfxBase UnloadResource()
	{
		return ClassBattleCardView.UnloadResource();
	}

	public override VfxBase RecoveryInPlay(int inPlayIndex, bool newReplayMoveTurn = false)
	{
		return SequentialVfxPlayer.Create(base.BattleCardView.RecoveryInPlay(), NullVfx.GetInstance());
	}

	public override BattleCardBase VirtualClone(BattlePlayerBase virtualSelfBattlePlayer, BattlePlayerBase virtualOpponentBattlePlayer)
	{
		VirtualClassBattleCard virtualClassBattleCard = new VirtualClassBattleCard(_classBuildInfo.VirtualClone(virtualSelfBattlePlayer, virtualOpponentBattlePlayer));
		virtualClassBattleCard.InitBaseMaxLife(BaseMaxLife);
		CopyToVirtualCardBase(virtualClassBattleCard);
		return virtualClassBattleCard;
	}

	public void ClearSpineObject()
	{
		if (ClassBattleCardView != null)
		{
			ClassBattleCardView.ClearSpineObject();
		}
	}

	private static BuildInfo CreateBaseBuildInfo(ClassBuildInfo classBuildInfo)
	{
		CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(0);
		if (_sharedEmptySkillInfo == null)
		{
			_sharedEmptySkillInfo = SkillCreator.CreateBuildInfo(cardParameterFromId);
		}
		return new BuildInfo(null, 0, classBuildInfo.selfBattlePlayer, classBuildInfo.opponentBattlePlayer, classBuildInfo.selfBattlePlayer, _isPlayer: classBuildInfo.isPlayer, _innerOptions: new CardInnerOptionsBase(), _normalSkillBuildInfos: _sharedEmptySkillInfo.normalSkillBuildInfos, _evolveSkillBuildInfos: _sharedEmptySkillInfo.evolveSkillBuildInfos, _battleCardIndex: 0, _battleMgr: classBuildInfo.battleMgr, _resourceMgr: classBuildInfo.resourceMgr);
	}

	protected override ISkillApplyInformation CreateSkillApplyInformation(BattleCardBase card)
	{
		return new ClassSkillApplyInformation(card);
	}
}
