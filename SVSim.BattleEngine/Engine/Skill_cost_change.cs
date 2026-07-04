using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_cost_change : SkillBase
{
	private class CostModifierContainer
	{
		public BattleCardBase _targetCard;

		public ICardCostModifier _costModifier;

		public CostModifierContainer(BattleCardBase targetCard, ICardCostModifier costModifier)
		{
			_targetCard = targetCard;
			_costModifier = costModifier;
		}
	}

	protected int _addValue = int.MinValue;

	protected int _setValue = int.MinValue;

	protected Action<BattleCardBase, List<BattleCardBase>, ICardCostModifier> OnAccumulationCostChange;

	private List<BuffInfoContainer> _targetList = new List<BuffInfoContainer>();

	protected List<BattleCardBase> _targets;

	protected List<bool> _isCostUpList;

	public override bool IsTargetIndicate
	{
		get
		{
			BattlePlayerBase battlePlayerBase = ((base.ApplyBattlePlayerFilter is SelfBattlePlayerFilter) ? base.SkillPrm.ownerCard.SelfBattlePlayer : base.SkillPrm.ownerCard.OpponentBattlePlayer);
			if (base.ApplyingTargetFilter is SkillTargetDeckFilter && battlePlayerBase.DeckCardList.Count == 0)
			{
				return true;
			}
			if (IsTargetInHand() && battlePlayerBase.HandCardList.Count == 0)
			{
				return true;
			}
			return false;
		}
	}

	public List<ICardCostModifier> CostModifierTypeList { get; private set; } = new List<ICardCostModifier>();

	protected override bool IsBattleLog
	{
		get
		{
			bool flag = !base.IsContainSelfFilter || IsOpen();
			return base.IsBattleLog && OnWhenSpellChargeStart == 0 && flag;
		}
	}

	public Skill_cost_change(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		bool flag = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.show_battle_log, "true") == "true";
		bool flag2 = IsHalfRoundUpCostSkill(SkillFilterCreator.ContentKeyword.add);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		if (!HasEachTargetOption())
		{
			SettingCostChangeData();
			CostModifierTypeList.Add(CreateCostModifier());
		}
		List<int> list = new List<int>();
		_isCostUpList = new List<bool>();
		_targets = new List<BattleCardBase>();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			if (HasEachTargetOption())
			{
				SettingCostChangeData();
				CostModifierTypeList.Add(CreateCostModifier());
			}
			if (CostModifierTypeList.LastOrDefault() == null)
			{
				break;
			}
			if ((!targetCard.IsInHand && !targetCard.IsInDeck) || IsUnconditionalCostChange())
			{
				continue;
			}
			if (targetCard.IsInHand && _setValue != int.MinValue)
			{
				list.Add(_setValue - targetCard.Cost);
			}
			if (targetCard.IsInHand && flag2)
			{
				list.Add(targetCard.Cost / 2 * -1);
			}
			int cost = targetCard.Cost;
			ICardCostModifier cardCostModifier = CostModifierTypeList.LastOrDefault();
			targetCard.AddCostModifier(cardCostModifier, this);
			buffInfoContainer.Add(new BuffInfoContainer(targetCard, null, -1, "", null, 0L, null, CostModifierTypeList.LastOrDefault()));
			_targets.Add(targetCard);
			_isCostUpList.Add(((_addValue != int.MinValue && _addValue > 0) || (_setValue != int.MinValue && _setValue > cost)) && base.SkillPrm.ownerCard.BaseParameter.BaseCardId != 123841020 && base.SkillPrm.ownerCard.BaseParameter.BaseCardId != 130241030);
			_ = string.Empty;
			BuffInfo buffInfo = null;
			if (_addValue != int.MinValue || _setValue != int.MinValue || cardCostModifier is CostHalfModifier)
			{
				buffInfo = AddBuffInfoIfNeeded(targetCard);
				BuffInfoContainer value = new BuffInfoContainer(targetCard, buffInfo, -1, "", null, 0L);
				_targetList.Add(value);
				SetOnLoseEvent(value._targetCard, buffInfo, value);
				targetCard.OnResetCardParameter += delegate
				{
					value._targetCard.RemoveBuffInfo(value._buffInfo);
					buffInfoContainer.Remove(value);
				};
				if (OnAccumulationCostChange != null && HasEachTargetOption())
				{
					OnAccumulationCostChange(base.SkillPrm.ownerCard, new List<BattleCardBase> { targetCard }, CostModifierTypeList.LastOrDefault());
				}
			}
		}
		VfxWithLoadingSequential costChangeVfx = VfxWithLoadingSequential.Create();
		parallelVfxPlayer.Register(costChangeVfx.MainVfx);
		if (OnAccumulationCostChange != null && !HasEachTargetOption())
		{
			OnAccumulationCostChange(base.SkillPrm.ownerCard, parameter.targetCards.Where((BattleCardBase c) => !c.IsInplay).ToList(), CostModifierTypeList.LastOrDefault());
		}
		parallelVfxPlayer.Register(InstantVfx.Create(delegate
		{
			base.SkillPrm.selfBattlePlayer.UpdateHandCardsPlayability();
		}));
		List<BattleCardBase> list2 = new List<BattleCardBase>();
		_ = base.SkillPrm.ownerCard.BaseParameter.BaseCardId;
		if ((base.SkillPrm.ownerCard.IsPlayer && base.SkillPrm.buildInfo._effectMoveType == EffectMgr.MoveType.DIRECT_HAND) || base.SkillPrm.buildInfo._effectMoveType == EffectMgr.MoveType.CENTER)
		{
			list2.Add(base.SkillPrm.ownerCard.OpponentBattlePlayer.Class);
		}
		else if (base.SkillPrm.buildInfo._effectMoveType != EffectMgr.MoveType.DIRECT_HAND)
		{
			list2.AddRange(parameter.targetCards);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(base.SkillPrm.resourceMgr, list2));
		vfxWithLoadingSequential.RegisterToLoadingVfx(costChangeVfx.LoadingVfx);
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		if (IsBattleLog && flag)
		{
			AddBattleLog(parameter.targetCards.Where((BattleCardBase c) => c.IsInHand), list, flag2);
		}
		SkillPreprocessBase skillPreprocessBase = base.PreprocessList.FirstOrDefault((SkillPreprocessBase p) => p is SkillPreprocessOpenCard);
		if (skillPreprocessBase != null && base.ApplyingTargetFilter is SkillTargetHandSelfFilter)
		{
			vfxWithLoadingSequential.RegisterToMainVfx((skillPreprocessBase as SkillPreprocessOpenCard).CreateOpenCardVfx(this));
		}
		return vfxWithLoadingSequential;
	}

	protected virtual bool IsUnconditionalCostChange()
	{
		return false;
	}

	protected void SettingCostChangeData()
	{
		bool num = IsHalfRoundUpCostSkill(SkillFilterCreator.ContentKeyword.set);
		bool flag = IsHalfRoundUpCostSkill(SkillFilterCreator.ContentKeyword.add);
		bool flag2 = IsHalfRoundDownCostSkill(SkillFilterCreator.ContentKeyword.set);
		bool flag3 = IsHalfRoundDownCostSkill(SkillFilterCreator.ContentKeyword.add);
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.random_set_range);
		if (num || flag2)
		{
			_setValue = int.MinValue;
		}
		else if (string.IsNullOrEmpty(text))
		{
			_setValue = base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set, int.MinValue);
		}
		else
		{
			_setValue = GetRandomCost(text);
		}
		_addValue = ((flag || flag3) ? int.MinValue : base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add, int.MinValue));
	}

	private int GetRandomCost(string costRange)
	{
		IEnumerable<int> source = from x in costRange.Split(':').ToList()
			select base.OptionValue.ParseInt(x);
		List<int> list = CreateSequentialCostList(source.Min(), source.Max());
		int index = base.SkillPrm.selfBattlePlayer.BattleMgr.StableRandom(list.Count);
		return list[index];
	}

	private List<int> CreateSequentialCostList(int min, int max)
	{
		List<int> list = new List<int>();
		for (int i = min; i <= max; i++)
		{
			list.Add(i);
		}
		return list;
	}

	protected ICardCostModifier CreateCostModifier()
	{
		if (_addValue != int.MinValue)
		{
			return new CostAddModifier(_addValue, base.IsHandResident);
		}
		if (_setValue != int.MinValue)
		{
			return new CostSetModifier(_setValue, base.IsHandResident);
		}
		if (IsHalfRoundUpCostSkill(SkillFilterCreator.ContentKeyword.add))
		{
			return new CostHalfRoundUpModifier(base.IsHandResident);
		}
		if (IsHalfRoundDownCostSkill(SkillFilterCreator.ContentKeyword.add))
		{
			return new CostHalfRoundDownModifier(base.IsHandResident);
		}
		return null;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		foreach (BuffInfoContainer target in _targetList)
		{
			target._targetCard.RemoveBuffInfo(target._buffInfo);
		}
		List<BattleCardBase> list = new List<BattleCardBase>();
		foreach (BuffInfoContainer item in buffInfoContainer)
		{
			item._targetCard.RemoveCostModifier(this, item.CostModifier);
			list.Add(item._targetCard);
		}
		buffInfoContainer.Clear();
		StopEnd(skillProcessor);
		return VfxWithLoading.Create(VfxWithLoadingSequential.Create().MainVfx);
	}

	protected bool IsHalfRoundUpCostSkill(SkillFilterCreator.ContentKeyword nameType)
	{
		if (base.OptionValue.GetString(nameType, "NONE") == "half")
		{
			return true;
		}
		return false;
	}

	protected bool IsHalfRoundDownCostSkill(SkillFilterCreator.ContentKeyword nameType)
	{
		if (base.OptionValue.GetString(nameType, "NONE") == "half_round_down")
		{
			return true;
		}
		return false;
	}

	protected bool HasEachTargetOption()
	{
		return base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "NONE") == "each_target";
	}

	protected bool IsOpen()
	{
		bool flag = base.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessOpenCard);
		int num = base.SkillPrm.ownerCard.Skills.IndexOf(this);
		if (num > 0 && OnWhenDraw != 0)
		{
			SkillBase skillBase = base.SkillPrm.ownerCard.Skills.ElementAt(num - 1);
			if (skillBase.OnWhenDraw != 0)
			{
				flag |= skillBase.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessOpenCard);
			}
		}
		return flag;
	}

	private void AddBattleLog(IEnumerable<BattleCardBase> targetCards, List<int> setCostDifferenceList, bool isOptionAddHalfRoundUp)
	{
		int cost = 0;
		bool flag = false;
		if (_addValue != int.MinValue)
		{
			cost = _addValue;
		}
		else if (_setValue != int.MinValue)
		{
			flag = true;
			cost = _setValue;
		}
		bool isTargetInOpponentHand = IsTargetInOpponentHand();
		BattleLogManager.GetInstance().AddLogCostChange(targetCards.ToList(), this, cost, flag || isOptionAddHalfRoundUp, isTargetInOpponentHand, setCostDifferenceList);
	}

	protected override bool CheckShowSideLogCondition(IEnumerable<BattleCardBase> targets, bool isTargetsAvailable, SkillProcessor.ProcessCallType type)
	{
		if (base.IsBeforAttackSkill)
		{
			return true;
		}
		switch (type)
		{
		case SkillProcessor.ProcessCallType.Start:
		{
			if ((!SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch && !base.SkillPrm.ownerCard.IsPlayer && !(base.ApplyingTargetFilter is SkillTargetSelfFilter) && !(base.ApplyingTargetFilter is SkillTargetHandSelfFilter) && base.SkillPrm.ownerCard.SelfBattlePlayer.HandCardList.Count > 0) || base.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessOpenCard))
			{
				return true;
			}
			if (!base.SkillPrm.ownerCard.IsPlayer && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch)
			{
				break;
			}
			bool num = !IsHalfRoundUpCostSkill(SkillFilterCreator.ContentKeyword.set) && !IsHalfRoundDownCostSkill(SkillFilterCreator.ContentKeyword.set) && (base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.set, int.MinValue) != int.MinValue || base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.random_set_range, "NONE") != "NONE");
			bool flag = IsHalfRoundUpCostSkill(SkillFilterCreator.ContentKeyword.add) || IsHalfRoundDownCostSkill(SkillFilterCreator.ContentKeyword.add) || base.OptionValue.GetInt(SkillFilterCreator.ContentKeyword.add, int.MinValue) != int.MinValue;
			if ((num || flag) && isTargetsAvailable)
			{
				return targets.Any((BattleCardBase t) => t.IsInHand || t.IsInDeck);
			}
			return false;
		}
		case SkillProcessor.ProcessCallType.ResidentStop:
			if (!base.SkillPrm.ownerCard.IsPlayer && base.SkillPrm.ownerCard.IsInHand && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch)
			{
				return false;
			}
			if (base.SkillPrm.ownerCard.IsPlayer || SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch)
			{
				return true;
			}
			break;
		default:
			return false;
		}
		return false;
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return NullVfx.GetInstance();
		};
	}
}
