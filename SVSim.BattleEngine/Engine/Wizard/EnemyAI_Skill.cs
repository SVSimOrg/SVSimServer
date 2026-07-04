using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cute;
using UnityEngine;
using Wizard.Battle;
using Wizard.Battle.Card;
using Wizard.Battle.Touch;

namespace Wizard;

public class EnemyAI_Skill
{

	public static float PLAYOUT_VALUE = 9999f;

	private EnemyAI _ai;

	private BattleCardBase _Cr_CurrentSkillTarget;

	private List<BattleCardBase> _Cr_SkillTargets;

	private List<AISelectedTargetInfo> SelectedTargetInfoList;

	private int _Cr_TargetIndex;

	private List<int> _skillTargetCountList;

	private List<BattleCardBase> _preDecidedTarget;

	public EnemyAI_Skill(EnemyAI ai)
	{
		_ai = ai;
	}

	public void SetPreDecidedTarget(List<BattleCardBase> targets)
	{
		_preDecidedTarget = targets;
	}

	public void ClearPreDecidedTarget()
	{
		_preDecidedTarget = null;
	}

	public IEnumerator _Cr_SelectSkillTarget(AIVirtualCard actCard, AIOperationType operationType, AISinglePlayptnRecord playPtnRecord)
	{
		BattleCardBase operateCard = actCard.BaseCard;
		_Cr_SkillTargets = new List<BattleCardBase>();
		SelectedTargetInfoList = new List<AISelectedTargetInfo>();
		_skillTargetCountList = new List<int>();
		_Cr_TargetIndex = 0;
		int num = 0;
		BattleCardBase skillActivator = null;
		IEnumerable<SkillBase> activatedSkills = GetActivatedSelectSkills(operateCard, operationType == AIOperationType.EVOLVE, out skillActivator);
		if (activatedSkills == null || activatedSkills.Count() < 1)
		{
			_ai.OprTargetSelect(actCard, null, operationType);
			yield break;
		}
		foreach (SkillBase item in activatedSkills)
		{
			num = ((!item.IsChoiceType) ? (num + 1) : (num + ChoiceUtility.GetNumberOfCardsToSelect(item)));
		}
		WaitForSeconds wait = new WaitForSeconds(0.2f);
		do
		{
			yield return wait;
		}
		while (_ai.BattleMgr.IsBattleEnd || !_ai.BattleMgr.VfxMgr.IsEnd);
		yield return ExecuteSelectTargets(skillActivator, operationType == AIOperationType.EVOLVE, activatedSkills, operateCard, playPtnRecord);
		do
		{
			yield return wait;
		}
		while (!_ai._isSetCardReady && operationType != AIOperationType.EVOLVE);
		AISelectedTargetInfoSet aISelectedTargetInfoSet = new AISelectedTargetInfoSet();
		for (int i = 0; i < SelectedTargetInfoList.Count && i < AISelectedTargetInfoSet.LENGTH; i++)
		{
			aISelectedTargetInfoSet.Set(SelectedTargetInfoList[i], i);
		}
		_ai.OprTargetSelect(actCard, aISelectedTargetInfoSet, operationType);
	}

