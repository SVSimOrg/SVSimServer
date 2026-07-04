using System.Collections.Generic;

namespace Wizard;

public static class AIVirtualEvolutionSimulator
{
	private static int EVOLUTION_ACTION_LENGTH_BONUS = 3;

	public static void AutoEvolve(AIVirtualCard evolver, AIVirtualField field, AISituationInfo situation)
	{
		evolver.EvolveStatusUp();
		List<int> bestPlayPtn = field.BestPlayPtn;
		evolver.EnqueueGiveSkill(field, bestPlayPtn, situation);
		field.ExecuteWhenChangeInplayTags(bestPlayPtn, situation);
		field.ApplyOtherEvolveTags(situation, evolver);
		if (evolver.TagCollectionContainer.HasTagCollection(TagCollectionType.EvolvedResident))
		{
			evolver.TagCollectionContainer.EvolvedResidentTags.Execute(evolver, situation);
		}
		IncrementEvolutionCount(field, evolver);
		if (evolver.IsNotAttackYet)
		{
			field.EvoBonus += AIEvaluateBonusFromOhterUtility.GetAllOtherEvoBonus(situation, bestPlayPtn);
		}
	}

	public static void ManualEvolve(AIVirtualActionInfo situation, AIVirtualField field)
	{
		AIVirtualCard actor = situation.Actor;
		List<int> bestPlayPtn = field.BestPlayPtn;
		actor.PreparateOtherToEvolve(field, bestPlayPtn, situation);
		AIPreprocessSimulationUtility.SimulatePreprocess(actor, situation, field, AIScriptTokenArgType.WHEN_EVO, isPseudo: false);
		if (actor.IsAlly)
		{
			field.EpValue = field.StyleQuery.GetEpValue(situation, bestPlayPtn);
			field.EvoHandPlus = actor.GetEvoHandPlusCount(field, bestPlayPtn, situation);
			if (!actor.IsNotConsumeEp)
			{
				field.AllyEvolutionCount--;
				field.UsedEpCount++;
			}
		}
		actor.EvolveStatusUp();
		actor.SelfField.IsLeftTurnEvol = false;
		actor.EnqueueGiveSkill(field, bestPlayPtn, situation);
		if (actor.IsAlly && actor.IsNotAttackYet)
		{
			field.EvoBonus += actor.GetEvoBonus(field.BestPlayPtn, situation);
			field.EvoBonus += AIEvaluateBonusFromOhterUtility.GetAllOtherEvoBonus(situation, field.BestPlayPtn);
		}
		field.ExecuteWhenChangeInplayTags(bestPlayPtn, situation);
		if (actor.TagCollectionContainer.HasTagCollection(TagCollectionType.WhenEvo))
		{
			actor.TagCollectionContainer.EvoTags.Execute(field, actor, actor, situation);
		}
		situation.ProcessCollection.CombinePreprocessToProcessQueue();
		field.ApplyOtherEvolveTags(situation, actor);
		if (actor.TagCollectionContainer.HasTagCollection(TagCollectionType.EvolvedResident))
		{
			actor.TagCollectionContainer.EvolvedResidentTags.Execute(actor, situation);
		}
		field.AllActivateCountHolderIncrement(situation, AIPlayTagType.EvoActivateCount, actor);
		situation.ExecuteAllSkillProcess();
		IncrementEvolutionCount(field, actor);
		if (actor.IsAlly)
		{
			field.ActionLength += EVOLUTION_ACTION_LENGTH_BONUS;
		}
	}

	private static void IncrementEvolutionCount(AIVirtualField field, AIVirtualCard evolver)
	{
		if (evolver.IsAlly)
		{
			field.AllyEvolvedCountInGame++;
		}
		else
		{
			field.EnemyEvolvedCountInGame++;
		}
	}
}
