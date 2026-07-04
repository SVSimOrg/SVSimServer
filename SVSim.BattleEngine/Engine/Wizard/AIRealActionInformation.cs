using System;
using System.Collections.Generic;
using System.Linq;
using Wizard.Battle.View.Vfx;

namespace Wizard;

public class AIRealActionInformation
{
	public BattleCardBase OriginalActor;

	public BattleCardBase Actor;

	public List<BattleCardBase> SelectedTargetCardSet;

	public List<BattleCardRealTargetInformation> RealTargetInformationList;

	public AIOperationType OperationType;

	private AIVirtualField.AIVirtualFieldSearchCardOption _searchTargetOption;

	public AIRealActionInformation(AIOperationType type, BattleCardBase actCard, BattleCardBase originalCard, IEnumerable<BattleCardBase> targetCards)
	{
		OperationType = type;
		OriginalActor = originalCard;
		if (actCard == null)
		{
			Actor = OriginalActor;
		}
		else
		{
			Actor = actCard;
		}
		SelectedTargetCardSet = targetCards?.ToList();
		_searchTargetOption = new AIVirtualField.AIVirtualFieldSearchCardOption
		{
			IsSearchFromDeck = false,
			IsOutputCannotFindError = false
		};
		RealTargetInformationList = null;
	}

	public AISituationInfo CreateSituationInfo(AIVirtualField field)
	{
		AIVirtualCard aIVirtualCard = field.SearchVirtualCard(OriginalActor);
		if (aIVirtualCard == null)
		{
			AIConsoleUtility.LogError("AIRealActionInformation.CreateSituationInfo() error!! virtualActor is not found");
			return null;
		}
		if (!aIVirtualCard.IsAlly && aIVirtualCard is EnemyHandVirtualCard)
		{
			aIVirtualCard = new AIVirtualCard(aIVirtualCard, field);
		}
		AIVirtualCard aIVirtualCard2 = aIVirtualCard;
		if (OriginalActor != Actor)
		{
			aIVirtualCard2 = new AIVirtualCard(Actor, field);
			aIVirtualCard2.InitializeTags(field.ParamQuery, null, null);
		}
		AISituationInfo aISituationInfo;
		switch (OperationType)
		{
		case AIOperationType.TURNEND:
			aISituationInfo = new AIVirtualTurnEndInfo(aIVirtualCard);
			break;
		case AIOperationType.ATTACK:
			aISituationInfo = GetTargetRegisteredAttackSituation(aIVirtualCard, field);
			break;
		case AIOperationType.EVOLVE:
		case AIOperationType.PLAY:
		case AIOperationType.FUSION:
			aISituationInfo = GetTargetRegisteredTargetSelectSituation(aIVirtualCard2, aIVirtualCard, field);
			break;
		default:
			AIConsoleUtility.LogError("AIRealActionInformation.CreateSituationInfo() error!! OperationType " + OperationType.ToString() + " is illegal");
			return null;
		}
		if (aISituationInfo != null && RealTargetInformationList != null)
		{
			aISituationInfo.RegisterRealTargetInfo(RealTargetInformationList);
		}
		return aISituationInfo;
	}

	private AIVirtualAttackInfo GetTargetRegisteredAttackSituation(AIVirtualCard attacker, AIVirtualField field)
	{
		if (SelectedTargetCardSet == null || SelectedTargetCardSet.Count <= 0)
		{
			AIConsoleUtility.LogError("AIRealActionInformation.GetTargetRegisteredAttackSituation error!! Attack situation cannot find target!!!!!");
			return null;
		}
		AIVirtualCard target = field.SearchVirtualCard(SelectedTargetCardSet.First());
		return new AIVirtualAttackInfo(attacker, target);
	}

