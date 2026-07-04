using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizard;
using Wizard.Battle;

public class OperateReceiveChecker
{
	private NetworkBattleData networkBattleData;

	private BattleManagerBase _battleMgr;

	public OperateReceiveChecker(BattleManagerBase mgr, NetworkBattleData networkData)
	{
		_battleMgr = mgr;
		networkBattleData = networkData;
	}

	public bool IsOperateReceive()
	{
		try
		{
			NetworkBattleReceiver.ReceiveData receiveData = networkBattleData.GetReceiveData();
			if (receiveData.dataUri == NetworkBattleDefine.NetworkBattleURI.PlayActions)
			{
				int playCardIndex = receiveData.playCardIndex;
				BattleCardBase battleCardIdx = _battleMgr.GetBattleCardIdx(_battleMgr.BattleEnemy.ClassAndInPlayCardList, playCardIndex);
				switch (receiveData.actionType)
				{
				case NetworkBattleDefine.PlayActionType.ATTACK:
				{
					if (battleCardIdx == null)
					{
						LocalLog.AccumulateTraceLog("IsOperateReceive actionCard null");
						return false;
					}
					List<BattleCardBase> opposingCardObjTarget = NetworkBattleGenericTool.GetOpposingCardObjTarget(_battleMgr, receiveData.OpponentTargetDataList);
					if (!AttackSelectControl.CanCardAttackTarget(battleCardIdx, opposingCardObjTarget.First(), _battleMgr.BattlePlayer.InPlayCards))
					{
						LocalLog.AccumulateTraceLog("IsOperateReceive CanCardAttackTarget");
						return false;
					}
					break;
				}
				case NetworkBattleDefine.PlayActionType.EVOLUTION:
				case NetworkBattleDefine.PlayActionType.EVOLUTION_SELECT:
					if (battleCardIdx == null)
					{
						LocalLog.AccumulateTraceLog("IsOperateReceive actionCard null");
						return false;
					}
					if (!battleCardIdx.CanEvolution(isSkill: false, isSelfBattlePlayer: true) || !battleCardIdx.IsInplay)
					{
						LocalLog.AccumulateTraceLog("IsOperateReceive CanEvolution");
						return false;
					}
					if (!IsSelectSkillCheck(receiveData, battleCardIdx, isEvol: true))
					{
						LocalLog.AccumulateTraceLog("IsOperateReceive IsSelectSkillCheck");
						return false;
					}
					if (!IsChoiceSkillActivate(receiveData, battleCardIdx))
					{
						LocalLog.AccumulateTraceLog("NotEvolChoiceActivate");
						return false;
					}
					break;
				case NetworkBattleDefine.PlayActionType.PLAY_HAND:
				case NetworkBattleDefine.PlayActionType.PLAY_HAND_SELECT:
				{
					if (receiveData.keyActionType.Contains(SendKeyActionDataManager.KeyActionType.ChoiceBrave))
					{
						break;
					}
					CardDataModel playCard = networkBattleData.GetPlayCard();
					if (playCard == null)
					{
						LocalLog.AccumulateTraceLog("IsOperateReceive cardData null");
						return false;
					}
					BattleCardBase indexToCardBase = NetworkBattleGenericTool.GetIndexToCardBase(_battleMgr, _battleMgr.BattleEnemy, playCard.Index);
					if (indexToCardBase == null)
					{
						LocalLog.AccumulateTraceLog("IsOperateReceive playCard null");
						return false;
					}
					if (NetworkBattleGenericTool.GetCardPlaceState(_battleMgr.BattleEnemy, playCard.Index) != NetworkBattleDefine.NetworkCardPlaceState.Hand)
					{
						LocalLog.AccumulateTraceLog(string.Concat("CardID" + indexToCardBase.CardId + "Idx" + indexToCardBase.Index + " ", "IsOperateReceive notHandCard"));
						return false;
					}
					if (receiveData.IsAcceleratedOrCrystallize)
					{
						int cardId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(receiveData.transformBeforeCardId).CardId;
						int num = (CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardId).IsFoil ? 1 : 0);
						string[] array = SkillCreator.SplitBothSkillText(CardMaster.GetInstanceForBattle().GetCardParameterFromId(cardId).SkillOption)[0].Split(',');
						for (int i = 0; i < array.Length; i++)
						{
							string elementAtString = SkillCreator.GetElementAtString(array, i);
							if (elementAtString.Contains("card_id="))
							{
								elementAtString = elementAtString.Replace("card_id=", "");
								if (receiveData.mutationAfterCardId == int.Parse(elementAtString) + num)
								{
									break;
								}
								if (i == array.Length - 1)
								{
									LocalLog.AccumulateTraceLog("Operate Receive Mutation MistakeID");
									return false;
								}
							}
						}
						if (!indexToCardBase.CheckConditionFixedUseCost(isPrePlay: true) || indexToCardBase.CalcFixedUseCost(indexToCardBase.SelfBattlePlayer.Pp) >= indexToCardBase.Cost)
						{
							LocalLog.AccumulateTraceLog("accelerated CostError");
							return false;
						}
					}
					else if (!TouchControl.IsPlayCard(_battleMgr.BattleEnemy, indexToCardBase, isDebugLog: true))
					{
						LocalLog.AccumulateTraceLog("NotPlayCard");
						return false;
					}
					if (!IsSelectSkillCheck(receiveData, indexToCardBase, isEvol: false))
					{
						LocalLog.AccumulateTraceLog("IsOperateReceive IsSelectSkillCheck");
						return false;
					}
					if (!IsChoiceSkillActivate(receiveData, indexToCardBase))
					{
						LocalLog.AccumulateTraceLog("NotPlayChoiceActivate");
						return false;
					}
					break;
				}
				}
			}
		}
		catch
		{
			return false;
		}
		return true;
	}

	private bool IsSelectSkillCheck(NetworkBattleReceiver.ReceiveData receiveData, BattleCardBase playCard, bool isEvol)
	{
		if (receiveData.OpponentTargetDataList.Count() == 0)
		{
			return true;
		}
		SkillCollectionBase skillCollectionBase = null;
		skillCollectionBase = (isEvol ? playCard.EvolutionSkills : playCard.Skills);
		BattlePlayerPair battlePlayerPair = _battleMgr.GetBattlePlayerPair(isPlayer: false);
		if (receiveData.OpponentTargetDataList.Count() == 0 && skillCollectionBase.Any())
		{
			IEnumerable<SkillBase> enumerable = skillCollectionBase.Where(delegate(SkillBase s)
			{
				bool flag = s.ApplyingTargetFilter is SkillTargetInPlayFilter || s.ApplyingTargetFilter is SkillTargetInPlayOtherSelfFilter || s.ApplyingTargetFilter is SkillTargetInplaySelfAndClassFilter;
				return s.IsUserSelectType && flag && ActionProcessor.GetSkillUserSelectableTargets(s, battlePlayerPair) != null && s.CheckCondition(battlePlayerPair, new SkillConditionCheckerOption(), isPrePlay: true);
			});
			if (enumerable != null && enumerable.Any())
			{
				return false;
			}
			return true;
		}
		IEnumerable<SkillBase> source = skillCollectionBase.Where((SkillBase s) => s.IsUserSelectType && !(s is Skill_fusion) && s.CheckCondition(battlePlayerPair, new SkillConditionCheckerOption(), isPrePlay: true));
		SkillBase skillBase = null;
		if (source.Count() > 0)
		{
			skillBase = ((source.Count() > 1) ? source.ToList().Last() : source.ToList().First());
		}
		if (skillBase == null)
		{
			return true;
		}
		IEnumerable<BattleCardBase> skillUserSelectableTargets = ActionProcessor.GetSkillUserSelectableTargets(skillBase, battlePlayerPair);
		if (skillUserSelectableTargets == null)
		{
			LocalLog.AccumulateTraceLog("IsSelectSkillCheck selectableTargets null");
			return false;
		}
		foreach (BattleCardBase item in NetworkBattleGenericTool.GetOpposingCardObjTarget(_battleMgr, receiveData.OpponentTargetDataList))
		{
			if (skillUserSelectableTargets.Contains(item))
			{
				return true;
			}
		}
		LocalLog.AccumulateTraceLog("IsSelectSkillCheck false");
		return false;
	}

	private bool IsChoiceSkillActivate(NetworkBattleReceiver.ReceiveData receiveData, BattleCardBase cardBase)
	{
		if (receiveData.IsChoice)
		{
			CardParameter cardParameterFromId = CardMaster.GetInstanceForBattle().GetCardParameterFromId(receiveData.transformBeforeCardId);
			BattleCardBase battleCardBase = _battleMgr.CreateBattleCard(cardParameterFromId.BaseCardId, cardBase.IsPlayer, null, cardParameterFromId, cardBase.SelfBattlePlayer, 0);
			foreach (CardDataModel item in receiveData.SkillConditionCheckList.FindAll((CardDataModel x) => x.publishedActiveSkillCount != 0))
			{
				if (item.publishedActiveSkillCount == -1)
				{
					continue;
				}
				foreach (SkillBase skill in battleCardBase.Skills)
				{
					if (skill.ApplyingTargetFilter is SkillTargetChosenCardsFilter)
					{
						Object.DestroyImmediate(battleCardBase.BattleCardView.GameObject);
						battleCardBase = null;
						return item.activate == 1;
					}
				}
			}
			Object.DestroyImmediate(battleCardBase.BattleCardView.GameObject);
			battleCardBase = null;
		}
		return true;
	}
}