	private IEnumerator ExecuteSelectTargets(BattleCardBase actCard, bool isEvol, IEnumerable<SkillBase> selectSkills, BattleCardBase realOperateCard, AISinglePlayptnRecord playPtnRecord)
	{
		List<Tuple<SkillBase, IEnumerable<BattleCardBase>>> skillAndTargetsList = new List<Tuple<SkillBase, IEnumerable<BattleCardBase>>>();
		AIVirtualCard virtualActCard = _ai.CurrentVirtualField.SearchVirtualCard(realOperateCard);
		foreach (SkillBase selectSkill in selectSkills)
		{
			IEnumerable<BattleCardBase> burialSelectableCards = AIBurialRiteSimulationUtility.GetBurialSelectableCards(selectSkill, realOperateCard);
			IEnumerable<BattleCardBase> selectableCards = _ai.ParamQuery.GetSelectableCards(selectSkill, _ai.PlayerPair, isSkipForceSelect: true);
			if (burialSelectableCards.IsNotNullOrEmpty() || selectableCards.IsNotNullOrEmpty())
			{
				if (burialSelectableCards.IsNotNullOrEmpty())
				{
					skillAndTargetsList.Add(new Tuple<SkillBase, IEnumerable<BattleCardBase>>(selectSkill, burialSelectableCards));
				}
				if (selectableCards.IsNotNullOrEmpty())
				{
					skillAndTargetsList.Add(new Tuple<SkillBase, IEnumerable<BattleCardBase>>(selectSkill, selectableCards));
				}
				continue;
			}
			yield break;
		}
		SkillBase skillBase = null;
		int beforeSkillsSelectedInfoOffset = SelectedTargetInfoList.Count;
		foreach (Tuple<SkillBase, IEnumerable<BattleCardBase>> item in skillAndTargetsList)
		{
			int num = ((!item.first.IsChoiceType) ? Mathf.Min(item.first.GetSkillSelectCount(), item.second.Count()) : ChoiceUtility.GetNumberOfCardsToSelect(item.first));
			_skillTargetCountList.Add(num);
			TargetSelectType targetSelectType = TargetSelectType.Default;
			if (item.first is Skill_choice)
			{
				targetSelectType = TargetSelectType.Choice;
			}
			else if (item.first.IsBurialRite && skillBase != null && skillBase == item.first)
			{
				targetSelectType = TargetSelectType.BurialRite;
			}
			AISelectedTargetInfo aISelectedTargetInfo = new AISelectedTargetInfo(targetSelectType);
			SelectedTargetInfoList.Add(aISelectedTargetInfo);
			foreach (BattleCardBase item2 in item.second)
			{
				if (!_Cr_SkillTargets.Contains(item2))
				{
					_Cr_SkillTargets.Add(item2);
					AIVirtualCard target = ((targetSelectType == TargetSelectType.Choice) ? new ChoiceVirtualCard(item2, virtualActCard.IsAlly, _ai.CurrentVirtualField) : _ai.CurrentVirtualField.SearchVirtualCard(item2));
					aISelectedTargetInfo.AddTarget(target);
					num--;
					if (num <= 0)
					{
						break;
					}
				}
			}
			skillBase = item.first;
		}
		int skillIndex = 0;
		while (skillIndex < skillAndTargetsList.Count)
		{
			int selectedInfoListIdx = beforeSkillsSelectedInfoOffset + skillIndex;
			Tuple<SkillBase, IEnumerable<BattleCardBase>> skillAndTargets = skillAndTargetsList[skillIndex];
			bool isBurialSelect = skillAndTargets.first.IsBurialRite;
			if (isBurialSelect)
			{
				SkillBase skillBase2 = ((skillIndex == 0) ? null : skillAndTargetsList[skillIndex - 1].first);
				if (skillBase2 != null)
				{
					isBurialSelect = skillBase2 != skillAndTargets.first;
				}
			}
			int skillTargetCount = _skillTargetCountList[skillIndex];
			if (_preDecidedTarget != null && _preDecidedTarget.Count > 0 && _preDecidedTarget.Count - _Cr_TargetIndex > 0 && _preDecidedTarget.Count - _Cr_TargetIndex - skillTargetCount >= 0)
			{
				AISelectedTargetInfo aISelectedTargetInfo2 = new AISelectedTargetInfo(isBurialSelect ? TargetSelectType.BurialRite : ((skillAndTargets.first is Skill_choice) ? TargetSelectType.Choice : TargetSelectType.Default));
				for (int i = 0; i < skillTargetCount; i++)
				{
					_Cr_SkillTargets[_Cr_TargetIndex] = _preDecidedTarget[_Cr_TargetIndex];
					if (aISelectedTargetInfo2.Type == TargetSelectType.Choice)
					{
						aISelectedTargetInfo2.AddTarget(new ChoiceVirtualCard(_preDecidedTarget[_Cr_TargetIndex], virtualActCard.IsAlly, _ai.CurrentVirtualField));
					}
					else
					{
						aISelectedTargetInfo2.AddTarget(_ai.CurrentVirtualField.SearchVirtualCard(_preDecidedTarget[_Cr_TargetIndex]));
					}
					_Cr_TargetIndex++;
				}
				SelectedTargetInfoList[selectedInfoListIdx] = aISelectedTargetInfo2;
			}
			else
			{
				_Cr_CurrentSkillTarget = null;
				if (skillAndTargets.first is Skill_choice)
				{
					List<int> playPtn = ((playPtnRecord == null) ? EnemyAI.EmptyPlayPtn : playPtnRecord.PlayPtn);
					List<BattleCardBase> condChoiceTargets = virtualActCard.GetCondChoiceTargets(_ai.CurrentVirtualField, playPtn, skillAndTargets.second);
					if (condChoiceTargets != null && condChoiceTargets.Count > 0)
					{
						int numberOfCardsToSelect = ChoiceUtility.GetNumberOfCardsToSelect(skillAndTargets.first);
						_ = condChoiceTargets.Count;
						BattleCardBase battleCardBase = null;
						AISelectedTargetInfo aISelectedTargetInfo3 = new AISelectedTargetInfo(TargetSelectType.Choice);
						for (int j = 0; j < numberOfCardsToSelect; j++)
						{
							if (j < condChoiceTargets.Count)
							{
								battleCardBase = condChoiceTargets[j];
							}
							if (battleCardBase != null && skillAndTargets.second.Contains(battleCardBase))
							{
								_Cr_SkillTargets[_Cr_TargetIndex] = battleCardBase;
								_Cr_TargetIndex++;
								aISelectedTargetInfo3.AddTarget(new ChoiceVirtualCard(battleCardBase, virtualActCard.IsAlly, _ai.CurrentVirtualField));
							}
						}
						SelectedTargetInfoList[selectedInfoListIdx] = aISelectedTargetInfo3;
						if (actCard.Skills.Any((SkillBase s) => s is Skill_transform) && battleCardBase is IVirtualBattleCard && battleCardBase.Skills.CheckWhenPlaySelectTargetSkillCondition)
						{
							IEnumerable<SkillBase> selectTypeSkill = battleCardBase.GetSelectTypeSkill(isEvol);
							yield return ExecuteSelectTargets(battleCardBase, isEvol, selectTypeSkill, realOperateCard, playPtnRecord);
						}
					}
				}
				else
				{
					AISelectedTargetInfo targetInfo = new AISelectedTargetInfo(isBurialSelect ? TargetSelectType.BurialRite : TargetSelectType.Default);
					for (int i2 = 0; i2 < skillTargetCount; i2++)
					{
						_Cr_SkillTargets[_Cr_TargetIndex] = null;
						yield return DecideBestSkillTarget(actCard, skillAndTargets.first, skillAndTargets.second, isEvol, isBurialSelect, _Cr_SkillTargets, _Cr_TargetIndex, realOperateCard, playPtnRecord);
						_Cr_SkillTargets[_Cr_TargetIndex] = _Cr_CurrentSkillTarget;
						_Cr_TargetIndex++;
						AIVirtualCard target2 = _ai.CurrentVirtualField.SearchVirtualCard(_Cr_CurrentSkillTarget);
						targetInfo.AddTarget(target2);
					}
					SelectedTargetInfoList[selectedInfoListIdx] = targetInfo;
				}
			}
			int num2 = skillIndex + 1;
			skillIndex = num2;
		}
	}

