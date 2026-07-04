using System;
using System.Collections;
using System.Collections.Generic;
using Cute;
using UnityEngine;

namespace Wizard;

public class EnemyAI_Play
{
	private class CalcMostValuableHandPtnObj
	{
		public AISinglePlayptnRecord PlayOutPlan;

		public PlayPtnWithToken[] BestPlayPtnCandidatesWithToken;
	}

	private EnemyAI _ai;

	private AIVirtualFieldRollBackStackProcessor _fieldRollBackProcessor;

	private bool _isOpenSpaceAttackCarryOut;

	public bool IsFusion { get; private set; }

	public AIFusionSituationInfo FusionPattern { get; private set; }

	public AIVirtualCard PriorityCard { get; private set; }

	public PlayedCardInfo PriorityCardPlayInfo { get; private set; }

	public PlayPtnWithToken BestPlayPtnWithToken { get; private set; }

	public AIVirtualActionInfo InstantAttackActionInfo { get; private set; }

	public EnemyAI_Play(EnemyAI ai)
	{
		_ai = ai;
	}

	public void InitOnIterationStart()
	{
		_isOpenSpaceAttackCarryOut = false;
		IsFusion = false;
		FusionPattern = null;
		PriorityCard = null;
		PriorityCardPlayInfo = null;
		BestPlayPtnWithToken = null;
		InstantAttackActionInfo = null;
	}

