using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_powerup : SkillBase
{
	private enum BuffType
	{
		Null,
		Add,
		Multiply,
		AddMaxLife	}

	public class PowerUpModifierContainer
	{
		public BattleCardBase TargetCard;

		public ICardOffenseModifier OffenseModifier;

		public ICardLifeModifier LifeModifier;

		public BuffInfo BuffInfo;

		public PowerUpModifierContainer(BattleCardBase targetCard, ICardOffenseModifier offenseModifier, ICardLifeModifier lifeModifier, BuffInfo buffInfo)
		{
			TargetCard = targetCard;
			OffenseModifier = offenseModifier;
			LifeModifier = lifeModifier;
			BuffInfo = buffInfo;
		}
	}

	protected readonly List<PowerUpModifierContainer> _targetList = new List<PowerUpModifierContainer>();

	protected int _addOffense = -1;

	protected int _addLife = -1;

	protected int _gainOffense = -1;

	protected int _gainLife = -1;

	protected int _multiplyOffense = -1;

	protected int _multiplyLife = -1;

	protected int _addMaxLife = -1;

	public override bool IsTargetIndicate
	{
		get
		{
			if (base.ApplyingTargetFilter is SkillTargetDeckFilter && base.SkillPrm.ownerCard.SelfBattlePlayer.DeckCardList.Count == 0)
			{
				return true;
			}
			return false;
		}
	}

	protected bool IsBuffAddOffense
	{
		get
		{
			if (_addOffense != 0)
			{
				return _addOffense != -1;
			}
			return false;
		}
	}

	private bool IsBuffGainOffense
	{
		get
		{
			if (_gainOffense != 0)
			{
				return _gainOffense != -1;
			}
			return false;
		}
	}

	private bool IsBuffGainLife
	{
		get
		{
			if (_gainLife != 0)
			{
				return _gainLife != -1;
			}
			return false;
		}
	}

	protected bool IsBuffAddLife
	{
		get
		{
			if (_addLife != 0)
			{
				return _addLife != -1;
			}
			return false;
		}
	}

	private bool IsBuffMultiplyOffense => _multiplyOffense != -1;

	private bool IsBuffMultiplyLife => _multiplyLife != -1;

	private bool IsAddMaxLife => _addMaxLife != -1;

	public Skill_powerup(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int num = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.repeat_count, 1);
		List<ICardOffenseModifier> list = new List<ICardOffenseModifier>();
		List<ICardLifeModifier> list2 = new List<ICardLifeModifier>();
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		SequentialVfxPlayer vfxToRegister = SequentialVfxPlayer.Create();
		List<BattleCardBase> targetCards = parameter.targetCards.Where((BattleCardBase c) => (c.IsPlayer || !c.IsInHand) && !c.IsInCemetery).ToList();
		List<BattleCardBase> list3 = parameter.targetCards.ToList();
		List<BattleCardBase> inplayTargetCards = parameter.targetCards.Where((BattleCardBase c) => c.IsInplay).ToList();
		_addOffense = GetAddOffense();
		_addLife = GetAddLife();
		_gainOffense = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_offense, -1);
		_gainLife = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_life, -1);
		_multiplyOffense = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.multiply_offense, -1);
		_multiplyLife = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.multiply_life, -1);
		_addMaxLife = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_max_life, -1);
		list.Add(CreateOffenseModifier());
		list2.Add(CreateLifeModifier());
		if (!IsBuffAddLife && !IsBuffMultiplyLife && !IsBuffGainLife && !IsBuffAddOffense && !IsBuffMultiplyOffense && !IsBuffGainOffense && !IsAddMaxLife)
		{
			return NullVfxWithLoading.GetInstance();
		}
		for (int num2 = 0; num2 < num; num2++)
		{
			for (int num3 = 0; num3 < list3.Count; num3++)
			{
				parallelVfxPlayer.Register(GiveCombatModifier(list3[num3], list.FirstOrDefault(), list2.FirstOrDefault(), parameter));
			}
			if (IsBattleLog)
			{
				AddBattleLog(parameter.targetCards);
			}
		}
		CallPowerUpEvent(list3);
		if (IsBattleLog && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch && !base.SkillPrm.ownerCard.IsPlayer && OnWhenDraw != 0)
		{
			BattleLogManager.GetInstance().AddLogSkillBuffAddClass(new List<BattleCardBase> { SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.BattleEnemy.Class }, this);
		}
		IncrementGameBuffCount(inplayTargetCards);
		RegisterOtherSkillInfo(parameter, inplayTargetCards);
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, targetCards, isFollowInHand: false, addToLastOperation: true));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		vfxWithLoadingSequential.RegisterToMainVfx(vfxToRegister);
		SkillPreprocessBase skillPreprocessBase = base.PreprocessList.FirstOrDefault((SkillPreprocessBase p) => p is SkillPreprocessOpenCard);
		if (skillPreprocessBase != null && base.ApplyingTargetFilter is SkillTargetHandSelfFilter)
		{
			vfxWithLoadingSequential.RegisterToMainVfx((skillPreprocessBase as SkillPreprocessOpenCard).CreateOpenCardVfx(this));
		}
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (PowerUpModifierContainer target in _targetList)
		{
			parallelVfxPlayer.Register(target.TargetCard.SkillApplyInformation.DepriveCombatValueModifire(target.OffenseModifier, target.LifeModifier));
			target.TargetCard.RemoveBuffInfo(target.BuffInfo);
			target.TargetCard.SkillApplyInformation.DepriveBuff();
		}
		_targetList.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	protected virtual VfxBase GiveCombatModifier(BattleCardBase card, ICardOffenseModifier offenseModifier, ICardLifeModifier lifeModifier, CallParameter parameter)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		sequentialVfxPlayer.Register(card.SkillApplyInformation.GiveCombatValueModifier(offenseModifier, lifeModifier, parameter.skillProcessor));
		BuffInfo buff = AddBuffInfoIfNeeded(card);
		PowerUpModifierContainer powerUp = new PowerUpModifierContainer(card, offenseModifier, lifeModifier, buff);
		_targetList.Add(powerUp);
		card.OnRemoveFromInPlayAfterOneTime += delegate
		{
			card.RemoveBuffInfo(buff);
			_targetList.Remove(powerUp);
			card.SkillApplyInformation.FourceDepriveBuff();
			return card.SkillApplyInformation.ForceDepriveCombatValueModifire();
		};
		card.SkillApplyInformation.GiveBuff();
		if (GetAddLife() > 0)
		{
			card.SkillApplyInformation.GiveBuffLife();
		}
		else if (_gainLife > 0)
		{
			sequentialVfxPlayer.Register(DeadCheck(card, parameter.skillProcessor));
		}
		RegisterWhenBuffInfo(card, parameter);
		return sequentialVfxPlayer;
	}

	protected virtual VfxBase DeadCheck(BattleCardBase targetCard, SkillProcessor skillProcessor)
	{
		return NullVfx.GetInstance();
	}

	protected virtual void RegisterWhenBuffInfo(BattleCardBase card, CallParameter parameter)
	{
		BattlePlayerPair playerInfoPair = new BattlePlayerPair(card.SelfBattlePlayer, card.OpponentBattlePlayer);
		parameter.skillProcessor.Register(card.Skills.CreateWhenBuffInfo(parameter.skillProcessor, playerInfoPair, card));
	}

	protected virtual void RegisterOtherSkillInfo(CallParameter parameter, List<BattleCardBase> inplayTargetCards)
	{
		bool flag = _addOffense > 0 || _addLife > 0;
		bool flag2 = _gainOffense > 0 || _gainLife > 0;
		BattlePlayerBase selfBattlePlayer = base.SkillPrm.ownerCard.SelfBattlePlayer;
		BattlePlayerBase battlePlayerBase = (selfBattlePlayer.IsSelfTurn ? selfBattlePlayer : base.SkillPrm.ownerCard.OpponentBattlePlayer);
		BattlePlayerBase battlePlayerBase2 = (selfBattlePlayer.IsSelfTurn ? base.SkillPrm.ownerCard.OpponentBattlePlayer : selfBattlePlayer);
		if (flag && flag2)
		{
			battlePlayerBase.StartSkillWhenBuffDebuffSelfAndOther(parameter.targetCards, inplayTargetCards, parameter.skillProcessor);
			battlePlayerBase2.StartSkillWhenBuffDebuffSelfAndOther(parameter.targetCards, inplayTargetCards, parameter.skillProcessor);
		}
		else if (flag)
		{
			battlePlayerBase.StartSkillWhenBuffSelfAndOther(parameter.targetCards, inplayTargetCards, parameter.skillProcessor);
			battlePlayerBase2.StartSkillWhenBuffSelfAndOther(parameter.targetCards, inplayTargetCards, parameter.skillProcessor);
		}
		else if (flag2)
		{
			battlePlayerBase.StartSkillWhenDebuffSelfAndOther(parameter.targetCards, inplayTargetCards, parameter.skillProcessor);
			battlePlayerBase2.StartSkillWhenDebuffSelfAndOther(parameter.targetCards, inplayTargetCards, parameter.skillProcessor);
		}
	}

	protected virtual void IncrementGameBuffCount(List<BattleCardBase> inplayTargetCards)
	{
		if (inplayTargetCards.Any())
		{
			BattlePlayerBase battlePlayer = base.SkillPrm.ownerCard.SelfBattlePlayer;
			TurnAndIntValue turnAndIntValue = battlePlayer.GameSkillBuffCountList.FirstOrDefault((TurnAndIntValue t) => t.IsSelfTurn == battlePlayer.IsSelfTurn && t.Turn == battlePlayer.Turn);
			if (turnAndIntValue != null)
			{
				turnAndIntValue.Increment();
			}
			else
			{
				battlePlayer.GameSkillBuffCountList.Add(new TurnAndIntValue(1, battlePlayer.Turn, battlePlayer.IsSelfTurn));
			}
		}
	}

	public int GetAddOffense()
	{
		return base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_offense, 0);
	}

	public int GetAddLife()
	{
		return base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add_life, 0);
	}

	private ICardOffenseModifier CreateOffenseModifier()
	{
		if (IsBuffAddOffense)
		{
			return new OffenseAddModifier(_addOffense);
		}
		if (IsBuffMultiplyOffense)
		{
			return new OffenseMultiplyModifier(_multiplyOffense);
		}
		if (IsBuffGainOffense)
		{
			return new OffenseAddModifier(_gainOffense * -1);
		}
		return new OffenseAddModifier(0);
	}

	private ICardLifeModifier CreateLifeModifier()
	{
		if (IsBuffAddLife)
		{
			return new LifeAddModifier(_addLife);
		}
		if (IsBuffMultiplyLife)
		{
			return new LifeMultiplyModifier(_multiplyLife);
		}
		if (IsBuffGainLife)
		{
			return new LifeAddModifier(_gainLife * -1);
		}
		if (IsAddMaxLife)
		{
			return new MaxLifeAddModifier(_addMaxLife);
		}
		return new LifeAddModifier(0);
	}

	public void SettingPowerUpData(int offense, int life)
	{
		_addOffense = offense;
		_addLife = life;
		_gainOffense = -1;
		_gainLife = -1;
	}

	protected virtual void AddBattleLog(IEnumerable<BattleCardBase> targetCards)
	{
		BattleLogManager instance = BattleLogManager.GetInstance();
		List<BattleCardBase> list = targetCards.Where((BattleCardBase c) => c.IsInplay).ToList();
		if (list.Count > 0)
		{
			switch (GetBuffType())
			{
			case BuffType.Add:
				instance.AddLogSkillBuffAdd(list, _addOffense, _addLife, _gainOffense, _gainLife, this);
				break;
			case BuffType.Multiply:
				instance.AddLogSkillBuffMultiply(list, _multiplyOffense, _multiplyLife, this);
				break;
			case BuffType.AddMaxLife:
				instance.AddLogSkillBuffAddMaxLife(list, _addMaxLife, this);
				break;
			}
		}
		bool isAdminWatch = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch;
		bool isTargetSelfOpenCardSkill = base.IsContainSelfFilter && (base.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessOpenCard) || (isAdminWatch && OnWhenDraw != 0));
		list = targetCards.Where((BattleCardBase c) => c.IsInHand && (c.IsPlayer == base.SkillPrm.selfBattlePlayer.IsPlayer || isAdminWatch || isTargetSelfOpenCardSkill)).ToList();
		if ((IsTargetInHand() || isTargetSelfOpenCardSkill || ((base.SkillPrm.selfBattlePlayer.IsPlayer || base.ApplyingTargetFilter is SkillTargetReturnCardFilter) && list.Count > 0)) && (!(base.ApplyingTargetFilter is SkillTargetSkillDrewCardFilter) || list.Count != 0))
		{
			bool isTargetInOpponentHand = IsTargetInOpponentHand() || (base.ApplyingTargetFilter is SkillTargetReturnCardFilter && !base.SkillPrm.selfBattlePlayer.IsPlayer && !isTargetSelfOpenCardSkill);
			if (GetBuffType() == BuffType.Add)
			{
				instance.AddLogSkillBuffInHandAdd(list, _addOffense, _addLife, this, isTargetInOpponentHand, isTargetSelfOpenCardSkill);
			}
		}
		list = targetCards.Where((BattleCardBase c) => c.IsInDeck && (c.IsPlayer == base.SkillPrm.selfBattlePlayer.IsPlayer || isAdminWatch)).ToList();
		if ((base.ApplyingTargetFilter is SkillTargetDeckFilter || base.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetDeckFilter)) && GetBuffType() == BuffType.Add)
		{
			instance.AddLogSkillBuffInDeckAdd(base.SkillPrm.selfBattlePlayer.Class, _addOffense, _addLife, this);
		}
	}

	private BuffType GetBuffType()
	{
		if (IsBuffAddOffense || IsBuffAddLife)
		{
			return BuffType.Add;
		}
		if (IsBuffMultiplyOffense || IsBuffMultiplyLife)
		{
			return BuffType.Multiply;
		}
		if (IsAddMaxLife)
		{
			return BuffType.AddMaxLife;
		}
		return BuffType.Null;
	}

	protected virtual void CallPowerUpEvent(List<BattleCardBase> targetCards)
	{
	}
}
