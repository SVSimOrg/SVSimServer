using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_power_down : SkillBase
{
	public static int SETPRM_NONE = -1;

	protected int _gainOffense;

	protected int _gainLife;

	protected int _gainMaxLife;

	protected int _setOffense;

	protected int _setLife;

	protected int _setMaxLife;

	public static readonly int BIT_FLAG_BUFF_EFFECT = 1;

	public static readonly int BIT_FLAG_NOT_BUFF_EFFECT = 0;

	public static readonly string MINUS_ZERO = "-0";

	public Skill_power_down(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		_gainOffense = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_offense, 0);
		_gainLife = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_life, 0);
		_gainMaxLife = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.gain_max_life, 0);
		_setOffense = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set_offense, SETPRM_NONE);
		_setLife = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set_life, SETPRM_NONE);
		_setMaxLife = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set_max_life, SETPRM_NONE);
		bool isMinusZeroAttack = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.gain_offense, "0") == MINUS_ZERO;
		bool isMinusZeroLife = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.gain_life, "0") == MINUS_ZERO;
		OffenseAddModifier offenseAddModifier = null;
		LifeAddModifier lifeAddModifier = null;
		OffenseSetModifier offenseSetModifier = null;
		LifeSetModifier lifeSetModifier = null;
		if (_setOffense != SETPRM_NONE)
		{
			offenseSetModifier = new OffenseSetModifier(_setOffense);
		}
		if (_gainOffense > 0)
		{
			offenseAddModifier = new OffenseAddModifier(_gainOffense * -1);
		}
		if (_setLife != SETPRM_NONE)
		{
			lifeSetModifier = new LifeSetModifier(_setLife);
		}
		else if (_setMaxLife != SETPRM_NONE)
		{
			lifeSetModifier = new MaxLifeSetModifier(_setMaxLife);
		}
		if (_gainMaxLife > 0)
		{
			lifeAddModifier = new MaxLifeAddModifier(_gainMaxLife * -1);
		}
		if (_gainLife > 0)
		{
			lifeAddModifier = new LifeAddModifier(_gainLife * -1);
		}
		if (offenseAddModifier == null && lifeAddModifier == null && offenseSetModifier == null && lifeSetModifier == null)
		{
			return NullVfxWithLoading.GetInstance();
		}
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		BattlePlayerBase selfBattlePlayer = base.SkillPrm.ownerCard.SelfBattlePlayer;
		BattlePlayerBase battlePlayerBase = (selfBattlePlayer.IsSelfTurn ? selfBattlePlayer : base.SkillPrm.ownerCard.OpponentBattlePlayer);
		BattlePlayerBase battlePlayerBase2 = (selfBattlePlayer.IsSelfTurn ? base.SkillPrm.ownerCard.OpponentBattlePlayer : selfBattlePlayer);
		if (offenseAddModifier != null || lifeAddModifier != null)
		{
			battlePlayerBase.StartSkillWhenDebuffSelfAndOther(parameter.targetCards, parameter.targetCards.Where((BattleCardBase c) => c.IsInplay), parameter.skillProcessor);
			battlePlayerBase2.StartSkillWhenDebuffSelfAndOther(parameter.targetCards, parameter.targetCards.Where((BattleCardBase c) => c.IsInplay), parameter.skillProcessor);
		}
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		List<int> list3 = new List<int>();
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		foreach (BattleCardBase target in parameter.targetCards)
		{
			if (offenseAddModifier != null || lifeAddModifier != null || offenseSetModifier == null || lifeSetModifier == null)
			{
				battlePlayerBase.StartSkillWhenDebuffIncludeSetMaxLife(target, parameter.targetCards.Where((BattleCardBase c) => c.IsInplay), parameter.skillProcessor);
				battlePlayerBase2.StartSkillWhenDebuffIncludeSetMaxLife(target, parameter.targetCards.Where((BattleCardBase c) => c.IsInplay), parameter.skillProcessor);
			}
			if (target.SkillApplyInformation.IsNotBeDebuffed)
			{
				if (!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery)
				{
					parallelVfxPlayer2.Register(SkillBase.CreateSingleVfx(base.SkillPrm.resourceMgr, () => target.BattleCardView.GameObject.transform.position, new List<BattleCardBase> { target }, target.IsPlayer, target.BattleCardView, "btl_nerva_2", EffectMgr.EngineType.SHURIKEN, "se_btl_nerva_2", EffectMgr.MoveType.DIRECT_LEADER, EffectMgr.TargetType.SINGLE, 0f));
				}
				continue;
			}
			int isBuffEffect = BIT_FLAG_NOT_BUFF_EFFECT;
			BattleCardBase targetCard = target;
			ICardOffenseModifier offenseModifire = null;
			ICardLifeModifier lifeModifire = null;
			list.Add(target.Atk);
			list2.Add(target.Life);
			list3.Add(target.MaxLife);
			if (offenseSetModifier != null && lifeSetModifier != null)
			{
				parallelVfxPlayer.Register(targetCard.SkillApplyInformation.GiveCombatValueModifier(offenseSetModifier, lifeSetModifier, parameter.skillProcessor));
			}
			if (offenseAddModifier != null && lifeAddModifier != null)
			{
				parallelVfxPlayer.Register(targetCard.SkillApplyInformation.GiveCombatValueModifier(offenseAddModifier, lifeAddModifier, parameter.skillProcessor));
			}
			if (offenseSetModifier != null)
			{
				int atk = targetCard.Atk;
				if (lifeSetModifier == null)
				{
					parallelVfxPlayer.Register(targetCard.SkillApplyInformation.GiveCombatValueModifier(offenseSetModifier, null, parameter.skillProcessor));
				}
				offenseModifire = offenseSetModifier;
				if (targetCard.Atk > atk)
				{
					isBuffEffect = BIT_FLAG_BUFF_EFFECT;
				}
				else if (targetCard.Atk == atk)
				{
					isBuffEffect = BIT_FLAG_NOT_BUFF_EFFECT;
				}
			}
			if (offenseAddModifier != null)
			{
				if (lifeAddModifier == null)
				{
					parallelVfxPlayer.Register(targetCard.SkillApplyInformation.GiveCombatValueModifier(offenseAddModifier, null, parameter.skillProcessor));
				}
				offenseModifire = offenseAddModifier;
			}
			if (lifeSetModifier != null)
			{
				int life = targetCard.Life;
				if (offenseSetModifier == null)
				{
					parallelVfxPlayer.Register(targetCard.SkillApplyInformation.GiveCombatValueModifier(null, lifeSetModifier, parameter.skillProcessor));
				}
				if (targetCard.IsDead)
				{
					parallelVfxPlayer.Register(targetCard.SelfBattlePlayer.CardManagement(targetCard, parameter.skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, base.UsedRandom, null, null, this));
				}
				lifeModifire = lifeSetModifier;
				if (targetCard.Life > life)
				{
					isBuffEffect = BIT_FLAG_BUFF_EFFECT;
				}
				else if (targetCard.Life == life)
				{
					isBuffEffect = BIT_FLAG_NOT_BUFF_EFFECT;
				}
			}
			if (lifeAddModifier != null)
			{
				if (offenseAddModifier == null)
				{
					parallelVfxPlayer.Register(targetCard.SkillApplyInformation.GiveCombatValueModifier(null, lifeAddModifier, parameter.skillProcessor));
				}
				if (targetCard.IsDead)
				{
					parallelVfxPlayer.Register(targetCard.SelfBattlePlayer.CardManagement(targetCard, parameter.skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, base.UsedRandom, null, null, this));
				}
				lifeModifire = lifeAddModifier;
			}
			if (offenseAddModifier != null || lifeAddModifier != null || offenseSetModifier != null || lifeSetModifier != null)
			{
				BuffInfo buff = AddBuffInfoIfNeeded(target);
				BuffInfoContainer powerDown = new BuffInfoContainer(target, buff, isBuffEffect, "", null, 0L, null, null, offenseModifire, lifeModifire);
				buffInfoContainer.Add(powerDown);
				targetCard.OnRemoveFromInPlayAfterOneTime += delegate
				{
					buffInfoContainer.Remove(powerDown);
					targetCard.RemoveBuffInfo(buff);
					if ((isBuffEffect & BIT_FLAG_BUFF_EFFECT) != 0)
					{
						targetCard.SkillApplyInformation.FourceDepriveBuff();
					}
					else
					{
						targetCard.SkillApplyInformation.FourceDepriveDebuff();
					}
					return targetCard.SkillApplyInformation.ForceDepriveCombatValueModifire();
				};
			}
			if (offenseAddModifier != null && lifeAddModifier != null)
			{
				if ((isBuffEffect & BIT_FLAG_BUFF_EFFECT) != 0)
				{
					targetCard.SkillApplyInformation.GiveBuff();
				}
				else
				{
					targetCard.SkillApplyInformation.GiveDebuff();
				}
			}
		}
		if (IsBattleLog && parameter.targetCards.Count((BattleCardBase t) => !t.SkillApplyInformation.IsNotBeDebuffed) > 0)
		{
			List<BattleCardBase> buffCards = parameter.targetCards.Where((BattleCardBase t) => !t.SkillApplyInformation.IsNotBeDebuffed).ToList();
			bool isTargetInOpponentHand = IsTargetInOpponentHand() && (!base.IsContainSelfFilter || !base.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessOpenCard));
			if (_setOffense != SETPRM_NONE || _setLife != SETPRM_NONE)
			{
				BattleLogManager.GetInstance().AddLogSkillBuffSet(buffCards, _setOffense, _setLife, this, isTargetInOpponentHand, list, list2);
			}
			if (_setMaxLife != SETPRM_NONE)
			{
				BattleLogManager.GetInstance().AddLogSkillBuffSetMaxLife(buffCards, _setMaxLife, this, list3);
			}
			if (_gainMaxLife > 0)
			{
				BattleLogManager.GetInstance().AddLogSkillBuffAddMaxLife(buffCards, -_gainMaxLife, this);
			}
			if (_gainOffense > 0 || _gainLife > 0)
			{
				BattleLogManager.GetInstance().AddLogSkillBuffAdd(buffCards, -_gainOffense, -_gainLife, this, isMinusZeroAttack, isMinusZeroLife);
			}
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer2);
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
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			BattleCardBase targetCard = item._targetCard;
			parallelVfxPlayer.Register(targetCard.SkillApplyInformation.DepriveCombatValueModifire(item.OffenseModifier, item.LifeModifier));
			targetCard.RemoveBuffInfo(item._buffInfo);
			if ((item._intValue & BIT_FLAG_BUFF_EFFECT) != 0)
			{
				targetCard.SkillApplyInformation.DepriveBuff();
			}
			else
			{
				targetCard.SkillApplyInformation.DepriveDebuff();
			}
		}
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}
}