	public IEnumerator BattleAI_HandPlay()
	{
		if (_ai.CurrentVirtualField.AllyHandCards.Count <= 0)
		{
			_ai.BestPlayPtnRecord = _ai.PlayPtnRecorder.GetEmptyPlayPtnRecord();
			_ai.cr_isOprPlay = false;
			yield break;
		}
		CalcMostValuableHandPtnObj obj = new CalcMostValuableHandPtnObj();
		yield return CalcMostValuableHandPtnJob(obj);
		AISinglePlayptnRecord playOutPlan = obj.PlayOutPlan;
		PlayPtnWithToken[] bestPlayPtnCandidatesWithToken = obj.BestPlayPtnCandidatesWithToken;
		BestPlayPtnWithToken = null;
		InstantAttackActionInfo = null;
		if (_isOpenSpaceAttackCarryOut)
		{
			List<AIVirtualActionInfo> allMovesForFullSimulation = AISimulationUtility.GetAllMovesForFullSimulation(_ai.CurrentVirtualField, doesUseEvo: false, null);
			if (allMovesForFullSimulation.Count <= AISimulationUtility.FULL_SIMULATION_MOVE_COUNT_TURESHOLD)
			{
				FullSimulationAI fullSimulationInstantAttack = new FullSimulationAI(_ai, allMovesForFullSimulation);
				yield return fullSimulationInstantAttack._CrSimulate(_ai.CurrentVirtualField, doesUseEvo: false, checkTimeOverLogic: true, bestPlayPtnCandidatesWithToken);
				if (!_ai.IsRunWeakLogic && fullSimulationInstantAttack.Cr_BestPlayPtnWithToken != null)
				{
					BestPlayPtnWithToken = fullSimulationInstantAttack.Cr_BestPlayPtnWithToken;
					InstantAttackActionInfo = fullSimulationInstantAttack.Cr_FirstActionInfo;
				}
			}
			else
			{
				BestOpenSpaceAttackAI instantAttack = new BestOpenSpaceAttackAI(_ai);
				yield return instantAttack._CrSimulate(_ai.CurrentVirtualField, bestPlayPtnCandidatesWithToken);
				if (!_ai.IsRunWeakLogic && instantAttack.Cr_BestPlayPtnWithToken != null)
				{
					BestPlayPtnWithToken = instantAttack.Cr_BestPlayPtnWithToken;
					InstantAttackActionInfo = instantAttack.Cr_FirstActionInfo;
				}
			}
			if (_ai.IsRunWeakLogic)
			{
				AITimeOverAttackSimulator timeOverLogic = new AITimeOverAttackSimulator();
				yield return timeOverLogic._CrSimulate(_ai.CurrentVirtualField, doesUseEvo: false, checkTimeOverLogic: false, bestPlayPtnCandidatesWithToken);
				if (timeOverLogic.Cr_BestPlayPtnWithToken != null)
				{
					BestPlayPtnWithToken = timeOverLogic.Cr_BestPlayPtnWithToken;
					InstantAttackActionInfo = timeOverLogic.Cr_FirstActionInfo;
				}
			}
		}
		else
		{
			BestPlayPtnWithToken = bestPlayPtnCandidatesWithToken[0];
		}
		List<int> priorityList;
		if (BestPlayPtnWithToken == null)
		{
			priorityList = EnemyAI.EmptyPlayPtn;
			BestPlayPtnWithToken = null;
			_ai.BestPlayPtnRecord = _ai.PlayPtnRecorder.GetEmptyPlayPtnRecord();
		}
		else
		{
			priorityList = BestPlayPtnWithToken.PlayPtn;
			_ai.BestPlayPtnRecord = BestPlayPtnWithToken.Record;
		}
		if (playOutPlan != null && playOutPlan.PlayPtnCount > 0)
		{
			AILethalSimulator aILethalSimulator = new AILethalSimulator(_ai);
			_ai.OutputLethalPlan = null;
			yield return EnemyAICoroutine.GetInstance().StartCoroutine(aILethalSimulator.SimulateByAISimulator(playOutPlan));
			if (_ai.OutputLethalPlan != null && _ai.OutputLethalPlan.IsSuccess)
			{
				AISituationInfo action = _ai.OutputLethalPlan.ActionSequence[0];
				if (_ai.OprLethalAction(action))
				{
					_ai.cr_isOprPlay = true;
					yield break;
				}
			}
		}
		PriorityCard = null;
		PriorityCardPlayInfo = null;
		float priority = float.MinValue;
		AISinglePlayptnRecord bestPlayPtnRecord = _ai.BestPlayPtnRecord;
		if (priorityList != null && priorityList.Count > 0)
		{
			PriorityCard = GetPriorCard(priorityList, bestPlayPtnRecord, ref priority);
			if (bestPlayPtnRecord != null && PriorityCard != null)
			{
				PriorityCardPlayInfo = bestPlayPtnRecord.PlayedCardList.Find((PlayedCardInfo info) => PriorityCard.IsSameCard(info.Card));
			}
		}
		IsFusion = false;
		FusionPattern = AIBestFusionPatternCalculator.CalculateBestFusionPattern(_ai.CurrentVirtualField, bestPlayPtnRecord);
		if (FusionPattern != null && FusionPattern.IsTargetExists(AIScriptTokenArgType.TARGET_SELECT))
		{
			FusionPattern.UpdatePriority(priorityList);
			if (!EnemyAI.IsLargerThan(priority, FusionPattern.Priority))
			{
				IsFusion = true;
				PriorityCard = FusionPattern.Actor;
				_ = FusionPattern.Priority;
			}
		}
		if (PriorityCard == null)
		{
			_ai.cr_isOprPlay = false;
			yield break;
		}
		AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(PriorityCard.FindRealActor(bestPlayPtnRecord), PriorityCard, AIOperationType.PLAY);
		if (!IsFusion)
		{
			_ai.PlaySkipInfo = PriorityCard.GetPlaySkipInformation(priorityList, _ai.PlaySkipInfo, situation);
		}
		for (int num = 0; num < _ai.CurrentVirtualField.CardListSet.BothClassAndInplayCards.Count; num++)
		{
			AIVirtualCard owner = _ai.CurrentVirtualField.CardListSet.BothClassAndInplayCards[num];
			_ai.PlaySkipInfo = owner.GetPlaySkipInformation(priorityList, _ai.PlaySkipInfo, situation);
		}
		if (_ai.PlaySkipInfo != null)
		{
			_ai.cr_isOprPlay = false;
			yield break;
		}
		for (int num2 = 0; num2 < _ai.CurrentVirtualField.CardListSet.BothInplayCards.Count; num2++)
		{
			if (_ai.CurrentVirtualField.CardListSet.BothInplayCards[num2].IsBreakBeforePlay(priorityList))
			{
				_ai.IsBreakBeforePlay = true;
				_ai.cr_isOprPlay = false;
				yield break;
			}
		}
		yield return _ai.HandCardAction();
	}

