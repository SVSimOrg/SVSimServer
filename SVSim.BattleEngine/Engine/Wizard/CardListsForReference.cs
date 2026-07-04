using System;
using System.Collections.Generic;

namespace Wizard;

public class CardListsForReference
{
	public enum TagHolderReferenceType
	{
		WhenHeal,
		AttackByLife,
		AttackableClass,
		Break,
		OtherAttackBuff,
		OtherAttackDamage,
		OtherAttackToken,
		AllyPlayoutDamageBonus,
		OtherBreakBonus,
		OtherBanishBonus,
		OtherLeaveBonus,
		EnemyTurnEnd,
		AfterDiscard,
		WhenLeaveOther,
		Banish,
		GetOn,
		WhenGetOff,
		OtherSummon,
		Bounce,
		RallyCountPlus,
		WhenNecromance,
		Resonance,
		TurnStart,
		SelfTurnEnd,
		ForceBerserk,
		RemoveByDestroy,
		WhenChangeInplay,
		OtherDamaged,
		OtherEvo,
		OtherPlay,
		ActivateCount,
		ForceImmediateAttack,
		None
	}

	private Dictionary<TagHolderReferenceType, List<AIVirtualCard>> _tagHoldersList;

	public List<AIVirtualCard> BothClassAndInplayCards { get; private set; }

	public List<AIVirtualCard> BothInplayCards { get; private set; }

	public List<AIVirtualCard> AllReferableCards { get; private set; }

	public List<AIVirtualCard> AllyClassAndInplayCards { get; private set; }

	public List<AIVirtualCard> AllAllyCards { get; private set; }

	public List<AIVirtualCard> EnemyClassAndInplayCards { get; private set; }

	public List<AIVirtualCard> AllyDestroyedCards { get; private set; }

	public List<AIVirtualCard> EnemyDestroyedCards { get; private set; }

	public List<AIVirtualCard> AllyLeftCards { get; private set; }

	public List<AIVirtualCard> EnemyLeftCards { get; private set; }

	public List<AIVirtualCard> AllyLeftCardsThisTurn { get; private set; }

	public List<AIVirtualCard> EnemyLeftCardsThisTurn { get; private set; }

	public List<AIVirtualCard> AllyBurialCards { get; private set; }

	public List<AIVirtualCard> EnemyBurialCards { get; private set; }

	public List<AIVirtualCard> BanishedCards { get; private set; }

	public List<AIVirtualCard> AttackableClassHolders => _tagHoldersList[TagHolderReferenceType.AttackableClass];

	public List<AIVirtualCard> BreakTagHolders => _tagHoldersList[TagHolderReferenceType.Break];

	public List<AIVirtualCard> OtherAttackBuffTagHolders => _tagHoldersList[TagHolderReferenceType.OtherAttackBuff];

	public List<AIVirtualCard> OtherAttackDamageHolders => _tagHoldersList[TagHolderReferenceType.OtherAttackDamage];

	public List<AIVirtualCard> AllyPlayoutDamageBonusHolders => _tagHoldersList[TagHolderReferenceType.AllyPlayoutDamageBonus];

	public List<AIVirtualCard> OtherBreakBonusHolders => _tagHoldersList[TagHolderReferenceType.OtherBreakBonus];

	public List<AIVirtualCard> OtherBanishBonusHolders => _tagHoldersList[TagHolderReferenceType.OtherBanishBonus];

	public List<AIVirtualCard> OtherLeaveBonusHolders => _tagHoldersList[TagHolderReferenceType.OtherLeaveBonus];

	public List<AIVirtualCard> EnemyTurnEndTagHolders => _tagHoldersList[TagHolderReferenceType.EnemyTurnEnd];

	public List<AIVirtualCard> AfterDiscardTagHolders => _tagHoldersList[TagHolderReferenceType.AfterDiscard];

	public List<AIVirtualCard> WhenLeaveOtherTagHolders => _tagHoldersList[TagHolderReferenceType.WhenLeaveOther];

	public List<AIVirtualCard> BanishTagHolders => _tagHoldersList[TagHolderReferenceType.Banish];

