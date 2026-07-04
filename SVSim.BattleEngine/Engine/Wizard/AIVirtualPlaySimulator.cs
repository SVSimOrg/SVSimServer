namespace Wizard;

public static class AIVirtualPlaySimulator
{
	public static void PlayCard(AIVirtualTargetSelectAction situation, AIVirtualField field, PlaySimulationInfo playInfo)
	{
		if (situation.ActionType != AIOperationType.PLAY)
		{
			AIConsoleUtility.LogError("AIVirtualPlaySimulator:PlayCard() error!! situation is not [PLAY] ActionType!!!!!");
			return;
		}
		if (playInfo == null)
		{
			AIConsoleUtility.LogError($"AIVirtualPlaySimulator:PlayCard() error!! PlayInfo is null!\nMaybe field does not have enough cost!!!!! PlayCardName:{situation.Actor.CardName} PlayCardId:{situation.Actor.BaseId}");
			return;
		}
		AIVirtualCard originalCard = situation.OriginalCard;
		int useCost = playInfo.UseCost;
		if (playInfo.Type == PlaySimulationType.ChoiceTransform)
		{
			AIPlayCardSimulationUtility.SetChoiceTargetAsActor(situation);
		}
		originalCard.PlayedCost = useCost;
		if (originalCard.IsAlly)
		{
			field.AllyPp -= useCost;
			field.UsedPpCount += useCost;
		}
		else
		{
			field.EnemyPp -= useCost;
		}
		bool isChoiceBrave = situation.IsChoiceBrave;
		AIVirtualCard summonedCard = null;
		if (!isChoiceBrave)
		{
			SimulatePlayCardNormal(situation, field, out summonedCard);
		}
		AIPreprocessSimulationUtility.SimulatePreprocess(situation.Actor, situation, field, AIScriptTokenArgType.WHEN_PLAY, isPseudo: false);
		AIPlayCardSimulationUtility.CreateWhenPlayTagExecutingQueue(situation, field, playInfo.Type);
		situation.ProcessCollection.CombinePreprocessToProcessQueue();
		if (summonedCard != null)
		{
			AISummonTokenUtility.ExecuteSummonTags(field, summonedCard, field.BestPlayPtn, situation);
			AIGetOnSimulationUtility.GetOnAtField(field, summonedCard, situation);
			field.AllActivateCountHolderIncrement(situation, AIPlayTagType.SummonActivateCount, summonedCard);
		}
		if (!isChoiceBrave)
		{
			field.AllActivateCountHolderIncrement(situation, AIPlayTagType.PlayActivateCount, originalCard);
		}
		situation.ExecuteAllSkillProcess();
	}

	private static void SimulatePlayCardNormal(AISituationInfo play, AIVirtualField field, out AIVirtualCard summonedCard)
	{
		AIVirtualCard originalCard = play.OriginalCard;
		if (originalCard.IsAlly)
		{
			field.RemoveAllyHandCard(originalCard, isRemoveByPlay: true);
		}
		else
		{
			field.RemoveEnemyHandCard(originalCard);
		}
		AIVirtualCard actor = play.Actor;
		field.PlayedCardContainer.AddPlayedCard(actor);
		if (actor.IsUnit || actor.IsAmulet)
		{
			if (actor.IsAlly)
			{
				actor.NormalPlay();
				field.AllyInplayCards.Add(actor);
				field.CardListSet.AddAllyInplayCard(actor);
				field.SummonedCardContainer.AddSummonedCard(actor);
			}
			else
			{
				AIVirtualCard aIVirtualCard;
				if (actor is EnemyHandVirtualCard)
				{
					aIVirtualCard = new AIVirtualCard(actor.BaseCard, field);
					aIVirtualCard.InitializeTags(field.ParamQuery, actor.TagCollectionContainer.AttachedTags, actor.TagCollectionContainer.RemovedTagCollection);
				}
				else
				{
					aIVirtualCard = actor;
				}
				aIVirtualCard.NormalPlay();
				field.EnemyInplayCards.Add(aIVirtualCard);
				field.CardListSet.AddEnemyInplayCard(aIVirtualCard);
				field.SummonedCardContainer.AddSummonedCard(actor);
				play.SetActor(aIVirtualCard);
			}
			summonedCard = actor;
		}
		else
		{
			AISpellboostSimulationUtility.SpellboostWhenPlaySpell(actor, field);
			summonedCard = null;
		}
	}
}
