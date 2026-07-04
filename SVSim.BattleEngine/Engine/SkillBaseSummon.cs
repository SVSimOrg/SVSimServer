using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Cute;
using Wizard.Battle;
using Wizard.Battle.View.Vfx;

public abstract class SkillBaseSummon : SkillBase
{
	public enum SUMMON_TYPE
	{
		TOKEN,
		DECK,
		HAND,
		DESTROYED
	}

	public class SummonedCardsList : IEnumerable<BattleCardBase>, IEnumerable
	{
		public class CardEffectPair
		{
			public BattleCardBase card;

			public VfxBase summonEffect;

			public CardEffectPair(BattleCardBase _card, bool _overrideSummonEffect)
			{
				card = _card;
				summonEffect = (_overrideSummonEffect ? NullVfx.GetInstance() : null);
			}
		}

		public readonly ReadOnlyCollection<CardEffectPair> summonedCardEffectPairList;

		public readonly ReadOnlyCollection<CardEffectPair> overflowCardEffectPairList;

		private List<CardEffectPair> m_summonedCardEffectPairList;

		private List<CardEffectPair> m_overflowCardEffectPairList;

		public IEnumerable<BattleCardBase> summonedCards
		{
			get
			{
				foreach (CardEffectPair summonedCardEffectPair in m_summonedCardEffectPairList)
				{
					yield return summonedCardEffectPair.card;
				}
			}
		}

		public IEnumerable<BattleCardBase> overflowCards
		{
			get
			{
				foreach (CardEffectPair overflowCardEffectPair in m_overflowCardEffectPairList)
				{
					yield return overflowCardEffectPair.card;
				}
			}
		}

		public IEnumerable<CardEffectPair> cardEffectPairList
		{
			get
			{
				foreach (CardEffectPair summonedCardEffectPair in m_summonedCardEffectPairList)
				{
					yield return summonedCardEffectPair;
				}
				foreach (CardEffectPair overflowCardEffectPair in m_overflowCardEffectPairList)
				{
					yield return overflowCardEffectPair;
				}
			}
		}

		public SummonedCardsList()
		{
			m_summonedCardEffectPairList = new List<CardEffectPair>();
			m_overflowCardEffectPairList = new List<CardEffectPair>();
			summonedCardEffectPairList = new ReadOnlyCollection<CardEffectPair>(m_summonedCardEffectPairList);
			overflowCardEffectPairList = new ReadOnlyCollection<CardEffectPair>(m_overflowCardEffectPairList);
		}

		public IEnumerator<BattleCardBase> GetEnumerator()
		{
			foreach (BattleCardBase summonedCard in summonedCards)
			{
				yield return summonedCard;
			}
			foreach (BattleCardBase overflowCard in overflowCards)
			{
				yield return overflowCard;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void AddCardToSummonedCards(BattleCardBase summonedCard, bool overrideSummonEffect = false)
		{
			m_summonedCardEffectPairList.Add(new CardEffectPair(summonedCard, overrideSummonEffect));
		}

		public void AddCardToOverflowCards(BattleCardBase overflowCard, bool overrideSummonEffect = false)
		{
			m_overflowCardEffectPairList.Add(new CardEffectPair(overflowCard, overrideSummonEffect));
		}
	}

	protected bool _isIgnoreVoice;

	protected bool _isRandomVoice;

	protected bool _isEvoVoice;

	protected float _voiceWaitTime = -1f;

	public override bool IsAllowDestroyTarget => true;

	protected SkillBaseSummon(SkillParameter skillPrm, string option)
		: base(skillPrm, option)
	{
	}

	protected void InitSummonParameter()
	{
		_isIgnoreVoice = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.ignore_voice, "false") == "true";
		_isRandomVoice = base.OptionValue.GetString(SkillFilterCreator.ContentKeyword.random_voice, "false") == "true";
		_isEvoVoice = base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.summon_voice, "_OPT_NULL_") == "evo_voice";
		if (!float.TryParse(base.OptionValue.GetOption(SkillFilterCreator.ContentKeyword.voice_wait_time, "_OPT_NULL_"), out _voiceWaitTime))
		{
			_voiceWaitTime = -1f;
		}
	}