	private IEnumerator CalcMostValuableHandPtnJob(CalcMostValuableHandPtnObj obj)
	{
		ParallelJob job = ParallelJob.Dispatch(delegate
		{
			try
			{
				AISinglePlayptnRecord playOutPlan;
				PlayPtnWithToken[] bestPlayPtnCandidatesWithToken = CalcMostValuableHandPtn(out playOutPlan);
				obj.PlayOutPlan = playOutPlan;
				obj.BestPlayPtnCandidatesWithToken = bestPlayPtnCandidatesWithToken;
			}
			catch (Exception)
			{
			}
		});
		while (!job.isDone)
		{
			yield return null;
		}
	}

	public AIVirtualCard GetPriorCard(List<int> priorityList, AISinglePlayptnRecord playptnRecord, ref float priority)
	{
		if (priorityList.Count <= 0)
		{
			return null;
		}
		AIVirtualCard result = null;
		for (int i = 0; i < priorityList.Count; i++)
		{
			int num = priorityList[i];
			AIVirtualCard aIVirtualCard = _ai.CurrentVirtualField.AllyHandCards[num];
			aIVirtualCard.FindRealActor(playptnRecord);
			if (aIVirtualCard.IsSpell ? aIVirtualCard.BaseCard.Movable() : aIVirtualCard.BaseCard.BaseMovable)
			{
				priority = EvaluateHandPriority(num, priorityList);
				result = aIVirtualCard;
				break;
			}
		}
		return result;
	}

	private PlayPtnWithToken[] CalcMostValuableHandPtn(out AISinglePlayptnRecord playOutPlan)
	{
		PlayPtnWithToken[] bestPlayPtnsWithToken = new PlayPtnWithToken[6];
		float[] maxValues = new float[6];
		for (int i = 0; i < 6; i++)
		{
			bestPlayPtnsWithToken[i] = null;
			maxValues[i] = float.MinValue;
		}
		playOutPlan = null;
		int num = 6 - _ai.CurrentVirtualField.CardListSet.AllyClassAndInplayCards.Count;
		int num2 = 0;
		AISinglePlayptnRecord aISinglePlayptnRecord = null;
		AIVirtualField aIVirtualField = new AIVirtualField(_ai.CurrentVirtualField);
		_fieldRollBackProcessor = new AIVirtualFieldRollBackStackProcessor(_ai.CurrentVirtualField);
		AIPlayptnRecorder playPtnRecorder = _ai.PlayPtnRecorder;
		for (int j = 0; j < playPtnRecorder.ValidPlayPtnList.Count; j++)
		{
			AISinglePlayptnRecord aISinglePlayptnRecord2 = playPtnRecorder.ValidPlayPtnList[j];
			List<int> playPtn = aISinglePlayptnRecord2.PlayPtn;
			if (!CheckPlayConditon(aIVirtualField, aISinglePlayptnRecord2))
			{
				continue;
			}
			int num3 = Mathf.Max(aIVirtualField.CalculateHandPtnRequiredSpace(aISinglePlayptnRecord2) - num, 0);
			EvaluateHandPtn_Normal(playPtn, aISinglePlayptnRecord2, ref bestPlayPtnsWithToken, ref maxValues);
			if (_fieldRollBackProcessor.HasRecord)
			{
				AIConsoleUtility.LogError("EnemyAI_Play.CalcMostValuableHandPtn() error!! Called ForceResetVirtualField()");
				_fieldRollBackProcessor.ResetVirtualFieldToStart();
			}
			if (num3 <= 0)
			{
				int num4 = AIPlayOutChecker.CalculatePlayOutDamageProspected(_ai.ParamQuery, aIVirtualField, aISinglePlayptnRecord2, _ai);
				if (num4 > num2)
				{
					num2 = num4;
					aISinglePlayptnRecord = aISinglePlayptnRecord2;
				}
				else if (num4 == num2)
				{
					int num5 = aISinglePlayptnRecord?.PlayPtnCount ?? 0;
					if (playPtn.Count < num5)
					{
						aISinglePlayptnRecord = aISinglePlayptnRecord2;
					}
				}
			}
			if (_ai.IsRunWeakLogic)
			{
				break;
			}
		}
		if (num2 >= aIVirtualField.EnemyBattlePlayer.Class.Life)
		{
			playOutPlan = aISinglePlayptnRecord;
		}
		for (int k = 1; k < bestPlayPtnsWithToken.Length; k++)
		{
			PlayPtnWithToken playPtnWithToken = bestPlayPtnsWithToken[k];
			if (playPtnWithToken != null && playPtnWithToken.PlayPtn != null && playPtnWithToken.PlayPtn.Count > 0)
			{
				_isOpenSpaceAttackCarryOut = true;
				break;
			}
		}
		return bestPlayPtnsWithToken;
	}

