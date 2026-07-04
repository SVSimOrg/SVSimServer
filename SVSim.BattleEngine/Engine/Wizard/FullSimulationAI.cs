using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;

namespace Wizard;

public class FullSimulationAI : IBattleSimulationAI
{
	private EnemyAI AI;

	private int bestMoveCount;

	private bool _isOverComplexity;

	private int _bestFirstDeadIndex;

	private int _originalFieldInplayCount;

	private ulong[] _defaultPlayptnHashArray;

	private List<AIVirtualActionInfo> _simulationAllMoves;

	public AIVirtualActionInfo Cr_FirstActionInfo { get; private set; }

	public AIVirtualField Cr_MaxField { get; private set; }

	public float Cr_MaxFieldValue { get; private set; }

	public float Cr_MaxFieldValueAtSelfTurnEnd { get; private set; }

	public List<AIVirtualActionInfo> Cr_MaxActionInfoSequence { get; private set; }

	public PlayPtnWithToken Cr_BestPlayPtnWithToken { get; private set; }

	public FullSimulationAI(EnemyAI ai, List<AIVirtualActionInfo> allMoves)
	{
		AI = ai;
		Cr_FirstActionInfo = null;
		bestMoveCount = int.MaxValue;
		Cr_MaxFieldValue = float.MinValue;
		Cr_MaxFieldValueAtSelfTurnEnd = float.MinValue;
		Cr_MaxActionInfoSequence = null;
		Cr_MaxField = null;
		_isOverComplexity = false;
		_originalFieldInplayCount = 0;
		_simulationAllMoves = allMoves;
	}

	public IEnumerator _CrSimulate(AIVirtualField originalField, bool doesUseEvo, bool checkTimeOverLogic, PlayPtnWithToken[] bestPlayPtnsWithToken = null, SimulationAdditionalActionInfoSet additionalActions = null)
	{
		_ = originalField.ParamQuery;
		Cr_FirstActionInfo = null;
		bestMoveCount = int.MaxValue;
		Cr_MaxFieldValue = float.MinValue;
		Cr_MaxFieldValueAtSelfTurnEnd = float.MinValue;
		Cr_MaxActionInfoSequence = null;
		Cr_MaxField = null;
		_isOverComplexity = false;
		AI.IsInVirutalSimulation = true;
		_originalFieldInplayCount = originalField.AllyInplayCards.Count((AIVirtualCard c) => !c.IsDead);
		_defaultPlayptnHashArray = AISimulationUtility.CalculatePlayptnWithTokenHashs(originalField, bestPlayPtnsWithToken);
		AI.IsFullSimulation = true;
		ParallelJob job = ParallelJob.Dispatch(delegate
		{
			try
			{
				List<AIVirtualActionInfo> recursiveMoveList = null;
				RunAllMoves(originalField, doesUseEvo, _simulationAllMoves, bestPlayPtnsWithToken, recursiveMoveList, additionalActions);
			}
			catch (Exception)
			{
			}
		});
		while (!job.isDone)
		{
			yield return null;
			if (checkTimeOverLogic && AI.IsRunWeakLogic)
			{
				AI.IsInVirutalSimulation = false;
				yield break;
			}
		}
		AI.IsFullSimulation = false;
		if (_isOverComplexity)
		{
			BestInPlayMoveAI safety = new BestInPlayMoveAI(AI);
			yield return EnemyAICoroutine.GetInstance().StartCoroutine(safety._CrSimulate(originalField, doesUseEvo, checkTimeOverLogic: false, bestPlayPtnsWithToken, additionalActions));
			Cr_FirstActionInfo = safety.Cr_FirstActionInfo;
			Cr_MaxField = safety.Cr_MaxField;
			Cr_MaxFieldValue = safety.Cr_MaxFieldValue;
			Cr_MaxActionInfoSequence = safety.Cr_MaxActionInfoSequence;
		}
		AI.IsInVirutalSimulation = false;
	}

