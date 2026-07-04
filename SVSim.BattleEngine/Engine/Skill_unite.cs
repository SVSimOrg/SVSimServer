using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_unite : SkillBaseSummon
{
	public class UniteCardPair
	{
		public List<BattleCardBase> _materialCards;

		public List<BattleCardBase> _uniteCards;

		public UniteCardPair(List<BattleCardBase> materialCards, List<BattleCardBase> uniteCards)
		{
			_materialCards = materialCards;
			_uniteCards = uniteCards;
		}
	}

	protected UniteCardPair _uniteCardPair;

	public Skill_unite(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		bool flag = false;
		_uniteCardPair = null;
		List<BattleCardBase> list = new List<BattleCardBase>();
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type, "combine");
		string option = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.summon_token, "_OPT_NULL_");
		string option2 = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.base_card_id, "_OPT_NULL_");
		IEnumerable<int> enumerable = SkillOptionValue.ParseOptionTokenID(option);
		IEnumerable<int> source = SkillOptionValue.ParseOptionTokenID(option2);
		if (parameter.targetCards.Where((BattleCardBase c) => !c.IsDead).Count() < source.Count())
		{
			return NullVfxWithLoading.GetInstance();
		}
		for (int num = 0; num < parameter.targetCards.Count(); num++)
		{
			BattleCardBase card = parameter.targetCards.ElementAt(num);
			int num2 = source.Where((int x) => x == card.BaseParameter.BaseCardId).Count();
			int num3 = list.Where((BattleCardBase x) => x.BaseParameter.BaseCardId == card.BaseParameter.BaseCardId).Count();
			if (num2 - num3 > 0 && !card.IsDead)
			{
				list.Add(card);
			}
		}
		if (text == "combine" && !base.SkillPrm.ownerCard.IsDead)
		{
			list.Add(base.SkillPrm.ownerCard);
		}
		if ((text == "combine" && list.Count == source.Count() + 1) || (text == "mix" && list.Count == source.Count()))
		{
			for (int num4 = 0; num4 < list.Count; num4++)
			{
				parallelVfxPlayer.Register(list[num4].UniteInPlay(parameter.skillProcessor, this));
			}
			flag = true;
		}
		if (IsMakeFoil)
		{
			CardMaster cardMaster = CardMaster.GetInstanceForBattle();
			enumerable = from id in enumerable
				select cardMaster.GetCardParameterFromId(id) into param
				select param.FoilCardId;
		}
		if (flag)
		{
			VfxWithLoading vfxWithLoading = CreateSkillEffect(base.SkillPrm.resourceMgr, parameter.targetCards);
			SummonedCardsList summonedCardsList = CreateSummonedCardsList(parameter.targetCards, enumerable, base.SkillPrm.selfBattlePlayer);
			_uniteCardPair = new UniteCardPair(list, summonedCardsList.ToList());
			CallOnUnite();
			BattlePlayerBase.SummonInfo summonInfo = new BattlePlayerBase.SummonInfo(base.SkillPrm.ownerCard.IsPlayer, summonedCardsList, SUMMON_TYPE.TOKEN);
			BattlePlayerBase selfBattlePlayer = base.SkillPrm.selfBattlePlayer;
			SkillProcessor skillProcessor = parameter.skillProcessor;
			BattlePlayerBase.SummonInfo summonInfo2 = summonInfo;
			VfxWithLoadingSequential vfxWithLoadingSequential = selfBattlePlayer.CardManagement(null, skillProcessor, BattlePlayerBase.CARD_MANAGEMENT.SUMMON, isRandom: false, null, null, this, summonInfo2) as VfxWithLoadingSequential;
			base.SkillPrm.selfBattlePlayer.UpdateHandCardsPlayability();
			VfxWithLoading vfxWithLoading2 = CreateSummonCardAnimation(base.SkillPrm.ownerCard.IsPlayer, summonedCardsList);
			if (IsBattleLog && _uniteCardPair != null)
			{
				BattleLogManager.GetInstance().AddLogSkillUnite(_uniteCardPair, this);
			}
			VfxWithLoadingSequential vfxWithLoadingSequential2 = VfxWithLoadingSequential.Create(vfxWithLoading.MainVfx, parallelVfxPlayer, vfxWithLoading2.MainVfx, vfxWithLoadingSequential);
			vfxWithLoadingSequential2.RegisterToLoadingVfx(vfxWithLoading.LoadingVfx);
			vfxWithLoadingSequential2.RegisterToLoadingVfx(vfxWithLoading2.LoadingVfx);
			vfxWithLoadingSequential2.RegisterToLoadingVfx(vfxWithLoadingSequential.LoadingVfx);
			return vfxWithLoadingSequential2;
		}
		return NullVfxWithLoading.GetInstance();
	}

	protected virtual void CallOnUnite()
	{
	}
}