	protected SummonedCardsList CreateSummonedCardsList(IEnumerable<BattleCardBase> originalCards, IEnumerable<int> tokenIds, BattlePlayerBase summonPlayer, string summonSideOption = "null", bool isGetoff = false)
	{
		SummonedCardsList summonedList = new SummonedCardsList();
		List<BattleCardBase> list = originalCards.ToList();
		BattlePlayerBase obj = ((summonSideOption == "null" && list.Count > 0) ? list[0].SelfBattlePlayer : summonPlayer);
		bool isPlayer = obj.IsPlayer;
		int num = obj.ClassAndInPlayCardList.Count;
		bool isCopy = list.Count > 0 && IsHaveApplicableTargetFilter<SkillTargetDiscardCardListFilter>();
		for (int i = 0; i < tokenIds.Count(); i++)
		{
			int copyIndex = -1;
			if (list.Count > 0)
			{
				copyIndex = ((list.Count < tokenIds.Count()) ? list.First().Index : list.ElementAt(i).Index);
			}
			if (!AddTokenToList(ref summonedList, tokenIds.ElementAt(i), num, isPlayer, isGetoff, isCopy, copyIndex))
			{
				break;
			}
			num++;
		}
		return summonedList;
	}

	private bool AddTokenToList(ref SummonedCardsList summonedList, int tokenID, int inPlayCardCount, bool isPlayer, bool isGetoff = false, bool isCopy = false, int copyIndex = -1)
	{
		if (inPlayCardCount < 6)
		{
			summonedList.AddCardToSummonedCards(CreateSummonedToken(isPlayer, tokenID, isGetoff, isCopy, copyIndex));
			return true;
		}
		summonedList.AddCardToOverflowCards(CreateDummyToken(isPlayer, tokenID));
		return false;
	}

	private BattleCardBase CreateSummonedToken(bool isPlayer, int tokenId, bool isGetoff = false, bool isCopy = false, int copyIndex = -1)
	{
		if (SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsNetworkBattle && !SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.GameMgr.IsAINetwork && !base.SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.InstanceIsForecast)
		{
			NetworkBattleDefine.NetworkCardPlaceState placeStatus = (isGetoff ? NetworkBattleDefine.NetworkCardPlaceState.Riding : NetworkBattleDefine.NetworkCardPlaceState.Field);
			return SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.CreateBattleCardWithGameObject(new BattleManagerBase.CardCreateInfo(tokenId, isPlayer, base.ApplyingTargetFilter is SkillTargetChosenCardsFilter, placeStatus, isReferenceOpponentCard: false, this), new BattleManagerBase.IndexInfo(-1, -1, isCopy, copyIndex));
		}
		return ((base.SkillPrm.selfBattlePlayer.IsPlayer == isPlayer) ? base.SkillPrm.selfBattlePlayer : base.SkillPrm.opponentBattlePlayer).CreateNextIndexCard(tokenId);
	}

	private BattleCardBase CreateDummyToken(bool isPlayer, int tokenId)
	{
		return CardCreatorBase.CreateCard(tokenId, isPlayer, 0, SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.SBattleLoad, SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr, SkillPrm.ownerCard.SelfBattlePlayer.BattleMgr.BattleResourceMgr, NullInnerOptionsBuilder.GetInstance());
	}

	protected virtual VfxWithLoading CreateSummonCardAnimation(bool isPlayer, SummonedCardsList summonedCardsList, bool isOwnerEffect = false)
	{
		List<string> list = new List<string>();
		foreach (BattleCardBase summonedCards in summonedCardsList)
		{
			list.Add(Toolbox.ResourcesManager.GetAssetTypePath(summonedCards.BaseParameter.SummonEffectPath, ResourcesManager.AssetLoadPathType.Effect2D));
		}
		foreach (string item in list)
		{
			if (item == null)
			{
				return NullVfxWithLoading.GetInstance();
			}
		}
		return VfxWithLoading.Create(new StartPickMultiCardVfx(summonedCardsList, base.SkillPrm.resourceMgr, isPlayer, isToken: true, _isIgnoreVoice, _isRandomVoice));
	}
}