	public List<AIVirtualCard> GetOnTagHolders => _tagHoldersList[TagHolderReferenceType.GetOn];

	public List<AIVirtualCard> BounceTagHolders => _tagHoldersList[TagHolderReferenceType.Bounce];

	public List<AIVirtualCard> OtherSummonTagHolders => _tagHoldersList[TagHolderReferenceType.OtherSummon];

	public List<AIVirtualCard> WhenNecromanceTagHolders => _tagHoldersList[TagHolderReferenceType.WhenNecromance];

	public List<AIVirtualCard> ResonanceTagHolders => _tagHoldersList[TagHolderReferenceType.Resonance];

	public List<AIVirtualCard> TurnStartTagHolders => _tagHoldersList[TagHolderReferenceType.TurnStart];

	public List<AIVirtualCard> ForceBerserkTagHolders => _tagHoldersList[TagHolderReferenceType.ForceBerserk];

	public List<AIVirtualCard> RemoveByDestroyHolders => _tagHoldersList[TagHolderReferenceType.RemoveByDestroy];

	public List<AIVirtualCard> WhenChangeInplayHolders => _tagHoldersList[TagHolderReferenceType.WhenChangeInplay];

	public List<AIVirtualCard> OtherDamagedTagHolders => _tagHoldersList[TagHolderReferenceType.OtherDamaged];

	public List<AIVirtualCard> OtherEvoTagHolders => _tagHoldersList[TagHolderReferenceType.OtherEvo];

	public List<AIVirtualCard> OtherPlayTagHolders => _tagHoldersList[TagHolderReferenceType.OtherPlay];

	public List<AIVirtualCard> ActivateCountHolders => _tagHoldersList[TagHolderReferenceType.ActivateCount];

	public List<AIVirtualCard> ForceImmediateAttackHolders => _tagHoldersList[TagHolderReferenceType.ForceImmediateAttack];

