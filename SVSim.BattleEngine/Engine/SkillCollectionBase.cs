using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.View.Vfx;

public class SkillCollectionBase : IEnumerable<SkillBase>, IEnumerable
{
	public enum WhenPlayEffectType
	{
		Berserk,
		Awake,
		WhenPlay,
		WhenDestroy,
		None
	}

	private class ActiveSkillWhenPlayEffectInfo
	{
		public BattleCardBase OwnerCard { get; private set; }

		public int Index { get; private set; }

		public WhenPlayEffectType Type { get; set; }

		public ActiveSkillWhenPlayEffectInfo(BattleCardBase ownerCard, int index, WhenPlayEffectType type)
		{
			OwnerCard = ownerCard;
			Index = index;
			Type = type;
		}
	}

	private BattleCardBase _ownerCard;

	protected List<SkillBase> _skillList = new List<SkillBase>();

	public SkillTimingInfo _skillTimingInfo;

	private bool HasInductionSkillBeenActivated;

	public bool CheckWhenPlaySelectTargetSkillCondition
	{
		get
		{
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(_ownerCard.SelfBattlePlayer, _ownerCard.OpponentBattlePlayer);
			SkillConditionCheckerOption option = new SkillConditionCheckerOption();
			return _skillList.Any((SkillBase s) => s.IsWhenPlaySkill && s.CheckCondition(playerInfoPair, option, isPrePlay: true) && ((s.IsUserSelectType && s.GetSelectableCards(playerInfoPair, option).Any()) || (!s.IsUserSelectType && s.IsBurialRite && s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessBurialRite && p.IsRightPrePlay(playerInfoPair, option))) || (s.IsUserSelectType && s.IsBurialRite && s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessBurialRite && p.IsRightPrePlay(playerInfoPair, option)) && s.GetSelectableCards(playerInfoPair, option).Any())));
		}
	}

	public bool HasEvolutionSkillWithSelection
	{
		get
		{
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(_ownerCard.SelfBattlePlayer, _ownerCard.OpponentBattlePlayer);
			SkillConditionCheckerOption option = new SkillConditionCheckerOption();
			return _skillList.Any((SkillBase s) => s.IsWhenEvolveSkill && s.CheckCondition(playerInfoPair, option, isPrePlay: true) && ((s.IsUserSelectType && s.GetSelectableCards(playerInfoPair, option).Any()) || (!s.IsUserSelectType && s.IsBurialRite && s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessBurialRite && p.IsRightPrePlay(playerInfoPair, option))) || (s.IsUserSelectType && s.IsBurialRite && s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessBurialRite && p.IsRightPrePlay(playerInfoPair, option)) && s.GetSelectableCards(playerInfoPair, option).Any())));
		}
	}

	public bool CheckWhenPlayChoice
	{
		get
		{
			BattlePlayerReadOnlyInfoPair playerInfoPair = new BattlePlayerReadOnlyInfoPair(_ownerCard.SelfBattlePlayer, _ownerCard.OpponentBattlePlayer);
			SkillConditionCheckerOption option = new SkillConditionCheckerOption();
			return _skillList.Any((SkillBase s) => s.IsChoiceType && s.CheckCondition(playerInfoPair, option, isPrePlay: true));
		}
	}

	public SkillCollectionBase(BattleCardBase ownerCard)
	{
		_ownerCard = ownerCard;
	}

	public IEnumerator<SkillBase> GetEnumerator()
	{
		return _skillList.GetEnumerator();
	}

	public int IndexOf(SkillBase skill)
	{
		return _skillList.IndexOf(skill);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Add(SkillBase skill)
	{
		if (skill is NetworkSkill_cost_change networkSkill_cost_change)
		{
			networkSkill_cost_change.CheckForHandResident(_skillList);
		}
		_skillList.Add(skill);
	}

	public SkillBase Get(int index)
	{
		if (_skillList.Count <= index)
		{
			return null;
		}
		return _skillList[index];
	}

	public void Remove(SkillBase skill)
	{
		_skillList.Remove(skill);
	}

	public void FirstAdd(SkillBase skill)
	{
		_skillList.Insert(0, skill);
	}

	public void Clear()
	{
		_skillList.Clear();
	}

	public SkillCollectionBase Clone(BattleCardBase card)
	{
		SkillCollectionBase obj = (SkillCollectionBase)MemberwiseClone();
		obj._ownerCard = card;
		obj._skillList = new List<SkillBase>(_skillList);
		return obj;
	}

	public void SetAndAddPublishedActiveSkillsCount()
	{
		for (int i = 0; i < _skillList.Count; i++)
		{
			_skillList[i].SetAndAddPublishedActiveSkillCount();
		}
	}

	public bool HaveBeforeChoiceSkill()
	{
		for (int i = 0; i < _skillList.Count(); i++)
		{
			if (_skillList[i].OptionValue.GetString(SkillFilterCreator.ContentKeyword.is_before_choice_transform, string.Empty) == "true")
			{
				return true;
			}
		}
		return false;
	}

	public bool HaveNotAttachedResidentChantCountChangeSkill()
	{
		return _skillList.Any((SkillBase s) => s is Skill_not_attached_resident_chant_count_change);
	}

	public bool HaveChoiceTransformSkill()
	{
		for (int i = 1; i < _skillList.Count(); i++)
		{
			if (_skillList[i] is Skill_transform && _skillList[i - 1] is Skill_choice)
			{
				return true;
			}
		}
		return false;
	}

	public IEnumerable<SkillBase> GetSelectTypeSkill(bool isEvolve, bool isFusion, bool isActivateFanfare, BattlePlayerReadOnlyInfoPair readOnlyInfoPair)
	{
		SkillConditionCheckerOption option = new SkillConditionCheckerOption();
		bool flag = HaveChoiceTransformSkill();
		bool flag2 = isEvolve || flag || isEvolve || !_skillTimingInfo.IsWhenPlay || isActivateFanfare;
		List<SkillBase> list = new List<SkillBase>();
		for (int i = 0; i < _skillList.Count; i++)
		{
			if ((isFusion && !(_skillList[i] is Skill_fusion)) || (!isFusion && _skillList[i] is Skill_fusion))
			{
				continue;
			}
			if ((_skillList[i].IsUserSelectType || (_skillList[i].IsChoiceType && flag2)) && _skillList[i].CheckCondition(readOnlyInfoPair, option, !isEvolve))
			{
				list.Add(_skillList[i]);
			}
			else
			{
				if (!_skillList[i].IsBurialRite)
				{
					continue;
				}
				for (int j = 0; j < _skillList[i].PreprocessList.Count; j++)
				{
					if (_skillList[i].CheckCondition(readOnlyInfoPair, option, !isEvolve) && ((_skillList[i].PreprocessList[j] is SkillPreprocessBurialRite && isEvolve) ? _skillList[i].PreprocessList[j].IsRight(readOnlyInfoPair, option) : _skillList[i].PreprocessList[j].IsRightPrePlay(readOnlyInfoPair, option)))
					{
						list.Add(_skillList[i]);
						break;
					}
				}
			}
		}
		return list;
	}

	public int GetIndividualId()
	{
		return _skillList.FirstOrDefault((SkillBase s) => s.HasIndividualId)?.IndividualId ?? (-1);
	}

	public static List<BattleCardBase> GetCardsOrderBySkillActivation(BattlePlayerBase player, BattlePlayerBase opponent, bool isAll = false, bool containsHand = false, bool containsClass = false, bool containsInplay = false, bool containsDeck = false, Func<BattleCardBase, bool> checkFunc = null)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		bool num = ((!player.IsSelfTurn && !opponent.IsSelfTurn) ? player.IsGameFirst : player.IsSelfTurn);
		BattlePlayerBase battlePlayerBase = (num ? player : opponent);
		BattlePlayerBase battlePlayerBase2 = (num ? opponent : player);
		if (isAll || containsHand)
		{
			list.AddRange(battlePlayerBase.HandCardList);
			list.AddRange(battlePlayerBase2.HandCardList);
		}
		if (isAll || (containsClass && containsInplay))
		{
			list.AddRange(battlePlayerBase.ClassAndInPlayCardList);
			list.AddRange(battlePlayerBase2.ClassAndInPlayCardList);
		}
		else if (containsInplay)
		{
			list.AddRange(battlePlayerBase.InPlayCards);
			list.AddRange(battlePlayerBase2.InPlayCards);
		}
		else if (containsClass)
		{
			list.Add(battlePlayerBase.Class);
			list.Add(battlePlayerBase2.Class);
		}
		if (isAll || containsDeck)
		{
			list.AddRange(battlePlayerBase.DeckSkillCardList);
			list.AddRange(battlePlayerBase2.DeckSkillCardList);
		}
		if (checkFunc != null)
		{
			list = list.Where((BattleCardBase c) => checkFunc(c)).ToList();
		}
		return list;
	}

	public virtual VfxBase RegisterAndProcessWhenChangeInplayImmediateInfo(BattlePlayerPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		SkillProcessor skillProcessor = new SkillProcessor();
		CreateAndRegisterResidentProcessInfo((SkillBase s) => s.OnWhenChangeInPlayImmediate, skillProcessor, playerInfoPair, checkerOption);
		return skillProcessor.Process(playerInfoPair, isImmediate: true);
	}

	public virtual void CreateAndRegisterWhenChangeInplaySelfhandInfo(List<BattleCardBase> drawCards, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		CreateAndRegisterResidentProcessInfo((drawCards != null && drawCards.Any((BattleCardBase c) => c == _ownerCard)) ? ((Func<SkillBase, uint>)((SkillBase s) => s.OnWhenChangeInplaySelfhand + s.OnWhenChangeClassLifeSelfhand)) : ((Func<SkillBase, uint>)((SkillBase s) => s.OnWhenChangeInplaySelfhand)), skillProcessor, playerInfoPair, checkerOption);
	}

	public virtual void CreateAndRegisterWhenChangeInplayInfo(List<BattleCardBase> summonCards, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, bool isSummonCheck = true, Func<SkillBase, uint> checkFunc = null, SkillConditionCheckerOption option = null)
	{
		SkillConditionCheckerOption checkerOption = ((option != null) ? option : new SkillConditionCheckerOption());
		Func<SkillBase, uint> checkFuncOnSummonSelf = ((summonCards != null && summonCards.Any((BattleCardBase c) => c == _ownerCard)) ? ((Func<SkillBase, uint>)((SkillBase s) => s.OnWhenChangeClassLifeInplay + s.OnWhenChangePPTotal + (isSummonCheck ? s.OnWhenSummonStart : 0))) : ((Func<SkillBase, uint>)((SkillBase s) => 0u)));
		Func<SkillBase, uint> checkFuncOtherTiming = ((checkFunc != null) ? checkFunc : ((Func<SkillBase, uint>)((SkillBase s) => 0u)));
		CreateAndRegisterResidentProcessInfo((SkillBase s) => s.OnWhenChangeInPlay + checkFuncOnSummonSelf(s) + checkFuncOtherTiming(s), skillProcessor, playerInfoPair, checkerOption);
	}

	public virtual void CreateAndRegisterWhenAddToHandInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option)
	{
		CreateAndRegisterResidentProcessInfo((SkillBase s) => s.OnWhenAddToHand, skillProcessor, playerInfoPair, option);
	}

	public virtual void CreateAndRegisterWhenChangeClassLifeSelfHandInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		CreateAndRegisterResidentProcessInfo((SkillBase s) => s.OnWhenChangeClassLifeSelfhand, skillProcessor, playerInfoPair, checkerOption);
	}

	public virtual void CreateAndRegisterWhenChangeClassLifeInplayInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		CreateAndRegisterResidentProcessInfo((SkillBase s) => s.OnWhenChangeClassLifeInplay, skillProcessor, playerInfoPair, checkerOption);
	}

	public virtual void CreateAndRegisterWhenChangePPTotalInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		CreateAndRegisterResidentProcessInfo((SkillBase s) => s.OnWhenChangePPTotal, skillProcessor, playerInfoPair, checkerOption);
	}

	public virtual VfxWith<SkillProcessor.ProcessInfo> CreateWhenPlayInfo(BattleCardBase playCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option)
	{
		SkillProcessor.ProcessInfo value = CreateProcessInfo((SkillBase s) => s.OnWhenPlayStart, skillProcessor, playerInfoPair, option, !playCard.IsSpell);
		return new VfxWith<SkillProcessor.ProcessInfo>(NullVfx.GetInstance(), value);
	}

	public virtual SkillProcessor.ProcessInfo CreateWhenBattleStartInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenBattleStart, skillProcessor, playerInfoPair, checkerOption);
	}

	public virtual VfxWith<SkillProcessor.ProcessInfo> CreateWhenChoicePlayInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option)
	{
		SkillProcessor.ProcessInfo value = CreateProcessInfo((SkillBase s) => s.OnWhenChoicePlayStart, skillProcessor, playerInfoPair, option);
		return new VfxWith<SkillProcessor.ProcessInfo>(NullVfx.GetInstance(), value);
	}

	public SkillProcessor.ProcessInfo CreateWhenHandToNotPlayInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option)
	{
		return CreateProcessInfo((SkillBase s) => s.OnWhenHandToNotPlayStart, skillProcessor, playerInfoPair, option);
	}

	public VfxWith<SkillProcessor.ProcessInfo> CreateWhenDestroyInfo(BattleCardBase destroyCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.DestroyedCard = destroyCard;
		SkillProcessor.ProcessInfo processInfo = CreateProcessInfo((SkillBase s) => s.OnWhenDestroyStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
		VfxBase vfx = NullVfx.GetInstance();
		if (_skillList.Any((SkillBase s) => s.IsWhenDestroySkill && !s.IsUserSelectType))
		{
			if (processInfo == null && destroyCard.SelfBattlePlayer.Class.SkillApplyInformation.RepeatSkillTimingList.Any((RepeatSkillInfo s) => s.Timing == "when_destroy" && Skill_repeat_skill.CheckCardType(s.Target, destroyCard)))
			{
				vfx = new RepeatSkillEffectVfx(destroyCard.SelfBattlePlayer.BattleMgr, destroyCard.SelfBattlePlayer.Class.BattleCardView, "when_destroy", destroyCard.IsPlayer);
			}
			AllStopRepeatSkill(destroyCard, destroyCard.SelfBattlePlayer, "when_destroy", processInfo != null, skillProcessor);
		}
		return new VfxWith<SkillProcessor.ProcessInfo>(vfx, processInfo);
	}

	public SkillProcessor.ProcessInfo CreateWhenDestroyOtherInfo(BattleCardBase destroyedCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.DestroyedCard = destroyedCard;
		skillConditionCheckerOption.LeftCards = new List<IReadOnlyBattleCardInfo> { destroyedCard };
		return CreateProcessInfo((SkillBase s) => s.OnWhenDestroyOtherStart + s.OnWhenLeaveOther, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenBanishInfo(BattleCardBase banishedCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.BanishedCard = banishedCard;
		skillConditionCheckerOption.LeftCards = new List<IReadOnlyBattleCardInfo> { banishedCard };
		return CreateProcessInfo((SkillBase s) => s.OnWhenBanish, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenReturnInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenReturnStart, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenReturnOtherInfo(BattleCardBase returnedCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, List<IReadOnlyBattleCardInfo> cantAttackAllReturnCards)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.ReturnCards = new List<IReadOnlyBattleCardInfo> { returnedCard };
		skillConditionCheckerOption.LeftCards = new List<IReadOnlyBattleCardInfo> { returnedCard };
		skillConditionCheckerOption.CantAttackAllReturnCards = cantAttackAllReturnCards;
		return CreateProcessInfo((SkillBase s) => s.OnWhenReturnOtherStart + s.OnWhenLeaveOther, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenReturnSkillActivateInfo(List<BattleCardBase> returnedCards, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.ReturnCards = BattlePlayerBase.ConvertToSkillInfoCollection(returnedCards);
		return CreateProcessInfo((SkillBase s) => s.OnWhenReturnSkillActivateStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenLeaveInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenLeave, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenFusionOtherInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, List<BattleCardBase> fusionIngredientCards)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.FusionIngredientCards = BattlePlayerBase.ConvertToSkillInfoCollection(fusionIngredientCards);
		return CreateProcessInfo((SkillBase s) => s.OnWhenFusionOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public VfxWith<SkillProcessor.ProcessInfo> CreateBeforeAttackInfo(SkillBase attackSkill, BattleCardBase attacker, BattleCardBase target, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.AttackerCard = attacker;
		skillConditionCheckerOption.AttackTargetCard = target;
		SkillProcessor.ProcessInfo value = null;
		if (attackSkill.IsBeforAttackSkill)
		{
			value = CreateProcessInfo(attackSkill, (SkillBase s) => attackSkill.OnBeforeAttackStart, skillProcessor, playerInfoPair, skillConditionCheckerOption, isInductionSkill: false);
		}
		return new VfxWith<SkillProcessor.ProcessInfo>(NullVfx.GetInstance(), value);
	}

	public VfxWith<SkillProcessor.ProcessInfo> CreateBeforeFightInfo(SkillBase attackSkill, BattleCardBase attacker, BattleCardBase target, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.AttackerCard = attacker;
		skillConditionCheckerOption.AttackTargetCard = target;
		SkillProcessor.ProcessInfo value = null;
		if ((attackSkill.IsWhenFightSkill && attackSkill.SkillPrm.ownerCard == attacker && target.IsUnit) || attackSkill.SkillPrm.ownerCard == target)
		{
			value = CreateProcessInfo(attackSkill, (SkillBase s) => attackSkill.OnWhenFightStart, skillProcessor, playerInfoPair, skillConditionCheckerOption, isInductionSkill: false);
		}
		return new VfxWith<SkillProcessor.ProcessInfo>(NullVfx.GetInstance(), value);
	}

	public VfxWith<SkillProcessor.ProcessInfo> CreateBeforeAttackSelfAndOtherInfo(SkillBase attackSkill, BattleCardBase attacker, BattleCardBase target, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.AttackerCard = attacker;
		skillConditionCheckerOption.AttackTargetCard = target;
		SkillProcessor.ProcessInfo value = null;
		if (attackSkill.IsBeforeAttackSelfAndOtherSkill)
		{
			value = CreateProcessInfo(attackSkill, (SkillBase s) => attackSkill.OnBeforeAttackSelfAndOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption, isInductionSkill: false);
		}
		return new VfxWith<SkillProcessor.ProcessInfo>(NullVfx.GetInstance(), value);
	}

	public SkillProcessor.ProcessInfo CreateAfterAttackInfo(SkillBase skill, BattleCardBase attacker, BattleCardBase target, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.AttackerCard = attacker;
		skillConditionCheckerOption.AttackTargetCard = target;
		SkillProcessor.ProcessInfo result = null;
		if (skill.OnAfterAttackStart != 0)
		{
			result = CreateProcessInfo(skill, (SkillBase s) => s.OnAfterAttackStart, skillProcessor, playerInfoPair, skillConditionCheckerOption, isInductionSkill: false);
		}
		return result;
	}

	public VfxWith<SkillProcessor.ProcessInfo> CreateAfterAttackSelfAndOtherInfo(SkillBase attackSkill, BattleCardBase attacker, BattleCardBase target, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.AttackerCard = attacker;
		skillConditionCheckerOption.AttackTargetCard = target;
		SkillProcessor.ProcessInfo value = null;
		if (attackSkill.IsAfterAttackSelfAndOtherSkill)
		{
			value = CreateProcessInfo(attackSkill, (SkillBase s) => attackSkill.OnAfterAttackSelfAndOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption, isInductionSkill: false);
		}
		return new VfxWith<SkillProcessor.ProcessInfo>(NullVfx.GetInstance(), value);
	}

	public SkillProcessor.ProcessInfo CreateAfterFightInfo(SkillBase skill, BattleCardBase attacker, BattleCardBase target, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.AttackerCard = attacker;
		skillConditionCheckerOption.AttackTargetCard = target;
		SkillProcessor.ProcessInfo result = null;
		if ((skill.OnAfterFightStart != 0 && skill.SkillPrm.ownerCard == attacker && target.IsUnit) || skill.SkillPrm.ownerCard == target)
		{
			result = CreateProcessInfo(skill, (SkillBase s) => s.OnAfterFightStart, skillProcessor, playerInfoPair, skillConditionCheckerOption, isInductionSkill: false);
		}
		return result;
	}

	public SkillProcessor.ProcessInfo CreateNecromanceInfo(BattleCardBase necromanceCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, int necromanceCount)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.NecromanceCard = necromanceCard;
		skillConditionCheckerOption.NecromanceCount = necromanceCount;
		return CreateProcessInfo((SkillBase s) => s.OnWhenNecromance, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateTurnStartInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		Func<SkillBase, uint> getStartFunc = ((!playerInfoPair.ReadOnlySelf.IsSelfTurn) ? ((Func<SkillBase, uint>)((SkillBase s) => s.OnOpponentTurnStartStart)) : ((Func<SkillBase, uint>)((SkillBase s) => s.OnSelfTurnStartStart)));
		return CreateProcessInfo(getStartFunc, skillProcessor, playerInfoPair, checkerOption);
	}

	public VfxBase RegisterAndProcessWhenTurnStartImmediateInfo(BattlePlayerPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		SkillProcessor skillProcessor = new SkillProcessor();
		CreateAndRegisterResidentProcessInfo((SkillBase s) => s.OnWhenTurnStartStartImmediate, skillProcessor, playerInfoPair, checkerOption);
		return skillProcessor.Process(playerInfoPair, isImmediate: true);
	}

	public SkillProcessor.ProcessInfo CreateTurnEndInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		Func<SkillBase, uint> getStartFunc = ((!playerInfoPair.ReadOnlySelf.IsSelfTurn) ? ((Func<SkillBase, uint>)((SkillBase s) => s.OnOpponentTurnEndStart)) : ((Func<SkillBase, uint>)((SkillBase s) => s.OnSelfTurnEndStart)));
		return CreateProcessInfo(getStartFunc, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateDisCardInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		return CreateProcessInfo((SkillBase s) => s.OnDisCardStart, skillProcessor, playerInfoPair, new SkillConditionCheckerOption());
	}

	public SkillProcessor.ProcessInfo CreateDisCardOtherInfo(List<BattleCardBase> targetCards, bool isPlayer, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.Discards = BattlePlayerBase.ConvertToSkillInfoCollection(targetCards.Where((BattleCardBase c) => c != _ownerCard));
		return CreateProcessInfo((SkillBase s) => s.OnDisCardOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenUseEpSelfAndOtherInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenUseEpSelfAndOtherStart, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenEvolveInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option)
	{
		return CreateProcessInfo((SkillBase s) => s.OnWhenEvolveStart, skillProcessor, playerInfoPair, option);
	}

	public SkillProcessor.ProcessInfo CreateWhenEvolveOtherInfo(List<BattleCardBase> targetCards, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.EvolutionCards = BattlePlayerBase.ConvertToSkillInfoCollection(targetCards.Where((BattleCardBase c) => c != _ownerCard));
		return CreateProcessInfo((SkillBase s) => s.OnWhenEvolveOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenEvolveSelfAndOtherInfo(List<BattleCardBase> targetCards, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.EvolutionCards = BattlePlayerBase.ConvertToSkillInfoCollection(targetCards);
		return CreateProcessInfo((SkillBase s) => s.OnWhenEvolveSelfAndOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenFusionInfo(List<BattleCardBase> ingredientCards, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.FusionIngredientCards = BattlePlayerBase.ConvertToSkillInfoCollection(ingredientCards.Where((BattleCardBase c) => c != _ownerCard));
		return CreateProcessInfo((SkillBase s) => s.OnWhenFusion, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenFusionedInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenFusioned, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenFusionMetamorphoseInfo(List<BattleCardBase> ingredientCards, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.FusionIngredientCards = BattlePlayerBase.ConvertToSkillInfoCollection(ingredientCards.Where((BattleCardBase c) => c != _ownerCard));
		return CreateProcessInfo((SkillBase s) => s.OnWhenFusionMetamorphose, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public VfxWith<SkillProcessor.ProcessInfo> CreateWhenPlayOtherEnhanceAndAccelerateAndCrystallizeInfo(SkillBase skill, BattleCardBase playCard, BattleCardBase enhanceCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.PlayedCard = playCard;
		skillConditionCheckerOption.EnhanceCard = enhanceCard;
		skillConditionCheckerOption.AcceleratedCard = ((playCard.TransformInfo.Type == BattleCardBase.TransformType.Accelerate) ? playCard : null);
		skillConditionCheckerOption.CrystallizedCard = ((playCard.TransformInfo.Type == BattleCardBase.TransformType.Crystallize) ? playCard : null);
		SkillProcessor.ProcessInfo value = null;
		if (skill.OnWhenPlayOtherStart != 0)
		{
			value = CreateProcessInfo(skill, (SkillBase s) => skill.OnWhenPlayOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption, isInductionSkill: false);
		}
		else if (skill.OnWhenEnhanceStart != 0)
		{
			value = CreateProcessInfo(skill, (SkillBase s) => skill.OnWhenEnhanceStart, skillProcessor, playerInfoPair, skillConditionCheckerOption, isInductionSkill: false);
		}
		else if (skill.OnWhenAccelerateStart != 0 && playCard.TransformInfo.Type == BattleCardBase.TransformType.Accelerate)
		{
			value = CreateProcessInfo(skill, (SkillBase s) => skill.OnWhenAccelerateStart, skillProcessor, playerInfoPair, skillConditionCheckerOption, isInductionSkill: false);
		}
		else if (skill.OnWhenCrystallizeStart != 0 && playCard.TransformInfo.Type == BattleCardBase.TransformType.Crystallize)
		{
			value = CreateProcessInfo(skill, (SkillBase s) => skill.OnWhenCrystallizeStart, skillProcessor, playerInfoPair, skillConditionCheckerOption, isInductionSkill: false);
		}
		return new VfxWith<SkillProcessor.ProcessInfo>(NullVfx.GetInstance(), value);
	}

	public SkillProcessor.ProcessInfo CreateWhenSummonOtherInfo(BattleCardBase summonedCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, bool isReanimate)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.SummonedCard = summonedCard;
		if (isReanimate)
		{
			skillConditionCheckerOption.ReanimatedCards.Add(summonedCard);
		}
		return CreateProcessInfo((SkillBase s) => s.OnWhenSummonOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenSummonSelfAndOtherInfo(BattleCardBase summonedCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, bool isReanimate)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.SummonedCard = summonedCard;
		if (isReanimate)
		{
			skillConditionCheckerOption.ReanimatedCards.Add(summonedCard);
		}
		return CreateProcessInfo((SkillBase s) => s.OnWhenSummonSelfAndOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenSpellChargeInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, int addChargeCount)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.AddChargeCount = addChargeCount;
		bool flag = (_ownerCard.SpellChargeCount - addChargeCount) % 2 == 0;
		int num = addChargeCount / 2;
		int num2 = (addChargeCount + 1) / 2;
		skillConditionCheckerOption.AddOddChargeCount = (flag ? num : num2);
		skillConditionCheckerOption.AddEvenChargeCount = (flag ? num2 : num);
		return CreateProcessInfo((SkillBase s) => s.OnWhenSpellChargeStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenHealing(BattleCardBase healCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, int healAmount)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.HealingCardAndValue = new List<BattlePlayerBase.CardAndValue>
		{
			new BattlePlayerBase.CardAndValue(healCard, healAmount)
		};
		return CreateProcessInfo((SkillBase s) => s.OnWhenHealing, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenHealingSelfAndOtherInfo(List<BattleCardBase> targetCards, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, List<int> healAmountList)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.HealingCardAndValue = new List<BattlePlayerBase.CardAndValue>();
		for (int i = 0; i < targetCards.Count; i++)
		{
			skillConditionCheckerOption.HealingCardAndValue.Add(new BattlePlayerBase.CardAndValue(targetCards[i], healAmountList[i]));
		}
		return CreateProcessInfo((SkillBase s) => s.OnWhenHealingSelfAndOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenDamageInfo(SkillBase skill, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, int defDamage, int fixedDamage)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.DefaultDamage = new DamageInfo(skill, defDamage);
		skillConditionCheckerOption.FixedDamage = new DamageInfo(skill, fixedDamage);
		return CreateProcessInfo((SkillBase s) => s.OnWhenDamageStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenDamageSelfAndOtherInfo(SkillBase skill, List<BattleCardBase> targetCards, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, int defDamage, int fixedDamage)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.DamageCards = BattlePlayerBase.ConvertToSkillInfoCollection(targetCards);
		skillConditionCheckerOption.DefaultDamage = new DamageInfo(skill, defDamage);
		skillConditionCheckerOption.FixedDamage = new DamageInfo(skill, fixedDamage);
		return CreateProcessInfo((SkillBase s) => s.OnWhenDamageSelfAndOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenBuffInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, BattleCardBase target)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		if (target.IsInplay)
		{
			List<IReadOnlyBattleCardInfo> inplayBuffingCards = BattlePlayerBase.ConvertToSkillInfoCollection(new List<BattleCardBase> { target });
			skillConditionCheckerOption.InplayBuffingCards = inplayBuffingCards;
		}
		return CreateProcessInfo((SkillBase s) => s.OnWhenBuffStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenAttachAbilityInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillBase skill, List<IReadOnlyBattleCardInfo> targetCards)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.AttachingAbility = new AttachingAbilityInfo(skill, targetCards);
		return CreateProcessInfo((SkillBase s) => s.OnWhenAttachAbility, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenAddToDeck(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenAddToDeckStart, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenBurialRiteOther(BattleCardBase burialRiteCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.BurialRiteCards.Add(burialRiteCard);
		return CreateProcessInfo((SkillBase s) => s.OnWhenBurialRiteOther, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenBanishOther(BattleCardBase banishedCard, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, bool isInplay)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.BanishedCard = banishedCard;
		if (!isInplay)
		{
			return CreateProcessInfo((SkillBase s) => s.OnWhenBanishOther, skillProcessor, playerInfoPair, skillConditionCheckerOption);
		}
		skillConditionCheckerOption.LeftCards = new List<IReadOnlyBattleCardInfo> { banishedCard };
		return CreateProcessInfo((SkillBase s) => s.OnWhenBanishOther + s.OnWhenLeaveOther, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenUseWhiteRitualStackInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
	{
		return CreateProcessInfo((SkillBase s) => s.OnWhenUseWhiteRitualStack, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenResonanceStart(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenResonanceStart, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenDraw(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checker)
	{
		return CreateProcessInfo((SkillBase s) => s.OnWhenDraw, skillProcessor, playerInfoPair, checker);
	}

	public SkillProcessor.ProcessInfo CreateWhenDrawOther(List<BattleCardBase> deckDrawCards, List<BattleCardBase> tokenDrawCards, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, bool isSkillDraw)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.TokenDrewCards = BattlePlayerBase.ConvertToSkillInfoCollection(tokenDrawCards.Where((BattleCardBase c) => c != _ownerCard));
		skillConditionCheckerOption.DeckDrewCards = BattlePlayerBase.ConvertToSkillInfoCollection(deckDrawCards.Where((BattleCardBase c) => c != _ownerCard));
		if (isSkillDraw)
		{
			skillConditionCheckerOption.SkillDrewCards = skillConditionCheckerOption.TokenDrewCards.ToList();
			skillConditionCheckerOption.SkillDrewCards.AddRange(skillConditionCheckerOption.DeckDrewCards);
		}
		return CreateProcessInfo((SkillBase s) => s.OnWhenDrawOtherStart, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenPpHealingInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenPpHealStart, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenChantCountChangeInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenChantCountChangeStart, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenChantCountGainInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenChantCountGain, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenChantCountGainSelfAndOtherInfo(List<BattleCardBase> targetCards, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption skillConditionCheckerOption = new SkillConditionCheckerOption();
		skillConditionCheckerOption.ChantCountChangeCard = BattlePlayerBase.ConvertToSkillInfoCollection(targetCards);
		return CreateProcessInfo((SkillBase s) => s.OnWhenChantCountGainSelfAndOther, skillProcessor, playerInfoPair, skillConditionCheckerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenGetOnInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenGetOnStart, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenGetOffInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenGetOff, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenBuffDebuffSelfAndOtherInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
	{
		return CreateProcessInfo((SkillBase s) => s.OnWhenBuffSelfAndOther + s.OnWhenDebuffSelfAndOther, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenBuffSelfAndOtherInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
	{
		return CreateProcessInfo((SkillBase s) => s.OnWhenBuffSelfAndOther, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenDebuffSelfAndOtherInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
	{
		return CreateProcessInfo((SkillBase s) => s.OnWhenDebuffSelfAndOther, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenDebuffIncludeSetMaxLifeInfo(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
	{
		return CreateProcessInfo((SkillBase s) => s.OnWhenDebuffIncludeSetMaxLife, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenShortageDeck(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenShortageDeck, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenShortageDeckWinSkillActivate(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
	{
		return CreateProcessInfo((SkillBase s) => s.OnWhenShortageDeckWinSkillActivate, skillProcessor, playerInfoPair, checkerOption);
	}

	public SkillProcessor.ProcessInfo CreateWhenSpecialLose(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair)
	{
		SkillConditionCheckerOption checkerOption = new SkillConditionCheckerOption();
		return CreateProcessInfo((SkillBase s) => s.OnWhenSpecialLose, skillProcessor, playerInfoPair, checkerOption);
	}

	protected SkillProcessor.ProcessInfo CreateProcessInfo(Func<SkillBase, uint> getStartFunc, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption, bool isCheckPreviousSkill = false)
	{
		List<SkillBase> activeSkills = GetActiveSkills(getStartFunc, playerInfoPair, checkerOption, skillProcessor, isCheckPreviousSkill);
		if (activeSkills.Count() <= 0)
		{
			return null;
		}
		return new SkillProcessor.ProcessInfoCollection(_ownerCard, this, skillProcessor, playerInfoPair, checkerOption, activeSkills);
	}

	protected void CreateAndRegisterResidentProcessInfo(Func<SkillBase, uint> getStartFunc, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
	{
		List<SkillBase> skills = _skillList.Where((SkillBase s) => getStartFunc(s) != 0).ToList();
		int i;
		for (i = 0; i < skills.Count; i++)
		{
			if (skills[i].IsAllResidentTiming)
			{
				if (skills[i].IsResidentSkillStartFlag)
				{
					if (skills[i].OnWhenAddToHand == 1 && skills[i].ApplyingTargetFilter is SkillTargetInHandCardFilter)
					{
						if (skills[i].CheckCondition(playerInfoPair, checkerOption, isPrePlay: false))
						{
							skillProcessor.Register(skills[i].CreateProcessInfo(skills[i].SkillPrm.ownerCard.IsPlayer, skillProcessor, playerInfoPair, checkerOption, isInductionSkill: false));
						}
					}
					else if (!skills[i].CheckCondition(playerInfoPair, checkerOption, isPrePlay: false))
					{
						skillProcessor.Register(skills[i].CreateStopProcessInfoResidentSkill(skills[i].SkillPrm.ownerCard.IsPlayer, skillProcessor, playerInfoPair, checkerOption));
						skills[i].SetIsResidentSkillStartFlag(flg: false);
					}
				}
				else if (skills[i].CheckCondition(playerInfoPair, checkerOption, isPrePlay: false))
				{
					skillProcessor.Register(skills[i].CreateProcessInfo(skills[i].SkillPrm.ownerCard.IsPlayer, skillProcessor, playerInfoPair, checkerOption, isInductionSkill: false));
					skills[i].SetIsResidentSkillStartFlag(flg: true);
				}
			}
			else if (skills[i].CheckCondition(playerInfoPair, checkerOption, isPrePlay: false))
			{
				skillProcessor.Register(CreateProcessInfo(skills[i], (SkillBase s) => skills[i].OnWhenSummonStart, skillProcessor, playerInfoPair, checkerOption, isInductionSkill: false));
			}
		}
	}

	protected SkillProcessor.ProcessInfo CreateProcessInfo(SkillBase skill, Func<SkillBase, uint> getStartTiming, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption, bool isInductionSkill)
	{
		List<SkillBase> activeSkills = GetActiveSkills(skill, getStartTiming, playerInfoPair, checkerOption, skillProcessor);
		if (activeSkills.Count() <= 0)
		{
			return null;
		}
		return new SkillProcessor.ProcessInfoCollection(_ownerCard, this, skillProcessor, playerInfoPair, checkerOption, activeSkills);
	}

	private bool IsActiveSkill(SkillBase skill, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
	{
		if (skill.IsUserSelectType)
		{
			if (skill.ApplyExclutionFilterList.Any((ISkillExclutionFilter f) => f is SkillExclutionApplyFilter && (f as SkillExclutionApplyFilter).IsExcludeSelectCard))
			{
				checkerOption = checkerOption.ShallowCopy();
				checkerOption.SelectedCards.Clear();
			}
			if (!skill._executionInfoCreator.IsSkipTargetAiSelect() && !skill.GetSelectableCards(playerInfoPair, checkerOption).Any())
			{
				return false;
			}
		}
		return skill.CheckCondition(playerInfoPair, checkerOption, isPrePlay: false);
	}

	private bool IsContainPreviousSkill(SkillBase checkSkill, List<SkillBase> activeSkills, List<SkillBase> checkTimingSkills)
	{
		int num = checkTimingSkills.IndexOf(checkSkill);
		bool flag = checkSkill.ApplyingTargetFilter is SkillTargetChosenCardsFilter;
		while (num > 0)
		{
			SkillBase skillBase = checkTimingSkills.ElementAt(num - 1);
			if (skillBase.IsReferencePreviousSkill)
			{
				num--;
				continue;
			}
			if (flag && !skillBase.IsChoiceType)
			{
				num--;
				continue;
			}
			return activeSkills.Contains(skillBase);
		}
		return true;
	}

	protected List<SkillBase> GetActiveSkills(Func<SkillBase, uint> getStartTiming, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor, bool isCheckPreviousSkillTiming = false, bool isCheckCondition = true)
	{
		List<SkillBase> list = _skillList.Where((SkillBase s) => getStartTiming(s) != 0).ToList();
		foreach (SkillBase item in list)
		{
			item.SetScanCondition(playerInfoPair, checkerOption, isPrePlay: false);
		}
		IEnumerable<SkillBase> source = list.Where((SkillBase s) => IsActiveSkill(s, playerInfoPair, checkerOption));
		List<SkillBase> activeSkills = (isCheckCondition ? source.ToList() : list);
		if (isCheckPreviousSkillTiming)
		{
			foreach (SkillBase item2 in list)
			{
				if (item2.IsReferencePreviousSkill && !IsContainPreviousSkill(item2, activeSkills, list))
				{
					activeSkills.Remove(item2);
				}
			}
		}
		skillProcessor.AddInactiveSkilList(list.Where((SkillBase s) => !activeSkills.Contains(s)).ToList());
		return activeSkills;
	}

	protected List<SkillBase> GetActiveSkills(SkillBase skill, Func<SkillBase, uint> getStartTiming, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption, SkillProcessor skillProcessor, bool isCheckPreviousSkill = false, bool isCheckCondition = true)
	{
		IEnumerable<SkillBase> enumerable = from s in _skillList
			where s == skill
			where getStartTiming(s) != 0
			select s;
		skill.SetScanCondition(playerInfoPair, checkerOption, isPrePlay: false);
		if (enumerable.Any((SkillBase s) => s.IsStopTiming()))
		{
			return enumerable.ToList();
		}
		IEnumerable<SkillBase> enumerable2 = enumerable.Where((SkillBase s) => IsActiveSkill(s, playerInfoPair, checkerOption));
		IEnumerable<SkillBase> activeSkills = (isCheckCondition ? enumerable2 : enumerable);
		skillProcessor.AddInactiveSkilList(enumerable.Where((SkillBase s) => !activeSkills.Contains(s)).ToList());
		return activeSkills.ToList();
	}

	private void DistinctDeckSkillCards(BattlePlayerReadOnlyInfoPair playerInfoPair, List<SkillBase> activeSkills, SkillProcessor skillProcessor, SkillConditionCheckerOption checkerOption)
	{
		if (!activeSkills.Any((SkillBase c) => c.ConditionTargetFilter is SkillTargetDeckSelfFilter))
		{
			return;
		}
		List<SkillBase> deckSkills = activeSkills.Where((SkillBase c) => c.ConditionTargetFilter is SkillTargetDeckSelfFilter).ToList();
		bool flag = _ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsUseUnapprovedList(playerInfoPair.ReadOnlySelf.IsPlayer);
		if (deckSkills.Any((SkillBase s) => s is Skill_summon_card || s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessTimesPerTurn && (p as SkillPreprocessTimesPerTurn).IsSameBaseCardId)))
		{
			DistinctDeckActiveSkillCards(flag, activeSkills, deckSkills, skillProcessor, playerInfoPair, checkerOption);
		}
		else if (flag)
		{
			List<CardDataModel> receiveCardList = (_ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase).networkBattleData.GetReceiveData().GetReceiveCardList();
			if (receiveCardList != null && !receiveCardList.Where((CardDataModel c) => c.fromState == NetworkBattleDefine.NetworkCardPlaceState.Deck).Any((CardDataModel i) => i.Index == _ownerCard.Index))
			{
				activeSkills.RemoveAll((SkillBase s) => deckSkills.Contains(s));
			}
		}
		else if (!_ownerCard.SelfBattlePlayer.OkSkillInProcess.Any((SkillBase s) => s.SkillPrm.ownerCard.BaseParameter.BaseCardId == _ownerCard.BaseParameter.BaseCardId))
		{
			_ownerCard.SelfBattlePlayer.OkSkillInProcess.AddRange(deckSkills);
		}
	}

	private void DistinctDeckActiveSkillCards(bool isUseUnapprovedList, List<SkillBase> activeSkills, List<SkillBase> deckSkills, SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair pair, SkillConditionCheckerOption checkerOption)
	{
		if (isUseUnapprovedList)
		{
			NetworkBattleManagerBase battleMgr = _ownerCard.SelfBattlePlayer.BattleMgr as NetworkBattleManagerBase;
			NetworkBattleReceiver.ReceiveData receiveData = battleMgr.networkBattleData.GetReceiveData();
			bool isCheckHand = deckSkills.Any((SkillBase s) => s is Skill_draw);
			List<int> isOpenIndexList = (from c in receiveData.GetReceiveCardList()
				where c.IsOpen
				select c.Index).ToList();
			List<CardDataModel> list = (from c in receiveData.unapprovedList
				where (c.fromState == NetworkBattleDefine.NetworkCardPlaceState.Deck || (c.fromState == NetworkBattleDefine.NetworkCardPlaceState.None && (_ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsWatchBattle || battleMgr.IsRecovery))) && c.ToStateList.Count > 0 && (c.ToStateList[0] == NetworkBattleDefine.NetworkCardPlaceState.Field || (isCheckHand && c.ToStateList[0] == NetworkBattleDefine.NetworkCardPlaceState.Hand && isOpenIndexList.Contains(c.Index)))
				where c.Index == c.skillCardIndex
				select c).ToList();
			CardDataModel cardDataModel = list.FirstOrDefault((CardDataModel i) => i.Index == _ownerCard.Index);
			if (cardDataModel == null)
			{
				activeSkills.RemoveAll((SkillBase s) => deckSkills.Contains(s));
				return;
			}
			List<BattleCardBase> list2 = list.Select((CardDataModel c) => battleMgr.GetBattleCardIdx(_ownerCard.IsPlayer ? battleMgr.BattlePlayer.AllCardsWithCemeteryAndBanish : battleMgr.BattleEnemy.AllCardsWithCemeteryAndBanish, c.Index)).ToList();
			int num = list.IndexOf(cardDataModel);
			BattleCardBase deckActiveSkillCard = list2[num];
			List<BattleCardBase> list3 = (from c in list2.GetRange(0, num)
				where c.BaseParameter.BaseCardId == deckActiveSkillCard.BaseParameter.BaseCardId
				select c).ToList();
			List<SkillBase> source = _ownerCard.SelfBattlePlayer.OkSkillInProcess.Where((SkillBase s) => s.SkillPrm.ownerCard.BaseParameter.BaseCardId == _ownerCard.BaseParameter.BaseCardId).ToList();
			List<BattleCardBase> list4 = source.Select((SkillBase s) => s.SkillPrm.ownerCard).ToList();
			for (int num2 = 0; num2 < list3.Count; num2++)
			{
				if (!list4.Contains(list3[num2]))
				{
					activeSkills.RemoveAll((SkillBase s) => deckSkills.Contains(s));
					return;
				}
			}
			if (list4.Contains(deckActiveSkillCard))
			{
				activeSkills.RemoveAll((SkillBase s) => deckSkills.Contains(s));
				return;
			}
			for (int num3 = 0; num3 < deckSkills.Count; num3++)
			{
				int deckSkillIndex = deckSkills[num3].SkillPrm.ownerCard.Skills.IndexOf(deckSkills[num3]);
				if (source.Any((SkillBase s) => s.SkillPrm.ownerCard.Skills.IndexOf(s) == deckSkillIndex))
				{
					activeSkills.RemoveAll((SkillBase s) => deckSkills.Contains(s));
					return;
				}
			}
			_ownerCard.SelfBattlePlayer.OkSkillInProcess.AddRange(deckSkills);
			return;
		}
		for (int num4 = 0; num4 < deckSkills.Count; num4++)
		{
			SkillBase deckSkill = deckSkills[num4];
			if (!(deckSkill is Skill_summon_card) && !deckSkill.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessTimesPerTurn && (p as SkillPreprocessTimesPerTurn).IsSameBaseCardId))
			{
				continue;
			}
			int deckSkillIndex2 = deckSkill.SkillPrm.ownerCard.Skills.IndexOf(deckSkill);
			List<SkillBase> source2 = _ownerCard.SelfBattlePlayer.OkSkillInProcess.Where((SkillBase s) => s.SkillPrm.ownerCard.BaseParameter.BaseCardId == _ownerCard.BaseParameter.BaseCardId).ToList();
			bool hasSingleDeckSkill = deckSkill.SkillPrm.ownerCard.Skills.Where((SkillBase c) => c.ConditionTargetFilter is SkillTargetDeckSelfFilter).Count() == 1;
			if (!source2.Any((SkillBase s) => s.SkillPrm.ownerCard.Skills.IndexOf(s) == deckSkillIndex2 || s.SkillPrm.ownerCard == deckSkill.SkillPrm.ownerCard || (hasSingleDeckSkill && s.SkillPrm.ownerCard.BaseParameter.BaseCardId == deckSkill.SkillPrm.ownerCard.BaseParameter.BaseCardId)))
			{
				List<BattleCardBase> list5 = new List<BattleCardBase>();
				List<SkillBase> list6 = new List<SkillBase>(deckSkills);
				list6.AddRange(skillProcessor.GetDeckSkils());
				list5.AddRange(from s in list6
					where s.PreprocessList.All((SkillPreprocessBase p) => p.IsRight(pair, checkerOption, PreexecutionCheck: true))
					select s.SkillPrm.ownerCard into c
					where c.BaseParameter.BaseCardId == _ownerCard.BaseParameter.BaseCardId
					select c);
				List<BattleCardBase> list7 = _ownerCard.SelfBattlePlayer.DeckCardList.Where((BattleCardBase c) => c.BaseParameter.BaseCardId == _ownerCard.BaseParameter.BaseCardId).ToList();
				List<BattleCardBase> list8 = new List<BattleCardBase>();
				for (int num5 = 0; num5 < list7.Count; num5++)
				{
					if (list5.Contains(list7[num5]))
					{
						list8.Add(list7[num5]);
					}
				}
				if (list8.Count > 0)
				{
					int index = _ownerCard.SelfBattlePlayer.BattleMgr.StableRandomOnlySelf(list8.Count);
					BattleCardBase battleCardBase = list8[index];
					if (battleCardBase != deckSkill.SkillPrm.ownerCard)
					{
						if (hasSingleDeckSkill)
						{
							SkillBase item = battleCardBase.Skills.Where((SkillBase c) => c.ConditionTargetFilter is SkillTargetDeckSelfFilter).First();
							activeSkills.Add(item);
							_ownerCard.SelfBattlePlayer.OkSkillInProcess.Add(item);
						}
						else
						{
							for (int num6 = 0; num6 < deckSkills.Count; num6++)
							{
								activeSkills.Add(battleCardBase.Skills.ElementAt(deckSkill.SkillPrm.ownerCard.Skills.IndexOf(deckSkills[num6])));
							}
							_ownerCard.SelfBattlePlayer.OkSkillInProcess.Add(battleCardBase.Skills.ElementAt(deckSkillIndex2));
						}
					}
					else
					{
						_ownerCard.SelfBattlePlayer.OkSkillInProcess.Add(deckSkill);
					}
				}
			}
			if (!_ownerCard.SelfBattlePlayer.OkSkillInProcess.Any((SkillBase s) => s == deckSkill))
			{
				activeSkills.RemoveAll((SkillBase s) => deckSkills.Contains(s));
			}
		}
	}

	public VfxBase CallStart(SkillProcessor skillProcessor, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption, List<SkillBase> activeSkills)
	{
		DistinctDeckSkillCards(playerInfoPair, activeSkills, skillProcessor, checkerOption);
		List<SkillBase> list = IfNeededRepeatSkills(activeSkills, skillProcessor);
		SkillConditionCheckerOption skillConditionCheckerOption = null;
		int num = activeSkills.Count;
		int num2 = 0;
		bool flag = false;
		SkillConditionCheckerOption skillConditionCheckerOption2 = null;
		checkerOption.IsRefPrev = false;
		if (list != null)
		{
			skillConditionCheckerOption = checkerOption.ShallowCopy();
			activeSkills.AddRange(list);
		}
		else if (activeSkills.Any((SkillBase s) => s is Skill_invoke_skill))
		{
			skillConditionCheckerOption = checkerOption.ShallowCopy();
		}
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		SkillBase.CallParameter callParameter = new SkillBase.CallParameter();
		callParameter.skillProcessor = skillProcessor;
		callParameter.calledSkillResultInfo = new SkillBase.SkillResultInfo();
		List<int> list2 = new List<int>();
		List<ActiveSkillWhenPlayEffectInfo> list3 = new List<ActiveSkillWhenPlayEffectInfo>();
		if (!_ownerCard.SelfBattlePlayer.BattleMgr.IsBattleEnd)
		{
			for (int num3 = 0; num3 < activeSkills.Count; num3++)
			{
				AddActiveSkillWhenPlayEffectInfo(list3, activeSkills[num3], num3, playerInfoPair, checkerOption);
			}
		}
		int i;
		for (i = 0; i < activeSkills.Count; i++)
		{
			SkillBase skillBase = activeSkills[i];
			num2++;
			if (num2 > num)
			{
				skillConditionCheckerOption2 = skillConditionCheckerOption.ShallowCopy();
				_ownerCard.SelfBattlePlayer.ReturnList.Clear();
				_ownerCard.OpponentBattlePlayer.ReturnList.Clear();
				callParameter.calledSkillResultInfo.drawCards.Clear();
				callParameter.calledSkillResultInfo.drewOverHandLimitCards.Clear();
				callParameter.calledSkillResultInfo.UpdatedDeckCards.Clear();
				flag = true;
				num2 -= num;
			}
			ActiveSkillWhenPlayEffectInfo activeSkillWhenPlayEffectInfo = list3.Find((ActiveSkillWhenPlayEffectInfo e) => e.Index == i);
			if (activeSkillWhenPlayEffectInfo != null && !_ownerCard.SelfBattlePlayer.BattleMgr.IsBattleEnd)
			{
				if (!_ownerCard.SelfBattlePlayer.BattleMgr.IsRecovery)
				{
					switch (activeSkillWhenPlayEffectInfo.Type)
					{
					case WhenPlayEffectType.Berserk:
						sequentialVfxPlayer.Register(NullVfx.GetInstance());
						break;
					case WhenPlayEffectType.Awake:
						sequentialVfxPlayer.Register(NullVfx.GetInstance());
						break;
					case WhenPlayEffectType.WhenPlay:
						sequentialVfxPlayer.Register(NullVfx.GetInstance());
						break;
					case WhenPlayEffectType.WhenDestroy:
						sequentialVfxPlayer.Register(VfxWithLoadingSequential.Create());
						break;
					}
				}
				_ownerCard.SelfBattlePlayer.CallOnWhenPlayEffect(activeSkillWhenPlayEffectInfo.Type, activeSkillWhenPlayEffectInfo.OwnerCard, skillBase.IsInvoked);
			}
			if (flag && num2 == 1 && !_ownerCard.SelfBattlePlayer.BattleMgr.IsBattleEnd)
			{
				sequentialVfxPlayer.Register(new RepeatSkillEffectVfx(skillBase.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr, skillBase.SkillPrm.ownerCard.SelfBattlePlayer.Class.BattleCardView, "when_destroy", skillBase.SkillPrm.ownerCard.IsPlayer));
			}
			sequentialVfxPlayer.Register(skillBase.CallStart(skillProcessor, playerInfoPair, flag ? skillConditionCheckerOption2 : checkerOption, callParameter, SkillProcessor.ProcessCallType.Start, list2.Contains(i)));
			if (skillBase is Skill_invoke_skill skill_invoke_skill)
			{
				int num4 = i + 1;
				List<ActiveSkillWhenPlayEffectInfo> list4 = new List<ActiveSkillWhenPlayEffectInfo>();
				foreach (SkillBase insertSkill in skill_invoke_skill.InsertSkillList)
				{
					activeSkills.Insert(num4, insertSkill);
					AddActiveSkillWhenPlayEffectInfo(list4, insertSkill, num4, playerInfoPair, checkerOption);
					if (insertSkill.IsWhenDestroySkill && list4.Find((ActiveSkillWhenPlayEffectInfo e) => e.OwnerCard == insertSkill.SkillPrm.ownerCard) == null)
					{
						list4.Add(new ActiveSkillWhenPlayEffectInfo(insertSkill.SkillPrm.ownerCard, num4, WhenPlayEffectType.WhenDestroy));
					}
					num4++;
					num++;
				}
				if (skill_invoke_skill.InsertSkillList.Count == 0)
				{
					SkillBase skillBase2 = skill_invoke_skill.NotInsertSkillList.FirstOrDefault((SkillBase s) => s.IsWhenDestroySkill);
					if (skillBase2 != null)
					{
						sequentialVfxPlayer.Register(VfxWithLoadingSequential.Create());
					}
				}
				list = IfNeededRepeatSkills(skill_invoke_skill.InsertSkillList, skillProcessor);
				if (list != null)
				{
					activeSkills.AddRange(list);
				}
				else if (skill_invoke_skill.SkillPrm.selfBattlePlayer.Class.SkillApplyInformation.RepeatSkillTimingList.Count() > 0)
				{
					list = IfNeededRepeatSkills(skill_invoke_skill.NotInsertSkillList, skillProcessor);
					if (list != null && !_ownerCard.SelfBattlePlayer.BattleMgr.IsBattleEnd)
					{
						sequentialVfxPlayer.Register(new RepeatSkillEffectVfx(skillBase.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr, skillBase.SkillPrm.ownerCard.SelfBattlePlayer.Class.BattleCardView, "when_destroy", skillBase.SkillPrm.ownerCard.IsPlayer));
					}
				}
				list3.AddRange(list4);
				skill_invoke_skill.InsertSkillList.Clear();
				skill_invoke_skill.NotInsertSkillList.Clear();
			}
			if (skillBase is Skill_loop_skill skill_loop_skill)
			{
				int num5 = i + 1;
				for (int num6 = 0; num6 < skill_loop_skill.LoopSkillList.Count; num6++)
				{
					activeSkills.Insert(num5, skill_loop_skill.LoopSkillList[num6]);
					list2.Add(num5);
					num5++;
					num++;
				}
				skill_loop_skill.LoopSkillList.Clear();
			}
			checkerOption.NextTargetCards.Clear();
		}
		_ownerCard.SelfBattlePlayer.ReturnList.Clear();
		_ownerCard.OpponentBattlePlayer.ReturnList.Clear();
		_ownerCard.ResetSkillActivateCountBySimultaneousDestroyedCardList();
		_ownerCard.ResetSkillActivateCountBySimultaneousBuffingCards();
		_ownerCard.ResetSkillActivateCountBySimultaneousSummonedCard();
		HasInductionSkillBeenActivated = false;
		return sequentialVfxPlayer;
	}

	private void AddActiveSkillWhenPlayEffectInfo(List<ActiveSkillWhenPlayEffectInfo> effectList, SkillBase skill, int index, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
	{
		ActiveSkillWhenPlayEffectInfo activeSkillWhenPlayEffectInfo = effectList.Find((ActiveSkillWhenPlayEffectInfo e) => e.OwnerCard == skill.SkillPrm.ownerCard);
		if (activeSkillWhenPlayEffectInfo == null)
		{
			WhenPlayEffectType activeSkillWhenPlayEffectType = GetActiveSkillWhenPlayEffectType(skill, playerInfoPair, checkerOption);
			if (activeSkillWhenPlayEffectType != WhenPlayEffectType.None)
			{
				effectList.Add(new ActiveSkillWhenPlayEffectInfo(skill.SkillPrm.ownerCard, index, activeSkillWhenPlayEffectType));
			}
		}
		else if (activeSkillWhenPlayEffectInfo.Type == WhenPlayEffectType.WhenPlay)
		{
			WhenPlayEffectType activeSkillWhenPlayEffectType2 = GetActiveSkillWhenPlayEffectType(skill, playerInfoPair, checkerOption);
			if (activeSkillWhenPlayEffectType2 == WhenPlayEffectType.Berserk || activeSkillWhenPlayEffectType2 == WhenPlayEffectType.Awake)
			{
				activeSkillWhenPlayEffectInfo.Type = activeSkillWhenPlayEffectType2;
			}
		}
	}

	protected List<SkillBase> IfNeededRepeatSkills(List<SkillBase> skills, SkillProcessor skillProcessor)
	{
		if (skills.Count <= 0 || skills[0].SkillPrm.ownerCard.IsSpell || skills.Any((SkillBase s) => s.IsBurialRite))
		{
			return null;
		}
		BattleCardBase ownerCard = skills.First().SkillPrm.ownerCard;
		BattlePlayerBase selfBattlePlayer = ownerCard.SelfBattlePlayer;
		BattleUtility.RepeatSkillsAndTiming repeatSkillsWithTiming = BattleUtility.GetRepeatSkillsWithTiming(skills);
		if (repeatSkillsWithTiming != null)
		{
			List<string> timings = repeatSkillsWithTiming._timings;
			for (int num = 0; num < timings.Count; num++)
			{
				AllStopRepeatSkill(ownerCard, selfBattlePlayer, timings[num], reservation: false, skillProcessor, isProcess: true);
			}
			return repeatSkillsWithTiming._skills;
		}
		return null;
	}

	private void AllStopRepeatSkill(BattleCardBase ownerCard, BattlePlayerBase player, string timing, bool reservation, SkillProcessor skillProcessor, bool isProcess = false)
	{
		player.Class.SkillApplyInformation.DepriveRepeatSkill(timing, ownerCard.IsUnit ? "unit" : "field", reservation, isProcess, skillProcessor);
	}

	private WhenPlayEffectType GetActiveSkillWhenPlayEffectType(SkillBase skillBase, BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption)
	{
		BattleCardBase ownerCard = skillBase.SkillPrm.ownerCard;
		if (ownerCard.IsSpell || (!ownerCard.IsInplay && !skillBase.IsWhenDestroySkill))
		{
			return WhenPlayEffectType.None;
		}
		bool num = ownerCard.GetSelectTypeSkill().ToList().Any((SkillBase s) => s.IsChoiceType);
		bool flag = ownerCard.Skills.Any((SkillBase s) => s is Skill_transform);
		bool flag2 = skillBase.PreprocessList.All((SkillPreprocessBase p) => p.IsRight(playerInfoPair, checkerOption, PreexecutionCheck: true));
		if (!(num && flag) && skillBase.OnWhenChoicePlayStart == 0 && flag2)
		{
			foreach (ISkillConditionChecker conditionChecker in skillBase.ConditionCheckerList)
			{
				if (conditionChecker.GetType() == typeof(SkillConditionHalfLife) && ((SkillConditionHalfLife)conditionChecker).IsConditionLesserHalfLife)
				{
					return WhenPlayEffectType.Berserk;
				}
				if (conditionChecker.GetType() == typeof(SkillConditionAwake) && ((SkillConditionAwake)conditionChecker).judgeFlg)
				{
					return WhenPlayEffectType.Awake;
				}
			}
			List<SkillAnyConditionFilter> anyConditionFilter = skillBase.ConditionFilterCollection.AnyConditionFilter;
			for (int num2 = 0; num2 < anyConditionFilter.Count; num2++)
			{
				List<ConditionSkillFilterCollection> filters = anyConditionFilter[num2].Filters;
				for (int num3 = 0; num3 < filters.Count; num3++)
				{
					if (filters[num3].ConditionCheckerFilterList.Any((ISkillConditionChecker f) => f.GetType() == typeof(SkillConditionHalfLife) && ((SkillConditionHalfLife)f).IsRight(playerInfoPair, checkerOption)))
					{
						return WhenPlayEffectType.Berserk;
					}
					if (filters[num3].ConditionCheckerFilterList.Any((ISkillConditionChecker f) => f.GetType() == typeof(SkillConditionAwake) && ((SkillConditionAwake)f).IsRight(playerInfoPair, checkerOption)))
					{
						return WhenPlayEffectType.Awake;
					}
				}
			}
		}
		if (skillBase.IsWhenPlaySkill)
		{
			return WhenPlayEffectType.WhenPlay;
		}
		return WhenPlayEffectType.None;
	}

	public static void SetupOptionValue(SkillOptionValue optionValue, BattlePlayerReadOnlyInfoPair playerInfoPair, BattleCardBase ownerCard, SkillBase skill, SkillConditionCheckerOption checkerOption = null, bool isPrePlay = false)
	{
		int num = playerInfoPair.ReadOnlySelf.SkillInfoHandCards.Count();
		int num2 = playerInfoPair.ReadOnlySelf.SkillInfoInPlayCards.Count();
		int num3 = playerInfoPair.ReadOnlyOpponent.SkillInfoInPlayCards.Count();
		int num4 = playerInfoPair.ReadOnlySelf.SkillInfoInPlayCards.Count(SkillUnitFilter.IsUnit);
		int num5 = playerInfoPair.ReadOnlyOpponent.SkillInfoInPlayCards.Count(SkillUnitFilter.IsUnit);
		optionValue.SetVariable("PLAY_COUNT", playerInfoPair.ReadOnlySelf.GetCurrentTurnPlayCount().ToString());
		optionValue.SetVariable("HAND_COUNT", num.ToString());
		optionValue.SetVariable("HAND_SPACE_COUNT", (9 - num).ToString());
		optionValue.SetVariable("CHANT_COUNT", ownerCard.ChantCount.ToString());
		optionValue.SetVariable("CHARGE_COUNT", ownerCard.SpellChargeCount.ToString());
		optionValue.SetVariable("INPLAY_ME_COUNT", num2.ToString());
		optionValue.SetVariable("INPLAY_OP_COUNT", num3.ToString());
		optionValue.SetVariable("INPLAY_UNIT_ME_COUNT", num4.ToString());
		optionValue.SetVariable("INPLAY_UNIT_OP_COUNT", num5.ToString());
		optionValue.SetVariable("CLASS_ME_LIFE", playerInfoPair.ReadOnlySelf.SkillInfoClass.Life.ToString());
		optionValue.SetVariable("CLASS_OP_LIFE", playerInfoPair.ReadOnlyOpponent.SkillInfoClass.Life.ToString());
		if (checkerOption != null)
		{
			optionValue.SetVariable("ADD_CHARGE_COUNT", checkerOption.AddChargeCount.ToString());
			optionValue.SetVariable("ADD_ODD_CHARGE_COUNT", checkerOption.AddOddChargeCount.ToString());
			optionValue.SetVariable("ADD_EVEN_CHARGE_COUNT", checkerOption.AddEvenChargeCount.ToString());
		}
		optionValue.SetupFilterVariable(playerInfoPair, ownerCard, isPrePlay, skill, checkerOption);
	}

	public virtual bool CheckWhenPlayCondition(BattlePlayerReadOnlyInfoPair playerInfoPair, bool isPrePlay)
	{
		return _skillList.Where((SkillBase s) => s.IsWhenPlaySkill).Any((SkillBase s) => s.CheckCondition(playerInfoPair, new SkillConditionCheckerOption(), isPrePlay));
	}

	public void Complete()
	{
		InitTimingInfo();
		for (int i = 0; i < _skillList.Count; i++)
		{
			SkillBase skill = _skillList[i];
			skill.CreateInductionSkillActivationVfxFunc = delegate(Func<VfxBase> CreateInductionSkillVoiceVfx)
			{
				if (!HasInductionSkillBeenActivated)
				{
					AttachedSkillInformation attachedSkillsInfo = _ownerCard.SkillApplyInformation.AttachedSkillsInfo;
					if (!attachedSkillsInfo.AttachedSkills.Any((SkillBase s) => s == skill))
					{
						HasInductionSkillBeenActivated = true;
						skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnSkillInductionEffect(skill);
						return CreateInductionSkillVoiceVfx.GetAllFuncVfxResults();
					}
					if (attachedSkillsInfo.AttachedSkills.Any((SkillBase s) => s == skill && attachedSkillsInfo.OwnerCardIdList.Count > 0 && attachedSkillsInfo.OwnerCardIdList[(attachedSkillsInfo.AttachedSkills.IndexOf(s) != -1) ? attachedSkillsInfo.AttachedSkills.IndexOf(s) : 0] == s.SkillPrm.ownerCard.CardId))
					{
						HasInductionSkillBeenActivated = true;
						skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnSkillInductionEffect(skill);
						return CreateInductionSkillVoiceVfx.GetAllFuncVfxResults();
					}
				}
				skill.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.OperateMgr.CallOnSkillInductionEffect(skill, isIgnoreVoice: true);
				return NullVfx.GetInstance();
			};
		}
	}

	public void InitTimingInfo()
	{
		_skillTimingInfo = new SkillTimingInfo(_skillList);
	}
}