	private IEnumerator DecideBestSkillTarget(BattleCardBase actCard, SkillBase skill, IEnumerable<BattleCardBase> selectableCards, bool isEvol, bool isBurialSelect, List<BattleCardBase> selectedTargets, int selectIndex, BattleCardBase realOperateCard, AISinglePlayptnRecord playPtnRecord)
	{
		IEnumerable<BattleCardBase> enumerable = _ai.ParamQuery.RemoveDuplicatedCards(selectableCards, selectedTargets);
		if (enumerable.Count() > 0)
		{
			selectableCards = enumerable;
		}
		AIVirtualField currentVirtualField = _ai.CurrentVirtualField;
		AIVirtualCard virtualActCard = GetVirtualActCard(actCard, realOperateCard);
		List<int> playPtn = ((playPtnRecord == null) ? EnemyAI.EmptyPlayPtn : playPtnRecord.PlayPtn);
		List<AIVirtualCard> list = AITargetSelectFilteringUtility.ExecuteTargetFilteringTagToRealCardList(virtualActCard, selectableCards, currentVirtualField, playPtn);
		selectableCards = list.Select((AIVirtualCard c) => c.BaseCard);
		AIOperationType aIOperationType = (isEvol ? AIOperationType.EVOLVE : AIOperationType.PLAY);
		AIVirtualCard sourceCard = virtualActCard;
		if (aIOperationType == AIOperationType.PLAY)
		{
			sourceCard = virtualActCard.FindRealActor(playPtnRecord);
		}
		AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(sourceCard, virtualActCard, aIOperationType);
		if (isBurialSelect && virtualActCard.TagCollectionContainer.HasTag(AIPlayTagType.BurialRite))
		{
			AIVirtualCard bestBurialRiteTargetForOperationSimulator = AIBurialRiteSimulationUtility.GetBestBurialRiteTargetForOperationSimulator(virtualActCard, currentVirtualField, _ai.BestPlayPtn, situation, list);
			if (bestBurialRiteTargetForOperationSimulator != null)
			{
				_Cr_CurrentSkillTarget = bestBurialRiteTargetForOperationSimulator.BaseCard;
				yield break;
			}
		}
		if ((_ai.ParamQuery.GetLogicLv() == AI_LOGIC_LV.STRONG || _ai.ParamQuery.GetLogicLv() == AI_LOGIC_LV.MIDDLE) && (skill is Skill_powerup || skill is Skill_power_down || skill is Skill_attach_skill || skill is Skill_return_card || skill is Skill_destroy || skill is Skill_banish || skill is Skill_metamorphose || skill is Skill_damage || skill is Skill_select))
		{
			yield return EnemyAICoroutine.GetInstance().StartCoroutine(_Cr_SelectSkillTarget_WithSim(actCard, skill, selectableCards, isEvol, selectedTargets, selectIndex, realOperateCard));
		}
		if (skill is Skill_cant_attack)
		{
			_Cr_CurrentSkillTarget = selectableCards.FindMax((BattleCardBase c) => c.Atk * ((!_ai.IsAllyCard(c)) ? 1 : (-1)));
		}
		if (skill is Skill_heal)
		{
			BattleCardBase battleCardBase = _ai.PlayerPair.Self.Class;
			if (selectableCards.Contains(battleCardBase))
			{
				if (_ai.ParamQuery.CalcAllyUnitTotalDamage() < 1)
				{
					_Cr_CurrentSkillTarget = battleCardBase;
				}
			}
			else
			{
				yield return EnemyAICoroutine.GetInstance().StartCoroutine(_Cr_SelectSkillTarget_WithSim(actCard, skill, selectableCards, isEvol, selectedTargets, selectIndex, realOperateCard));
			}
		}
		if (_Cr_CurrentSkillTarget == null)
		{
			yield return EnemyAICoroutine.GetInstance().StartCoroutine(_Cr_SelectSkillTarget_WithSim(actCard, skill, selectableCards, isEvol, selectedTargets, selectIndex, realOperateCard));
		}
		if (_Cr_CurrentSkillTarget == null)
		{
			_Cr_CurrentSkillTarget = selectableCards.ElementAt(0);
		}
	}

