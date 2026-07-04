using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_damage_cut : SkillBase
{
	private class DamageCutBuffContainer : BuffInfoContainer
	{
		public DamageCutInfo DamageCutInfo { get; private set; }

		public DamageClippingInfo DamageClipping { get; private set; }

		public DamageCutBuffContainer(BattleCardBase card, BuffInfo info, DamageCutInfo damageCut, DamageClippingInfo damageClipping)
			: base(card, info, -1, "", null, 0L)
		{
			DamageCutInfo = damageCut;
			DamageClipping = damageClipping;
		}
	}

	private string _maxParameter;

	private string _minParameter;

	protected List<BattleCardBase> _targetCards;

	private DamageCutInfo.DamageType _type;

	public int CutAmount { get; private set; }

	public int ClippingMax { get; private set; }

	public int LifeLowerLimit { get; private set; }

	public Skill_damage_cut(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		SetDuplicateBanSkillNum();
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		CutAmount = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.cut_amount, 0);
		LifeLowerLimit = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.life_lower_limit, -1);
		ClippingMax = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.cut_clipping, int.MaxValue);
		_maxParameter = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.cut_clipping_range_max, string.Empty);
		_minParameter = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.cut_clipping_range_min, string.Empty);
		_type = base.OptionValue.GetDamageCutGainType();
		_targetCards = parameter.targetCards.ToList();
		if (base.DuplicateBanSkillNum != string.Empty)
		{
			_targetCards = _targetCards.Where((BattleCardBase c) => !c.SkillApplyInformation.DamageCutList.Any((DamageCutInfo d) => (!base.IsDuplicateBanSelfSkill || d.OwnerCard == base.SkillPrm.ownerCard) && d.DuplicateBanSkillNum == base.DuplicateBanSkillNum)).ToList();
		}
		VfxBase vfxToRegister = NullVfx.GetInstance();
		if (CutAmount > 0)
		{
			DamageCutInfo damageCutInfo = new DamageCutInfo(CutAmount, _type, base.SkillPrm.ownerCard, base.DuplicateBanSkillNum);
			vfxToRegister = ProcessGiveAndDeprive(_targetCards, (BattleCardBase c) => c.SkillApplyInformation.GiveDamageCut(damageCutInfo), damageCutInfo, null);
			if (IsBattleLog)
			{
				BattleLogManager.GetInstance().AddLogSkillGain(_targetCards, this, SkillGainType.DamageCut, CutAmount);
			}
		}
		else if (ClippingMax != int.MaxValue || LifeLowerLimit > 0)
		{
			DamageClippingInfo clipping = new DamageClippingInfo(ClippingMax, _maxParameter, _minParameter, LifeLowerLimit);
			vfxToRegister = ProcessGiveAndDeprive(_targetCards, (BattleCardBase c) => c.SkillApplyInformation.GiveDamageMaxClipping(clipping), null, clipping);
			if (IsBattleLog)
			{
				if (ClippingMax != int.MaxValue)
				{
					BattleLogManager.GetInstance().AddLogSkillGain(_targetCards, this, SkillGainType.DamageMaxClipping, ClippingMax);
				}
				else
				{
					BattleLogManager.GetInstance().AddLogSkillGain(_targetCards, this, SkillGainType.LifeLowerLimit, LifeLowerLimit);
				}
			}
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, _targetCards));
		vfxWithLoadingSequential.RegisterToMainVfx(vfxToRegister);
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (DamageCutBuffContainer item in buffInfoContainer)
		{
			if (item.DamageCutInfo != null)
			{
				parallelVfxPlayer.Register(item._targetCard.SkillApplyInformation.DepriveDamageCut(item.DamageCutInfo));
			}
			else if (item.DamageClipping != null)
			{
				parallelVfxPlayer.Register(item._targetCard.SkillApplyInformation.DepriveDamageMaxClipping(item.DamageClipping));
			}
			list.Add(item._targetCard);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
			if (item._targetCard.IsClass)
			{
				UpdateClassBuffIfActive(item._targetCard);
			}
		}
		CallOnUpdateSkillEffect(list);
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	private VfxBase ProcessGiveAndDeprive(IEnumerable<BattleCardBase> targets, Func<BattleCardBase, VfxBase> giveFunc, DamageCutInfo damageCut, DamageClippingInfo damageClipping)
	{
		BuffInfo buffInfo = null;
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase target in targets)
		{
			BattleCardBase battleCardBase = target;
			VfxBase vfx = giveFunc(battleCardBase);
			if (IsBattleLog)
			{
				buffInfo = AddBuffInfoIfNeeded(battleCardBase);
			}
			if (buffInfo != null && base.SkillPrm.ownerCard.IsClass && base.SkillPrm.ownerCard == target)
			{
				buffInfo.IsHiddenClassLogSkill = true;
			}
			if (target.IsClass)
			{
				UpdateClassBuffIfActive(target);
			}
			DamageCutBuffContainer damageCutBuffContainer = new DamageCutBuffContainer(battleCardBase, buffInfo, damageCut, damageClipping);
			buffInfoContainer.Add(damageCutBuffContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, damageCutBuffContainer);
			parallelVfxPlayer.Register(vfx);
		}
		return parallelVfxPlayer;
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return (CutAmount > 0) ? card.SkillApplyInformation.FourceDepriveDamageCut() : card.SkillApplyInformation.ForceDepriveDamageMaxClipping();
		};
	}
}
