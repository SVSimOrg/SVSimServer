using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.UI;
using Wizard.Battle.View.Vfx;

public class Skill_change_affiliation : SkillBase
{
	public class BuffInfoAffiliationContainer : BuffInfoContainer
	{
		public string _clanType;

		public List<CardBasePrm.TribeType> Tribe;

		public BuffInfoAffiliationContainer(BattleCardBase targetCard, BuffInfo buffInfo, string clanType, List<CardBasePrm.TribeType> tribeType)
			: base(targetCard, buffInfo, -1, "", null, 0L)
		{
			_clanType = clanType;
			Tribe = tribeType;
		}
	}

	protected CardBasePrm.TribeInfo _tribeInfo;

	protected CardBasePrm.ClanType _clan;

	public override bool IsTargetIndicate
	{
		get
		{
			bool num = base.ApplyAndFilter.Any((ApplySkillTargetFilterCollection f) => f.TargetFilter is SkillTargetDeckFilter) || base.ApplyingTargetFilter is SkillTargetDeckFilter;
			bool flag = base.SkillPrm.ownerCard.SelfBattlePlayer.DeckCardList.Count == 0;
			if (num && flag)
			{
				return true;
			}
			return false;
		}
	}

	public Skill_change_affiliation(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	public override VfxWithLoading Start(CallParameter parameter)
	{
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.clan);
		_clan = CardBasePrm.GetClanType(text);
		List<CardBasePrm.TribeType> list = CreateChangeTribeList();
		CardBasePrm.TribeChangeType type = CreateTribeChangeType();
		_tribeInfo = new CardBasePrm.TribeInfo(list, type);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BattleCardBase targetCard in parameter.targetCards)
		{
			parallelVfxPlayer.Register(targetCard.SkillApplyInformation.GiveChangeAffiliation(_clan, _tribeInfo, showEffect: true));
			BattleCardBase battleCardBase = targetCard;
			BuffInfo buffInfo = AddBuffInfoIfNeeded(targetCard);
			BuffInfoAffiliationContainer buffInfoAffiliationContainer = new BuffInfoAffiliationContainer(battleCardBase, buffInfo, text, list);
			buffInfoContainer.Add(buffInfoAffiliationContainer);
			SetOnLoseEvent(battleCardBase, buffInfo, buffInfoAffiliationContainer);
		}
		if (!base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast)
		{
			AddBattleLog(parameter.targetCards, _clan, list);
		}
		VfxWithLoadingSequential vfxWithLoadingSequential = VfxWithLoadingSequential.Create();
		vfxWithLoadingSequential.RegisterVfxWithLoading(CreateSkillEffect(targetCards: parameter.targetCards.Where((BattleCardBase s) => s.IsPlayer || s.IsInplay), resourceMgr: base.SkillPrm.resourceMgr));
		vfxWithLoadingSequential.RegisterToMainVfx(parallelVfxPlayer);
		return vfxWithLoadingSequential;
	}

	public override VfxWithLoading Stop(SkillProcessor skillProcessor)
	{
		base.Stop(skillProcessor);
		ParallelVfxPlayer parallelVfxPlayer = ParallelVfxPlayer.Create();
		foreach (BuffInfoAffiliationContainer item in buffInfoContainer)
		{
			VfxBase vfx = item._targetCard.SkillApplyInformation.DepriveChangeAffiliation(CardBasePrm.GetClanType(item._clanType), _tribeInfo);
			item._targetCard.RemoveBuffInfo(item._buffInfo);
			parallelVfxPlayer.Register(vfx);
		}
		buffInfoContainer.Clear();
		return VfxWithLoading.Create(parallelVfxPlayer);
	}

	public List<CardBasePrm.TribeType> CreateChangeTribeList()
	{
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.tribe, string.Empty);
		if (text == string.Empty)
		{
			return null;
		}
		return CardBasePrm.CreateTribeTypeList(text);
	}

	public CardBasePrm.TribeChangeType CreateTribeChangeType()
	{
		string text = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.type);
		if (text == SkillFilterCreator.ContentKeyword.change.ToString())
		{
			return CardBasePrm.TribeChangeType.CHANGE;
		}
		if (text == SkillFilterCreator.ContentKeyword.add.ToString())
		{
			return CardBasePrm.TribeChangeType.ADD;
		}
		return CardBasePrm.TribeChangeType.CHANGE;
	}

	public CardBasePrm.ClanType GetClanType()
	{
		return CardBasePrm.GetClanType(base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.clan));
	}

	private void AddBattleLog(IEnumerable<BattleCardBase> targetCards, CardBasePrm.ClanType newClan, List<CardBasePrm.TribeType> newTribe)
	{
		bool isAdminWatch = SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAdminWatch;
		List<BattleCardBase> cards = targetCards.Where((BattleCardBase c) => c.IsInplay || (c.IsInHand && (c.IsPlayer || isAdminWatch))).ToList();
		bool isTargetInOpponentHand = IsTargetInOpponentHand();
		if (CardBasePrm.ClanType.NONE != newClan)
		{
			BattleLogManager.GetInstance().AddLogSkillChangeClan(cards, newClan, this, isTargetInOpponentHand);
		}
		if (newTribe != null && newTribe.Count > 0)
		{
			BattleLogManager.GetInstance().AddLogSkillChangeTribe(cards, newTribe, this, isTargetInOpponentHand);
		}
	}

	public virtual void RegisterStop(BattleCardBase card)
	{
	}

	public override void SetOnLoseEvent(BattleCardBase targetCard, BuffInfo buff, BuffInfoContainer container)
	{
		targetCard.OnLoseSkillOneTime += delegate(SkillBase loseSkill, SkillProcessor skillProcessor, BattleCardBase card)
		{
			RegisterStop(card);
			card.RemoveBuffInfo(buff);
			buffInfoContainer.Remove(container);
			return card.SkillApplyInformation.ForceDepriveChangeAffiliation();
		};
	}
}
