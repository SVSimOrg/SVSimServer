using System.Collections.Generic;

namespace Wizard;

public class AIHandPlayEstimator
{
	public List<AIVirtualCard> InplayEstimations { get; private set; }

	public AIHandPlayEstimator(AIParamQuery paramQuery, int handIndex, AIVirtualCard handOriginal, BattlePlayerPair sourcePair, EnemyAI ai)
	{
		BattleCardBase baseCard = handOriginal.BaseCard;
		int num = ai.CurrentVirtualField.AllyInplayCards.Count;
		if (baseCard.IsUnit || baseCard.IsField)
		{
			num++;
		}
		List<int> playPtn = new List<int> { handOriginal.SelfField.AllyHandCards.FindIndex((AIVirtualCard c) => c.CardIndex == handOriginal.CardIndex) };
		AISinglePlayptnRecord playptnRecord = ai.PlayPtnRecorder.FindMatchedPlayPtnRecord(playPtn, ai.CurrentVirtualField);
		AIVirtualCard aIVirtualCard = handOriginal.FindRealActor(playptnRecord);
		if (!aIVirtualCard.IsSameCard(handOriginal))
		{
			ai.tokenManager.AddTokenFromCard(aIVirtualCard);
		}
		AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(aIVirtualCard, handOriginal, AIOperationType.PLAY);
		if (aIVirtualCard.BaseCard.Movable())
		{
			List<AITokenInformation> allySideTokenIdsOfPlaySituation = AIPlayTokenSimulationUtility.GetAllySideTokenIdsOfPlaySituation(ai.CurrentVirtualField, new List<int> { handOriginal.SelfField.AllyHandCards.FindIndex((AIVirtualCard c) => c.CardIndex == handOriginal.CardIndex) }, situation);
			if (allySideTokenIdsOfPlaySituation == null || allySideTokenIdsOfPlaySituation.Count <= 0)
			{
				return;
			}
			for (int num2 = 0; num2 < allySideTokenIdsOfPlaySituation.Count; num2++)
			{
				AIVirtualCard tokenFromId = ai.tokenManager.GetTokenFromId(allySideTokenIdsOfPlaySituation[num2].TokenId, isAlly: true, ai.CurrentVirtualField, needsClone: true);
				if (InplayEstimations == null)
				{
					InplayEstimations = new List<AIVirtualCard>();
				}
				InplayEstimations.Add(tokenFromId);
				num++;
				if (num >= 6)
				{
					break;
				}
			}
		}
		else
		{
			InplayEstimations = null;
		}
	}
}