	private void EvaluateHandPtn_Normal(List<int> handList, AISinglePlayptnRecord playPtnRecord, ref PlayPtnWithToken[] bestPlayPtnsWithToken, ref float[] maxValues)
	{
		float num = 2f;
		float num2 = 0f;
		float[] array = new float[handList.Count];
		AIVirtualField currentVirtualField = _ai.CurrentVirtualField;
		bool isPlagueCity = _ai.IsPlagueCityTagged;
		AIVirtualFieldRollBackBasicProcessor aIVirtualFieldRollBackBasicProcessor = new AIVirtualFieldRollBackBasicProcessor(currentVirtualField);
		for (int i = 0; i < handList.Count; i++)
		{
			int index = handList[i];
			PlayedCardInfo playedCardInfo = playPtnRecord.PlayedCardList[i];
			AIVirtualCard aIVirtualCard = currentVirtualField.AllyHandCards[index];
			AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(aIVirtualCard, aIVirtualCard, AIOperationType.PLAY);
			aIVirtualTargetSelectAction.SetDiscardInfo(playedCardInfo.DiscardInfo);
			AIPreprocessSimulationUtility.ExecuteRecordingToPlayedCardInfo(playedCardInfo, aIVirtualCard, currentVirtualField, aIVirtualTargetSelectAction, isPseudo: true);
			array[i] = EvaluateHandValue(aIVirtualCard, handList, isOnBattleSimulate: false, aIVirtualTargetSelectAction, ref isPlagueCity, playPtnRecord);
			currentVirtualField.UpdateVirtualFieldWhenEvaluation(aIVirtualCard, aIVirtualTargetSelectAction);
		}
		aIVirtualFieldRollBackBasicProcessor.ResetVirtualFieldToStart();
		int dstHandCount = currentVirtualField.AllyHandCards.Count;
		float num3 = _ai.ParamQuery.CalcHandCardOverflowPenalty(_ai.CurrentVirtualField, handList, num, ref dstHandCount);
		num2 -= num3;
		if (currentVirtualField.AllyBattlePlayer.IsEvolve)
		{
			float num4 = 0f;
			for (int j = 0; j < handList.Count; j++)
			{
				int index2 = handList[j];
				AIVirtualCard aIVirtualCard2 = currentVirtualField.AllyHandCards[index2];
				if (!aIVirtualCard2.IsFollower(handList))
				{
					continue;
				}
				AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(aIVirtualCard2, aIVirtualCard2, AIOperationType.EVOLVE, (AISelectedTargetInfoSet)null);
				int evoTokenCount = aIVirtualCard2.GetEvoTokenCount(currentVirtualField, handList, situation);
				if (0 < evoTokenCount && evoTokenCount > _ai.GetAllySpaceNum())
				{
					continue;
				}
				float num5 = (float)Mathf.Max(aIVirtualCard2.GetEvoHandPlusCount(currentVirtualField, handList, situation) + dstHandCount - 8, 0) * num;
				float num6 = aIVirtualCard2.EvaluateAllEvoBonus(handList, situation) - num5;
				if (num6 > num4)
				{
					num4 = num6;
				}
				if (aIVirtualCard2.CreateAIVirtualSelectInfo(currentVirtualField, situation) != null)
				{
					float num7 = 0f - num5;
					if (num7 > num4)
					{
						num4 = num7;
					}
				}
			}
			num2 += num4;
		}
		num2 += AIEvaluateBonusFromOhterUtility.GetPlayPtnBonus(currentVirtualField, handList) + _ai.StyleQuery.GetPlayptnBonus(currentVirtualField, handList);
		List<TokenPlayPattern> list = new List<TokenPlayPattern>();
		List<int> list2 = new List<int>(handList);
		if (list2.Count <= 0)
		{
			if (num2 > maxValues[0])
			{
				bestPlayPtnsWithToken[0] = new PlayPtnWithToken(list2, playPtnRecord, list);
				maxValues[0] = num2;
			}
			return;
		}
		int currentSpace = 6 - currentVirtualField.CardListSet.AllyClassAndInplayCards.Count;
		List<float> list3 = new List<float> { 0f };
		for (int k = 0; k < _ai.AllySuicideList.Count; k++)
		{
			AIVirtualCard aIVirtualCard3 = _ai.AllySuicideList[k];
			float num8 = aIVirtualCard3.EvaluateValueOnField(list2, null, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true) - aIVirtualCard3.EvaluateBreakValue(handList, useIgnoreBreak: true) - aIVirtualCard3.EvaluateLeaveValue(handList, useIgnoreInBattle: true) - (float)aIVirtualCard3.Attack;
			list3.Add(list3[k] - num8);
		}
		EstimateMaxPlayPtnWithToken(playPtnRecord, list3, array, num2, isPlagueCity, 0, 0, currentSpace, list, 0f, ref bestPlayPtnsWithToken, ref maxValues);
	}