	public bool HasAttackableClassHolder
	{
		get
		{
			if (AttackableClassHolders != null)
			{
				return AttackableClassHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasBreakTagHolder
	{
		get
		{
			if (BreakTagHolders != null)
			{
				return BreakTagHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasOtherAttackBuffHolder
	{
		get
		{
			if (OtherAttackBuffTagHolders != null)
			{
				return OtherAttackBuffTagHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasAllyPlayoutDamageBonusHolder
	{
		get
		{
			if (AllyPlayoutDamageBonusHolders != null)
			{
				return AllyPlayoutDamageBonusHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasOtherBreakBonusHolder
	{
		get
		{
			if (OtherBreakBonusHolders != null)
			{
				return OtherBreakBonusHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasOtherBanishBonusHolder
	{
		get
		{
			if (OtherBanishBonusHolders != null)
			{
				return OtherBanishBonusHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasOtherLeaveBonusHolder
	{
		get
		{
			if (OtherLeaveBonusHolders != null)
			{
				return OtherLeaveBonusHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasEnemyTurnEndTagHolder
	{
		get
		{
			if (EnemyTurnEndTagHolders != null)
			{
				return EnemyTurnEndTagHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasAfterDiscardTagHolder
	{
		get
		{
			if (AfterDiscardTagHolders != null)
			{
				return AfterDiscardTagHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasBanishTagHolder
	{
		get
		{
			if (BanishTagHolders != null)
			{
				return BanishTagHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasLeaveOtherTagHolders
	{
		get
		{
			if (WhenLeaveOtherTagHolders != null)
			{
				return WhenLeaveOtherTagHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasGetOnTagHolders
	{
		get
		{
			if (GetOnTagHolders != null)
			{
				return GetOnTagHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasBounceTagHolders
	{
		get
		{
			if (BounceTagHolders != null)
			{
				return BounceTagHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasWhenNecromanceHolder
	{
		get
		{
			if (WhenNecromanceTagHolders != null)
			{
				return WhenNecromanceTagHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasResonanceHolder
	{
		get
		{
			if (ResonanceTagHolders != null)
			{
				return ResonanceTagHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasWhenChangeInplayHolder
	{
		get
		{
			if (WhenChangeInplayHolders != null)
			{
				return WhenChangeInplayHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasActivateCountHolder
	{
		get
		{
			if (ActivateCountHolders != null)
			{
				return ActivateCountHolders.Count > 0;
			}
			return false;
		}
	}

	public bool HasOtherPlayTagHolder
	{
		get
		{
			if (OtherPlayTagHolders != null)
			{
				return OtherPlayTagHolders.Count > 0;
			}
			return false;
		}
	}

	public CardListsForReference()
	{
		BothClassAndInplayCards = new List<AIVirtualCard>();
		BothInplayCards = new List<AIVirtualCard>();
		AllReferableCards = new List<AIVirtualCard>();
		AllyClassAndInplayCards = new List<AIVirtualCard>();
		AllAllyCards = new List<AIVirtualCard>();
		EnemyClassAndInplayCards = new List<AIVirtualCard>();
		AllyDestroyedCards = new List<AIVirtualCard>();
		EnemyDestroyedCards = new List<AIVirtualCard>();
		AllyLeftCards = new List<AIVirtualCard>();
		EnemyLeftCards = new List<AIVirtualCard>();
		AllyLeftCardsThisTurn = new List<AIVirtualCard>();
		EnemyLeftCardsThisTurn = new List<AIVirtualCard>();
		AllyBurialCards = new List<AIVirtualCard>();
		EnemyBurialCards = new List<AIVirtualCard>();
		BanishedCards = new List<AIVirtualCard>();
		InitTagHoldersList();
	}

	private void InitTagHoldersList()
	{
		_tagHoldersList = new Dictionary<TagHolderReferenceType, List<AIVirtualCard>>();
		foreach (TagHolderReferenceType value in Enum.GetValues(typeof(TagHolderReferenceType)))
		{
			_tagHoldersList.Add(value, null);
		}
	}

	public void AddAllyClass(AIVirtualCard card)
	{
		AllReferableCards.Add(card);
		BothClassAndInplayCards.Add(card);
		AllyClassAndInplayCards.Add(card);
		AllAllyCards.Add(card);
		TagClassification(card);
	}

	public void AddEnemyClass(AIVirtualCard card)
	{
		AllReferableCards.Add(card);
		BothClassAndInplayCards.Add(card);
		EnemyClassAndInplayCards.Add(card);
		TagClassification(card);
	}

	public void AddHandCard(AIVirtualCard card)
	{
		if (card.IsAlly)
		{
			AllReferableCards.Add(card);
			AllAllyCards.Add(card);
		}
		TagClassification(card);
	}

	public void AddAllyInplayCard(AIVirtualCard card)
	{
		AllReferableCards.Add(card);
		BothClassAndInplayCards.Add(card);
		AllyClassAndInplayCards.Add(card);
		AllAllyCards.Add(card);
		if (!card.IsLeader)
		{
			BothInplayCards.Add(card);
		}
		TagClassification(card);
	}

	public void RemoveAllyInplayCard(AIVirtualCard card)
	{
		AllReferableCards.Remove(card);
		BothClassAndInplayCards.Remove(card);
		AllyClassAndInplayCards.Remove(card);
		AllAllyCards.Remove(card);
		if (!card.IsLeader)
		{
			BothInplayCards.Remove(card);
		}
		RemoveTagClassification(card);
	}

	public void AddEnemyInplayCard(AIVirtualCard card)
	{
		AllReferableCards.Add(card);
		BothClassAndInplayCards.Add(card);
		EnemyClassAndInplayCards.Add(card);
		if (!card.IsLeader)
		{
			BothInplayCards.Add(card);
		}
		TagClassification(card);
	}

	public void RemoveEnemyInplayCard(AIVirtualCard card)
	{
		AllReferableCards.Remove(card);
		BothClassAndInplayCards.Remove(card);
		EnemyClassAndInplayCards.Remove(card);
		if (!card.IsLeader)
		{
			BothInplayCards.Remove(card);
		}
		RemoveTagClassification(card);
	}

	public void AddAllyDestroyedCard(AIVirtualCard card, bool isDestroyTurn)
	{
		if (!card.IsLeader && !AllyDestroyedCards.Contains(card) && card.IsAlly)
		{
			card.DeadTurn = isDestroyTurn;
			AllyDestroyedCards.Add(card);
		}
	}

	public void AddAllyLeftCard(AIVirtualCard card)
	{
		if (!card.IsLeader && !AllyLeftCards.Contains(card) && card.IsAlly)
		{
			AllyLeftCards.Add(card);
		}
	}

	public void AddEnemyDestroyedCard(AIVirtualCard card)
	{
		if (!card.IsLeader && !EnemyDestroyedCards.Contains(card) && !card.IsAlly)
		{
			EnemyDestroyedCards.Add(card);
		}
	}

	public void AddEnemyLeftCard(AIVirtualCard card)
	{
		if (!card.IsLeader && !EnemyLeftCards.Contains(card) && !card.IsAlly)
		{
			EnemyLeftCards.Add(card);
		}
	}

	public void AddAllyLeftCardThisTurn(AIVirtualCard card)
	{
		if (!card.IsLeader && card.IsAlly)
		{
			AllyLeftCardsThisTurn.Add(card);
		}
	}

	public void AddEnemyLeftCardThisTurn(AIVirtualCard card)
	{
		if (!card.IsLeader && !card.IsAlly)
		{
			EnemyLeftCardsThisTurn.Add(card);
		}
	}

	public void AddAllyBurialCard(AIVirtualCard card)
	{
		if (!card.IsLeader && !AllyBurialCards.Contains(card) && card.IsAlly)
		{
			AllyBurialCards.Add(card);
		}
	}

	public void AddEnemyBurialCard(AIVirtualCard card)
	{
		if (!card.IsLeader && !EnemyBurialCards.Contains(card) && !card.IsAlly)
		{
			EnemyBurialCards.Add(card);
		}
	}

	public void AddBanishedCard(AIVirtualCard card)
	{
		if (card.IsLeader)
		{
			AIConsoleUtility.LogError("AddBanishedCard() : Leader card is ilegal");
		}
		else
		{
			BanishedCards.Add(card);
		}
	}

	public void TagClassification(AIVirtualCard card)
	{
		List<TagCollectionWithTypeBase> tagDictionary = card.TagCollectionContainer.TagDictionary;
		if (tagDictionary == null || tagDictionary.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < tagDictionary.Count; i++)
		{
			TagCollection collection = tagDictionary[i].Collection;
			for (int j = 0; j < collection.TagList.Count; j++)
			{
				AIPlayTag aIPlayTag = collection.TagList[j];
				if (card.IsOnField || IsPermitInHandTagHolder(aIPlayTag.Type, card))
				{
					TagHolderReferenceType tagHolderReferenceType = ConvertTagTypeToTagHolderReferenceType(aIPlayTag, card);
					if (tagHolderReferenceType != TagHolderReferenceType.None)
					{
						AddCardToHolderList(card, tagHolderReferenceType);
					}
				}
			}
		}
	}

	public void TagClassificationWhenAttachTag(AIVirtualCard card, AIPlayTag attachedTag)
	{
		TagHolderReferenceType tagHolderReferenceType = ConvertTagTypeToTagHolderReferenceType(attachedTag, card);
		if (tagHolderReferenceType != TagHolderReferenceType.None)
		{
			AddCardToHolderList(card, tagHolderReferenceType);
		}
	}

	private bool IsPermitInHandTagHolder(AIPlayTagType tagType, AIVirtualCard card)
	{
		if (!card.IsInHand)
		{
			return false;
		}
		if (tagType == AIPlayTagType.TurnEndAttachTag)
		{
			return true;
		}
		return false;
	}

	private TagHolderReferenceType ConvertTagTypeToTagHolderReferenceType(AIPlayTag tag, AIVirtualCard owner)
	{
		switch (tag.Type)
		{
		case AIPlayTagType.HealDamage:
		case AIPlayTagType.HealBuff:
		case AIPlayTagType.HealToken:
		case AIPlayTagType.HealHeal:
		case AIPlayTagType.HealEvo:
		case AIPlayTagType.HealAttachTag:
			return TagHolderReferenceType.WhenHeal;
		case AIPlayTagType.OtherLeaveDamage:
		case AIPlayTagType.OtherLeaveToken:
			return TagHolderReferenceType.WhenLeaveOther;
		case AIPlayTagType.AttackByLife:
			return TagHolderReferenceType.AttackByLife;
		case AIPlayTagType.OtherAttackBuff:
			return TagHolderReferenceType.OtherAttackBuff;
		case AIPlayTagType.OtherAttackDamage:
			return TagHolderReferenceType.OtherAttackDamage;
		case AIPlayTagType.OtherAttackToken:
			return TagHolderReferenceType.OtherAttackToken;
		case AIPlayTagType.AttackableClass:
			return TagHolderReferenceType.AttackableClass;
		case AIPlayTagType.BreakBuff:
		case AIPlayTagType.BreakSetLeaderMaxLife:
		case AIPlayTagType.BreakHeal:
		case AIPlayTagType.BreakDamage:
		case AIPlayTagType.BreakRecoverAttackableCount:
		case AIPlayTagType.BreakDestroy:
		case AIPlayTagType.BreakRecoverPp:
		case AIPlayTagType.BreakAttachTag:
		case AIPlayTagType.BreakAddStack:
			return TagHolderReferenceType.Break;
		case AIPlayTagType.AllyPlayoutDamageBonus:
			return TagHolderReferenceType.AllyPlayoutDamageBonus;
		case AIPlayTagType.OtherBreakBonus:
			return TagHolderReferenceType.OtherBreakBonus;
		case AIPlayTagType.OtherBanishBonus:
			return TagHolderReferenceType.OtherBanishBonus;
		case AIPlayTagType.OtherLeaveBonus:
			return TagHolderReferenceType.OtherLeaveBonus;
		case AIPlayTagType.TurnEndDestroy:
		case AIPlayTagType.TurnEndBanish:
		case AIPlayTagType.TurnEndDamage:
		case AIPlayTagType.TurnEndHeal:
		case AIPlayTagType.TurnEndSetLeaderMaxLife:
		case AIPlayTagType.TurnEndDiscard:
		case AIPlayTagType.TurnEndBuff:
		case AIPlayTagType.TurnEndSubtractCountdown:
		case AIPlayTagType.TurnEndToken:
		case AIPlayTagType.TurnEndBounce:
		case AIPlayTagType.TurnEndAddDeck:
		case AIPlayTagType.TurnEndEvo:
		case AIPlayTagType.TurnEndDraw:
		case AIPlayTagType.TurnEndShield:
		case AIPlayTagType.TurnEndDamageClip:
		case AIPlayTagType.TurnEndDamageCut:
		case AIPlayTagType.TurnEndGuard:
		case AIPlayTagType.TurnEndBanAttack:
		case AIPlayTagType.TurnEndMetamorphose:
		case AIPlayTagType.TurnEndRemoveTag:
			if (tag.ArgumentExpressions is IAITurnEndArgument iAITurnEndArgument && (owner.IsAlly ? (!iAITurnEndArgument.IsAllyTurn) : iAITurnEndArgument.IsAllyTurn))
			{
				return TagHolderReferenceType.EnemyTurnEnd;
			}
			break;
		case AIPlayTagType.DiscardDamage:
		case AIPlayTagType.DiscardHeal:
		case AIPlayTagType.AllyDiscardBonus:
			return TagHolderReferenceType.AfterDiscard;
		case AIPlayTagType.OtherSummonDamage:
		case AIPlayTagType.OtherSummonRush:
		case AIPlayTagType.OtherSummonGuard:
		case AIPlayTagType.OtherSummonQuick:
		case AIPlayTagType.OtherSummonKiller:
		case AIPlayTagType.OtherSummonDrain:
		case AIPlayTagType.OtherSummonBanish:
		case AIPlayTagType.OtherSummonHeal:
		case AIPlayTagType.OtherSummonBuff:
		case AIPlayTagType.OtherSummonEvo:
		case AIPlayTagType.OtherSummonDamageCut:
		case AIPlayTagType.OtherSummonDamageClip:
		case AIPlayTagType.OtherSummonSubtractCountdown:
		case AIPlayTagType.OtherSummonAddCemetery:
		case AIPlayTagType.OtherSummonUntouchable:
		case AIPlayTagType.OtherSummonDestory:
		case AIPlayTagType.OtherSummonDraw:
		case AIPlayTagType.OtherSummonAttachTag:
			return TagHolderReferenceType.OtherSummon;
		case AIPlayTagType.OtherBanishToken:
		case AIPlayTagType.OtherBanishAddCemetery:
			return TagHolderReferenceType.Banish;
		case AIPlayTagType.GetOn:
			return TagHolderReferenceType.GetOn;
		case AIPlayTagType.GetOffMetamorphose:
		case AIPlayTagType.GetOffEvo:
			return TagHolderReferenceType.WhenGetOff;
		case AIPlayTagType.BounceDamage:
			return TagHolderReferenceType.Bounce;
		case AIPlayTagType.RallyCountPlus:
			return TagHolderReferenceType.RallyCountPlus;
		case AIPlayTagType.NecromanceAttachTag:
		case AIPlayTagType.NecromanceAddCemetery:
		case AIPlayTagType.NecromanceDamage:
		case AIPlayTagType.NecromanceHeal:
			return TagHolderReferenceType.WhenNecromance;
		case AIPlayTagType.ResonanceDamage:
		case AIPlayTagType.ResonanceHeal:
		case AIPlayTagType.ResonanceKiller:
			return TagHolderReferenceType.Resonance;
		case AIPlayTagType.TurnEndAttachTag:
			return TagHolderReferenceType.SelfTurnEnd;
		case AIPlayTagType.ForceBerserk:
			return TagHolderReferenceType.ForceBerserk;
		case AIPlayTagType.RemoveByDestroy:
			return TagHolderReferenceType.RemoveByDestroy;
		case AIPlayTagType.TurnStartAttachTag:
		case AIPlayTagType.TurnStartSubtractCountdown:
		case AIPlayTagType.TurnStartDamageCut:
		case AIPlayTagType.TurnStartShield:
		case AIPlayTagType.TurnStartDamage:
			return TagHolderReferenceType.TurnStart;
		case AIPlayTagType.ChangeInplayAttachTag:
		case AIPlayTagType.ChangeInplayImmediateRemoveByBanish:
		case AIPlayTagType.ChangeInplayImmediateRemoveByDestroy:
		case AIPlayTagType.ChangeInplayCannotPlay:
		case AIPlayTagType.ChangeInplayCannotAttack:
		case AIPlayTagType.ChangeInplayImmediateShield:
		case AIPlayTagType.ChangeInplayImmediateDamageCut:
		case AIPlayTagType.ChangeInplayImmediateDamageClip:
		case AIPlayTagType.ChangeInplayImmediateLifeLowerLimit:
		case AIPlayTagType.ChangeInplayImmediateDamageModifier:
		case AIPlayTagType.ChangeInplayImmediateUntouchable:
		case AIPlayTagType.ChangeInplayImmediateIndestructible:
		case AIPlayTagType.ChangePpTotalBuff:
			return TagHolderReferenceType.WhenChangeInplay;
		case AIPlayTagType.OtherDamagedDamage:
		case AIPlayTagType.OtherDamagedHeal:
		case AIPlayTagType.OtherDamagedSetLeaderMaxLife:
		case AIPlayTagType.OtherDamagedSubtractCountdown:
		case AIPlayTagType.OtherDamagedBanish:
			return TagHolderReferenceType.OtherDamaged;
		case AIPlayTagType.OtherEvoBuff:
		case AIPlayTagType.OtherEvoDamage:
		case AIPlayTagType.OtherEvoBanish:
		case AIPlayTagType.OtherEvoSubtractCountdown:
		case AIPlayTagType.OtherEvoEvo:
		case AIPlayTagType.OtherEvoToken:
		case AIPlayTagType.OtherEvoShield:
		case AIPlayTagType.SelfAndOtherEvoDamage:
		case AIPlayTagType.SelfAndOtherEvoAttachTag:
		case AIPlayTagType.SelfAndOtherEvoShield:
		case AIPlayTagType.SelfAndOtherEvoDestroy:
		case AIPlayTagType.SelfAndOtherEvoBounce:
		case AIPlayTagType.SelfAndOtherEvoToken:
		case AIPlayTagType.SelfAndOtherEvoDraw:
		case AIPlayTagType.SelfAndOtherEvoAddCemetery:
		case AIPlayTagType.SelfAndOtherEvoHeal:
		case AIPlayTagType.SelfAndOtherEvoTokenDraw:
			return TagHolderReferenceType.OtherEvo;
		case AIPlayTagType.OtherPlayAttachTag:
		case AIPlayTagType.OtherPlayEvo:
		case AIPlayTagType.OtherEnhanceEvo:
		case AIPlayTagType.OtherPlayDamage:
		case AIPlayTagType.OtherPlayRecoverPp:
		case AIPlayTagType.OtherPlayDestroy:
		case AIPlayTagType.OtherPlayBuff:
		case AIPlayTagType.OtherPlayToken:
		case AIPlayTagType.OtherPlayRemoveTag:
		case AIPlayTagType.OtherPlayBounce:
		case AIPlayTagType.OtherPlayQuick:
			return TagHolderReferenceType.OtherPlay;
		case AIPlayTagType.ForceImmediateAttack:
			return TagHolderReferenceType.ForceImmediateAttack;
		case AIPlayTagType.PlayActivateCount:
		case AIPlayTagType.AttackActivateCount:
		case AIPlayTagType.BreakActivateCount:
		case AIPlayTagType.BanishActivateCount:
		case AIPlayTagType.DamagedActivateCount:
		case AIPlayTagType.BuffActivateCounnt:
		case AIPlayTagType.TurnEndActivateCount:
		case AIPlayTagType.HealActivateCount:
		case AIPlayTagType.NecromanceActivateCount:
		case AIPlayTagType.EvoActivateCount:
		case AIPlayTagType.SummonActivateCount:
			return TagHolderReferenceType.ActivateCount;
		}
		return TagHolderReferenceType.None;
	}

	private void AddCardToHolderList(AIVirtualCard card, TagHolderReferenceType type)
	{
		List<AIVirtualCard> value = ClassifyCardToHolderList(card, _tagHoldersList[type]);
		_tagHoldersList[type] = value;
	}

	public void RemoveCardFromTagHolder(AIVirtualCard card, AIPlayTag removedTag)
	{
		TagHolderReferenceType key = ConvertTagTypeToTagHolderReferenceType(removedTag, card);
		if (_tagHoldersList.ContainsKey(key) && _tagHoldersList[key] != null && _tagHoldersList[key].Contains(card))
		{
			_tagHoldersList[key].Remove(card);
		}
	}

	private List<AIVirtualCard> ClassifyCardToHolderList(AIVirtualCard card, List<AIVirtualCard> holderList)
	{
		if (holderList == null)
		{
			holderList = new List<AIVirtualCard>();
		}
		if (!holderList.Contains(card))
		{
			holderList.Add(card);
		}
		return holderList;
	}

	public bool HasOtherAttackDamage()
	{
		if (OtherAttackDamageHolders != null)
		{
			return OtherAttackDamageHolders.Count > 0;
		}
		return false;
	}

	public List<AIVirtualCard> GetTagHolders(TagHolderReferenceType type)
	{
		return _tagHoldersList[type];
	}

	public void ReplaceInplayCard(bool isAlly, AIVirtualCard replace_source, AIVirtualCard replace_target)
	{
		if (isAlly)
		{
			ReplaceAllyInplayCard(replace_source, replace_target);
		}
		else
		{
			ReplaceEnemyInplayCard(replace_source, replace_target);
		}
		if (AllReferableCards.Contains(replace_target))
		{
			AllReferableCards.Insert(AllReferableCards.IndexOf(replace_target), replace_source);
			AllReferableCards.Remove(replace_target);
		}
		else
		{
			AIConsoleUtility.LogError("ReplaceInplayCard: Target card is not exist in AllReferableCards");
		}
		if (BothClassAndInplayCards.Contains(replace_target))
		{
			BothClassAndInplayCards.Insert(BothClassAndInplayCards.IndexOf(replace_target), replace_source);
			BothClassAndInplayCards.Remove(replace_target);
		}
		else
		{
			AIConsoleUtility.LogError("ReplaceInplayCard: Target card is not exist in BothClassAndInplayCards");
		}
		if (BothInplayCards.Contains(replace_target))
		{
			BothInplayCards.Insert(BothInplayCards.IndexOf(replace_target), replace_source);
			BothInplayCards.Remove(replace_target);
		}
		else
		{
			AIConsoleUtility.LogError("ReplaceInplayCard: Target card is not exist in BothInplayCards");
		}
		RemoveTagClassification(replace_target);
		TagClassification(replace_source);
	}

	private void ReplaceAllyInplayCard(AIVirtualCard replace_source, AIVirtualCard replace_target)
	{
		if (AllyClassAndInplayCards.Contains(replace_target) && AllAllyCards.Contains(replace_target))
		{
			AllyClassAndInplayCards.Insert(AllyClassAndInplayCards.IndexOf(replace_target), replace_source);
			AllyClassAndInplayCards.Remove(replace_target);
			AllAllyCards.Insert(AllAllyCards.IndexOf(replace_target), replace_source);
			AllAllyCards.Remove(replace_target);
		}
		else
		{
			AIConsoleUtility.LogError("ReplaceAllyInplayCard: Target card is not exist in AllAllyCards or AllyClassAndInplayCards");
		}
	}

	private void ReplaceEnemyInplayCard(AIVirtualCard replace_source, AIVirtualCard replace_target)
	{
		if (EnemyClassAndInplayCards.Contains(replace_target))
		{
			EnemyClassAndInplayCards.Insert(EnemyClassAndInplayCards.IndexOf(replace_target), replace_source);
			EnemyClassAndInplayCards.Remove(replace_target);
		}
		else
		{
			AIConsoleUtility.LogError("ReplaceEnemyInplayCard: Target card is not exist in EnemyClassAndInplayCards");
		}
	}

	public void RemoveTagClassification(AIVirtualCard card)
	{
		List<TagCollectionWithTypeBase> tagDictionary = card.TagCollectionContainer.TagDictionary;
		if (tagDictionary == null || tagDictionary.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < tagDictionary.Count; i++)
		{
			TagCollection collection = tagDictionary[i].Collection;
			for (int j = 0; j < collection.TagList.Count; j++)
			{
				AIPlayTag tag = collection.TagList[j];
				TagHolderReferenceType key = ConvertTagTypeToTagHolderReferenceType(tag, card);
				if (!_tagHoldersList.ContainsKey(key) || _tagHoldersList[key] == null)
				{
					return;
				}
				if (_tagHoldersList[key].Contains(card))
				{
					_tagHoldersList[key].Remove(card);
				}
			}
		}
	}

	public void RemoveAllyHandCard(AIVirtualCard handCard)
	{
		AllReferableCards.Remove(handCard);
		AllAllyCards.Remove(handCard);
		RemoveTagClassification(handCard);
	}

	public void ReplaceAllyHandCard(AIVirtualCard replace_source, AIVirtualCard replace_target)
	{
		if (!replace_target.IsAlly)
		{
			AIConsoleUtility.LogError("ReplaceAllyHandCard: Target card is not AllyCards");
		}
		else if (AllReferableCards.Contains(replace_target) && AllAllyCards.Contains(replace_target))
		{
			AllReferableCards.Insert(AllReferableCards.IndexOf(replace_target), replace_source);
			AllReferableCards.Remove(replace_target);
			AllAllyCards.Insert(AllAllyCards.IndexOf(replace_target), replace_source);
			AllAllyCards.Remove(replace_target);
			RemoveTagClassification(replace_target);
			TagClassification(replace_source);
		}
		else
		{
			AIConsoleUtility.LogError("ReplaceAllyHandCard: Target card is not exist in AllyCards");
		}
	}
}
