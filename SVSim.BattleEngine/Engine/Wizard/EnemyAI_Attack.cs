using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Wizard;

public class EnemyAI_Attack
{
	private class PlaySkipExecutingInfo
	{
		public bool IsEvolutionPermitted;

		public List<AIVirtualActionInfo> SimulationExtraActionBaseList;

		public PlaySkipExecutingInfo()
		{
			IsEvolutionPermitted = false;
			SimulationExtraActionBaseList = null;
		}

		public void AddExtraAction(AIVirtualActionInfo action)
		{
			SimulationExtraActionBaseList = AIParamQuery.AddElementToList(action, SimulationExtraActionBaseList);
		}
	}

	private EnemyAI _ai;

	private IBattleSimulationAI _simulationResultWithEmptyPlayPtn;

	public bool IsLethalAttackPlan { get; private set; }

	public EnemyAI_Attack(EnemyAI ai)
	{
		_ai = ai;
		_simulationResultWithEmptyPlayPtn = null;
	}

	public void InitOnTurnStart()
	{
		_simulationResultWithEmptyPlayPtn = null;
	}

	public void InitOnIterationStart()
	{
		_simulationResultWithEmptyPlayPtn = null;
	}

	public IEnumerator BattleAI_UseHighSimu()
	{
		AIVirtualField currentVirtualField = _ai.CurrentVirtualField;
		AISinglePlayptnRecord playPtnRecord = _ai.BestPlayPtnRecord;
		PlaySkipExecutingInfo playSkipExecutingInfo = CreatePlaySkipExecutingInfo(playPtnRecord);
		bool flag = playSkipExecutingInfo?.IsEvolutionPermitted ?? true;
		bool isEvol = _ai.IsEvoPermissionOnSimu && flag && !_ai.IsBreakBeforePlay;
		float fastEvoMaxValue = float.MinValue;
		AIVirtualCard evolveFollower = null;
		AIVirtualActionInfo firstAction = null;
		bool isLethalByEvo = false;
		if (isEvol)
		{
			int allyIndex = 0;
			while (allyIndex < currentVirtualField.AllyInplayCards.Count)
			{
				AIVirtualCard inplayTemp = currentVirtualField.AllyInplayCards[allyIndex];
				if (inplayTemp.IsAbleEvolution())
				{
					AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(inplayTemp, inplayTemp, AIOperationType.EVOLVE, (AISelectedTargetInfoSet)null);
					SimulationResult ref_skillSim = new SimulationResult();
					List<AIVirtualTargetSelectInfo> list = inplayTemp.CreateAIVirtualSelectInfo(currentVirtualField, situation);
					if (list != null)
					{
						AIVirtualTargetSelectSimulationInfo simulationInfo = new AIVirtualTargetSelectSimulationInfo
						{
							OriginalActor = inplayTemp,
							RealActor = inplayTemp,
							SelectInfoList = list,
							OperationType = AIOperationType.EVOLVE,
							NextPlayCardInfo = null,
							IsFirstAction = true
						};
						yield return EnemyAICoroutine.GetInstance().StartCoroutine(AIVirtualTargetSelectSimulator.SimulateTargetSelect(simulationInfo, _ai.CurrentVirtualField, playPtnRecord, ref_skillSim, isBattleSim: true));
						if (IsTargetSelectEvolutionValid(ref_skillSim, inplayTemp, fastEvoMaxValue))
						{
							isLethalByEvo = ref_skillSim.IsLethalPlan;
							fastEvoMaxValue = ref_skillSim.MaxFieldValue;
							firstAction = ref_skillSim.FirstActionInfo;
							evolveFollower = inplayTemp;
						}
					}
				}
				int num = allyIndex + 1;
				allyIndex = num;
			}
		}
		AIVirtualField field = new AIVirtualField(_ai.CurrentVirtualField)
		{
			BestPlayPtn = _ai.BestPlayPtn
		};
		SimulationAdditionalActionInfoSet additionalActionInfoSet = new SimulationAdditionalActionInfoSet
		{
			ForceEvoAction = null,
			ExtraActionBaseList = playSkipExecutingInfo?.SimulationExtraActionBaseList,
			PlayPtnRecord = playPtnRecord
		};
		AIBattleSimulationLauncher battleSimLauncher = new AIBattleSimulationLauncher(field, _ai, isEvol, _simulationResultWithEmptyPlayPtn, additionalActionInfoSet);
		yield return battleSimLauncher.ExecuteBattleSimulationAI(null, checkTimeOverLogic: true);
		IBattleSimulationAI battleSimAI = battleSimLauncher.BattleSimAI;
		if (_ai.IsBreakBeforePlay && battleSimAI.Cr_MaxField != null && !battleSimAI.Cr_MaxField.IsBreakBeforePlayKilled)
		{
			_ai.cr_isOprAtk = false;
			yield break;
		}
		if (battleSimAI.Cr_MaxFieldValue > fastEvoMaxValue)
		{
			if (battleSimAI.Cr_FirstActionInfo != null && !(battleSimAI.Cr_FirstActionInfo is AIVirtualTurnEndInfo))
			{
				firstAction = battleSimAI.Cr_FirstActionInfo;
				evolveFollower = battleSimAI.Cr_MaxField.EvoUsedCard;
				IsLethalAttackPlan = battleSimAI.Cr_MaxField.EnemyClass.IsDead;
			}
			else
			{
				firstAction = null;
				IsLethalAttackPlan = isLethalByEvo;
				evolveFollower = null;
			}
		}
		if (firstAction == null)
		{
			_ai.cr_isOprAtk = false;
		}
		else if (!IsPlaySkipPermitted(evolveFollower))
		{
			_ai.cr_isOprAtk = false;
		}
		else if (firstAction.ActionType == AIOperationType.PLAY)
		{
			foreach (PlayedCardInfo playedCard in _ai.BestPlayPtnRecord.PlayedCardList)
			{
				if (playedCard.PreDecidedSelectTargets != null && firstAction.OriginalCard.IsSameCard(playedCard.Card))
				{
					_ai.SetPreDecidedTargets(playedCard.PreDecidedSelectTargets);
					break;
				}
			}
			_ai.OprPlay(firstAction.OriginalCard, _ai.BestPlayPtnRecord);
			_ai.cr_isOprAtk = true;
		}
		else if (firstAction.ActionType == AIOperationType.EVOLVE)
		{
			_ai.CurrentBattleBeforeSimEvoEvolCount = _ai.PlayerPair.Self.CurrentEpCount;
			_ai.OprEvolution(firstAction.Actor);
			_ai.CurrentBattleSimEvoCard = evolveFollower;
			_ai.cr_isOprAtk = true;
		}
		else if (firstAction.ActionType == AIOperationType.ATTACK)
		{
			_ai.OprAttack(firstAction);
			_ai.cr_isOprAtk = true;
		}
		else
		{
			_ai.cr_isOprAtk = false;
		}
	}