	private void RunAllMoves(AIVirtualField field, bool doesUseEvo, List<AIVirtualActionInfo> allMoveSet, PlayPtnWithToken[] bestPlayPtnsWithToken, List<AIVirtualActionInfo> recursiveMoveList, SimulationAdditionalActionInfoSet additionalActions)
	{
		if (AI.IsRunWeakLogic)
		{
			return;
		}
		if (allMoveSet.Count > AISimulationUtility.FULL_SIMULATION_MOVE_COUNT_TURESHOLD)
		{
			_isOverComplexity = true;
			return;
		}
		AIVirtualTargetSelectAction aIVirtualTargetSelectAction = null;
		if (additionalActions != null)
		{
			aIVirtualTargetSelectAction = additionalActions.ForceEvoAction;
		}
		if (aIVirtualTargetSelectAction != null && !IsAttackPlanContainsSituation(recursiveMoveList, aIVirtualTargetSelectAction) && !IsAttackPlanContainsSituation(allMoveSet, aIVirtualTargetSelectAction))
		{
			return;
		}
		List<ulong> list = new List<ulong>();
		bool isPlayptnSimulation = bestPlayPtnsWithToken != null;
		for (int i = 0; i < allMoveSet.Count && !AI.IsRunWeakLogic; i++)
		{
			AIVirtualField aIVirtualField = new AIVirtualField(field, isLatestAction: false, isPlayptnSimulation);
			AIVirtualActionInfo cloneActionInfo = GetCloneActionInfo(aIVirtualField, allMoveSet[i]);
			if (cloneActionInfo == null)
			{
				continue;
			}
			ulong hash = cloneActionInfo.GetHash();
			if (list.Contains(hash))
			{
				continue;
			}
			List<AIVirtualActionInfo> list2 = ((recursiveMoveList == null) ? new List<AIVirtualActionInfo>() : new List<AIVirtualActionInfo>(recursiveMoveList));
			list2.Add(cloneActionInfo);
			if (AI.IsRunWeakLogic)
			{
				break;
			}
			switch (cloneActionInfo.ActionType)
			{
			case AIOperationType.PLAY:
				RunPlay(cloneActionInfo, aIVirtualField);
				break;
			case AIOperationType.EVOLVE:
				RunEvolution(cloneActionInfo, aIVirtualField);
				break;
			case AIOperationType.ATTACK:
				RunAttack(cloneActionInfo, aIVirtualField);
				break;
			case AIOperationType.TURNEND:
				RunTurnEnd(cloneActionInfo, aIVirtualField, bestPlayPtnsWithToken, list2, aIVirtualTargetSelectAction);
				continue;
			}
			if (AI.IsRunWeakLogic)
			{
				break;
			}
			if (aIVirtualField.EnemyClass.IsDead || aIVirtualField.AllyClass.IsDead)
			{
				UpdateBestRecord(aIVirtualField, list2, null, 0, aIVirtualTargetSelectAction);
				continue;
			}
			SimulationAdditionalActionInfoSet simulationAdditionalActionInfoSet = new SimulationAdditionalActionInfoSet
			{
				ForceEvoAction = ((cloneActionInfo.ActionType == AIOperationType.EVOLVE) ? null : aIVirtualTargetSelectAction),
				ExtraActionBaseList = additionalActions?.ExtraActionBaseList,
				PlayPtnRecord = additionalActions?.PlayPtnRecord
			};
			List<AIVirtualActionInfo> allMovesForFullSimulation = AISimulationUtility.GetAllMovesForFullSimulation(aIVirtualField, doesUseEvo, simulationAdditionalActionInfoSet);
			RunAllMoves(aIVirtualField, doesUseEvo, allMovesForFullSimulation, bestPlayPtnsWithToken, list2, simulationAdditionalActionInfoSet);
			if (_isOverComplexity || AI.IsRunWeakLogic)
			{
				break;
			}
			list.Add(hash);
		}
	}

