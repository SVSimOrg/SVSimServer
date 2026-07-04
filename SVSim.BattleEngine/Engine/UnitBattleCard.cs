using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard;
using Wizard.Battle.Card;
using Wizard.Battle.Resource;
using Wizard.Battle.UI;
using Wizard.Battle.View;
using Wizard.Battle.View.Vfx;

public class UnitBattleCard : BattleCardBase
{

	private bool _isEvolution;

	public bool _isEvolvedOnWhenLeave;

	public override bool IsUnit => true;

	public override bool IsEvolution => _isEvolution;

	public override bool IsEvolvedOnWhenLeave => _isEvolvedOnWhenLeave;

	public override bool BaseMovable => base.Movable();

	public override bool IsCantActivateFanfare => base.SelfBattlePlayer.Class.SkillApplyInformation.IsCantActivateFanfareUnit;

	public event Func<VfxBase> OnAfterAttack;

	public override bool Movable(bool isCheckOnDraw = true, bool isSkipSelecting = false, CHECK_CONDITION_MUTATIONSKILL_TYPE type = CHECK_CONDITION_MUTATIONSKILL_TYPE.NONE, bool isRecording = false)
	{
		type = IsCheckActiveMutationSkill;
		if (!base.Movable(isCheckOnDraw, isSkipSelecting, type, isRecording))
		{
			return false;
		}
		return IsMutationMovable(type);
	}

	public UnitBattleCard(BuildInfo buildInfo)
		: base(buildInfo)
	{
	}

	public override void Setup(bool createNullView = false, bool isRecreate = false)
	{
		base.Setup(createNullView, isRecreate);
		base.BattleCardView.SetupIconAnimations(this, base.Skills);
	}

	public override VfxBase TurnStart(SkillProcessor skillProcessor)
	{
		VfxBase vfxBase = base.TurnStart(skillProcessor);
		if (Attackable)
		{
			return SequentialVfxPlayer.Create(vfxBase, InstantVfx.Create(delegate
			{
				base.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
			}));
		}
		return vfxBase;
	}

	public override VfxBase TurnEndPostProcess()
	{
		base.TurnEndPostProcess();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (Attackable)
		{
			parallelVfxPlayer.Register(InstantVfx.Create(base.BattleCardView._inPlayFrameEffect.HideFrameEffect));
		}
		if (_buildInfo.BattleMgr.GameMgr.IsWatchBattle || _buildInfo.BattleMgr.GameMgr.IsReplayBattle)
		{
			parallelVfxPlayer.Register(InstantVfx.Create(delegate
			{
				base.BattleCardView.HideAttackFinished();
			}));
		}
		base.AttackableCount = 0;
		return parallelVfxPlayer;
	}

