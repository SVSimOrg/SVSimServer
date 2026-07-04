using System.Collections.Generic;

namespace Wizard;

public static class AICountdownChangeUtility
{
	public static float EvalChantCountChange(AIVirtualCard actCard, int amount, List<int> playPtn, bool isSelect, AISituationInfo situation)
	{
		if (GetMemberChantFieldNum(actCard.SelfField, playPtn) <= 0)
		{
			return 0f;
		}
		float num = 0f;
		List<AIVirtualCard> allyClassAndInplayCards = actCard.SelfField.CardListSet.AllyClassAndInplayCards;
		List<AIVirtualCard> list = new List<AIVirtualCard>();
		for (int i = 0; i < allyClassAndInplayCards.Count; i++)
		{
			AIVirtualCard aIVirtualCard = allyClassAndInplayCards[i];
			if ((aIVirtualCard.CardIndex != actCard.CardIndex || aIVirtualCard.IsPlayer != actCard.IsPlayer) && aIVirtualCard.IsCountdownAmulet)
			{
				list.Add(aIVirtualCard);
			}
		}
		if (playPtn != null && playPtn.Count > 0)
		{
			for (int j = 0; j < playPtn.Count; j++)
			{
				AIVirtualCard aIVirtualCard2 = actCard.SelfField.AllyHandCards[playPtn[j]];
				if ((aIVirtualCard2.CardIndex != actCard.CardIndex || aIVirtualCard2.IsPlayer != actCard.IsPlayer) && aIVirtualCard2.IsCountdownAmulet)
				{
					list.Add(aIVirtualCard2);
				}
			}
		}
		if (isSelect)
		{
			int bestCandidateIndex = 0;
			return CalcMaxSelectChantCountChangeBonus(amount, list, playPtn, ref bestCandidateIndex, situation);
		}
		return CalcAllChantCountChangeBonus(amount, list, playPtn, situation);
	}

	public static float CalcMaxSelectChantCountChangeBonus(int amount, List<AIVirtualCard> targets, List<int> playPtn, ref int bestCandidateIndex, AISituationInfo situation)
	{
		float num = 0f;
		int count = targets.Count;
		for (int i = 0; i < count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			float num2 = aIVirtualCard.EvaluateValueOnField(playPtn, situation, useStyle: false, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true);
			AIVirtualCard aIVirtualCard2 = new AIVirtualCard(aIVirtualCard, aIVirtualCard.SelfField);
			int chantCount = aIVirtualCard2.ChantCount;
			chantCount = ((chantCount > amount) ? (chantCount - amount) : 0);
			float num3 = 0f;
			if (chantCount > 0)
			{
				aIVirtualCard2.ChantCount = chantCount;
				num3 = aIVirtualCard2.EvaluateValueOnField(playPtn, situation, useStyle: false, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true);
			}
			else
			{
				num3 = aIVirtualCard.EvaluateBreakValue(playPtn, useIgnoreBreak: true);
			}
			float num4 = num3 - num2;
			if (num4 > num)
			{
				num = num4;
				bestCandidateIndex = i;
			}
		}
		return num;
	}

	public static float CalcAllChantCountChangeBonus(int amount, List<AIVirtualCard> targets, List<int> playPtn, AISituationInfo situation)
	{
		float num = 0f;
		int count = targets.Count;
		for (int i = 0; i < count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			float num2 = aIVirtualCard.EvaluateValueOnField(playPtn, situation, useStyle: false, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true);
			AIVirtualCard aIVirtualCard2 = new AIVirtualCard(aIVirtualCard, aIVirtualCard.SelfField);
			int chantCount = aIVirtualCard2.ChantCount;
			chantCount = ((chantCount > amount) ? (chantCount - amount) : 0);
			float num3 = 0f;
			if (chantCount > 0)
			{
				aIVirtualCard2.ChantCount = chantCount;
				num3 = aIVirtualCard2.EvaluateValueOnField(playPtn, situation, useStyle: false, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true);
			}
			else
			{
				num3 = aIVirtualCard.EvaluateBreakValue(playPtn, useIgnoreBreak: true);
			}
			num += num3 - num2;
		}
		return num;
	}

	public static int GetMemberChantFieldNum(AIVirtualField field, List<int> handPtn)
	{
		int num = 0;
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			if (field.AllyInplayCards[i].IsCountdownAmulet)
			{
				num++;
			}
		}
		if (handPtn != null)
		{
			for (int j = 0; j < handPtn.Count; j++)
			{
				if (field.AllyHandCards[handPtn[j]].IsCountdownAmulet)
				{
					num++;
				}
			}
		}
		return num;
	}
}
