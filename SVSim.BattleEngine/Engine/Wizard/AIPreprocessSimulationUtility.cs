using System.Collections.Generic;

namespace Wizard;

public static class AIPreprocessSimulationUtility
{
	public static void SimulatePreprocess(AIVirtualCard activator, AISituationInfo situation, AIVirtualField field, AIScriptTokenArgType timing, bool isPseudo)
	{
		ExecuteEarthRite(activator, situation, field, timing, isPseudo);
		ExecuteNecromance(activator, situation, field, timing, isPseudo);
		ExecuteBurialRite(activator, situation, field, timing, isPseudo);
	}

	public static void ResetPreprocess(AISituationInfo situation, AIVirtualField field)
	{
		AISimulationPreprocessRecorder preprocessRecorder = situation.PreprocessRecorder;
		if (preprocessRecorder.HasRecord)
		{
			for (int i = 0; i < preprocessRecorder.RecordList.Count; i++)
			{
				ResetBySingleRecord(preprocessRecorder.RecordList[i], field);
			}
			preprocessRecorder.RemoveAll();
		}
	}

	public static AIScriptTokenArgType ConvertAIOperationTypeToTiming(AIOperationType operationType)
	{
		return operationType switch
		{
			AIOperationType.PLAY => AIScriptTokenArgType.WHEN_PLAY, 
			AIOperationType.EVOLVE => AIScriptTokenArgType.WHEN_EVO, 
			AIOperationType.FUSION => AIScriptTokenArgType.WHEN_FUSION, 
			_ => AIScriptTokenArgType.NONE, 
		};
	}

	private static void ExecuteEarthRite(AIVirtualCard activator, AISituationInfo situation, AIVirtualField field, AIScriptTokenArgType timing, bool isPseudo)
	{
		int earthRiteCount = activator.GetEarthRiteCount(field, situation, timing);
		if (earthRiteCount > 0)
		{
			AISinglePreprocessRecord record = new AISinglePreprocessRecord(activator, situation.OriginalCard, timing);
			field.ExecuteEarthRite(activator.IsAlly, earthRiteCount, situation, isPseudo, record);
			situation.PreprocessRecorder.AddRecord(record);
		}
	}

	private static void ExecuteNecromance(AIVirtualCard activator, AISituationInfo situation, AIVirtualField field, AIScriptTokenArgType timing, bool isPseudo)
	{
		int necromanceCountOrDefault = activator.GetNecromanceCountOrDefault(field, situation, timing);
		if (necromanceCountOrDefault > 0)
		{
			AISinglePreprocessRecord record = new AISinglePreprocessRecord(activator, situation.OriginalCard, timing)
			{
				NecromanceCount = necromanceCountOrDefault
			};
			SimulateNecromanceOnField(activator, situation, field, necromanceCountOrDefault, isPseudo);
			situation.PreprocessRecorder.AddRecord(record);
		}
	}

	private static void SimulateNecromanceOnField(AIVirtualCard activator, AISituationInfo situation, AIVirtualField field, int necromanceCount, bool isPseudo)
	{
		field.VirtualCemetery.ExecuteNecromance(necromanceCount, activator.IsAlly, isPseudo);
		if (!isPseudo)
		{
			ExecuteAllWhenNecromanceTags(field, situation);
			if (activator.IsAlly)
			{
				field.AllyNecromancedCountInGame += necromanceCount;
			}
			else
			{
				field.EnemyNecromancedCountInGame += necromanceCount;
			}
		}
		field.AllActivateCountHolderIncrement(situation, AIPlayTagType.NecromanceActivateCount, activator);
	}

