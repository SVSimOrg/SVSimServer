using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class SkillPreprocessBurialRite : SkillPreprocessBase
{
	private readonly BattleCardBase _ownerCard;

	private readonly SkillBase _skill;

	private bool _isInvoked;

	public SkillPreprocessBurialRite(SkillBase skill, BattleCardBase ownerCard)
	{
		_skill = skill;
		_ownerCard = ownerCard;
	}

	public override bool IsRight(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (_isInvoked)
		{
			return true;
		}
		IBattlePlayerReadOnlyInfo readOnlySelf = playerInfoPair.ReadOnlySelf;
		int num = readOnlySelf.SkillInfoInPlayCards.Count();
		BattleCardBase originalCard = ((_ownerCard.TransformInfo.Type != BattleCardBase.TransformType.Metamorphose) ? _ownerCard.TransformInfo.OriginalCard : null);
		int num2 = readOnlySelf.SkillInfoHandCards.Count((IReadOnlyBattleCardInfo s) => s != _ownerCard && s != originalCard && s.IsUnit);
		int burialRiteCount = _ownerCard.GetBurialRiteCount(playerInfoPair, option, isPrePlay: false);
		int num3 = 5 - burialRiteCount;
		if (num <= num3)
		{
			return num2 >= burialRiteCount;
		}
		return false;
	}

	public override bool IsRightPrePlay(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption option, bool PreexecutionCheck = false)
	{
		if (_isInvoked)
		{
			return true;
		}
		IBattlePlayerReadOnlyInfo readOnlySelf = playerInfoPair.ReadOnlySelf;
		int num = readOnlySelf.SkillInfoInPlayCards.Count();
		BattleCardBase originalCard = ((_ownerCard.TransformInfo.Type != BattleCardBase.TransformType.Metamorphose) ? _ownerCard.TransformInfo.OriginalCard : null);
		int num2 = readOnlySelf.SkillInfoHandCards.Count((IReadOnlyBattleCardInfo s) => s != _ownerCard && s != originalCard && s.IsUnit);
		int burialRiteCount = _ownerCard.GetBurialRiteCount(playerInfoPair, option, isPrePlay: true);
		int num3 = ((_ownerCard.IsInplay || _ownerCard.IsSpell) ? (5 - burialRiteCount) : (5 - burialRiteCount - 1));
		if (num <= num3 && num2 >= burialRiteCount)
		{
			if (!_ownerCard.IsSpell && _skill.OnWhenEvolveStart == 0)
			{
				return !_ownerCard.SelfBattlePlayer.Class.SkillApplyInformation.IsCantActivateFanfareUnit;
			}
			return true;
		}
		return false;
	}

	public override VfxBase Start(BattlePlayerPair playerPair, SkillBase skill, SkillProcessor skillProcessor, SkillOptionValue optionValue, SkillConditionCheckerOption checkerOption)
	{
		SequentialVfxPlayer sequentialVfxPlayer = SequentialVfxPlayer.Create();
		if (skill != _ownerCard.Skills.LastOrDefault((SkillBase s) => s.CheckCondition(playerPair, checkerOption, isPrePlay: true) && s.PreprocessList.Any((SkillPreprocessBase p) => p is SkillPreprocessBurialRite)))
		{
			return sequentialVfxPlayer;
		}
		if (_isInvoked)
		{
			return sequentialVfxPlayer;
		}
		if (skill.OptionValue.HasInfoByName(SkillFilterCreator.ContentKeyword.save_burial_rite_target))
		{
			skill.SkillPrm.ownerCard.SkillApplyInformation.SaveBurialRiteTargetList(checkerOption.BurialRiteCards);
			for (int num = 0; num < checkerOption.BurialRiteCards.Count; num++)
			{
				CardParameter baseParameter = checkerOption.BurialRiteCards.ElementAt(num).BaseParameter;
				BuffInfo buffInfo = new BuffInfo(baseParameter.CardId, baseParameter.NormalCardId, skill);
				buffInfo.IsSaveBurialRiteSkill = true;
				buffInfo.TargetCard = checkerOption.BurialRiteCards.ElementAt(num);
				_ownerCard.AddBuffInfo(buffInfo);
			}
		}
		SkillBaseSummon.SummonedCardsList summonedCardsList = new SkillBaseSummon.SummonedCardsList();
		foreach (BattleCardBase burialRiteCard in checkerOption.BurialRiteCards)
		{
			summonedCardsList.AddCardToSummonedCards(burialRiteCard, overrideSummonEffect: true);
			sequentialVfxPlayer.Register(burialRiteCard.LoseSkill());
			burialRiteCard.IsSelectedDuringSelectingBurialRiteTarget = false;
			if (!burialRiteCard.SelfBattlePlayer.BattleMgr.IsRecovery)
			{
				burialRiteCard.SelfBattlePlayer.BurialRiteOrDiscardCardHandIndexList.Add(burialRiteCard.SelfBattlePlayer.HandCardList.IndexOf(burialRiteCard));
			}
		}
		BattlePlayerBase.SummonInfo summonInfo = new BattlePlayerBase.SummonInfo(summonedCardsList.Any((BattleCardBase c) => c.IsPlayer), summonedCardsList, SkillBaseSummon.SUMMON_TYPE.HAND);
		BattlePlayerBase self = playerPair.Self;
		BattlePlayerBase.SummonInfo summonInfo2 = summonInfo;
		VfxWithLoadingSequential vfx = self.CardManagement(null, skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.SUMMON, isRandom: false, null, null, skill, summonInfo2) as VfxWithLoadingSequential;
		sequentialVfxPlayer.Register(vfx);
		foreach (BattleCardBase item in summonedCardsList)
		{
			ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
			parallelVfxPlayer.Register(Skill_summon_card.RemoveHandCardFromViewVfx(item, playerPair.Self, isBurialRite: true));
			parallelVfxPlayer.Register(item.StopSpellCharge());
			sequentialVfxPlayer.Register(parallelVfxPlayer);
		}
		sequentialVfxPlayer.Register(InstantVfx.Create(delegate
		{
			_ownerCard.SelfBattlePlayer.BurialRiteOrDiscardCardHandIndexList.Clear();
		}));
		StartPickMultiCardVfx vfx2 = new StartPickMultiCardVfx(summonedCardsList, _ownerCard.ResourceMgr, _ownerCard.IsPlayer, isToken: true);
		sequentialVfxPlayer.Register(vfx2);
		_ownerCard.SelfBattlePlayer.CallOnSummonCards(_ownerCard, checkerOption.BurialRiteCards, _ownerCard.IsPlayer, isDeckSelf: false, isIgnoreVoice: false, isBurialRite: true);
		_ownerCard.SelfBattlePlayer.CallOnSkillDestroyOrBanish(_ownerCard, isBurialRite: true);
		ParallelVfxPlayer parallelVfxPlayer2 = ParallelVfxPlayer.Create();
		foreach (BattleCardBase item2 in summonedCardsList)
		{
			item2.SelfBattlePlayer.GameBurialRiteCards.Add(item2);
			item2.SelfBattlePlayer.TurnBurialRiteCards.Add(item2);
			item2.SelfBattlePlayer.StartSkillWhenBurialRiteOther(item2, skillProcessor);
			SequentialVfxPlayer sequentialVfxPlayer2 = SequentialVfxPlayer.Create();
			item2.FlagCardAsDestroyedBySkill();
			item2.DeathTypeInfo.BurialRite = true;
			sequentialVfxPlayer2.Register(item2.SkillApplyInformation.AllSkillEffectStop());
			sequentialVfxPlayer2.Register(item2.SelfBattlePlayer.CardManagement(item2, skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.DESTROY, isRandom: false, null, null, skill));
			parallelVfxPlayer2.Register(sequentialVfxPlayer2);
		}
		if (!_ownerCard.SelfBattlePlayer.BattleMgr.IsVirtualBattle)
		{
			BattleLogManager.GetInstance().AddLogSkillDeath(summonedCardsList.ToList(), skill);
		}
		sequentialVfxPlayer.Register(parallelVfxPlayer2);
		return sequentialVfxPlayer;
	}

	public static List<BattleCardBase> GetBurialRiteTarget(BattlePlayerBase self, BattleCardBase ownerCard)
	{
		List<BattleCardBase> list = new List<BattleCardBase>();
		List<BattleCardBase> handCardList = self.HandCardList;
		for (int i = 0; i < handCardList.Count; i++)
		{
			BattleCardBase battleCardBase = handCardList[i];
			if (ownerCard.Index != battleCardBase.Index && battleCardBase.IsUnit)
			{
				list.Add(battleCardBase);
			}
		}
		return list;
	}

	public void SetInvoked()
	{
		_isInvoked = true;
	}
}