	private IEnumerator _Cr_SelectSkillTarget_WithSim(BattleCardBase actCard, SkillBase skill, IEnumerable<BattleCardBase> selectableCards, bool isEvol, List<BattleCardBase> selectedTargets, int selectIndex, BattleCardBase realOperateCard)
	{
		SkillCollectionBase.SetupOptionValue(skill.OptionValue, _ai.PlayerPair, actCard, skill);
		float maxValue = float.MinValue;
		float maxValueAtSelfTurnEnd = float.MinValue;
		float maxValueFirstSkill = float.MinValue;
		BattleCardBase maxTarget = null;
		List<BattleCardBase> targets = new List<BattleCardBase>(selectedTargets);
		IEnumerable<BattleCardBase> enumerable = selectableCards;
		IEnumerable<IReadOnlyBattleCardInfo> enumerable2 = skill.FilteringForceSelectTargets(selectableCards);
		if (enumerable2 != null && enumerable2.Count() > 0)
		{
			enumerable = enumerable2.Cast<BattleCardBase>();
		}
		List<ulong> selectedHashList = new List<ulong>();
		foreach (BattleCardBase target in enumerable)
		{
			bool flag = false;
			for (int i = 0; i < targets.Count; i++)
			{
				if (i != selectIndex && targets[i].IsPlayer == target.IsPlayer && targets[i].Index == target.Index)
				{
					flag = true;
					break;
				}
			}
			AIVirtualCard virtualCard = _ai.CurrentVirtualField.SearchVirtualCard(target);
			ulong hash = virtualCard.GetHash();
			if (selectedHashList.Contains(hash))
			{
				flag = true;
			}
			if (flag)
			{
				continue;
			}
			targets[selectIndex] = target;
			selectedHashList.Add(hash);
			SimulationResult ref_skillSim = new SimulationResult();
			yield return EnemyAICoroutine.GetInstance().StartCoroutine(SimulateSkillWithTarget(_ai.PlayerPair, actCard, targets, isEvol, selectedTargets.Count <= 1, ref_skillSim, realOperateCard));
			if (ref_skillSim.MaxFieldValue >= PLAYOUT_VALUE)
			{
				_Cr_CurrentSkillTarget = target;
				yield break;
			}
			if (skill is Skill_return_card)
			{
				ref_skillSim.MaxFieldValue += (float)(virtualCard.IsAlly ? 1 : (-1)) * (virtualCard.GetBounceBonus() + virtualCard.EvaluateLeaveValue(_ai.CurrentVirtualField.BestPlayPtn, useIgnoreInBattle: true));
			}
			else if (!(skill is Skill_destroy))
			{
				if (skill is Skill_banish)
				{
					ref_skillSim.MaxFieldValue += (float)((!virtualCard.IsAlly) ? 1 : (-1)) * (virtualCard.EvaluateBreakValue(_ai.CurrentVirtualField.BestPlayPtn, useIgnoreBreak: true) - virtualCard.GetAllBanishBonus(_ai.CurrentVirtualField.BestPlayPtn, useIgnoreInBattle: true) - virtualCard.EvaluateLeaveValue(_ai.CurrentVirtualField.BestPlayPtn, useIgnoreInBattle: true));
				}
				else if (skill is Skill_metamorphose)
				{
					ref_skillSim.MaxFieldValue += (float)((!virtualCard.IsAlly) ? 1 : (-1)) * (virtualCard.EvaluateBreakValue(_ai.CurrentVirtualField.BestPlayPtn, useIgnoreBreak: true) + virtualCard.GetAllBanishBonus(_ai.CurrentVirtualField.BestPlayPtn, useIgnoreInBattle: true));
				}
			}
			if (EnemyAI.IsSameValue(ref_skillSim.MaxFieldValue, maxValue))
			{
				if (ref_skillSim.MaxFieldValueFirstSkill > maxValueFirstSkill)
				{
					maxValue = ref_skillSim.MaxFieldValue;
					maxValueFirstSkill = ref_skillSim.MaxFieldValueFirstSkill;
					maxValueAtSelfTurnEnd = ref_skillSim.MaxFieldValueAtSelfTurnEnd;
					maxTarget = target;
				}
				else if (ref_skillSim.MaxFieldValueAtSelfTurnEnd > maxValueAtSelfTurnEnd)
				{
					maxValue = ref_skillSim.MaxFieldValue;
					maxValueFirstSkill = ref_skillSim.MaxFieldValueFirstSkill;
					maxValueAtSelfTurnEnd = ref_skillSim.MaxFieldValueAtSelfTurnEnd;
					maxTarget = target;
				}
			}
			else if (ref_skillSim.MaxFieldValue > maxValue)
			{
				maxValue = ref_skillSim.MaxFieldValue;
				maxValueFirstSkill = ref_skillSim.MaxFieldValueFirstSkill;
				maxValueAtSelfTurnEnd = ref_skillSim.MaxFieldValueAtSelfTurnEnd;
				maxTarget = target;
			}
		}
		_Cr_CurrentSkillTarget = maxTarget;
	}