	private void EstimateMaxPlayPtnWithToken(AISinglePlayptnRecord playPtnRecord, List<float> suicideList, float[] handValues, float optionValue, bool isPlagueCity, int playPtnIndex, int suicideIndex, int currentSpace, List<TokenPlayPattern> currentPlayedCardsPattern, float currentValue, ref PlayPtnWithToken[] bestPlayPtnsWithToken, ref float[] maxValues)
	{
		List<int> playPtn = playPtnRecord.PlayPtn;
		AIVirtualField currentVirtualField = _ai.CurrentVirtualField;
		int i = playPtnIndex;
		int num = 0;
		float num2 = currentValue;
		int num3 = 0;
		foreach (AIVirtualCard allyInplayCard in currentVirtualField.AllyInplayCards)
		{
			if (allyInplayCard.WhiteRitualCount > 0)
			{
				num3++;
			}
		}
		for (; i < playPtn.Count; i++)
		{
			int index = playPtn[i];
			PlayedCardInfo playedCardInfo = playPtnRecord.PlayedCardList[i];
			AIVirtualCard aIVirtualCard = currentVirtualField.AllyHandCards[index];
			AIVirtualCard sourceCard = aIVirtualCard;
			if (playedCardInfo.TransformCard != null)
			{
				sourceCard = playedCardInfo.TransformCard;
			}
			AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(sourceCard, aIVirtualCard, AIOperationType.PLAY);
			aIVirtualTargetSelectAction.SetDiscardInfo(playedCardInfo.DiscardInfo);
			_fieldRollBackProcessor.RegisterRecord();
			AIPreprocessSimulationUtility.ExecuteRecordingToPlayedCardInfo(playedCardInfo, aIVirtualCard, currentVirtualField, aIVirtualTargetSelectAction, isPseudo: true);
			int playSpaceRequired = currentVirtualField.GetPlaySpaceRequired(aIVirtualCard, playPtn, aIVirtualTargetSelectAction, needsTokenCount: false);
			AITokenIdCollection bothSideTokenIdsOfPlaySituation = AIPlayTokenSimulationUtility.GetBothSideTokenIdsOfPlaySituation(currentVirtualField, playPtn, aIVirtualTargetSelectAction);
			List<AITokenInformation> allDiscardedTokenIds = AIDiscardUtility.GetAllDiscardedTokenIds(aIVirtualTargetSelectAction.DiscardInfo, currentVirtualField, playPtn, aIVirtualTargetSelectAction);
			List<AITokenInformation> list = new List<AITokenInformation>();
			aIVirtualTargetSelectAction.SetActor(aIVirtualCard);
			currentVirtualField.UpdateVirtualFieldWhenEvaluation(aIVirtualCard, aIVirtualTargetSelectAction);
			bool flag = false;
			if (bothSideTokenIdsOfPlaySituation != null && bothSideTokenIdsOfPlaySituation.HasAllyToken)
			{
				List<AITokenInformation> allyTokenIdList = bothSideTokenIdsOfPlaySituation.AllyTokenIdList;
				for (int j = 0; j < allyTokenIdList.Count; j++)
				{
					list.Add(allyTokenIdList[j]);
					if (num3 > 0)
					{
						AIVirtualCard tokenFromId = currentVirtualField.AI.tokenManager.GetTokenFromId(allyTokenIdList[j].TokenId, isAlly: true, currentVirtualField);
						if (tokenFromId != null && tokenFromId.IsStackWhiteRitual)
						{
							flag = true;
						}
					}
				}
			}
			if (allDiscardedTokenIds != null)
			{
				for (int k = 0; k < allDiscardedTokenIds.Count; k++)
				{
					list.Add(allDiscardedTokenIds[k]);
				}
			}
			int num4 = playSpaceRequired + list.Count;
			if (flag)
			{
				bool num5 = i == playPtn.Count - 1;
				bool flag2 = currentSpace - num4 < 0;
				num3 = ((!(num5 && flag2)) ? num3 : 0);
				num4 -= num3;
			}
			bool flag3 = _ai.StyleQuery.IsPlayBreak(aIVirtualCard, playPtn, aIVirtualTargetSelectAction);
			int num6 = 0;
			if (flag3)
			{
				num6 = aIVirtualCard.GetAllyLastwordTokenCount(playPtn);
				num4 += num6;
				num4 -= playSpaceRequired;
			}
			float num7 = 0f;
			if (bothSideTokenIdsOfPlaySituation != null && bothSideTokenIdsOfPlaySituation.HasOpponentToken)
			{
				List<AITokenInformation> opponentTokenIdList = bothSideTokenIdsOfPlaySituation.OpponentTokenIdList;
				for (int l = 0; l < opponentTokenIdList.Count; l++)
				{
					int tokenId = opponentTokenIdList[l].TokenId;
					num7 += GetTokenValueFromId(tokenId, AITokenType.Default, playPtn, isAllyToken: false, isPlagueCity);
				}
			}
			if (num + num4 <= currentSpace)
			{
				num += num4;
				float num8 = 0f;
				for (int m = 0; m < list.Count; m++)
				{
					AITokenInformation aITokenInformation = list[m];
					num8 += GetTokenValueFromId(aITokenInformation.TokenId, aITokenInformation.TokenType, playPtn, isAllyToken: true, isPlagueCity);
				}
				num8 -= num7;
				float num9 = handValues[i] + num8 + AIDiscardUtility.CalcAllDiscardedBonus(_ai.CurrentVirtualField, aIVirtualTargetSelectAction, playPtn);
				if (num9 < aIVirtualCard.GetPlayLimit(playPtn))
				{
					_fieldRollBackProcessor.RollBackFieldWithIteration(i - playPtnIndex + 1);
					return;
				}
				num2 += num9;
				TokenPlayPattern item = new TokenPlayPattern(index, num8, list);
				currentPlayedCardsPattern.Add(item);
				continue;
			}
			currentSpace -= num;
			for (int n = suicideIndex; n < suicideList.Count; n++)
			{
				float num10 = num2;
				int num11 = currentSpace + (n - suicideIndex) - playSpaceRequired;
				if (num11 < 0)
				{
					continue;
				}
				bool flag4 = num11 >= list.Count;
				List<AITokenInformation> list2 = (flag4 ? new List<AITokenInformation>(list) : list.GetRange(0, num11));
				num11 -= list2.Count;
				if (flag3)
				{
					num11 += playSpaceRequired;
					bool flag5 = num11 >= num6;
					flag4 = flag5 && flag4;
					num11 -= (flag5 ? num6 : num11);
				}
				float num12 = 0f;
				for (int num13 = 0; num13 < list2.Count; num13++)
				{
					AITokenInformation aITokenInformation2 = list2[num13];
					num12 += GetTokenValueFromId(aITokenInformation2.TokenId, aITokenInformation2.TokenType, playPtn, isAllyToken: true, isPlagueCity);
				}
				num12 -= num7;
				float num14 = handValues[i] + num12 + AIDiscardUtility.CalcAllDiscardedBonus(currentVirtualField, aIVirtualTargetSelectAction, playPtn);
				if (num14 < aIVirtualCard.GetPlayLimit(playPtn))
				{
					continue;
				}
				num10 += num14;
				TokenPlayPattern tokenPlayPattern = new TokenPlayPattern(index, num12, list2);
				currentPlayedCardsPattern.Add(tokenPlayPattern);
				EstimateMaxPlayPtnWithToken(playPtnRecord, suicideList, handValues, optionValue, isPlagueCity, i + 1, n, num11, currentPlayedCardsPattern, num10, ref bestPlayPtnsWithToken, ref maxValues);
				for (int num15 = currentPlayedCardsPattern.Count - 1; num15 >= 0; num15--)
				{
					TokenPlayPattern tokenPlayPattern2 = currentPlayedCardsPattern[num15];
					currentPlayedCardsPattern.Remove(tokenPlayPattern2);
					if (tokenPlayPattern2 == tokenPlayPattern)
					{
						break;
					}
				}
				if (flag4)
				{
					break;
				}
			}
			_fieldRollBackProcessor.RollBackFieldWithIteration(i - playPtnIndex + 1);
			return;
		}
		_fieldRollBackProcessor.RollBackFieldWithIteration(i - playPtnIndex);
		if (i == playPtn.Count)
		{
			float num16 = num2 + suicideList[suicideIndex] + optionValue;
			if (bestPlayPtnsWithToken[suicideIndex] == null || num16 >= maxValues[suicideIndex])
			{
				PlayPtnWithToken playPtnWithToken = new PlayPtnWithToken(playPtn, playPtnRecord, new List<TokenPlayPattern>(currentPlayedCardsPattern));
				bestPlayPtnsWithToken[suicideIndex] = playPtnWithToken;
				maxValues[suicideIndex] = num16;
			}
		}
	}

