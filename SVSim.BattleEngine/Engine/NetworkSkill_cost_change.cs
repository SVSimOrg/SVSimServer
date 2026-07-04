using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

public class NetworkSkill_cost_change : Skill_cost_change
{
	private bool isCall;

	public bool IsForHandResident { get; private set; }

	public NetworkSkill_cost_change(NetworkBattleManagerBase battleManager, SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
		NetworkSkill_cost_change networkSkill_cost_change = this;
		base.OnSkillEnd += delegate
		{
			networkSkill_cost_change.isCall = false;
			return NullVfx.GetInstance();
		};
		base.OnSkillStart += delegate(SkillBase skill, List<BattleCardBase> targetCards, SkillConditionCheckerOption checkerOption)
		{
			if (!networkSkill_cost_change.isCall && networkSkill_cost_change.IsSend(networkSkill_cost_change) && !(skill.ApplyBattlePlayerFilter is BothBattlePlayerFilter) && !skill.IsUserSelectType && battleManager._networkBattleSetupCardEventBase.IsSettingUnapprovedCard(networkSkill_cost_change) && ((networkSkill_cost_change.SkillPrm.ownerCard.IsPlayer && networkSkill_cost_change.ApplyBattlePlayerFilter is OpponentBattlePlayerFilter) || (!networkSkill_cost_change.SkillPrm.ownerCard.IsPlayer && networkSkill_cost_change.ApplyBattlePlayerFilter is SelfBattlePlayerFilter)) && networkSkill_cost_change.IsPrivate())
			{
				networkSkill_cost_change.SettingCostChangeData();
				ICardCostModifier modifire = networkSkill_cost_change.CreateCostModifier();
				networkSkill_cost_change.PrivateTargetRegister(battleManager, modifire, targetCards);
				networkSkill_cost_change.isCall = true;
			}
		};
		OnAccumulationCostChange = (Action<BattleCardBase, List<BattleCardBase>, ICardCostModifier>)Delegate.Combine(OnAccumulationCostChange, (Action<BattleCardBase, List<BattleCardBase>, ICardCostModifier>)delegate(BattleCardBase ownerCard, List<BattleCardBase> targetCards, ICardCostModifier modifier)
		{
			List<BattleCardBase> list = targetCards.Where((BattleCardBase c) => c.IsInHand || c.IsInDeck).ToList();
			bool flag = list.Count == 1 && list.Any((BattleCardBase c) => c == ownerCard) && networkSkill_cost_change.OnWhenFusion == 0 && networkSkill_cost_change.OnWhenReturnStart == 0 && !networkSkill_cost_change.IsOpen();
			if ((!networkSkill_cost_change.isCall || networkSkill_cost_change.HasEachTargetOption()) && networkSkill_cost_change.IsSend(networkSkill_cost_change) && list != null && list.Count > 0 && !flag)
			{
				List<BattleCardBase> list2 = list.Where((BattleCardBase c) => c.IsPlayer).ToList();
				List<BattleCardBase> list3 = list.Where((BattleCardBase c) => !c.IsPlayer).ToList();
				bool flag2 = networkSkill_cost_change.IsPrivate();
				if (list2.Count > 0)
				{
					networkSkill_cost_change.PublicTargetRegister(battleManager, modifier, list2);
				}
				if (list3.Count > 0)
				{
					if (flag2)
					{
						networkSkill_cost_change.PrivateTargetRegister(battleManager, modifier, list3);
					}
					else
					{
						networkSkill_cost_change.PublicTargetRegister(battleManager, modifier, list3);
					}
				}
				networkSkill_cost_change.isCall = true;
			}
		});
	}

	private bool IsPrivate()
	{
		if (NetworkBattleGenericTool.IsNeedUnapprovedListSkill(this) || RegisterFilter.IsFilterCard(this))
		{
			return !IsOpen();
		}
		return false;
	}

	public override void SkillCreateEnd()
	{
		base.SkillCreateEnd();
		if (base.SkillPrm.ownerCard.IsPlayer)
		{
			if (NetworkBattleGenericTool.IsNeedUnapprovedListSkill(this))
			{
				NetworkExecutionInfoCreator networkExecutionInfoCreator = base._executionInfoCreator as NetworkExecutionInfoCreator;
				if (base.IsWhenDestroySkill || OnWhenReturnStart != 0)
				{
					networkExecutionInfoCreator.SetPlaySkill();
					base.OnSkillEnd += PublicAnytimeRandomTargetRegister;
					isCall = true;
				}
				else
				{
					base.OnSkillEnd += NetworkBattleGenericTool.Event_SetupPlayerUnapprovedAddEvent;
				}
			}
		}
		else if (NetworkBattleGenericTool.IsNeedUnapprovedListSkill(this))
		{
			NetworkExecutionInfoCreator networkExecutionInfoCreator2 = base._executionInfoCreator as NetworkExecutionInfoCreator;
			if (base.IsWhenDestroySkill || OnWhenReturnStart != 0)
			{
				networkExecutionInfoCreator2.SetPlaySkill();
				base.OnSkillEnd += PrivateAnytimeRandomTargetRegister;
				isCall = true;
			}
			networkExecutionInfoCreator2.SetUnapproved();
		}
	}