	private IEnumerator SimulateSkillWithTarget(BattlePlayerPair sourcePair, BattleCardBase actCard, List<BattleCardBase> targets, bool isEvol, bool simuNextPlay, SimulationResult ref_skillSim, BattleCardBase realOperateCard = null)
	{
		AIOperationSimulatorAccessor operationSim = new AIOperationSimulatorAccessor(_ai, _ai.CurrentVirtualField);
		float maxValue = float.MinValue;
		float maxValueAtSelfTurnEnd = float.MinValue;
		if (realOperateCard == null)
		{
			realOperateCard = actCard;
		}
		BattlePlayerPair virtualPair;
		if (isEvol)
		{
			List<int> playPtn = ((_ai.PlaySkipInfo != null) ? _ai.BestPlayPtn : _ai.CurrentVirtualField.BestPlayPtn);
			virtualPair = operationSim.CallEvolve(sourcePair, realOperateCard, targets, playPtn);
		}
		else
		{
			virtualPair = operationSim.CallPlay(sourcePair, realOperateCard, targets, _ai.CurrentVirtualField.BestPlayPtn);
		}
		AIVirtualField field = operationSim.CurrentField;
		yield return null;
		if (virtualPair.Opponent.Class.Life <= 0)
		{
			ref_skillSim.MaxFieldValue = PLAYOUT_VALUE;
			yield break;
		}
		if (simuNextPlay)
		{
			yield return EnemyAICoroutine.GetInstance().StartCoroutine(SimulateSkillWithTargetNext(virtualPair, isEvol, ref_skillSim, operationSim));
			if (ref_skillSim.MaxFieldValue >= PLAYOUT_VALUE)
			{
				yield break;
			}
			maxValue = ref_skillSim.MaxFieldValue;
			maxValueAtSelfTurnEnd = ref_skillSim.MaxFieldValueAtSelfTurnEnd;
		}
		if (isEvol)
		{
			AIVirtualCard aIVirtualCard = field.AllyInplayCards.Find((AIVirtualCard c) => c.IsUnit && c.CardIndex == actCard.Index);
			if (aIVirtualCard != null)
			{
				aIVirtualCard.IsUseEvo = true;
				field.EvoUsedCard = aIVirtualCard;
				if (!aIVirtualCard.IsNotConsumeEp)
				{
					field.AllyEvolutionCount--;
				}
				AIVirtualFieldBuildParameterCollction buildParameters = new AIVirtualFieldBuildParameterCollction(_ai.CurrentVirtualField);
				AIVirtualCard aIVirtualCard2 = AIVirtualField.CreateTemporaryVirtualField(_ai, _ai.ParamQuery, _ai.StyleQuery, sourcePair, new List<int>(), buildParameters).SearchVirtualCard(actCard);
				AIVirtualTargetSelectAction situation = new AIVirtualTargetSelectAction(aIVirtualCard2, aIVirtualCard2, AIOperationType.EVOLVE);
				field.EpValue = _ai.StyleQuery.GetEpValue(situation, field.BestPlayPtn);
			}
		}
		field.AllyClass.DefLife = _ai.CurrentVirtualField.AllyClass.Life;
		field.EnemyClass.DefLife = _ai.CurrentVirtualField.EnemyClass.Life;
		AIBattleSimulationLauncher battleSimLauncher = new AIBattleSimulationLauncher(field, _ai, _ai.IsEvoPermissionOnSimu && !isEvol);
		yield return battleSimLauncher.ExecuteBattleSimulationAI(null, checkTimeOverLogic: true);
		IBattleSimulationAI battleSimAI = battleSimLauncher.BattleSimAI;
		ref_skillSim.IsLethalPlan = battleSimAI.Cr_MaxField.EnemyClass.IsDead;
		ref_skillSim.MaxFieldValue = battleSimAI.Cr_MaxFieldValue;
		ref_skillSim.MaxFieldValueAtSelfTurnEnd = battleSimAI.Cr_MaxFieldValueAtSelfTurnEnd;
		ref_skillSim.MaxFieldValueFirstSkill = ref_skillSim.MaxFieldValue;
		if (maxValue > ref_skillSim.MaxFieldValue || (EnemyAI.IsSameValue(maxValue, ref_skillSim.MaxFieldValue) && maxValueAtSelfTurnEnd > ref_skillSim.MaxFieldValueAtSelfTurnEnd))
		{
			ref_skillSim.MaxFieldValue = maxValue;
			ref_skillSim.MaxFieldValueAtSelfTurnEnd = maxValueAtSelfTurnEnd;
		}
	}

