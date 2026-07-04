using System.Collections.Generic;

namespace Wizard;

public static class AIPlayoutAttackerCountUtility
{
	public static int GetPlayoutAttackerCount(AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, List<AIScriptTokenBase> filters, AISituationInfo situation)
	{
		int num = 0;
		for (int i = 0; i < field.AllyInplayCards.Count; i++)
		{
			if (IsPlayoutAttacker(field.AllyInplayCards[i], field, filters, playPtn, tagOwner, situation))
			{
				num++;
			}
		}
		AISinglePlayptnRecord playptnRecordOnSim = field.GetPlayptnRecordOnSim(playPtn);
		for (int j = 0; j < playPtn.Count; j++)
		{
			AIVirtualCard aIVirtualCard = field.AllyHandCards[playPtn[j]];
			if (aIVirtualCard == null)
			{
				continue;
			}
			if (IsPlayoutAttacker(aIVirtualCard, field, filters, playPtn, tagOwner, situation))
			{
				num++;
			}
			AIVirtualCard actor = aIVirtualCard;
			if (playptnRecordOnSim != null)
			{
				PlayedCardInfo playedCardInfo = playptnRecordOnSim.PlayedCardList[j];
				actor = ((playedCardInfo.TransformCard != null) ? playedCardInfo.TransformCard : aIVirtualCard);
			}
			AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(actor, aIVirtualCard, AIOperationType.PLAY, (AISelectedTargetInfoSet)null);
			aIVirtualTargetSelectAction.forceLethalMode = true;
			List<AITokenInformation> allySideTokenIdsOfPlaySituation = AIPlayTokenSimulationUtility.GetAllySideTokenIdsOfPlaySituation(field, playPtn, aIVirtualTargetSelectAction);
			if (allySideTokenIdsOfPlaySituation == null || allySideTokenIdsOfPlaySituation.Count <= 0)
			{
				continue;
			}
			for (int k = 0; k < allySideTokenIdsOfPlaySituation.Count; k++)
			{
				if (IsPlayoutAttacker(field.AI.tokenManager.GetTokenFromId(allySideTokenIdsOfPlaySituation[k].TokenId, isAlly: true, field), field, filters, playPtn, tagOwner, situation))
				{
					num++;
				}
			}
			if (num >= 6)
			{
				break;
			}
		}
		return num;
	}

	private static bool IsPlayoutAttacker(AIVirtualCard card, AIVirtualField field, List<AIScriptTokenBase> filters, List<int> playPtn, AIVirtualCard tagOwner, AISituationInfo situation)
	{
		if (!card.IsAlly || !card.IsOnField)
		{
			return false;
		}
		if (!card.IsUnit || card.IsDead || !AIAttackSimulationUtility.IsAttackPossible(field, card.AttackLeaderSituation))
		{
			return false;
		}
		if (!AIFilteringUtility.CheckMatchTargetFiltering(card, field.AllyInplayCards, filters, playPtn, tagOwner, situation))
		{
			return false;
		}
		return true;
	}
}
