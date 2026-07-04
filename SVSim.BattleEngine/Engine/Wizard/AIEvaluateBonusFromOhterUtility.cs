using System.Collections.Generic;

namespace Wizard;

public static class AIEvaluateBonusFromOhterUtility
{
	private static float USE_MIN_BONUS_DEFAULT_VALUE = 10000f;

	public static float GetAllyPlayBonus(AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation)
	{
		if (targetCard == null)
		{
			return 0f;
		}
		float num = 0f;
		List<AIVirtualCard> allAllyCards = targetCard.SelfField.CardListSet.AllAllyCards;
		float currentUseMinValue = USE_MIN_BONUS_DEFAULT_VALUE;
		for (int i = 0; i < allAllyCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = allAllyCards[i];
			if (!aIVirtualCard.IsDead && !targetCard.IsSameCard(aIVirtualCard))
			{
				num += aIVirtualCard.GetAllyPlayBonus(targetCard, playPtn, situation, ref currentUseMinValue);
				num += aIVirtualCard.GetOtherSummonBonus(targetCard, playPtn, situation);
			}
		}
		num += targetCard.SelfField.StyleQuery.GetAllyPlayBonus(targetCard, playPtn, situation, ref currentUseMinValue);
		if (EnemyAI.IsLargerThan(USE_MIN_BONUS_DEFAULT_VALUE, currentUseMinValue))
		{
			num += currentUseMinValue;
		}
		return num;
	}

	private static float GetOtherSummonBonus(this AIVirtualCard tagOwner, AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation)
	{
		if (!tagOwner.TagCollectionContainer.HasTag(AIPlayTagType.OtherSummonQuick))
		{
			return 0f;
		}
		return tagOwner.TagCollectionContainer.OtherSummonTags.GetOtherSummonBonus(targetCard, tagOwner, tagOwner.SelfField, playPtn, situation);
	}

	public static float GetEnemyPlayBonus(AIVirtualCard targetCard, List<int> playPtn, AISituationInfo situation)
	{
		if (targetCard == null)
		{
			return 0f;
		}
		float num = 0f;
		List<AIVirtualCard> enemyClassAndInplayCards = targetCard.SelfField.CardListSet.EnemyClassAndInplayCards;
		for (int i = 0; i < enemyClassAndInplayCards.Count; i++)
		{
			AIVirtualCard tagOwner = enemyClassAndInplayCards[i];
			num += tagOwner.GetEnemyPlayBonus(targetCard, playPtn, situation);
		}
		return num;
	}

	public static float GetPlayPtnBonus(AIVirtualField field, List<int> playPtn)
	{
		float num = 0f;
		for (int i = 0; i < field.CardListSet.AllyClassAndInplayCards.Count; i++)
		{
			AIVirtualCard tagOwner = field.CardListSet.AllyClassAndInplayCards[i];
			num += tagOwner.GetPlayPtnBonus(playPtn);
		}
		return num;
	}

	public static int GetTotalRecoverPp(AIVirtualCard playCard, List<int> playPtn, AISituationInfo situation)
	{
		int num = 0;
		List<AIVirtualCard> allyHandCards = playCard.SelfField.AllyHandCards;
		for (int i = 0; i < playPtn.Count; i++)
		{
			AIVirtualCard aIVirtualCard = allyHandCards[playPtn[i]];
			if (aIVirtualCard.IsSameCard(playCard))
			{
				num += aIVirtualCard.GerWhenPlayRecoverPp(playCard, playPtn, situation);
				break;
			}
			num += aIVirtualCard.GetOtherPlayRecoverPp(playCard, playCard.SelfField, playPtn, situation);
		}
		allyHandCards = playCard.SelfField.CardListSet.AllyClassAndInplayCards;
		for (int j = 0; j < allyHandCards.Count; j++)
		{
			AIVirtualCard aIVirtualCard2 = allyHandCards[j];
			if (!aIVirtualCard2.IsDead)
			{
				num += aIVirtualCard2.GetOtherPlayRecoverPp(playCard, playCard.SelfField, playPtn, situation);
			}
		}
		return num;
	}

	public static float GetOtherBattleBonus(AIVirtualCard targetCard, List<int> playPtn)
	{
		float num = 0f;
		AIVirtualField selfField = targetCard.SelfField;
		for (int i = 0; i < selfField.CardListSet.BothClassAndInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = selfField.CardListSet.BothClassAndInplayCards[i];
			if (!aIVirtualCard.IsDead)
			{
				num += aIVirtualCard.GetOtherBattleBonusFromOneCard(targetCard, playPtn);
			}
		}
		List<AIVirtualCard> playptnCards = selfField.GetPlayptnCards(playPtn);
		if (playptnCards != null && playptnCards.Count > 0)
		{
			for (int j = 0; j < playptnCards.Count; j++)
			{
				AIVirtualCard tagOwner = playptnCards[j];
				num += tagOwner.GetOtherBattleBonusFromOneCard(targetCard, playPtn);
			}
		}
		return num;
	}