	private IEnumerator SimulateSkillWithTargetNext(BattlePlayerPair virtualPair, bool isEvol, SimulationResult ref_skillSim, AIOperationSimulatorAccessor operationSim)
	{
		float maxValue = float.MinValue;
		float maxValueAtSelfTurnEnd = float.MinValue;
		if (_ai.BestPlayPtn != null)
		{
			BattleCardBase virtualNextPlayCard = null;
			for (int i = 0; i < 2; i++)
			{
				if (_ai.BestPlayPtn.Count > i)
				{
					BattleCardBase nextPlayCard = _ai.PlayerPair.Self.HandCardList[_ai.BestPlayPtn[i]];
					virtualNextPlayCard = virtualPair.Self.HandCardList.FirstOrDefault((BattleCardBase card) => card.Index == nextPlayCard.Index);
					if (virtualNextPlayCard != null)
					{
						break;
					}
				}
			}
			if (virtualNextPlayCard != null)
			{
				IEnumerable<SkillBase> selectTypeSkill = virtualNextPlayCard.GetSelectTypeSkill();
				int num = selectTypeSkill.Count((SkillBase s) => s.IsUserSelectType);
				int num2 = selectTypeSkill.Count((SkillBase s) => s.IsBurialRite);
				if (num + num2 <= 1 && _ai.PlayerPair.Self.InPlayCards.Count((BattleCardBase card) => card.IsUnit) <= 3 && virtualNextPlayCard.Skills.CheckWhenPlaySelectTargetSkillCondition)
				{
					IEnumerable<BattleCardBase> selectSkillTargetCandidates = _ai.ParamQuery.GetSelectSkillTargetCandidates(virtualNextPlayCard, virtualPair);
					if (selectSkillTargetCandidates != null && selectSkillTargetCandidates.Count() > 0)
					{
						List<BattleCardBase> nextSelectCandidatesList = selectSkillTargetCandidates.ToList();
						int nextIndex = 0;
						while (nextIndex < nextSelectCandidatesList.Count)
						{
							BattleCardBase item = nextSelectCandidatesList[nextIndex];
							List<BattleCardBase> skillTargets = new List<BattleCardBase> { item };
							BattlePlayerPair nextVirtualPair = operationSim.CallPlay(virtualPair, virtualNextPlayCard, skillTargets, operationSim.CurrentField.BestPlayPtn);
							yield return null;
							if (nextVirtualPair.Opponent.Class.Life <= 0)
							{
								ref_skillSim.IsLethalPlan = true;
								ref_skillSim.MaxFieldValue = PLAYOUT_VALUE;
								yield break;
							}
							AIVirtualField currentField = operationSim.CurrentField;
							currentField.AllyClass.DefLife = _ai.CurrentVirtualField.AllyClass.Life;
							currentField.EnemyClass.DefLife = _ai.CurrentVirtualField.EnemyClass.Life;
							AIBattleSimulationLauncher battleSimLauncher = new AIBattleSimulationLauncher(currentField, _ai, _ai.IsEvoPermissionOnSimu && !isEvol);
							yield return battleSimLauncher.ExecuteBattleSimulationAI(null, checkTimeOverLogic: true);
							IBattleSimulationAI battleSimAI = battleSimLauncher.BattleSimAI;
							ref_skillSim.IsLethalPlan = battleSimAI.Cr_MaxField.EnemyClass.IsDead;
							ref_skillSim.MaxFieldValue = battleSimAI.Cr_MaxFieldValue;
							ref_skillSim.MaxFieldValueAtSelfTurnEnd = battleSimAI.Cr_MaxFieldValueAtSelfTurnEnd;
							if (ref_skillSim.MaxFieldValue > maxValue || (EnemyAI.IsSameValue(ref_skillSim.MaxFieldValue, maxValue) && ref_skillSim.MaxFieldValueAtSelfTurnEnd > maxValueAtSelfTurnEnd))
							{
								maxValue = ref_skillSim.MaxFieldValue;
								maxValueAtSelfTurnEnd = ref_skillSim.MaxFieldValueAtSelfTurnEnd;
							}
							yield return null;
							int num3 = nextIndex + 1;
							nextIndex = num3;
						}
					}
				}
			}
		}
		ref_skillSim.MaxFieldValue = maxValue;
		ref_skillSim.MaxFieldValueAtSelfTurnEnd = maxValueAtSelfTurnEnd;
	}