	private float GetTokenValueFromId(int id, AITokenType tokenType, List<int> playPtn, bool isAllyToken, bool isPlagueCity)
	{
		AIVirtualCard aIVirtualCard = _ai.tokenManager.GetTokenFromId(id, isAllyToken, _ai.CurrentVirtualField);
		if (aIVirtualCard == null)
		{
			AIConsoleUtility.LogError("CalculateMaxTokenPatternValue: Could not get a token from the tokenPool.");
			return 0f;
		}
		if (isPlagueCity && aIVirtualCard.IsUnit)
		{
			aIVirtualCard = _ai.tokenManager.GetZombieToken(isAlly: true, _ai.CurrentVirtualField);
		}
		float num = aIVirtualCard.EvaluateValueOnField(playPtn, null, useStyle: true, doesUseLostLife: true, useOthersTag: true, useIgnoreInBattle: true) + (aIVirtualCard.EvaluateBreakValue(playPtn, useIgnoreBreak: false) + aIVirtualCard.EvaluateLeaveValue(playPtn, useIgnoreInBattle: false)) * EnemyAI.BREAKBONUS_RATE_IN_HAND;
		if (tokenType == AITokenType.Reanimate)
		{
			num += aIVirtualCard.GetReanimateBonus(playPtn);
		}
		return num;
	}