	public static float GetOtherBattleBonusRate(AIVirtualCard targetCard, List<int> playPtn, bool useIgnoreInBattle, AISituationInfo situation)
	{
		float num = 1f;
		AIVirtualField selfField = targetCard.SelfField;
		for (int i = 0; i < selfField.CardListSet.BothClassAndInplayCards.Count; i++)
		{
			AIVirtualCard tagOwner = selfField.CardListSet.BothClassAndInplayCards[i];
			num *= tagOwner.GetOtherBattleBonusRateFromOneCard(targetCard, playPtn, useIgnoreInBattle, situation);
		}
		List<AIVirtualCard> playptnCards = selfField.GetPlayptnCards(playPtn);
		if (playptnCards != null && playptnCards.Count > 0)
		{
			for (int j = 0; j < playptnCards.Count; j++)
			{
				AIVirtualCard tagOwner2 = playptnCards[j];
				num *= tagOwner2.GetOtherBattleBonusRateFromOneCard(targetCard, playPtn, useIgnoreInBattle, situation);
			}
		}
		return num;
	}

	public static float GetAllOtherEvoBonus(AISituationInfo situation, List<int> playPtn)
	{
		return 0f + GetAllMemberEvoBonus(situation, playPtn) + GetAllEnemyEvoBonus(situation, playPtn);
	}

	private static float GetAllMemberEvoBonus(AISituationInfo situation, List<int> playPtn)
	{
		List<AIVirtualCard> list = (situation.Actor.IsAlly ? situation.Actor.SelfField.GetMemberCardList(playPtn) : situation.Actor.SelfField.CardListSet.EnemyClassAndInplayCards);
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard aIVirtualCard = list[i];
			if (!situation.Actor.IsSameCard(aIVirtualCard))
			{
				num += aIVirtualCard.GetMemberEvoBonus(situation, playPtn);
			}
		}
		return num;
	}

	private static float GetAllEnemyEvoBonus(AISituationInfo situation, List<int> playPtn)
	{
		List<AIVirtualCard> list = (situation.Actor.IsAlly ? situation.Actor.SelfField.CardListSet.EnemyClassAndInplayCards : situation.Actor.SelfField.GetMemberCardList(playPtn));
		float num = 0f;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualCard tagOwner = list[i];
			num += tagOwner.GetEnemyEvoBonus(situation, playPtn);
		}
		return num;
	}

	public static int GetPlayPlusCount(AIVirtualField field, List<int> playPtn)
	{
		int num = 0;
		for (int i = 0; i < playPtn.Count; i++)
		{
			int num2 = playPtn[i];
			if (num2 < field.AllyHandCards.Count)
			{
				AIVirtualCard tagOwner = field.AllyHandCards[num2];
				num += tagOwner.GetPlayPlusCount(playPtn);
			}
		}
		return num;
	}

	public static int GetOtherPlayoutDamageBonus(AIVirtualCard target, AIVirtualField field, List<int> playPtn)
	{
		int num = 0;
		if (field.CardListSet.HasAllyPlayoutDamageBonusHolder)
		{
			for (int i = 0; i < field.CardListSet.AllyPlayoutDamageBonusHolders.Count; i++)
			{
				AIVirtualCard aIVirtualCard = field.CardListSet.AllyPlayoutDamageBonusHolders[i];
				if (!aIVirtualCard.IsDead && !aIVirtualCard.IsSameCard(target))
				{
					num += aIVirtualCard.GetAllyPlayoutDamageBonus(target, field, playPtn);
				}
			}
		}
		return num;
	}

	public static float GetOtherBreakBonus(AIVirtualCard target, AIVirtualField field, List<int> playPtn, bool useIgnoreInBattle)
	{
		if (!field.CardListSet.HasOtherBreakBonusHolder)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < field.CardListSet.OtherBreakBonusHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.OtherBreakBonusHolders[i];
			if (!aIVirtualCard.IsDead && !aIVirtualCard.IsSameCard(target))
			{
				num += aIVirtualCard.GetOtherBreakBonus(target, field, playPtn, useIgnoreInBattle);
			}
		}
		return num;
	}

	public static float GetOtherBanishBonus(AIVirtualCard target, AIVirtualField field, List<int> playPtn, bool useIgnoreInBattle)
	{
		if (!field.CardListSet.HasOtherBanishBonusHolder)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < field.CardListSet.OtherBanishBonusHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.OtherBanishBonusHolders[i];
			if (!aIVirtualCard.IsDead && !aIVirtualCard.IsSameCard(target))
			{
				num += aIVirtualCard.GetOtherBanishBonus(target, field, playPtn, useIgnoreInBattle);
			}
		}
		return num;
	}

	public static float GetOtherLeaveBonus(AIVirtualCard target, AIVirtualField field, List<int> playPtn, bool useIgnoreInBattle)
	{
		if (!field.CardListSet.HasOtherLeaveBonusHolder)
		{
			return 0f;
		}
		float num = 0f;
		for (int i = 0; i < field.CardListSet.OtherLeaveBonusHolders.Count; i++)
		{
			AIVirtualCard aIVirtualCard = field.CardListSet.OtherLeaveBonusHolders[i];
			if (!aIVirtualCard.IsDead && !aIVirtualCard.IsSameCard(target))
			{
				num += aIVirtualCard.GetOtherLeaveBonus(target, field, playPtn, useIgnoreInBattle);
			}
		}
		return num;
	}
}