	public IEnumerable<SkillBase> GetActivatedSelectSkills(BattleCardBase operateCard, bool isEvol, out BattleCardBase skillActivator)
	{
		skillActivator = null;
		IEnumerable<SkillBase> enumerable = null;
		BattleCardBase battleCardBase = operateCard;
		Skill_transform accelerateOrCrystallizeTransformSkill = operateCard.GetAccelerateOrCrystallizeTransformSkill();
		if (accelerateOrCrystallizeTransformSkill != null && operateCard.CheckConditionFixedUseCost(isPrePlay: true) && operateCard.CalcFixedUseCost(operateCard.SelfBattlePlayer.Pp) < operateCard.Cost)
		{
			battleCardBase = _ai.BattleMgr.CreateTransformCardRegisterVfx(accelerateOrCrystallizeTransformSkill.SkillPrm.ownerCard, accelerateOrCrystallizeTransformSkill.TransformId, accelerateOrCrystallizeTransformSkill.SkillPrm.ownerCard.IsPlayer);
		}
		enumerable = battleCardBase.GetSelectTypeSkill(isEvol);
		if (enumerable == null || enumerable.Count() < 1)
		{
			return null;
		}
		if (enumerable.Any((SkillBase s) => !AIBurialRiteSimulationUtility.GetBurialSelectableCards(s, operateCard).IsNotNullOrEmpty() && !_ai.ParamQuery.GetSelectableCards(s, _ai.PlayerPair).IsNotNullOrEmpty()))
		{
			return null;
		}
		skillActivator = battleCardBase;
		return enumerable;
	}

	public AIVirtualCard GetVirtualActCard(BattleCardBase actCard, BattleCardBase realOperateCard)
	{
		AIVirtualCard aIVirtualCard;
		if (actCard.CardId != realOperateCard.CardId)
		{
			aIVirtualCard = new AIVirtualCard(actCard, _ai.CurrentVirtualField);
			aIVirtualCard.InitializeTags(_ai.ParamQuery, null, null);
		}
		else
		{
			aIVirtualCard = _ai.CurrentVirtualField.SearchVirtualCard(actCard);
		}
		return aIVirtualCard;
	}
}