	public void CheckForHandResident(List<SkillBase> skillList)
	{
		IsForHandResident = false;
		if (base.OptionValue.HasInfoByName(SkillFilterCreator.ContentKeyword.set) || base.OptionValue.HasInfoByName(SkillFilterCreator.ContentKeyword.random_set_range))
		{
			if (base.ApplyingTargetFilter is SkillTargetHandFilter && base.ApplySelectFilter is SkillSelectAllFilter && OnSelfTurnStartStart == 1)
			{
				IsForHandResident = true;
			}
			else if (base.ApplyingTargetFilter is SkillTargetSkillDrewCardFilter && skillList.Any((SkillBase s) => s.SkillTimingText == SkillTimingText && s.ApplyingTargetFilter is SkillTargetDestroyedThisTurnCardListFilter))
			{
				IsForHandResident = true;
			}
			else if (skillList.Any((SkillBase s) => s.SkillTimingText == SkillTimingText && s is Skill_draw) && !base.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessTurnEndStop))
			{
				IsForHandResident = true;
			}
		}
	}

	private VfxBase PublicAnytimeRandomTargetRegister(SkillBase skillBase, List<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		if (cards != null && cards.Count() >= 1)
		{
			PublicTargetRegister(SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase, CreateCostModifier(), cards);
			return NetworkBattleGenericTool.Event_SetupPlayerUnapprovedAddEvent(skillBase, cards, checkerOption, skillProcessor);
		}
		if (cards == null || cards.Count() == 0)
		{
			(SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase).StableRandomDouble();
		}
		return NullVfx.GetInstance();
	}

	private VfxBase PrivateAnytimeRandomTargetRegister(SkillBase skillBase, IEnumerable<BattleCardBase> cards, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor)
	{
		RegisterLotCardBase registerLotCardBase = null;
		NetworkBattleManagerBase networkBattleManagerBase = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase;
		if (skillBase.SkillPrm.ownerCard.SelfBattlePlayer.IsSelfTurn && cards != null && cards.Count() >= 1)
		{
			registerLotCardBase = NetworkBattleGenericTool.MakeRegisterLotAndRandomAdvance(skillBase, cards, checkerOption);
		}
		else
		{
			NetworkBattleDefine.NetworkCardPlaceState networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.None;
			if (skillBase.ApplyingTargetFilter is SkillTargetHandFilter || skillBase.ApplyingTargetFilter is SkillTargetHandOtherSelfFilter)
			{
				networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Hand;
			}
			else if (skillBase.ApplyingTargetFilter is SkillTargetDeckFilter)
			{
				networkCardPlaceState = NetworkBattleDefine.NetworkCardPlaceState.Deck;
			}
			double num = 0.0;
			registerLotCardBase = new RegisterLotCardBase(rand: (cards == null || cards.Count() < 1) ? networkBattleManagerBase.StableRandomDouble() : networkBattleManagerBase.randomResult, registerActionManager: networkBattleManagerBase.RegisterActionManager, mgr: networkBattleManagerBase, isplayer: false, from: networkCardPlaceState, targetIndex: -1, skill: this);
			registerLotCardBase.SettingTargetStatusToSearchSkill(networkBattleManagerBase, skillBase);
			networkBattleManagerBase.RegisterActionManager.Add(registerLotCardBase);
		}
		SettingCostChangeData();
		RegisterCostChangeCard data = new RegisterCostChangeCard(networkBattleManagerBase, networkBattleManagerBase.RegisterActionManager, CreateCostModifier(), null, this, checkerOption, registerLotCardBase, isNotCheckCard: true);
		networkBattleManagerBase.RegisterActionManager.Add(data);
		return NullVfx.GetInstance();
	}

	private void PrivateTargetRegister(NetworkBattleManagerBase battleManager, ICardCostModifier modifire, List<BattleCardBase> targets)
	{
		bool flag = base.ApplyBattlePlayerFilter is SelfBattlePlayerFilter;
		if (!base.SkillPrm.ownerCard.IsPlayer)
		{
			flag = !flag;
		}
		RegisterTargetBase registerTargetBase = null;
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		if (NetworkBattleGenericTool.IsNeedUnapprovedListSkill(this))
		{
			if (targets.Count == 0)
			{
				return;
			}
			battleManager._networkBattleSetupCardEventBase.SetSkillTargetsConditionCheckUList(this, targets, skillConditionCheckerOption);
			registerTargetBase = NetworkBattleGenericTool.MakeRegisterLotAndRandomAdvance(this, targets, skillConditionCheckerOption);
		}
		else if (RegisterFilter.IsFilterCard(this))
		{
			if ((base.ApplyingTargetFilter is SkillTargetSkillUpdateDeckCardFilter && targets.Count == 0) || (base.ApplyingTargetFilter is SkillTargetLoadTargetFilter && OnBeforeAttackStart != 0 && targets.Count((BattleCardBase c) => !c.IsInCemetery) == 0))
			{
				return;
			}
			battleManager._networkBattleSetupCardEventBase.SetSkillTargetsCondition(this, skillConditionCheckerOption);
			registerTargetBase = new RegisterFilter(battleManager.RegisterActionManager, battleManager, flag, this, targets, isStop: false, skillConditionCheckerOption);
			battleManager.RegisterActionManager.Add(registerTargetBase);
		}
		RegisterCostChangeCard register = new RegisterCostChangeCard(battleManager, battleManager.RegisterActionManager, modifire, targets, this, skillConditionCheckerOption, registerTargetBase);
		battleManager._networkBattleSetupCardEventBase.AddRegisterActionManager(register);
	}

	private void PublicTargetRegister(NetworkBattleManagerBase battleManager, ICardCostModifier modifire, List<BattleCardBase> targets)
	{
		SkillConditionCheckerOption option = new SkillConditionCheckerOption();
		battleManager._networkBattleSetupCardEventBase.SetSkillTargetsCondition(this, option);
		RegisterCostChangeCard register = new RegisterCostChangeCard(battleManager, battleManager.RegisterActionManager, modifire, targets, this, option);
		battleManager._networkBattleSetupCardEventBase.AddRegisterActionManager(register);
	}

	private bool IsSend(SkillBase skill)
	{
		if (((skill.SkillPrm.ownerCard.FinalMetamorphoseCard != null) ? skill.SkillPrm.ownerCard.FinalMetamorphoseCard : skill.SkillPrm.ownerCard).IsInHand && skill.OnWhenReturnStart == 0 && skill.OnWhenFusion == 0 && !IsOpen())
		{
			return false;
		}
		return true;
	}

	public override void SetEventAfterReplace(BattleCardBase card, BuffInfo buff)
	{
		card.OnResetCardParameter += delegate
		{
			card.RemoveBuffInfo(buff);
		};
	}

	protected override bool IsUnconditionalCostChange()
	{
		BattleCardBase ownerCard = base.SkillPrm.ownerCard;
		GameMgr ins = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr;
		if (!ownerCard.IsPlayer && !ins.IsAdminWatch && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsReplayBattle && RegisterFilter.IsFilterPreprocessCondition(this))
		{
			return !IsRightSkillPreprocessConditionCheck();
		}
		return false;
	}

	private bool IsRightSkillPreprocessConditionCheck()
	{
		if (!(base.PreprocessList.FirstOrDefault((SkillPreprocessBase p) => p is SkillPreprocessConditionCheck) is SkillPreprocessConditionCheck skillPreprocessConditionCheck))
		{
			return true;
		}
		return skillPreprocessConditionCheck.GetFilter().VariableCompareFilter.All((SkillVariableComareFilter s) => s.Filtering(base.OptionValue));
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		int count = base.CostModifierTypeList.Count;
		VfxWithLoading result = base.Start(parameter);
		List<ICardCostModifier> range = base.CostModifierTypeList.GetRange(count, base.CostModifierTypeList.Count - count);
		bool isOpenCard = base.SkillPrm.ownerCard.IsPlayer && base.PreprocessList.FirstOrDefault((SkillPreprocessBase p) => p is SkillPreprocessOpenCard) != null && base.ApplyingTargetFilter is SkillTargetHandSelfFilter;
		base.SkillPrm.selfBattlePlayer.CallOnCostChange(base.SkillPrm.ownerCard, _targets, (from c in range
			where c is CostAddModifier
			select c.Cost).ToList(), (from c in range
			where c is CostSetModifier
			select c.Cost).ToList(), _isCostUpList, IsHalfRoundUpCostSkill(SkillFilterCreator.ContentKeyword.add), OnWhenSpellChargeStart != 0, isOpenCard);
		return result;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.SkillPrm.ownerCard.SelfBattlePlayer.CallOnRemoveCostChange(buffInfoContainer, OnWhenSpellChargeStart != 0, _addValue == int.MinValue || _addValue <= 0);
		return base.Stop(skillProcessor);
	}
}
