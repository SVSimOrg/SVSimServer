using System.Collections.Generic;

namespace Wizard;

public static class AIVirtualFusionSimulator
{
	public static void Fusion(AIVirtualTargetSelectAction situation, AIVirtualField field)
	{
		if (situation.ActionType != AIOperationType.FUSION)
		{
			AIConsoleUtility.LogError("AIVirtualFusionSimulator:Fusion() error!! situation is not [FUSION] ActionType!!!!!");
			return;
		}
		if (!situation.IsTargetExists(AIScriptTokenArgType.TARGET_SELECT))
		{
			AIConsoleUtility.LogError("AIVirtualFusionSimulator:Fusion() error!! cannot find fusion ingredients!!!!!");
			return;
		}
		FusionMoveCard(situation, field);
		AIVirtualCard actor = situation.Actor;
		if (actor.TagCollectionContainer.HasTag(AIPlayTagType.FusionMetamorphose))
		{
			actor.TagCollectionContainer.FusionMetamorphoseTags.ExecuteMetamorphose(actor, field, field.BestPlayPtn, situation);
		}
	}

	private static void FusionMoveCard(AIVirtualTargetSelectAction situation, AIVirtualField field)
	{
		List<AIVirtualCard> targets = situation.GetSituationTarget(AIScriptTokenArgType.TARGET_SELECT).Targets;
		bool isAlly = situation.Actor.IsAlly;
		for (int i = 0; i < targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = targets[i];
			if (isAlly)
			{
				field.RemoveAllyHandCard(aIVirtualCard);
			}
			else
			{
				field.RemoveEnemyHandCard(aIVirtualCard);
			}
			situation.Actor.FusionIngredients.AddFusionIngredient(aIVirtualCard, field.CurrentTurnCount);
		}
	}
}