	public bool IsPlaySkipPermitted(AIVirtualCard evolveFollower)
	{
		if (_ai.PlaySkipInfo == null || !_ai.PlaySkipInfo.Any())
		{
			return true;
		}
		bool result = false;
		for (int i = 0; i < _ai.PlaySkipInfo.Count; i++)
		{
			if (_ai.PlaySkipInfo[i].IsEvoCardLegal(evolveFollower))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private PlaySkipExecutingInfo CreatePlaySkipExecutingInfo(AISinglePlayptnRecord playPtnRecord)
	{
		List<PlaySkipInformation> playSkipInfo = _ai.PlaySkipInfo;
		if (playSkipInfo == null || playSkipInfo.Count <= 0)
		{
			return null;
		}
		PlaySkipExecutingInfo playSkipExecutingInfo = new PlaySkipExecutingInfo();
		List<AIScriptTokenArgType> checkedActionTypeList = null;
		for (int i = 0; i < playSkipInfo.Count; i++)
		{
			PlaySkipInformation playSkipInformation = playSkipInfo[i];
			if (playSkipInformation.IsEvolutionPermittedTag)
			{
				playSkipExecutingInfo.IsEvolutionPermitted = true;
			}
			AIVirtualActionInfo aIVirtualActionInfo = null;
			if (!(playSkipInformation is PlaySkipWithActionInformation playSkipWithActionInformation))
			{
				if (playSkipInformation is PlaySkipWithActionIfEvoInformation playSkipWithActionIfEvoInformation)
				{
					aIVirtualActionInfo = playSkipWithActionIfEvoInformation.GetExtraActionBase(playPtnRecord, ref checkedActionTypeList);
				}
			}
			else
			{
				aIVirtualActionInfo = playSkipWithActionInformation.GetExtraActionBase(playPtnRecord, ref checkedActionTypeList);
			}
			if (aIVirtualActionInfo != null)
			{
				playSkipExecutingInfo.AddExtraAction(aIVirtualActionInfo);
			}
		}
		return playSkipExecutingInfo;
	}

	private bool IsTargetSelectEvolutionValid(SimulationResult result, AIVirtualCard targetSelectEvoOwner, float valueForComparing)
	{
		if (result.BestResultField == null)
		{
			return false;
		}
		AIVirtualCard evoUsedCard = result.BestResultField.EvoUsedCard;
		if (evoUsedCard == null || !evoUsedCard.IsSameCard(targetSelectEvoOwner))
		{
			return false;
		}
		if (result.MaxFieldValue <= valueForComparing)
		{
			return false;
		}
		return true;
	}

	public bool BattleAI_TurnEndAttack()
	{
		AIVirtualField aIVirtualField = new AIVirtualField(_ai.CurrentVirtualField)
		{
			BestPlayPtn = _ai.BestPlayPtn
		};
		AIVirtualField aIVirtualField2 = new AIVirtualField(aIVirtualField);
		List<AIVirtualActionInfo> moves = new List<AIVirtualActionInfo>();
		AIVirtualTurnEndInfo aIVirtualTurnEndInfo = new AIVirtualTurnEndInfo(aIVirtualField2.AllyClass);
		moves.Add(aIVirtualTurnEndInfo);
		BattleSequencer.OnAfterExecuteAttackCommon(aIVirtualField2, aIVirtualTurnEndInfo, ref moves);
		float num = AISimulationUtility.EvaluateField(aIVirtualField2, EvaluationType.SIMPLE);
		float num2 = float.MinValue;
		AIVirtualCard aIVirtualCard = null;
		AIVirtualCard aIVirtualCard2 = null;
		for (int i = 0; i < aIVirtualField.AllyInplayCards.Count; i++)
		{
			if (!aIVirtualField.AllyInplayCards[i].IsUnit)
			{
				continue;
			}
			for (int j = -1; j < aIVirtualField.EnemyInplayCards.Count; j++)
			{
				if (j >= 0 && !aIVirtualField.EnemyInplayCards[j].IsUnit)
				{
					continue;
				}
				aIVirtualField2 = new AIVirtualField(aIVirtualField);
				moves.Clear();
				AIVirtualCard aIVirtualCard3 = aIVirtualField2.AllyInplayCards[i];
				AIVirtualCard aIVirtualCard4 = ((j < 0) ? aIVirtualField2.EnemyClass : aIVirtualField2.EnemyInplayCards[j]);
				AIVirtualAttackInfo aIVirtualAttackInfo = new AIVirtualAttackInfo(aIVirtualCard3, aIVirtualCard4);
				if (AIAttackSimulationUtility.IsAttackPossible(aIVirtualField2, aIVirtualAttackInfo))
				{
					moves.Add(aIVirtualAttackInfo);
					AIVirtualAttackSimulator.Attack(aIVirtualAttackInfo, aIVirtualField2);
					AIVirtualTurnEndInfo aIVirtualTurnEndInfo2 = new AIVirtualTurnEndInfo(aIVirtualField2.AllyClass);
					moves.Add(aIVirtualTurnEndInfo2);
					BattleSequencer.OnAfterExecuteAttackCommon(aIVirtualField2, aIVirtualTurnEndInfo2, ref moves);
					num2 = AISimulationUtility.EvaluateField(aIVirtualField2, EvaluationType.SIMPLE);
					if (num2 > num)
					{
						num = num2;
						aIVirtualCard = aIVirtualCard3;
						aIVirtualCard2 = aIVirtualCard4;
					}
				}
			}
		}
		if (aIVirtualCard != null && aIVirtualCard2 != null)
		{
			_ai.OprAttack(new AIVirtualAttackInfo(aIVirtualCard, aIVirtualCard2));
			return true;
		}
		return false;
	}

	public IEnumerator Cr_BattleAI_ImmediateAttack()
	{
		AIVirtualField aIVirtualField = new AIVirtualField(_ai.CurrentVirtualField);
		AIVirtualTurnEndInfo turnEndSituation = new AIVirtualTurnEndInfo(aIVirtualField.AllyClass);
		BattleSequencer.ProcessAllyTurnEndToOpponentTurnStart(aIVirtualField, turnEndSituation);
		if (aIVirtualField.IsSuccess())
		{
			_ai.cr_isTerminate = true;
			_ai.IsTurnEndLethal = true;
			yield break;
		}
		if (!_ai.CurrentVirtualField.isForceImmediateAttack())
		{
			int num = _ai.CurrentVirtualField.EnemyInplayCards.Count((AIVirtualCard card) => card.IsGuard);
			int num2 = _ai.CurrentVirtualField.AllyInplayCards.Count((AIVirtualCard card) => card.IsAttackable(EnemyAI.EmptyPlayPtn));
			bool flag = _ai.CurrentVirtualField.AllyInplayCards.Any((AIVirtualCard card) => card.IsIgnoreGuard);
			if (num >= num2 && !flag)
			{
				_ai.cr_isTerminate = false;
				yield break;
			}
		}
		_simulationResultWithEmptyPlayPtn = new BestInPlayMoveAI(_ai);
		yield return EnemyAICoroutine.GetInstance().StartCoroutine(_simulationResultWithEmptyPlayPtn._CrSimulate(_ai.CurrentVirtualField, doesUseEvo: true, checkTimeOverLogic: true));
		if (_ai.IsRunWeakLogic)
		{
			_simulationResultWithEmptyPlayPtn = new AITimeOverAttackSimulator();
			yield return _simulationResultWithEmptyPlayPtn._CrSimulate(_ai.CurrentVirtualField, doesUseEvo: true, checkTimeOverLogic: false);
		}
		AIVirtualField cr_MaxField = _simulationResultWithEmptyPlayPtn.Cr_MaxField;
		if (cr_MaxField == null || !cr_MaxField.EnemyClass.IsDead || cr_MaxField.AllyClass.IsDead)
		{
			_ai.cr_isTerminate = false;
			yield break;
		}
		AIVirtualActionInfo cr_FirstActionInfo = _simulationResultWithEmptyPlayPtn.Cr_FirstActionInfo;
		if (cr_FirstActionInfo == null)
		{
			_ai.cr_isTerminate = false;
			yield break;
		}
		_ai.cr_isTerminate = true;
		if (cr_FirstActionInfo.ActionType == AIOperationType.EVOLVE && !cr_FirstActionInfo.Actor.BaseCard.IsEvolution)
		{
			_ai.OprEvolution(cr_FirstActionInfo.Actor);
		}
		else if (cr_FirstActionInfo.ActionType == AIOperationType.ATTACK)
		{
			if ((cr_FirstActionInfo as AIVirtualAttackInfo).AttackTarget != null)
			{
				_ai.OprAttack(cr_FirstActionInfo);
			}
		}
		else
		{
			_ai.cr_isTerminate = false;
		}
	}
}