	private AIVirtualTargetSelectAction GetTargetRegisteredTargetSelectSituation(AIVirtualCard playActor, AIVirtualCard originalCard, AIVirtualField field)
	{
		AIVirtualTargetSelectAction aIVirtualTargetSelectAction = new AIVirtualTargetSelectAction(playActor, originalCard, OperationType, (AISelectedTargetInfoSet)null);
		if (SelectedTargetCardSet == null || SelectedTargetCardSet.Count <= 0)
		{
			return aIVirtualTargetSelectAction;
		}
		AIVirtualCard owner = playActor;
		if (!playActor.IsSameCard(originalCard) && aIVirtualTargetSelectAction.IsChoiceAndChangeActor(field))
		{
			owner = originalCard;
		}
		List<AIVirtualTargetSelectInfo> list = owner.CreateAIVirtualSelectInfo(field, aIVirtualTargetSelectAction);
		if (list == null || list.Count <= 0)
		{
			AIConsoleUtility.LogError("AIRealActionInformation.CreateSituationInfo error!! Target selection is required while TargetCardSet is empty!!!!! Card ID == " + originalCard.BaseId);
			return aIVirtualTargetSelectAction;
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < list.Count; i++)
		{
			AIVirtualTargetSelectInfo aIVirtualTargetSelectInfo = list[i];
			int num3;
			if (aIVirtualTargetSelectAction.ActionType == AIOperationType.FUSION)
			{
				num3 = SelectedTargetCardSet.Count;
			}
			else
			{
				AIPlayTag selectRule = aIVirtualTargetSelectInfo.SelectRule;
				num3 = ((selectRule != null && selectRule.Type == AIPlayTagType.ChoiceBrave) ? 1 : Math.Min(aIVirtualTargetSelectInfo.Candidates.Count, aIVirtualTargetSelectInfo.Count));
			}
			List<AIVirtualCard> list2 = new List<AIVirtualCard>();
			if (SelectedTargetCardSet.Count - num < num3)
			{
				AIConsoleUtility.LogError("AIRealActionInformation.CreateSituationInfo error!! Targets are not enough!!!!!");
				break;
			}
			for (int j = 0; j < num3; j++)
			{
				BattleCardBase battleCardBase = SelectedTargetCardSet[num];
				AIVirtualCard aIVirtualCard = field.SearchVirtualCard(battleCardBase, _searchTargetOption);
				if (aIVirtualCard == null)
				{
					aIVirtualCard = new AIVirtualCard(battleCardBase, field);
					aIVirtualCard.InitializeTags(field.ParamQuery, null, null);
				}
				list2.Add(aIVirtualCard);
				num++;
			}
			if (aIVirtualTargetSelectInfo.Type == TargetSelectType.Choice)
			{
				aIVirtualTargetSelectAction.SetChoicedMultipleTargetInInfo(list2);
				continue;
			}
			AIScriptTokenArgType whichTarget = ((num2 != 0) ? AIScriptTokenArgType.SECOND_TARGET_SELECT : AIScriptTokenArgType.TARGET_SELECT);
			aIVirtualTargetSelectAction.SetMultipleTargetsInInfo(list2, aIVirtualTargetSelectInfo.Type, aIVirtualTargetSelectInfo.RemovalType, whichTarget);
			num2++;
		}
		return aIVirtualTargetSelectAction;
	}

	public void CreateRandomTargetInformation(BattleCardBase actor, Func<SkillBase, bool> skillCheckFunc, bool forceCheckEvolveSkills = false)
	{
		BattleCardRealTargetInformation information = GetRealRandomTargetInformation(actor);
		SkillCollectionBase skillCollectionBase = (forceCheckEvolveSkills ? actor.EvolutionSkills : actor.Skills);
		int num = skillCollectionBase.Count();
		for (int i = 0; i < num; i++)
		{
			SkillBase skillBase = skillCollectionBase.Get(i);
			if (skillCheckFunc(skillBase))
			{
				skillBase.OnSkillEnd += OnSkillEndRegistTargets;
			}
		}
		VfxBase OnSkillEndRegistTargets(SkillBase currentSkill, List<BattleCardBase> skillTargets, SkillConditionCheckerOption option, SkillProcessor skillProcessor)
		{
			if (skillTargets != null && skillTargets.Count > 0)
			{
				information.AddTargetList(skillTargets, currentSkill.ApplyingTargetFilter);
			}
			currentSkill.OnSkillEnd -= OnSkillEndRegistTargets;
			return NullVfx.GetInstance();
		}
	}

	private BattleCardRealTargetInformation GetRealRandomTargetInformation(BattleCardBase skillOwner)
	{
		if (RealTargetInformationList == null)
		{
			RealTargetInformationList = new List<BattleCardRealTargetInformation>();
		}
		BattleCardRealTargetInformation battleCardRealTargetInformation = null;
		for (int i = 0; i < RealTargetInformationList.Count; i++)
		{
			BattleCardRealTargetInformation battleCardRealTargetInformation2 = RealTargetInformationList[i];
			BattleCardBase skillOwner2 = battleCardRealTargetInformation2.SkillOwner;
			if (skillOwner.IsPlayer == skillOwner2.IsPlayer && skillOwner.Index == skillOwner2.Index && skillOwner.BaseParameter.BaseCardId == skillOwner2.BaseParameter.BaseCardId)
			{
				battleCardRealTargetInformation = battleCardRealTargetInformation2;
				break;
			}
		}
		if (battleCardRealTargetInformation == null)
		{
			battleCardRealTargetInformation = new BattleCardRealTargetInformation(skillOwner);
			RealTargetInformationList.Add(battleCardRealTargetInformation);
		}
		return battleCardRealTargetInformation;
	}
}