	private static void ExecuteBurialRite(AIVirtualCard activator, AISituationInfo situation, AIVirtualField field, AIScriptTokenArgType timing, bool isPseudo)
	{
		if (!AIBurialRite.CheckValidTimingType(timing))
		{
			return;
		}
		if (isPseudo)
		{
			int burialRiteCount = activator.GetBurialRiteCount(field, situation, EnemyAI.EmptyPlayPtn, timing);
			AISinglePreprocessRecord record = new AISinglePreprocessRecord(activator, situation.OriginalCard, timing)
			{
				BurialRiteCount = burialRiteCount
			};
			situation.PreprocessRecorder.AddRecord(record);
			return;
		}
		AISelectedTargetInfo burialRiteTarget = situation.GetBurialRiteTarget();
		if (burialRiteTarget != null)
		{
			AISinglePreprocessRecord record2 = new AISinglePreprocessRecord(activator, situation.OriginalCard, timing)
			{
				BurialRiteCount = burialRiteTarget.Targets.Count
			};
			situation.PreprocessRecorder.AddRecord(record2);
			AIBurialRiteSimulationUtility.ExecuteBurialRite(field, situation, burialRiteTarget);
		}
	}

	public static void ExecuteRecordingToPlayedCardInfo(PlayedCardInfo playedCardInfo, AIVirtualCard activator, AIVirtualField field, AISituationInfo situation, bool isPseudo)
	{
		AISimulationPreprocessRecorder preprocessRecorder = playedCardInfo.PreprocessRecorder;
		if (preprocessRecorder != null && preprocessRecorder.HasRecord)
		{
			for (int i = 0; i < preprocessRecorder.RecordList.Count; i++)
			{
				ExecuteBySingleRecord(preprocessRecorder.RecordList[i], activator, field, situation, isPseudo);
			}
			if (playedCardInfo.HasPreDecidedSelectTargets)
			{
				situation.SelectedTargets = playedCardInfo.PreDecidedSelectTargets;
			}
		}
	}

	private static void ExecuteBySingleRecord(AISinglePreprocessRecord record, AIVirtualCard activator, AIVirtualField field, AISituationInfo situation, bool isPseudo)
	{
		if (record.EarthRiteCount > 0)
		{
			field.ExecuteEarthRite(activator.IsAlly, record.EarthRiteCount, situation, isPseudo, record);
			situation.PreprocessRecorder.AddRecord(record);
		}
		if (record.NecromanceCount > 0)
		{
			SimulateNecromanceOnField(activator, situation, field, record.NecromanceCount, isPseudo);
			situation.PreprocessRecorder.AddRecord(record);
		}
		if (record.BurialRiteCount <= 0)
		{
			return;
		}
		if (isPseudo)
		{
			situation.PreprocessRecorder.AddRecord(record);
			return;
		}
		AISelectedTargetInfo situationTarget = situation.GetSituationTarget(AIScriptTokenArgType.TARGET_SELECT);
		if (situationTarget != null && situationTarget.Type == TargetSelectType.BurialRite)
		{
			AIBurialRiteSimulationUtility.ExecuteBurialRite(field, situation, situationTarget);
			situation.PreprocessRecorder.AddRecord(record);
		}
	}

	private static void ResetBySingleRecord(AISinglePreprocessRecord record, AIVirtualField field)
	{
		if (record.EarthRiteCount > 0)
		{
			record.RestoreAllEarthRiteCount(field);
		}
		if (record.NecromanceCount > 0)
		{
			field.VirtualCemetery.ResetNecromance(record.RealActor.IsAlly, record.NecromanceCount);
		}
	}

	private static void ExecuteAllWhenNecromanceTags(AIVirtualField field, AISituationInfo situation)
	{
		if (field.CardListSet.HasWhenNecromanceHolder)
		{
			List<AIVirtualCard> whenNecromanceTagHolders = field.CardListSet.WhenNecromanceTagHolders;
			List<int> bestPlayPtn = field.BestPlayPtn;
			for (int i = 0; i < whenNecromanceTagHolders.Count; i++)
			{
				AIVirtualCard aIVirtualCard = whenNecromanceTagHolders[i];
				aIVirtualCard.TagCollectionContainer.NecromanceTags.Execute(aIVirtualCard, field, bestPlayPtn, situation);
			}
		}
	}
}