	public float EvaluateHandValue(AIVirtualCard handCard, List<int> handList, bool isOnBattleSimulate, AIVirtualTargetSelectAction play, ref bool isPlagueCity, AISinglePlayptnRecord playptnRecord)
	{
		if (isPlagueCity && handCard.IsUnit)
		{
			handCard = _ai.tokenManager.GetZombieToken(isAlly: true, _ai.CurrentVirtualField);
		}
		else if (!isPlagueCity && handCard.IsPlagueCity(_ai.CurrentVirtualField, handList))
		{
			isPlagueCity = true;
		}
		AIVirtualCard aIVirtualCard = new AIVirtualCard(handCard, _ai.CurrentVirtualField);
		AIPlayptnBaseStatsRateUtility.Execute(_ai.CurrentVirtualField.CardListSet.AllyClassAndInplayCards, new List<AIVirtualCard> { aIVirtualCard }, EnemyAI.EmptyPlayPtn);
		AIPlayptnBaseStatsRateUtility.Execute(_ai.CurrentVirtualField.CardListSet.EnemyClassAndInplayCards, new List<AIVirtualCard> { aIVirtualCard }, EnemyAI.EmptyPlayPtn);
		return EvaluateHandValue(aIVirtualCard, handList, play, isOnBattleSimulate, playptnRecord);
	}