	public override VfxBase Evolution(bool isSkill, SkillProcessor skillProcessor, SkillConditionCheckerOption option, Func<BattleCardBase, IBattleResourceMgr, EvolveVfxBase> getEvolveVfxFunc = null)
	{
		BattlePlayerPair playerInfoPair = new BattlePlayerPair(base.SelfBattlePlayer, base.OpponentBattlePlayer);
		if (!CanEvolution(isSkill, isSelfBattlePlayer: true) || !IsInplay)
		{
			return NullVfx.GetInstance();
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			base.BattleCardView._inPlayFrameEffect.SetIsSelectingAttackTarget(enable: true);
		}));
		sequentialVfxPlayer.Register(OnBeforeEvolveEvent(skillProcessor));
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			base.BattleCardView._inPlayFrameEffect.SetIsSelectingAttackTarget(enable: false);
		}));
		base.SelfBattlePlayer.EvolvedCards.Add(this);
		base.SelfBattlePlayer.AddCurrentEvolvePlayCount(1);
		int attackableCount = base.AttackableCount;
		sequentialVfxPlayer.Register(base.SkillApplyInformation.AllSkillEffectStop(isEvolve: true));
		sequentialVfxPlayer.Register(InstantVfx.Create(base.BattleCardView._inPlayFrameEffect.HideFrameEffect));
		EpSetModifier modifier = new EpSetModifier(0);
		if (isSkill)
		{
			base.SkillApplyInformation.AddEpModifier(modifier);
		}
		OnEvolve(isSkill);
		if (!isSkill)
		{
			base.SelfBattlePlayer.NowTurnEvol = false;
			base.SelfBattlePlayer.IsEpEvolveThisTurn = true;
		}
		_isEvolution = true;
		base.AttackableCount = attackableCount;
		if (_buildInfo.BattleMgr.IsRecovery)
		{
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
		}
		else if (getEvolveVfxFunc == null)
		{
			if (base.IsPlayer && _buildInfo.BattleMgr.GameMgr.GetDataMgr().Is3DSkin(isPlayer: true))
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
			else if (base.IsPlayer && _buildInfo.BattleMgr.GameMgr.GetDataMgr().IsHighRankSkinPlayer())
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
			else
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
		}
		else
		{
			sequentialVfxPlayer.Register(getEvolveVfxFunc(this, _buildInfo.ResourceMgr));
		}
		if (_evolveSkillCollection.Count() > 0)
		{
			if (_evolveSkillCollection.Count((SkillBase s) => !s.IsAttachedSkill) == 0)
			{
				for (int num = 0; num < _evolveSkillCollection.Count(); num++)
				{
					base.Skills.Add(_evolveSkillCollection.ElementAt(num));
				}
				_evolveSkillCollection.Clear();
			}
			else
			{
				base.Skills = _evolveSkillCollection;
			}
			base.Skills.Complete();
			if (!isSkill)
			{
				skillProcessor.Register(base.Skills.CreateWhenEvolveInfo(skillProcessor, playerInfoPair, option));
			}
			sequentialVfxPlayer.Register(base.Skills.RegisterAndProcessWhenChangeInplayImmediateInfo(playerInfoPair));
			base.Skills.CreateAndRegisterWhenChangeInplayInfo(new List<BattleCardBase> { this }, skillProcessor, playerInfoPair);
			foreach (SkillBase attachedSkill in base.SkillApplyInformation.AttachedSkillsInfo.AttachedSkills)
			{
				if (!base.Skills.Contains(attachedSkill))
				{
					base.Skills.Add(attachedSkill);
				}
			}
			base.Skills.Complete();
			if (_evolveSkillCollection._skillTimingInfo.IsWhenDestroy)
			{
				base.BuffInfoList.RemoveAll((BuffInfo b) => b.SkillFrom is Skill_rob_skill && b.SkillFrom.SkillPrm.ownerCard != this);
			}
		}
		if (base.IsSelfTurn)
		{
			sequentialVfxPlayer.Register(InstantVfx.Create(delegate
			{
				base.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
			}));
		}
		sequentialVfxPlayer.Register(base.SkillApplyInformation.AllSkillEffectRestart());
		if (isSkill)
		{
			base.SkillApplyInformation.RemoveEpModifier(modifier);
		}
		sequentialVfxPlayer.Register(base.BattleCardView.InitializeBattleCardIcon(this, base.Skills));
		if (!isSkill)
		{
			List<BattleCardBase> list = base.SelfBattlePlayer.HandCardList.Where((BattleCardBase c) => c.Skills.Any((SkillBase s) => s.OnWhenEvolveOtherStart != 0 || s.OnWhenEvolveSelfAndOtherStart != 0 || s.OnWhenUseEpSelfAndOtherStart != 0)).ToList();
			list.AddRange(base.OpponentBattlePlayer.HandCardList.Where((BattleCardBase c) => c.Skills.Any((SkillBase s) => s.OnWhenEvolveOtherStart != 0 || s.OnWhenEvolveSelfAndOtherStart != 0 || s.OnWhenUseEpSelfAndOtherStart != 0)));
			list.AddRange(base.SelfBattlePlayer.ClassAndInPlayCardList.Where((BattleCardBase c) => c.Skills.Any((SkillBase s) => s.OnWhenEvolveOtherStart != 0 || s.OnWhenEvolveSelfAndOtherStart != 0 || s.OnWhenUseEpSelfAndOtherStart != 0)));
			list.AddRange(base.OpponentBattlePlayer.ClassAndInPlayCardList.Where((BattleCardBase c) => c.Skills.Any((SkillBase s) => s.OnWhenEvolveOtherStart != 0 || s.OnWhenEvolveSelfAndOtherStart != 0 || s.OnWhenUseEpSelfAndOtherStart != 0)));
			List<BattleCardBase> list2 = new List<BattleCardBase>();
			list2.Add(this);
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				if (list[num2] != this && list[num2].Skills.Any((SkillBase s) => s.OnWhenEvolveOtherStart != 0))
				{
					skillProcessor.Register(list[num2].Skills.CreateWhenEvolveOtherInfo(list2, skillProcessor, new BattlePlayerReadOnlyInfoPair(list[num2].SelfBattlePlayer, list[num2].OpponentBattlePlayer)));
				}
				if (list[num2] != this && list[num2].Skills.Any((SkillBase s) => s.OnWhenUseEpSelfAndOtherStart != 0))
				{
					if (base.SelfBattlePlayer.CheckNotConsumeEpCard(this))
					{
						continue;
					}
					skillProcessor.Register(list[num2].Skills.CreateWhenUseEpSelfAndOtherInfo(skillProcessor, new BattlePlayerReadOnlyInfoPair(list[num2].SelfBattlePlayer, list[num2].OpponentBattlePlayer)));
				}
				if (list[num2].Skills.Any((SkillBase s) => s.OnWhenEvolveSelfAndOtherStart != 0))
				{
					skillProcessor.Register(list[num2].Skills.CreateWhenEvolveSelfAndOtherInfo(list2, skillProcessor, new BattlePlayerReadOnlyInfoPair(list[num2].SelfBattlePlayer, list[num2].OpponentBattlePlayer)));
				}
			}
		}
		return sequentialVfxPlayer;
	}

	public override VfxBase SetUpInplay()
	{
		_isEvolution = false;
		_isEvolvedOnWhenLeave = false;
		return base.SetUpInplay();
	}

	public override VfxBase Banish(SkillProcessor skillProcessor, bool isReturn = false)
	{
		if (isReturn)
		{
			_isEvolvedOnWhenLeave = _isEvolution;
			_isEvolution = false;
		}
		return base.Banish(skillProcessor, isReturn);
	}

	public override VfxBase ReturnCard(SkillProcessor skillProcessor)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (Attackable)
		{
			sequentialVfxPlayer.Register(InstantVfx.Create(base.BattleCardView._inPlayFrameEffect.HideFrameEffect));
		}
		sequentialVfxPlayer.Register(base.SkillApplyInformation.AllSkillEffectStop(isEvolve: false, isReturn: true));
		InitializeParameterOnWhenReturn();
		VfxBase vfx = base.ReturnCard(skillProcessor);
		sequentialVfxPlayer.Register(vfx);
		return sequentialVfxPlayer;
	}

	public override void InitializeParameterOnWhenReturn()
	{
		base.IsFirstTurn = false;
		base.IsSummonDrunkenness = true;
		base.AttackableCount = 0;
		_isEvolvedOnWhenLeave = _isEvolution;
		_isEvolution = false;
		base.InitializeParameterOnWhenReturn();
	}

	protected override VfxBase StartPlayCard()
	{
		base.StartPlayCard();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		base.SelfBattlePlayer.HandCardToField(this);
		base.BattleCardView.ResetTemplate();
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		return sequentialVfxPlayer;
	}

	public override VfxBase StartAttack(BattleCardBase underAttackCard, BattlePlayerPair battlePlayerPair)
	{
		return ExecuteAttackSequence(underAttackCard, battlePlayerPair);
	}

	private VfxBase ExecuteAttackSequence(BattleCardBase targetCard, BattlePlayerPair battlePlayerPair)
	{
		SkillProcessor skillProcessor = new SkillProcessor();
		DamageParam damageCalculationAtkTypeAttack = base.DamageCalculationAtkTypeAttack;
		DamageParam damageCalculationAtkTypeBeAttacked = targetCard.DamageCalculationAtkTypeBeAttacked;
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		SequentialVfxPlayer sequentialVfxPlayer2 = SequentialVfxPlayer.Create();
		AttackOpponentResult attackOpponentResult = AttackOpponent(targetCard, damageCalculationAtkTypeAttack, skillProcessor, IsChallenge: true);
		if (base.SkillApplyInformation.IsDrain)
		{
			sequentialVfxPlayer2.Register(WaitVfx.Create(0.3f));
			sequentialVfxPlayer2.Register(ApplyDrain(damageCalculationAtkTypeAttack, skillProcessor, attackOpponentResult.damageResult));
		}
		AttackOpponentResult attackOpponentResult2 = targetCard.AttackOpponent(this, damageCalculationAtkTypeBeAttacked, skillProcessor, IsChallenge: false);
		SequentialVfxPlayer sequentialVfxPlayer3 = SequentialVfxPlayer.Create(attackOpponentResult.attackVfx, attackOpponentResult.damageVfx);
		SequentialVfxPlayer sequentialVfxPlayer4 = SequentialVfxPlayer.Create(WaitVfx.Create(0.4f), attackOpponentResult2.attackVfx);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create(attackOpponentResult2.damageVfx);
		if (base.SkillApplyInformation.IsDrain)
		{
			parallelVfxPlayer.Register(sequentialVfxPlayer2);
		}
		sequentialVfxPlayer4.Register(parallelVfxPlayer);
		sequentialVfxPlayer.Register(ParallelVfxPlayer.Create(sequentialVfxPlayer3, sequentialVfxPlayer4));
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		ParallelVfxPlayer parallelVfxPlayer3 = ParallelVfxPlayer.Create();
		AttackSelectControl attackSelectControl = base.SelfBattlePlayer.BattleView.AttackSelectControl;
		if (IsDead)
		{
			parallelVfxPlayer3.Register(base.SelfBattlePlayer.CardManagement(this, skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, isRandom: false));
			if (!targetCard.IsDead)
			{
				base.BattleCardView.playVoiceOnDeath = true;
				base.BattleCardView.VoiceInfo.SetDestroyCardId(targetCard.BaseParameter.BaseCardId);
			}
		}
		else
		{
			parallelVfxPlayer2.Register(attackSelectControl.ResetCardAfterAttack(base.BattleCardView));
		}
		if (targetCard.IsDead)
		{
			parallelVfxPlayer3.Register(targetCard.SelfBattlePlayer.CardManagement(targetCard, skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, isRandom: false));
			targetCard.BattleCardView.playVoiceOnDeath = true;
			targetCard.BattleCardView.VoiceInfo.SetDestroyCardId(base.BaseParameter.BaseCardId);
		}
		else
		{
			parallelVfxPlayer2.Register(attackSelectControl.ResetCardAfterAttack(targetCard.BattleCardView));
		}
		SequentialVfxPlayer sequentialVfxPlayer5 = SequentialVfxPlayer.Create();
		sequentialVfxPlayer5.Register(parallelVfxPlayer2);
		sequentialVfxPlayer5.Register(parallelVfxPlayer3);
		sequentialVfxPlayer.Register(sequentialVfxPlayer5);
		if (!base.SelfBattlePlayer.BattleMgr.IsVirtualBattle)
		{
			BattleLogManager.GetInstance().AddLogWar(this, targetCard);
		}
		VfxBase vfx = skillProcessor.Process(new BattlePlayerPair(base.SelfBattlePlayer, base.OpponentBattlePlayer));
		sequentialVfxPlayer.Register(vfx);
		sequentialVfxPlayer.Register(this.OnAfterAttack.GetAllFuncVfxResults());
		return sequentialVfxPlayer;
	}

	public override AttackOpponentResult AttackOpponent(BattleCardBase target, DamageParam damageParam, SkillProcessor skillProcessor, bool IsChallenge)
	{
		VfxBase vfx = NullVfx.GetInstance();
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		DamageResult damageResult = target.ApplyDamage(null, damageParam, base.SkillApplyInformation.IsKiller, isReflectedDamage: false, skillProcessor, null);
		BattleManagerBase ins = _buildInfo.BattleMgr;
		base.SelfBattlePlayer.Class.SkillApplyInformation.CausedDamageLife(damageResult.DamageApplied, ins.CurrentTurn, ins.BattlePlayer.IsSelfTurn);
		sequentialVfxPlayer.Register(damageResult.Vfx);
		SequentialVfxPlayer sequentialVfxPlayer2 = SequentialVfxPlayer.Create();
		if (IsChallenge)
		{
			base.AttackableCount--;
			if (base.SkillApplyInformation.IsSneak)
			{
				sequentialVfxPlayer2.Register(AfterAddDamage());
			}
		}
		sequentialVfxPlayer2.Register(vfx);
		return new AttackOpponentResult(sequentialVfxPlayer2, sequentialVfxPlayer, damageResult);
	}

	protected VfxBase ApplyDrain(DamageParam damageParam, SkillProcessor skillProcessor, DamageResult damageResult)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		BattleCardBase beforeHealCard = base.SelfBattlePlayer.Class.VirtualClone(base.SelfBattlePlayer.Class.SelfBattlePlayer, base.SelfBattlePlayer.Class.OpponentBattlePlayer);
		HealParam healParam = new HealParam(damageResult.GainLife, this, base.SelfBattlePlayer.Class);
		HealResult healResult = base.SelfBattlePlayer.Class.ApplyHealing(healParam, skillProcessor);
		SequentialVfxPlayer sequentialVfxPlayer2 = SequentialVfxPlayer.Create(healResult.HealVfx, healResult.PrehealVfxVfx, healResult.PosthealVfxVfx);
		skillProcessor.Register(base.SelfBattlePlayer.Class.Skills.CreateWhenHealing(base.SelfBattlePlayer.Class, skillProcessor, new BattlePlayerReadOnlyInfoPair(base.SelfBattlePlayer, base.OpponentBattlePlayer), healResult.HealAmount));
		sequentialVfxPlayer2.Register(base.SelfBattlePlayer.StartSkillWhenHealingSelfAndOther(new List<BattleCardBase> { base.SelfBattlePlayer.Class }, skillProcessor, new List<int> { healResult.HealAmount }));
		base.SelfBattlePlayer.CallOnDrain(healResult.HealAmount);
		if (base.SelfBattlePlayer.Class.BattleCardView.CardWrapObject != null)
		{
			EffectBattle effectBattle = null;
			Vector3 effectPosition = base.SelfBattlePlayer.Class.BattleCardView.CardWrapObject.transform.position;
			sequentialVfxPlayer.Register(new SkillBase.WaitEffectLoadVfx("btl_magic_cure_1", EffectMgr.EngineType.SHURIKEN, "se_btl_magic_cure_1", _buildInfo.BattleMgr.BattleResourceMgr, delegate(EffectBattle eb)
			{
				effectBattle = eb;
			}));
			VfxBase vfx = NullVfx.GetInstance();
			sequentialVfxPlayer2.Register(vfx);
		}
		sequentialVfxPlayer.Register(sequentialVfxPlayer2);
		if (!base.SelfBattlePlayer.BattleMgr.IsVirtualBattle)
		{
			BattleLogManager.GetInstance().AddLogHeal(beforeHealCard, healResult.HealAmount);
		}
		return sequentialVfxPlayer;
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
		damageParam.Damage = CalculateFinalDamageAmount(damageParam.Damage, isSkillDamage, isSpellDamage);
		new SkillConditionCheckerOption().DefaultDamage = new DamageInfo(skill, damage);
		BattleManagerBase ins = _buildInfo.BattleMgr;
		base.SkillApplyInformation.DamageLife(damageParam.Damage, ins.CurrentTurn, ins.BattlePlayer.IsSelfTurn);
		if (doesAttackerPossessKiller && !base.SkillApplyInformation.IsIndependent && !base.SkillApplyInformation.IsIndestructible)
		{
			FlagCardAsDestroyedByKiller();
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		skillProcessor.Register(base.Skills.CreateWhenDamageInfo(skill, skillProcessor, new BattlePlayerReadOnlyInfoPair(base.SelfBattlePlayer, base.OpponentBattlePlayer), damage, damageParam.Damage));
		sequentialVfxPlayer.Register(base.ApplyDamage(skill, damageParam, doesAttackerPossessKiller, isReflectedDamage, skillProcessor, reflectCard).Vfx);
		return new DamageResult(sequentialVfxPlayer, damageParam.Damage, damageParam.Damage, null, null, isReflectedDamage);
	}

	public override HealResult ApplyHealing(HealParam healParam, SkillProcessor skillProcessor)
	{
		BattleManagerBase ins = _buildInfo.BattleMgr;
		int num = HealLife(healParam.HealAmount, ins.CurrentTurn, ins.BattlePlayer.IsSelfTurn);
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		new SkillConditionCheckerOption().HealingCardAndValue = new List<BattlePlayerBase.CardAndValue>
		{
			new BattlePlayerBase.CardAndValue(this, num)
		};
		return new HealResult(num, sequentialVfxPlayer);
	}

	public override VfxBase RemoveFromInPlay()
	{
		VfxBase result = base.RemoveFromInPlay();
		if (this != null && !IsDead)
		{
			base.SkillApplyInformation.ClearParameterModifier();
		}
		return result;
	}


	protected override ISkillApplyInformation CreateSkillApplyInformation(BattleCardBase card)
	{
		return new UnitSkillApplyInformation(card);
	}

	protected override IBattleCardView CreateView(BattleCardView.BuildInfo buildInfo, bool isNullView)
	{
		if (isNullView)
		{
			return new NullBattleCardView(buildInfo);
		}
		return new UnitBattleCardView(buildInfo);
	}

	public override string GetCardSkillDescription(BattlePlayerBase.SideLogInfo sideLogInfo, bool? isForceGetEvolveText = null)
	{
		bool flag = IsEvolution;
		if (isForceGetEvolveText.HasValue)
		{
			flag = isForceGetEvolveText.Value;
		}
		if (!flag)
		{
			return SkillDescription(sideLogInfo);
		}
		return EvoSkillDescription(sideLogInfo);
	}

	public override VfxBase RecoveryInPlay(int inPlayIndex, bool newReplayMoveTurn = false)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(base.RecoveryInPlay(inPlayIndex, newReplayMoveTurn));
		if (IsEvolution)
		{
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
			if (base.IsChoiceEvolutionCard)
			{
				sequentialVfxPlayer.Register(NullVfx.GetInstance());
			}
			sequentialVfxPlayer.Register(NullVfx.GetInstance());
		}
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		sequentialVfxPlayer.Register(NullVfx.GetInstance());
		if (Attackable && base.IsSelfTurn)
		{
			sequentialVfxPlayer.Register(InstantVfx.Create(delegate
			{
				base.BattleCardView._inPlayFrameEffect.UpdateCanAttackEffect();
			}));
		}
		return sequentialVfxPlayer;
	}

	public override BattleCardBase VirtualClone(BattlePlayerBase selfBattlePlayer, BattlePlayerBase opponentBattlePlayer)
	{
		VirtualUnitBattleCard virtualUnitBattleCard = new VirtualUnitBattleCard(_buildInfo.VirtualClone(selfBattlePlayer, opponentBattlePlayer));
		virtualUnitBattleCard._isEvolution = IsEvolution;
		CopyToVirtualCardBase(virtualUnitBattleCard);
		return virtualUnitBattleCard;
	}
}
