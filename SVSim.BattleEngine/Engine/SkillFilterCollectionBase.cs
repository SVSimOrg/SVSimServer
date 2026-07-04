using System.Collections.Generic;
using System.Linq;
using Wizard;
using Wizard.Battle;

public class SkillFilterCollectionBase
{

	private bool _isSkipPrivateCardCheck;

	public ISkillBattlePlayerFilter BattlePlayerFilter { get; set; }

	public ISkillTargetFilter TargetFilter { get; set; }

	public List<ISkillCardFilter> CardFilterList { get; private set; }

	public ISkillSelectFilter SelectFilter { get; set; }

	protected bool IsPrivateCard(BattlePlayerReadOnlyInfoPair playerInfoPair, IEnumerable<IReadOnlyBattleCardInfo> cardInfos, SkillConditionCheckerOption checkerOption)
	{
		if (_isSkipPrivateCardCheck)
		{
			return false;
		}
		if (playerInfoPair.ReadOnlySelf.IsPlayer)
		{
			return false;
		}
		if (TargetFilter is SkillTargetGameLeftCardsFilter || TargetFilter is SkillTargetLeftThisTurnCardListFilter)
		{
			return false;
		}
		if (TargetFilter is SkillTargetInplayBuffingCardsFilter || TargetFilter is SkillTargetInplayDebuffingCardsFilter)
		{
			return false;
		}
		if (CardFilterList.Any((ISkillCardFilter f) => f is SkillFusionIngredientCardListFilter || f is SkillTargetDrewSkillFilter) || TargetFilter is SkillTargetFusionIngredientedCardListIncludeThisFusion || TargetFilter is SkillTargetFusionThisTurnCardList)
		{
			return false;
		}
		if (TargetFilter is SkillTargetGamePlayCardsOtherSelfFilter || TargetFilter is SkillTargetTurnPlayCardsOtherSelfFilter || TargetFilter is SkillTargetTurnSummonCardsFilter || TargetFilter is SkillTargetGameSummonCardsFilter || TargetFilter is SkillTargetGameSummonCardsOtherFilter)
		{
			return false;
		}
		if (TargetFilter is SkillTargetSelfFilter && CardFilterList.Any((ISkillCardFilter f) => f is SkillParameterLifeFilter))
		{
			return false;
		}
		if (TargetFilter is SkillTargetBurialRiteCardFilter)
		{
			return false;
		}
		BattleManagerBase _mgr = cardInfos.FirstOrDefault()?.SelfBattlePlayer?.BattleMgr;
		if (_mgr != null && _mgr.XorShiftRandom(isSelf: true) != null && _mgr.XorShiftRandom(isSelf: false) == null)
		{
			if (!cardInfos.All((IReadOnlyBattleCardInfo s) => (s.IsInHand && (checkerOption.LeftCards == null || checkerOption.LeftCards.IndexOf(s) == -1)) || s.IsInDeck))
			{
				return TargetFilter is SkillTargetBattleStartDeckFilter;
			}
			return true;
		}
		return false;
	}

	public SkillFilterCollectionBase(bool isSkipPrivateCardCheck = false)
	{
		CardFilterList = new List<ISkillCardFilter>(6);
		SelectFilter = new SkillSelectAllFilter();
		_isSkipPrivateCardCheck = isSkipPrivateCardCheck;
	}

	protected IEnumerable<IReadOnlyBattleCardInfo> FilteringBase(BattlePlayerReadOnlyInfoPair playerInfoPair, SkillConditionCheckerOption checkerOption, SkillOptionValue optionValue, bool isSkipPrivateCheck = false)
	{
		IEnumerable<IReadOnlyBattleCardInfo> enumerable = null;
		if (BattlePlayerFilter == null)
		{
			enumerable = TargetFilter.Filtering(null, checkerOption);
		}
		else if (TargetFilter != null)
		{
			IEnumerable<IBattlePlayerReadOnlyInfo> battlePlayerInfos = BattlePlayerFilter.Filtering(playerInfoPair);
			enumerable = TargetFilter.Filtering(battlePlayerInfos, checkerOption);
			if (IsPrivateCard(playerInfoPair, enumerable, checkerOption) || isSkipPrivateCheck)
			{
				return enumerable;
			}
		}
		foreach (ISkillCardFilter cardFilter in CardFilterList)
		{
			enumerable = cardFilter.Filtering(enumerable, optionValue);
		}
		if (enumerable != null)
		{
			IEnumerable<BattleCardBase> cards = enumerable.Cast<BattleCardBase>();
			enumerable = SelectFilter.Filtering(cards, optionValue, checkerOption).Cast<IReadOnlyBattleCardInfo>();
		}
		return enumerable;
	}
}
