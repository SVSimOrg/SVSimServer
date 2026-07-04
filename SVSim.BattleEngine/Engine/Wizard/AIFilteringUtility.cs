using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public static class AIFilteringUtility
{
	public struct FilteringParameter
	{
		public AIScriptTokenArgType Type;

		public bool IsNot;

		public bool IsSkipNextToken;

		public int FilteringThreshold;

		public string SubParameter;
	}

	public static bool CheckFilterPassOrNot(AIVirtualCard card, FilteringParameter filter, AIVirtualCard owner, List<int> playPtn, AISituationInfo situation)
	{
		bool flag = true;
		AIScriptTokenArgType type = filter.Type;
		int filteringThreshold = filter.FilteringThreshold;
		bool isNot = filter.IsNot;
		switch (type)
		{
		case AIScriptTokenArgType.NONE:
		case AIScriptTokenArgType.ALL:
		case AIScriptTokenArgType.BOTH:
		case AIScriptTokenArgType.ALL_SELECT:
			flag = true;
			break;
		case AIScriptTokenArgType.TARGET_ID:
			flag = card.BaseId == filteringThreshold;
			break;
		case AIScriptTokenArgType.SELF:
			flag = card.IsAlly == owner.IsAlly && card.CardIndex == owner.CardIndex;
			break;
		case AIScriptTokenArgType.OTHER_FOLLOWER:
			flag = card.IsFollower(playPtn) && (card.IsAlly != owner.IsAlly || card.CardIndex != owner.CardIndex);
			break;
		case AIScriptTokenArgType.CLASH_TARGET:
			flag = situation is AIVirtualAttackInfo { AttackTarget: not null } aIVirtualAttackInfo && ((owner.IsAlly != aIVirtualAttackInfo.Actor.IsAlly) ? (owner.IsAlly == aIVirtualAttackInfo.AttackTarget.IsAlly && card.IsSameCard(aIVirtualAttackInfo.Actor)) : card.IsSameCard(aIVirtualAttackInfo.AttackTarget));
			break;
		case AIScriptTokenArgType.BANISHED_TARGET:
			flag = IsBanishedTarget(situation, card);
			break;
		case AIScriptTokenArgType.GETOFF_CARD:
			flag = owner.GetOffCard != null && owner.GetOffCard.IsSameCard(card);
			break;
		case AIScriptTokenArgType.ATTACKER:
			flag = situation != null && situation.ActionType == AIOperationType.ATTACK && situation.Actor != null && card.IsSameCard(situation.Actor);
			break;
		case AIScriptTokenArgType.EVOLVER:
			flag = IsEvolver(card, situation);
			break;
		case AIScriptTokenArgType.PLAYED_CARD:
			flag = situation != null && situation.ActionType == AIOperationType.PLAY && situation.Actor != null && card.IsSameCard(situation.Actor);
			break;
		case AIScriptTokenArgType.CHOICED_TARGET:
			flag = IsChoicedTarget(situation, card);
			break;
		case AIScriptTokenArgType.SELECTED_TARGET:
			flag = IsSelectedTarget(situation, card, isFirst: true);
			break;
		case AIScriptTokenArgType.SECOND_SELECTED_TARGET:
			flag = IsSelectedTarget(situation, card, isFirst: false);
			break;
		case AIScriptTokenArgType.REVERSE_TARGET:
			flag = !IsContainSelectedAllTarget(situation, card);
			break;
		case AIScriptTokenArgType.TRIGGER:
			flag = IsTriggerdCard(situation, card);
			break;
		case AIScriptTokenArgType.CANDIDATE:
			flag = situation.CurrentCheckCard != null && situation.CurrentCheckCard.IsSameCard(card);
			break;
		case AIScriptTokenArgType.ALLY:
			flag = card.IsAlly == owner.IsAlly;
			break;
		case AIScriptTokenArgType.OPPONENT:
			flag = card.IsAlly != owner.IsAlly;
			break;
		case AIScriptTokenArgType.CLASS:
			flag = card.IsLeader;
			break;
		case AIScriptTokenArgType.ALLY_CLASS:
			flag = card.IsLeader && card.IsAlly == owner.IsAlly;
			break;
		case AIScriptTokenArgType.ENEMY_CLASS:
			flag = card.IsLeader && card.IsAlly != owner.IsAlly;
			break;
		case AIScriptTokenArgType.FOLLOWER:
			flag = card.IsFollower(playPtn);
			break;
		case AIScriptTokenArgType.FOLLOWER_CARD_TYPE:
			flag = card.IsUnit;
			break;
		case AIScriptTokenArgType.DAMAGED_FOLLOWER:
			flag = card.IsUnit && card.MaxLife - card.Life > 0;
			break;
		case AIScriptTokenArgType.NO_DAMAGED_FOLLOWER:
			flag = card.IsFollower(playPtn) && card.MaxLife == card.Life;
			break;
		case AIScriptTokenArgType.EVOLVED:
			flag = card.IsEvolution;
			break;
		case AIScriptTokenArgType.ATTACKABLE:
			flag = card.IsAttackable(playPtn);
			break;
		case AIScriptTokenArgType.CANT_ATTACK:
			flag = card.IsCantAttackAll;
			break;
		case AIScriptTokenArgType.BUFFED_FOLLOWER:
			flag = card.IsFollower(playPtn) && (card.Attack > card.BaseCard.BaseAtk || card.MaxLife > card.BaseCard.BaseMaxLife);
			break;
		case AIScriptTokenArgType.BUFFED_FOLLOWER_CARD_TYPE:
			flag = card.IsUnit && (card.Attack > card.BaseCard.BaseAtk || card.MaxLife > card.BaseCard.BaseMaxLife);
			break;
		case AIScriptTokenArgType.PREVIOUS_TURN_ATTACKED:
			flag = card.IsPreviousTurnAttacked;
			break;
		case AIScriptTokenArgType.ATTACKED:
			flag = card.IsAttacked;
			break;
		case AIScriptTokenArgType.CONSUME_EP_ZERO:
			flag = card.IsNotConsumeEp;
			break;
		case AIScriptTokenArgType.SELECTED_TARGET_ID:
			flag = IsSameSelectedTargetID(card, situation);
			break;
		case AIScriptTokenArgType.AMULET:
			flag = card.IsAmulet || card.IsCountdownAmulet || card.IsCrystalize(card.SelfField, playPtn, situation);
			break;
		case AIScriptTokenArgType.ALLY_AMULET:
			flag = (card.IsAmulet || card.IsCountdownAmulet || card.IsCrystalize(card.SelfField, playPtn, situation)) && card.IsAlly == owner.IsAlly;
			break;
		case AIScriptTokenArgType.CHANT_FIELD:
			flag = card.IsCountdownAmulet || card.IsCrystalize(card.SelfField, playPtn, situation);
			break;
		case AIScriptTokenArgType.NOT_COUNTDOWN_AMULET:
			flag = card.IsAmulet && !card.IsCountdownAmulet;
			break;
		case AIScriptTokenArgType.CRYSTALIZE:
			flag = card.IsCrystalize(card.SelfField, playPtn, situation) || card.SelfField.PlayedCardContainer.IsPlayedAsCrystalize(card);
			break;
		case AIScriptTokenArgType.SPELL:
			flag = card.IsSpell || card.IsAccelerated(card.SelfField, playPtn, situation);
			break;
		case AIScriptTokenArgType.SPELL_CARD_TYPE:
			flag = card.IsSpell;
			break;
		case AIScriptTokenArgType.ACCELERATE:
			flag = card.IsAccelerated(card.SelfField, playPtn, situation) || card.SelfField.PlayedCardContainer.IsPlayedAsAccelerate(card);
			break;
		case AIScriptTokenArgType.BASE_COST_INF:
			flag = card.BaseCost > filteringThreshold;
			break;
		case AIScriptTokenArgType.BASE_COST_SUP:
			flag = card.BaseCost < filteringThreshold;
			break;
		case AIScriptTokenArgType.BASE_COST_EQL:
			flag = card.BaseCost == filteringThreshold;
			break;
		case AIScriptTokenArgType.ATK_INF:
			flag = card.Attack > filteringThreshold;
			break;
		case AIScriptTokenArgType.ATK_SUP:
			flag = card.Attack < filteringThreshold;
			break;
		case AIScriptTokenArgType.ATK_EQL:
			flag = card.Attack == filteringThreshold;
			break;
		case AIScriptTokenArgType.LIFE_INF:
			flag = card.Life > filteringThreshold;
			break;
		case AIScriptTokenArgType.LIFE_SUP:
			flag = card.Life < filteringThreshold;
			break;
		case AIScriptTokenArgType.LIFE_EQL:
			flag = card.Life == filteringThreshold;
			break;
		case AIScriptTokenArgType.COUNTDOWN_EQL:
			flag = card.IsCountdownAmulet && card.ChantCount == filteringThreshold;
			break;
		case AIScriptTokenArgType.PLAY_COUNT_EQL:
		{
			AIVirtualField selfField = card.SelfField;
			int num4 = 0;
			for (int num5 = 0; num5 < playPtn.Count; num5++)
			{
				if (selfField.AllyHandCards[playPtn[num5]].IsSameCard(card))
				{
					num4 = num5 + 1;
					break;
				}
			}
			flag = card.SelfField.PlayedCardContainer.GetAllPlayedCountInTurn(card.IsAlly) + num4 == filteringThreshold;
			break;
		}
		case AIScriptTokenArgType.NEUTRAL:
			flag = card.Clan == CardBasePrm.ClanType.ALL;
			break;
		case AIScriptTokenArgType.ELF:
			flag = card.Clan == CardBasePrm.ClanType.MIN;
			break;
		case AIScriptTokenArgType.ROYAL:
			flag = card.Clan == CardBasePrm.ClanType.ROYAL;
			break;
		case AIScriptTokenArgType.WITCH:
			flag = card.Clan == CardBasePrm.ClanType.WITCH;
			break;
		case AIScriptTokenArgType.DRAGON:
			flag = card.Clan == CardBasePrm.ClanType.DRAGON;
			break;
		case AIScriptTokenArgType.NECROMANCER:
			flag = card.Clan == CardBasePrm.ClanType.NECRO;
			break;
		case AIScriptTokenArgType.VAMPIRE:
			flag = card.Clan == CardBasePrm.ClanType.VAMPIRE;
			break;
		case AIScriptTokenArgType.BISHOP:
			flag = card.Clan == CardBasePrm.ClanType.BISHOP;
			break;
		case AIScriptTokenArgType.NEMESIS:
			flag = card.Clan == CardBasePrm.ClanType.NEMESIS;
			break;
		case AIScriptTokenArgType.LEGION:
			flag = card.IsTribe(CardBasePrm.TribeType.LEGION);
			break;
		case AIScriptTokenArgType.LORD:
			flag = card.IsTribe(CardBasePrm.TribeType.LORD);
			break;
		case AIScriptTokenArgType.WHITE_RITUAL:
			flag = card.IsTribe(CardBasePrm.TribeType.WHITE_RITUAL);
			break;
		case AIScriptTokenArgType.ARTIFACT:
			flag = card.IsTribe(CardBasePrm.TribeType.ARTIFACT);
			break;
		case AIScriptTokenArgType.MACHINE:
			flag = card.IsTribe(CardBasePrm.TribeType.MACHINE);
			break;
		case AIScriptTokenArgType.MANARIA:
			flag = card.IsTribe(CardBasePrm.TribeType.MANARIA);
			break;
		case AIScriptTokenArgType.FOOD:
			flag = card.IsTribe(CardBasePrm.TribeType.FOOD);
			break;
		case AIScriptTokenArgType.LEVIN:
			flag = card.IsTribe(CardBasePrm.TribeType.LEVIN);
			break;
		case AIScriptTokenArgType.NATURE:
			flag = card.IsTribe(CardBasePrm.TribeType.NATURE);
			break;
		case AIScriptTokenArgType.LOOT:
			flag = card.IsTribe(CardBasePrm.TribeType.LOOTING);
			break;
		case AIScriptTokenArgType.BANQUET:
			flag = card.IsTribe(CardBasePrm.TribeType.BANQUET);
			break;
		case AIScriptTokenArgType.HERO:
			flag = card.IsTribe(CardBasePrm.TribeType.HERO);
			break;
		case AIScriptTokenArgType.ARMED:
			flag = card.IsTribe(CardBasePrm.TribeType.ARMED);
			break;
		case AIScriptTokenArgType.HELLBOUND:
			flag = card.IsTribe(CardBasePrm.TribeType.HELLBOUND);
			break;
		case AIScriptTokenArgType.SCHOOL:
			flag = card.IsTribe(CardBasePrm.TribeType.SCHOOL);
			break;
		case AIScriptTokenArgType.CHESS:
			flag = card.IsTribe(CardBasePrm.TribeType.CHESS);
			break;
		case AIScriptTokenArgType.ANY_TRIBE:
			flag = card.HasAnyTribe();
			break;
		case AIScriptTokenArgType.ALL_SPELLBOOST:
			flag = card.HasSpellboost;
			break;
		case AIScriptTokenArgType.KILLER:
		case AIScriptTokenArgType.SNEAK:
		case AIScriptTokenArgType.QUICK:
		case AIScriptTokenArgType.RUSH:
		case AIScriptTokenArgType.DRAIN:
		case AIScriptTokenArgType.GUARD:
		case AIScriptTokenArgType.UNTOUCHABLE:
		case AIScriptTokenArgType.FORCE_TARGETING:
		case AIScriptTokenArgType.UNBANISHABLE:
		case AIScriptTokenArgType.IGNORE_GUARD:
			flag = AISkillSimulationUtility.HasSkill(card, type, card.SelfField.AI, playPtn, isNot);
			break;
		case AIScriptTokenArgType.LASTWORD:
			flag = card.IsLastword;
			break;
		case AIScriptTokenArgType.MEDUSA:
			flag = card.IsDestroyWhenAttack;
			break;
		case AIScriptTokenArgType.NOT_BE_ATTACKED:
			flag = card.IsSkillCantUnderAnyAttack;
			break;
		case AIScriptTokenArgType.NO_SKILL:
			flag = !card.HasAnySkill;
			break;
		case AIScriptTokenArgType.UNION_BURST:
			flag = card.BaseCard.HasUnionBurst;
			break;
		case AIScriptTokenArgType.CRYSTALIZE_HOLDER:
			flag = card.TagCollectionContainer.HasTag(AIPlayTagType.Crystalize);
			break;
		case AIScriptTokenArgType.DESTRUCTIBLE:
			flag = !card.IsIndestructible && !card.IsIndependent && !card.IsDestroyByBanish;
			break;
		case AIScriptTokenArgType.IN_HAND:
			flag = card.IsInHand;
			break;
		case AIScriptTokenArgType.IN_PLAY:
			flag = card.IsOnField;
			break;
		case AIScriptTokenArgType.LAST_DRAW_CARD:
		{
			int num3 = card.SelfField.AllyHandCards.Count - 1;
			flag = card.SelfField.AllyHandCards.FindIndex((AIVirtualCard c) => c.IsSameCard(card)) == num3;
			break;
		}
		case AIScriptTokenArgType.NEWER:
			if (owner.IsOnField && card.IsOnField)
			{
				int num = owner.SelfField.AllyInplayCards.FindIndex((AIVirtualCard c) => c.IsSameCard(card));
				int num2 = owner.SelfField.AllyInplayCards.FindIndex((AIVirtualCard c) => c.IsSameCard(owner));
				flag = num > num2;
			}
			break;
		case AIScriptTokenArgType.OLDEST_FOLLOWER:
		{
			if (!card.IsOnField)
			{
				break;
			}
			List<AIVirtualCard> list2 = (card.IsAlly ? card.SelfField.AllyInplayCards : card.SelfField.EnemyInplayCards);
			flag = false;
			for (int j = 0; j < list2.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = list2[j];
				if (aIVirtualCard2.IsUnit)
				{
					flag = aIVirtualCard2.IsSameCard(card);
					break;
				}
			}
			break;
		}
		case AIScriptTokenArgType.OTHER_OLDEST_HAND_CARD_TYPE:
		{
			flag = false;
			List<AIVirtualCard> list = (owner.IsAlly ? owner.SelfField.AllyHandCards : owner.SelfField.GetEnemyHandCardList());
			if (list == null || list.Count <= 0)
			{
				break;
			}
			for (int i = 0; i < list.Count; i++)
			{
				AIVirtualCard aIVirtualCard = list[i];
				if (!aIVirtualCard.IsSameCard(owner))
				{
					flag = aIVirtualCard.IsSameCardType(card);
					break;
				}
			}
			break;
		}
		case AIScriptTokenArgType.SUMMON_FOLLOWER:
			flag = situation.IsOwnSummonedCard(card, AIScriptTokenArgType.FOLLOWER);
			break;
		case AIScriptTokenArgType.SUMMON_AMULET:
			flag = situation.IsOwnSummonedCard(card, AIScriptTokenArgType.AMULET);
			break;
		case AIScriptTokenArgType.LATEST_SUMMON_CARD:
			flag = situation.IsOwnSummonedCard(card, AIScriptTokenArgType.ALL, isLatest: true);
			break;
		case AIScriptTokenArgType.LATEST_DRAW_CARD:
			flag = situation.IsLatestOwnDrewCard(card);
			break;
		case AIScriptTokenArgType.ROMELIA_TARGET:
			if (owner.BaseId != 121441020)
			{
				AIConsoleUtility.LogError("ROMELIA_TARGET filtering error!! owner.BaseId = " + owner.BaseId);
				flag = false;
			}
			else
			{
				flag = situation.ActionType == AIOperationType.ATTACK && situation.IsLatestTarget(card);
			}
			break;
		case AIScriptTokenArgType.FIRST_TURN:
			flag = card.IsFirstTurn && card.IsUnit;
			break;
		case AIScriptTokenArgType.MIN_ATTACK:
		case AIScriptTokenArgType.MAX_ATTACK:
			flag = card.IsFollower(playPtn) && card.Attack == filteringThreshold;
			break;
		case AIScriptTokenArgType.MIN_COST:
		case AIScriptTokenArgType.MAX_COST:
			flag = card.Cost == filteringThreshold;
			break;
		case AIScriptTokenArgType.FIELD:
			flag = card.IsAmulet || card.IsCountdownAmulet;
			break;
		case AIScriptTokenArgType.EVOLVED_FOLLOWER:
			flag = card.IsUnit && card.IsEvolution;
			break;
		case AIScriptTokenArgType.COST_INF:
			flag = card.Cost > filteringThreshold;
			break;
		case AIScriptTokenArgType.COST_SUP:
			flag = card.Cost < filteringThreshold;
			break;
		case AIScriptTokenArgType.COST_EQL:
			flag = card.Cost == filteringThreshold;
			break;
		case AIScriptTokenArgType.REAL_SKILL_TARGET:
			flag = situation.IsRealSkillTarget(card, owner);
			break;
		case AIScriptTokenArgType.AI_TRIBE:
			flag = card.CheckAITribe(card.SelfField, playPtn, situation, filter.SubParameter);
			break;
		case AIScriptTokenArgType.ENHANCED:
		{
			List<AIVirtualCard> obj = (card.IsAlly ? card.SelfField.AllyGameEnhancePlayCards : card.SelfField.EnemyGameEnhancePlayCards);
			flag = false;
			foreach (AIVirtualCard item in obj)
			{
				if (item.IsSameCard(card))
				{
					flag = true;
					break;
				}
			}
			break;
		}
		case AIScriptTokenArgType.WHEN_PLAY_DAMAGE:
			flag = IsHoldingWhenPlayDamageTag(card);
			break;
		case AIScriptTokenArgType.WHEN_PLAY_DESTROY:
			flag = IsHoldingWhenPlayDestroyTag(card);
			break;
		case AIScriptTokenArgType.FIRST_SUMMON_FOLLOWER_IN_PLAYPTN:
		{
			AISinglePlayptnRecord playptnRecordOnSim = card.SelfField.GetPlayptnRecordOnSim(playPtn);
			flag = playptnRecordOnSim != null && playptnRecordOnSim.FirstSummonedAllyFollower != null && playptnRecordOnSim.FirstSummonedAllyFollower.IsSameCard(card);
			break;
		}
		default:
			return false;
		}
		return flag ^ isNot;
	}

	public static List<AIVirtualCard> MultipleFiltering<T>(List<T> targets, List<AIScriptTokenBase> argList, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDeadCard = true) where T : AIVirtualCard
	{
		if (targets == null || targets.Count <= 0)
		{
			return null;
		}
		List<AIVirtualCard> list = new List<AIVirtualCard>(targets);
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (aIVirtualCard.IsDead && isBlockDeadCard)
			{
				list.Remove(aIVirtualCard);
				continue;
			}
			bool flag = true;
			for (int j = 0; j < argList.Count; j++)
			{
				AIScriptTokenBase aIScriptTokenBase = argList[j];
				if (aIScriptTokenBase is AIScriptCalculationToken)
				{
					AIScriptCalculationToken calculation = aIScriptTokenBase as AIScriptCalculationToken;
					if (!FilteringCalculationToken(aIVirtualCard, tagOwner, calculation, playPtn, situation))
					{
						flag = false;
						break;
					}
					continue;
				}
				AIScriptTokenBase subToken = ((j + 1 < argList.Count) ? argList[j + 1] : null);
				FilteringParameter filter = CreateFilteringParameter(tagOwner, aIVirtualCard, playPtn, situation, aIScriptTokenBase, subToken, list);
				if (!CheckFilterPassOrNot(aIVirtualCard, filter, tagOwner, playPtn, situation))
				{
					flag = false;
					break;
				}
				if (filter.IsSkipNextToken)
				{
					j++;
				}
			}
			if (!flag)
			{
				list.Remove(aIVirtualCard);
			}
		}
		return list;
	}

	public static bool CheckMatchTargetFiltering(AIVirtualCard targetCard, List<AIVirtualCard> candidates, List<AIScriptTokenBase> filters, List<int> playPtn, AIVirtualCard tagOwner, AISituationInfo situation)
	{
		for (int i = 0; i < filters.Count; i++)
		{
			AIScriptTokenBase aIScriptTokenBase = filters[i];
			if (aIScriptTokenBase is AIScriptCalculationToken)
			{
				AIScriptCalculationToken calculation = aIScriptTokenBase as AIScriptCalculationToken;
				if (!FilteringCalculationToken(targetCard, tagOwner, calculation, playPtn, situation))
				{
					return false;
				}
				continue;
			}
			AIScriptTokenBase subToken = ((i + 1 < filters.Count) ? filters[i + 1] : null);
			FilteringParameter filter = CreateFilteringParameter(tagOwner, targetCard, playPtn, situation, aIScriptTokenBase, subToken, candidates);
			if (!CheckFilterPassOrNot(targetCard, filter, tagOwner, playPtn, situation))
			{
				return false;
			}
			if (filter.IsSkipNextToken)
			{
				i++;
			}
		}
		return true;
	}

	public static bool FilteringCalculationToken(AIVirtualCard candidate, AIVirtualCard owner, AIScriptCalculationToken calculation, List<int> playPtn, AISituationInfo situation)
	{
		return calculation.Expression(owner, candidate, owner.SelfField, playPtn, situation) > 0f;
	}

	private static FilteringParameter CreateFilteringParameter(AIVirtualCard tagOwner, AIVirtualCard currentCandidate, List<int> playPtn, AISituationInfo situation, AIScriptTokenBase token, AIScriptTokenBase subToken, List<AIVirtualCard> candidates)
	{
		AIScriptTokenArgType aIScriptTokenArgType = AIScriptTokenArgType.NONE;
		bool isNot = false;
		int filteringThreshold = -1;
		bool isSkipNextToken = false;
		string subParameter = null;
		if (token.Type == AIScriptTokenType.ID)
		{
			aIScriptTokenArgType = AIScriptTokenArgType.TARGET_ID;
			AIScriptIDToken obj = token as AIScriptIDToken;
			isNot = obj.IsNot;
			filteringThreshold = obj.ID;
		}
		else if (token.Type == AIScriptTokenType.ARG)
		{
			AIScriptArgumentToken aIScriptArgumentToken = token as AIScriptArgumentToken;
			aIScriptTokenArgType = aIScriptArgumentToken.ArgumentType;
			isNot = aIScriptArgumentToken.IsNot;
			isSkipNextToken = IsInfSupArgType(aIScriptTokenArgType);
			filteringThreshold = GetThresholdOfFilterArg(tagOwner, currentCandidate, playPtn, situation, aIScriptTokenArgType, subToken, candidates, isSkipNextToken);
			if (aIScriptTokenArgType == AIScriptTokenArgType.AI_TRIBE)
			{
				subParameter = ((AIScriptTextToken)aIScriptArgumentToken).Text;
			}
		}
		return new FilteringParameter
		{
			Type = aIScriptTokenArgType,
			IsNot = isNot,
			FilteringThreshold = filteringThreshold,
			SubParameter = subParameter,
			IsSkipNextToken = isSkipNextToken
		};
	}

	private static int GetThresholdOfFilterArg(AIVirtualCard tagOwner, AIVirtualCard currentCandidate, List<int> playPtn, AISituationInfo situation, AIScriptTokenArgType argType, AIScriptTokenBase next, List<AIVirtualCard> candidates, bool isSkipNextToken)
	{
		if (isSkipNextToken)
		{
			if (next != null)
			{
				AIVirtualField selfField = tagOwner.SelfField;
				if (next is AIScriptNumericToken)
				{
					return (int)next.Value;
				}
				if (next is AIScriptVariableToken)
				{
					AIScriptTokenBase aIScriptTokenBase = AIScriptExpressionCalculator.CalculateVariableToken(next, playPtn, tagOwner.SelfField, tagOwner, situation);
					if (aIScriptTokenBase != null)
					{
						return (int)aIScriptTokenBase.Value;
					}
				}
				else if (next is AIScriptCalculationToken aIScriptCalculationToken)
				{
					return (int)aIScriptCalculationToken.Expression(tagOwner, currentCandidate, selfField, playPtn, situation);
				}
			}
			return -1;
		}
		switch (argType)
		{
		case AIScriptTokenArgType.MAX_ATTACK:
			if (candidates != null && candidates.Count > 0)
			{
				return candidates.Max((AIVirtualCard c) => c.Attack);
			}
			return 0;
		case AIScriptTokenArgType.MIN_ATTACK:
			if (candidates != null && candidates.Count > 0)
			{
				return candidates.Min((AIVirtualCard c) => c.Attack);
			}
			return 0;
		case AIScriptTokenArgType.MIN_COST:
			if (candidates != null && candidates.Count > 0)
			{
				return candidates.Min((AIVirtualCard c) => c.Cost);
			}
			return -1;
		case AIScriptTokenArgType.MAX_COST:
			if (candidates != null && candidates.Count > 0)
			{
				return candidates.Max((AIVirtualCard c) => c.Cost);
			}
			return -1;
		default:
			return -1;
		}
	}

	private static bool IsContainSelectedAllTarget(AISituationInfo situation, AIVirtualCard card)
	{
		if (situation == null || situation.ActionTarget == null)
		{
			return false;
		}
		for (int i = 0; i < AISelectedTargetInfoSet.LENGTH; i++)
		{
			AISelectedTargetInfo aISelectedTargetInfo = situation.SelectedTargets.Get(i);
			if (aISelectedTargetInfo == null)
			{
				break;
			}
			if (aISelectedTargetInfo.ContainsTarget(card))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsSelectedTarget(AISituationInfo situation, AIVirtualCard card, bool isFirst)
	{
		if (situation == null || card == null || situation.ActionTarget == null)
		{
			return false;
		}
		AISelectedTargetInfo aISelectedTargetInfo = (isFirst ? situation.SelectedTargets.Get(0) : situation.SelectedTargets.Get(1));
		if (aISelectedTargetInfo != null && aISelectedTargetInfo.ContainsTarget(card))
		{
			return true;
		}
		return false;
	}

	private static bool IsChoicedTarget(AISituationInfo situation, AIVirtualCard card)
	{
		if (situation == null || card == null)
		{
			return false;
		}
		AISelectedTargetInfo choiceTarget = situation.GetChoiceTarget();
		if (choiceTarget != null && choiceTarget.ContainsTarget(card))
		{
			return true;
		}
		return false;
	}

	private static bool IsBanishedTarget(AISituationInfo situation, AIVirtualCard card)
	{
		return situation.IsSameCurrentTriggerCardAndTriggerType(card, AISituationTriggerInformation.TriggerType.Banish);
	}

	private static bool IsTriggerdCard(AISituationInfo situation, AIVirtualCard targetCard)
	{
		return situation.IsSameCurrentTriggerCard(targetCard);
	}

	private static bool IsSameSelectedTargetID(AIVirtualCard target, AISituationInfo situation)
	{
		if (situation == null || situation.SelectedTargets == null)
		{
			return false;
		}
		AISelectedTargetInfo aISelectedTargetInfo = situation.SelectedTargets.Get(0);
		if (aISelectedTargetInfo == null || !aISelectedTargetInfo.HasTarget)
		{
			return false;
		}
		List<AIVirtualCard> targets = aISelectedTargetInfo.Targets;
		for (int i = 0; i < targets.Count; i++)
		{
			if (targets[i].BaseId == target.BaseId)
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsEvolver(AIVirtualCard card, AISituationInfo situation)
	{
		if (situation == null || situation.Actor == null)
		{
			return false;
		}
		if (situation.ActionType == AIOperationType.EVOLVE && card.IsSameCard(situation.Actor))
		{
			return true;
		}
		if (situation.IsSameCurrentTriggerCardAndTriggerType(card, AISituationTriggerInformation.TriggerType.Evolver))
		{
			return true;
		}
		return false;
	}

	public static int GetCardNameCountFromList(List<AIVirtualCard> cardList, List<AIScriptTokenBase> filters, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation = null)
	{
		if (cardList == null || cardList.Count <= 0)
		{
			return 0;
		}
		List<AIVirtualCard> list = MultipleFiltering(cardList, filters, tagOwner, playPtn, situation, isBlockDeadCard: false);
		if (list == null)
		{
			return 0;
		}
		HashSet<int> hashSet = new HashSet<int>();
		for (int i = 0; i < list.Count; i++)
		{
			hashSet.Add(list[i].BaseId);
		}
		return hashSet.Count;
	}

	public static List<AIVirtualCard> FilteringForStatusEffectiveAbility(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation, bool isAttackEffective, bool isBlockDead)
	{
		List<AIVirtualCard> list = MultipleFiltering(candidates, filters, tagOwner, playPtn, situation, isBlockDead);
		if (list != null)
		{
			list.RemoveAll((AIVirtualCard c) => c.IsAmulet);
			if (isAttackEffective)
			{
				list.RemoveAll((AIVirtualCard c) => c.IsLeader);
			}
		}
		return list;
	}

	public static List<AIVirtualCard> FilteringForFollowerOnly(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation, bool isBlockDead)
	{
		List<AIVirtualCard> list = MultipleFiltering(candidates, filters, tagOwner, playPtn, situation, isBlockDead);
		list?.RemoveAll((AIVirtualCard c) => !c.IsUnit);
		return list;
	}

	public static List<AIVirtualCard> FilteringForCountdownAmuletOnly(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation, bool isBlockDead)
	{
		List<AIVirtualCard> list = MultipleFiltering(candidates, filters, tagOwner, playPtn, situation, isBlockDead);
		list?.RemoveAll((AIVirtualCard c) => !c.IsCountdownAmulet);
		return list;
	}

	public static List<AIVirtualCard> FilteringForSpellboost(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<AIScriptTokenBase> filters, List<int> playPtn, AISituationInfo situation)
	{
		List<AIVirtualCard> list = null;
		for (int i = 0; i < candidates.Count; i++)
		{
			AIVirtualCard aIVirtualCard = candidates[i];
			if (aIVirtualCard.HasSpellboost)
			{
				list = AIParamQuery.AddElementToList(aIVirtualCard, list);
			}
		}
		if (list == null || list.Count <= 0)
		{
			return AIGlobalEmptyList.EmptyVirtualCardList;
		}
		return MultipleFiltering(list, filters, tagOwner, playPtn, situation);
	}

	public static List<AIVirtualCard> FilteringForWhiteRitualOnly(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead)
	{
		if (candidates != null && candidates.Count > 0)
		{
			candidates.RemoveAll((AIVirtualCard c) => c.IsAlly != tagOwner.IsAlly || !c.IsTribe(AIScriptTokenArgType.WHITE_RITUAL) || !c.IsStackWhiteRitual || c.IsDead);
		}
		return candidates;
	}

	private static bool IsHoldingWhenPlayDamageTag(AIVirtualCard card)
	{
		AIPlayTagType[] typeArray = new AIPlayTagType[2]
		{
			AIPlayTagType.FanfareDamage,
			AIPlayTagType.PlayDamage
		};
		return card.TagCollectionContainer.HasAnyTag(typeArray);
	}

	private static bool IsHoldingWhenPlayDestroyTag(AIVirtualCard card)
	{
		AIPlayTagType[] typeArray = new AIPlayTagType[2]
		{
			AIPlayTagType.FanfareDestroy,
			AIPlayTagType.PlayDestroy
		};
		return card.TagCollectionContainer.HasAnyTag(typeArray);
	}

	private static bool IsInfSupArgType(AIScriptTokenArgType type)
	{
		return type > (AIScriptTokenArgType)1000;
	}
}