	private void RunPlay(AIVirtualActionInfo play, AIVirtualField field)
	{
		PlaySimulationInfo playInfo = AIPlayCardSimulationUtility.CreatePlaySimulationInfo(play.Actor, play, field);
		AIVirtualPlaySimulator.PlayCard((AIVirtualTargetSelectAction)play, field, playInfo);
	}

	private void RunEvolution(AIVirtualActionInfo evolution, AIVirtualField field)
	{
		AIVirtualCard actor = evolution.Actor;
		if (!actor.IsDead && !actor.IsEvolution)
		{
			actor.IsUseEvo = true;
			AIVirtualEvolutionSimulator.ManualEvolve(evolution, field);
			field.EvoUsedCard = actor;
		}
	}

	private void RunAttack(AIVirtualActionInfo attackSituation, AIVirtualField field)
	{
		AIVirtualAttackInfo aIVirtualAttackInfo = attackSituation as AIVirtualAttackInfo;
		AIVirtualCard actor = aIVirtualAttackInfo.Actor;
		AIVirtualAttackSimulator.Attack(aIVirtualAttackInfo, field);
		if (aIVirtualAttackInfo.AttackTarget.IsDead)
		{
			aIVirtualAttackInfo.IsBreakTarget = true;
		}
		if (aIVirtualAttackInfo.IsAttackSuccessed && actor.TagCollectionContainer.HasTag(AIPlayTagType.PuppetAttack))
		{
			actor.TagCollectionContainer.PuppetAttackTags.ExecutePuppetAttack(actor, aIVirtualAttackInfo, null);
		}
		if (aIVirtualAttackInfo.AttackTarget.IsDead)
		{
			aIVirtualAttackInfo.IsBreakTarget = true;
		}
	}

	private void RunTurnEnd(AIVirtualActionInfo move, AIVirtualField field, PlayPtnWithToken[] bestPlayPtnsWithToken, List<AIVirtualActionInfo> passToNext, AIVirtualTargetSelectAction forceEvoSituation)
	{
		int openCount = 0;
		if (bestPlayPtnsWithToken == null)
		{
			BattleSequencer.OnAfterExecuteAttackCommon(field, (AIVirtualTurnEndInfo)move, ref passToNext, isAttackToLeaderAgain: true);
		}
		else
		{
			openCount = _originalFieldInplayCount - field.AllyInplayCards.Count((AIVirtualCard c) => !c.IsDead);
		}
		UpdateBestRecord(field, passToNext, bestPlayPtnsWithToken, openCount, forceEvoSituation);
	}

