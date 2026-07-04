using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class AIPlayOutChecker
{
	public static int CalculatePlayOutDamageProspected(AIParamQuery paramQuery, AIVirtualField field, AISinglePlayptnRecord playPtnRecord, EnemyAI ai)
	{
		List<int> list = ((playPtnRecord == null) ? EnemyAI.EmptyPlayPtn : playPtnRecord.PlayPtn);
		if (ai.StyleQuery.IsDisableLethalCheck(field, list))
		{
			return 0;
		}
		List<AIVirtualCard> allyHandCards = field.AllyHandCards;
		List<AIVirtualCard> list2 = field.EnemyInplayCards;
		AIVirtualFieldRollBackBasicProcessor aIVirtualFieldRollBackBasicProcessor = new AIVirtualFieldRollBackBasicProcessor(field);
		List<AIVirtualCard> guardFollowers = RemoveAllGuard(list2, field, list);
		if (field.IsEnemyHasGuard())
		{
			list2 = GetSurvivorOfPlayptnDamageToAll(paramQuery, allyHandCards, playPtnRecord, list2);
			if (list2.Any((AIVirtualCard c) => c.IsGuard))
			{
				ResetGuard(guardFollowers);
				return GetTotalPlayoutDamageBonus(field, list);
			}
		}
		if (field.EnemyClass.IsAllShield)
		{
			ResetGuard(guardFollowers);
			return 0;
		}
		int num = 0;
		int num2 = 0;
		for (int num3 = 1; num3 < field.CardListSet.AllyClassAndInplayCards.Count; num3++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.AllyClassAndInplayCards[num3];
			AIVirtualAttackInfo attackLeaderSituation = aIVirtualCard.AttackLeaderSituation;
			bool flag = AIAttackSimulationUtility.IsAttackPossible(field, attackLeaderSituation, list2);
			if (flag)
			{
				AIPreprocessSimulationUtility.SimulatePreprocess(aIVirtualCard, attackLeaderSituation, field, AIScriptTokenArgType.WHEN_ATTACK, isPseudo: true);
			}
			num += CalculateInplayAttack(attackLeaderSituation, field, list2, paramQuery);
			num += aIVirtualCard.GetAllPlayoutDamageBonus(field, list, attackLeaderSituation);
			if (field.AllyBattlePlayer.IsEvolve && aIVirtualCard.IsUnit)
			{
				int num4 = 0;
				if (flag)
				{
					num4 = field.EnemyClass.SimulateDamageAmount(aIVirtualCard.EvoAttackPlus);
				}
				if (num2 < num4)
				{
					num2 = num4;
				}
			}
		}
		for (int num5 = 0; num5 < list.Count; num5++)
		{
			int index = list[num5];
			AIVirtualCard aIVirtualCard2 = allyHandCards[index];
			PlayedCardInfo playedCardInfo = playPtnRecord?.PlayedCardList[num5];
			AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(aIVirtualCard2, aIVirtualCard2, AIOperationType.PLAY, (AISelectedTargetInfoSet)null);
			if (playedCardInfo == null)
			{
				AIPreprocessSimulationUtility.SimulatePreprocess(aIVirtualCard2, aIVirtualTargetSelectAction, field, AIScriptTokenArgType.WHEN_PLAY, isPseudo: true);
			}
			else
			{
				AIPreprocessSimulationUtility.ExecuteRecordingToPlayedCardInfo(playedCardInfo, aIVirtualCard2, field, aIVirtualTargetSelectAction, isPseudo: true);
			}
			int num6 = 0;
			if (!field.IsNoInstantAttack)
			{
				num6 = CalculatePlayoutAttackBonus(aIVirtualCard2, list, aIVirtualTargetSelectAction);
			}
			num += num6;
			int allPlayoutDamageBonus = aIVirtualCard2.GetAllPlayoutDamageBonus(field, list, aIVirtualTargetSelectAction);
			num += allPlayoutDamageBonus;
			if (field.AllyBattlePlayer.IsEvolve)
			{
				int num7 = 0;
				if (aIVirtualCard2.IsUnit && num6 > 0)
				{
					num7 += field.EnemyClass.SimulateDamageAmount(aIVirtualCard2.EvoAttackPlus);
				}
				if (num2 < num7)
				{
					num2 = num7;
				}
			}
			field.UpdateVirtualFieldWhenEvaluation(aIVirtualCard2, aIVirtualTargetSelectAction);
		}
		ResetGuard(guardFollowers);
		aIVirtualFieldRollBackBasicProcessor.ResetVirtualFieldToStart();
		for (int num8 = 0; num8 < field.AllyInplayCards.Count; num8++)
		{
			field.AllyInplayCards[num8].AttackLeaderSituation.PreprocessRecorder.RemoveAll();
		}
		num += num2;
		num += field.AllyClass.GetAllPlayoutDamageBonus(field, list, null);
		return num + GetEnemyTotalPlayoutDamageBonus(field, list, null);
	}

	private static int GetTotalPlayoutDamageBonus(AIVirtualField field, List<int> playPtn)
	{
		int num = 0;
		_ = field.AI.ALLY.IsEvolve;
		for (int i = 0; i < playPtn.Count; i++)
		{
			AIVirtualCard card = field.AllyHandCards[playPtn[i]];
			num += card.GetAllPlayoutDamageBonus(field, playPtn, null);
		}
		for (int j = 1; j < field.CardListSet.AllyClassAndInplayCards.Count; j++)
		{
			AIVirtualCard card2 = field.CardListSet.AllyClassAndInplayCards[j];
			num += card2.GetAllPlayoutDamageBonus(field, playPtn, null);
		}
		return num + GetEnemyTotalPlayoutDamageBonus(field, playPtn, null);
	}

	private static int GetEnemyTotalPlayoutDamageBonus(AIVirtualField field, List<int> playPtn, AISituationInfo situation)
	{
		int num = 0;
		for (int i = 0; i < field.CardListSet.EnemyClassAndInplayCards.Count; i++)
		{
			AIVirtualCard card = field.CardListSet.EnemyClassAndInplayCards[i];
			num += card.GetAllPlayoutDamageBonus(field, playPtn, situation);
		}
		return num;
	}

	private static List<AIVirtualCard> GetSurvivorOfPlayptnDamageToAll(AIParamQuery query, List<AIVirtualCard> handCards, AISinglePlayptnRecord playPtnRecord, List<AIVirtualCard> opponentInplayCards)
	{
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		AIVirtualField currentVirtualField = query.enemyAI.CurrentVirtualField;
		if (playPtnRecord == null)
		{
			return list;
		}
		List<LifeRecord> list2 = new List<LifeRecord>();
		for (int i = 0; i < opponentInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = opponentInplayCards[i];
			list2.Add(new LifeRecord
			{
				MaxLife = aIVirtualCard.MaxLife,
				CurrentLife = aIVirtualCard.Life
			});
		}
		List<int> playPtn = playPtnRecord.PlayPtn;
		for (int j = 0; j < playPtn.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = handCards[playPtn[j]];
			AIVirtualCard transformCard = playPtnRecord.PlayedCardList[j].TransformCard;
			AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction((transformCard != null) ? transformCard : aIVirtualCard2, aIVirtualCard2, AIOperationType.PLAY);
			AISimulationRemovalUtility.PredictSurvivorsLifeAfterCardPlay(aIVirtualCard2, opponentInplayCards, currentVirtualField, playPtn, situation, list2);
		}
		for (int k = 0; k < list2.Count; k++)
		{
			if (list2[k].CurrentLife > 0)
			{
				list.Add(opponentInplayCards[k]);
			}
		}
		return list;
	}

	private static List<AIVirtualCard> RemoveAllGuard(List<AIVirtualCard> inplayCards, AIVirtualField field, List<int> playPtn)
	{
		if (playPtn == null || playPtn.Count <= 0)
		{
			return null;
		}
		List<AIVirtualCard> list = null;
		List<AIVirtualCard> list2 = inplayCards.Where((AIVirtualCard c) => c.IsGuard).ToList();
		if (list2.Count <= 0)
		{
			return null;
		}
		for (int num = 0; num < playPtn.Count; num++)
		{
			AIVirtualCard aIVirtualCard = field.AllyHandCards[playPtn[num]];
			if (!aIVirtualCard.TagCollectionContainer.HasTagCollection(TagCollectionType.Fanfare))
			{
				continue;
			}
			FanfareTagCollection fanfareTags = aIVirtualCard.TagCollectionContainer.FanfareTags;
			if (!fanfareTags.HasRemoveKeywordSkillTags)
			{
				continue;
			}
			AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(aIVirtualCard, aIVirtualCard, AIOperationType.PLAY, (AISelectedTargetInfoSet)null);
			List<AIVirtualCard> list3 = fanfareTags.RemoveGuardPrediction(aIVirtualCard, list2, field, playPtn, situation);
			if (list3 != null)
			{
				list = AIParamQuery.AddRangeToList(list3, list);
				list2.RemoveAll((AIVirtualCard c) => !c.IsGuard);
				if (list2.Count <= 0)
				{
					break;
				}
			}
		}
		return list;
	}

	private static void ResetGuard(List<AIVirtualCard> guardFollowers)
	{
		if (guardFollowers != null && guardFollowers.Count > 0)
		{
			for (int i = 0; i < guardFollowers.Count; i++)
			{
				guardFollowers[i].IsGuard = true;
			}
		}
	}

	private static int CalculateInplayAttack(AIVirtualAttackInfo attackSituation, AIVirtualField field, List<AIVirtualCard> opponentInPlayCards, AIParamQuery paramQuery)
	{
		AIVirtualCard actor = attackSituation.Actor;
		if (!actor.IsUnit)
		{
			return 0;
		}
		if (!AIAttackSimulationUtility.IsAttackPossible(field, attackSituation, opponentInPlayCards))
		{
			return 0;
		}
		int attackDamageToCertainCard = AIAttackTagSimulator.GetAttackDamageToCertainCard(field, attackSituation, actor);
		AISimulationBuffInfo aISimulationBuffInfo = AIAttackTagSimulator.GetBuffInfoListWhenCertainAttack(field, attackSituation)?.GetBuffInfoToCertainCard(actor);
		int num = aISimulationBuffInfo?.TotalAttackBuff ?? 0;
		int num2 = aISimulationBuffInfo?.TotalLifeBuff ?? 0;
		int allyAttack = actor.Attack + num;
		int allyLife = actor.Life + num2 - attackDamageToCertainCard;
		return CalculateDamageToLeader(attackSituation, allyAttack, allyLife);
	}

	private static int CalculatePlayoutAttackBonus(AIVirtualCard attacker, List<int> playPtn, AISituationInfo playSituation)
	{
		AIVirtualAttackInfo attackLeaderSituation = attacker.AttackLeaderSituation;
		AISimulationBuffInfo aISimulationBuffInfo = AIAttackTagSimulator.GetBuffInfoListWhenCertainAttack(attacker.SelfField, attackLeaderSituation)?.GetBuffInfoToCertainCard(attacker);
		int num = aISimulationBuffInfo?.TotalAttackBuff ?? 0;
		int num2 = aISimulationBuffInfo?.TotalLifeBuff ?? 0;
		int allyAttack = attacker.GetPlayoutAttackBonus(playPtn, playSituation) + num;
		int attackDamageToCertainCard = AIAttackTagSimulator.GetAttackDamageToCertainCard(attacker.SelfField, attackLeaderSituation, attacker);
		int allyLife = attacker.Life + num2 - attackDamageToCertainCard;
		return CalculateDamageToLeader(attackLeaderSituation, allyAttack, allyLife);
	}

	private static int CalculateDamageToLeader(AIVirtualAttackInfo situation, int allyAttack, int allyLife)
	{
		if (allyAttack <= 0 || allyLife <= 0)
		{
			return 0;
		}
		if (AIAttackTagSimulator.GetAttackDamageToCertainCard(situation.Actor.SelfField, situation, situation.Actor) >= allyLife)
		{
			return 0;
		}
		return allyAttack;
	}
}