	public float EvaluateHandValue(AIVirtualCard card, List<int> handList, AIVirtualTargetSelectAction play, bool isOnBattleSimulate, AISinglePlayptnRecord playptnRecord)
	{
		if (AITargetSelectFilteringUtility.IsOnlyIgnoreTarget(card, _ai.CurrentVirtualField, handList, playptnRecord))
		{
			return 0f;
		}
		if (card.IsSpell || card.IsAccelerated(_ai.CurrentVirtualField, handList) || card.IsCrystalize(_ai.CurrentVirtualField, handList))
		{
			return card.EvaluatePlayValue(handList, play) * _ai.StyleQuery.GetAllyPlayBonusRate(card, handList, play);
		}
		float num = 0f;
		if (_ai.StyleQuery.IsPlayBreak(card, handList, play))
		{
			num = card.EvaluatePlayValue(handList, play) + card.EvaluateBreakValue(handList, useIgnoreBreak: true) + card.EvaluateLeaveValue(handList, useIgnoreInBattle: true);
		}
		else
		{
			num = card.EvaluatePlayValue(handList, play);
			num += card.EvaluateValueOnField(handList, null, useStyle: true);
			num += (card.EvaluateBreakValue(handList, useIgnoreBreak: false) + card.EvaluateLeaveValue(handList, useIgnoreInBattle: false)) * EnemyAI.BREAKBONUS_RATE_IN_HAND;
		}
		return num * _ai.StyleQuery.GetAllyPlayBonusRate(card, handList, play);
	}

	public float EvaluateHandPriority(int handIndex, List<int> playPtn)
	{
		return _ai.CurrentVirtualField.AllyHandCards[handIndex].GetPriority(playPtn);
	}

	private bool CheckPlayConditon(AIVirtualField field, AISinglePlayptnRecord playPtnRecord)
	{
		List<int> playPtn = playPtnRecord.PlayPtn;
		if (CalcHandPtnLifeCost(field, playPtn) >= field.AllyBattlePlayer.Class.Life)
		{
			return false;
		}
		if (!field.IsPlayableHandList(playPtnRecord, field.AI.AllySuicideList))
		{
			return false;
		}
		return true;
	}

	private int CalcHandPtnLifeCost(AIVirtualField field, List<int> handList)
	{
		int num = 0;
		EnemyAI aI = field.AI;
		for (int i = 0; i < handList.Count; i++)
		{
			int index = handList[i];
			AIVirtualCard aIVirtualCard = field.AllyHandCards[index];
			num += AIPlayOnSkillUtility.GetLifePenaltyOnPlay(aIVirtualCard.BaseCard, aI);
		}
		return num;
	}
}