	private void UpdateBestRecord(AIVirtualField fieldTemp, List<AIVirtualActionInfo> moves, PlayPtnWithToken[] bestPlayPtnsWithToken, int openCount, AISituationInfo forceEvoSituation)
	{
		if (forceEvoSituation != null && !IsAttackPlanContainsSituation(moves, forceEvoSituation))
		{
			return;
		}
		float num = AISimulationUtility.EvaluateField(fieldTemp, EvaluationType.SELF_TURN);
		float num2 = num;
		int bestFirstDeadActionIndex = int.MaxValue;
		PlayPtnWithToken bestPlayPtnWithToken = null;
		if (bestPlayPtnsWithToken != null)
		{
			num2 += AISimulationUtility.CalculateOneOnOneBothDeadBonus(moves);
			num2 += AISimulationUtility.EvaluateLegalPlayPtnAfterSimulation(fieldTemp, moves, bestPlayPtnsWithToken, openCount, _defaultPlayptnHashArray, out bestPlayPtnWithToken, out bestFirstDeadActionIndex);
		}
		else
		{
			num2 -= (fieldTemp.EnemyClass.IsDead ? 0f : fieldTemp.EvaluateThreaten());
		}
		num2 -= (float)fieldTemp.ActionLength * 0.001f;
		num2 += AISimulationUtility.CalculateMoveFirstBonusValue(moves);
		bool flag = false;
		if (fieldTemp.EnemyClass.IsDead)
		{
			if (Cr_MaxField == null || !Cr_MaxField.EnemyClass.IsDead || fieldTemp.ActionLength < bestMoveCount || (fieldTemp.ActionLength == bestMoveCount && EnemyAI.IsLargerThan(num2, Cr_MaxFieldValue)))
			{
				if (moves[0].ActionType == AIOperationType.TURNEND)
				{
					AI.IsTurnEndLethal = true;
				}
				flag = true;
			}
		}
		else if (EnemyAI.IsLargerThan(num2, Cr_MaxFieldValue))
		{
			flag = true;
		}
		else if (EnemyAI.IsSameValue(num2, Cr_MaxFieldValue))
		{
			if (bestPlayPtnsWithToken == null)
			{
				if (EnemyAI.IsLargerThan(num, Cr_MaxFieldValueAtSelfTurnEnd))
				{
					flag = true;
				}
			}
			else if (bestFirstDeadActionIndex < _bestFirstDeadIndex)
			{
				flag = true;
			}
		}
		if (flag)
		{
			Cr_MaxFieldValue = num2;
			Cr_MaxFieldValueAtSelfTurnEnd = num;
			Cr_MaxActionInfoSequence = moves;
			Cr_FirstActionInfo = moves[0];
			bestMoveCount = fieldTemp.ActionLength;
			Cr_MaxField = fieldTemp;
			if (bestPlayPtnsWithToken != null)
			{
				_bestFirstDeadIndex = bestFirstDeadActionIndex;
				Cr_BestPlayPtnWithToken = bestPlayPtnWithToken;
			}
		}
	}

	private AIVirtualActionInfo GetCloneActionInfo(AIVirtualField fieldTemp, AIVirtualActionInfo action)
	{
		if (action == null || action is AIVirtualTurnEndInfo)
		{
			return action;
		}
		AIVirtualCard aIVirtualCard = null;
		aIVirtualCard = ((action.ActionType != AIOperationType.PLAY) ? fieldTemp.AllyInplayCards.Find((AIVirtualCard c) => c.CardIndex == action.OriginalCard.CardIndex) : fieldTemp.AllyHandCards.Find((AIVirtualCard c) => c.CardIndex == action.OriginalCard.CardIndex));
		if (aIVirtualCard == null)
		{
			AIConsoleUtility.LogError("FullSimulationAI.GetCloneActionInfo() error!! Cannot find originalCard!!!!!");
			return null;
		}
		if (action is AIVirtualTargetSelectAction aIVirtualTargetSelectAction)
		{
			return new AIVirtualTargetSelectAction(aIVirtualCard.IsSameCard(action.Actor) ? aIVirtualCard : new AIVirtualCard(action.Actor, fieldTemp), selectedTargetList: aIVirtualTargetSelectAction.SelectedTargets.GetSimilarTargetInfoSet(fieldTemp), original: aIVirtualCard, operationType: action.ActionType);
		}
		if (action is AIVirtualAttackInfo)
		{
			AIVirtualAttackInfo aIVirtualAttackInfo = action as AIVirtualAttackInfo;
			AIVirtualCard originalAttackTarget = aIVirtualAttackInfo.AttackTarget;
			AIVirtualCard target = fieldTemp.CardListSet.EnemyClassAndInplayCards.Find((AIVirtualCard c) => c.CardIndex == originalAttackTarget.CardIndex);
			return new AIVirtualAttackInfo(aIVirtualCard, target);
		}
		return null;
	}

	private bool IsAttackPlanContainsSituation(List<AIVirtualActionInfo> moves, AISituationInfo situation)
	{
		if (moves == null || moves.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < moves.Count; i++)
		{
			if (moves[i].IsSameSituation(situation))
			{
				return true;
			}
		}
		return false;
	}
}
