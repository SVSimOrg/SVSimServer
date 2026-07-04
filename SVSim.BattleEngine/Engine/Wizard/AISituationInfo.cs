using System.Collections.Generic;

namespace Wizard;

public abstract class AISituationInfo
{
	public AISelectedTargetInfoSet SelectedTargets;

	public List<AIVirtualCard> BounceCardList;

	public AISimulationPreprocessRecorder PreprocessRecorder;

	public List<BattleCardRealTargetInformation> RealTargetInformationList;

	public AIVirtualCard Actor { get; protected set; }

	public AIVirtualCard OriginalCard { get; protected set; }

	public AIVirtualCard ActionTarget => SelectedTargets.Get(0)?.FirstTarget;

	public AIVirtualCard SecondActionTarget => SelectedTargets.Get(1)?.FirstTarget;

	public AIOperationType ActionType { get; private set; }

	public AIVirtualCard CurrentCheckCard { get; private set; }

	public AIDiscardInfo DiscardInfo { get; private set; }

	public AISkillProcessInfoCollection ProcessCollection { get; private set; }

	public AISkillProcessInformation CurrentSkillProcessInfo { get; private set; }

	public bool IsLatestAction { get; private set; }

	public AISituationInfo(AIVirtualCard actor, AIVirtualCard target, AIVirtualCard secondTarget = null, AIOperationType type = AIOperationType.ATTACK)
	{
		Actor = actor;
		OriginalCard = actor;
		ActionType = type;
		SelectedTargets = new AISelectedTargetInfoSet();
		ProcessCollection = new AISkillProcessInfoCollection();
		if (target != null)
		{
			SelectedTargets.Set(new AISelectedTargetInfo(target, TargetSelectType.Default), 0);
		}
		if (secondTarget != null)
		{
			SelectedTargets.Set(new AISelectedTargetInfo(secondTarget, TargetSelectType.Default), 1);
		}
		PreprocessRecorder = new AISimulationPreprocessRecorder();
		IsLatestAction = false;
		RealTargetInformationList = null;
	}

	public AISituationInfo(AIVirtualCard actor, AIOperationType type, AISelectedTargetInfoSet selectedTargetInfoSet)
	{
		Actor = actor;
		OriginalCard = actor;
		ActionType = type;
		ProcessCollection = new AISkillProcessInfoCollection();
		if (selectedTargetInfoSet == null)
		{
			SelectedTargets = new AISelectedTargetInfoSet();
		}
		else
		{
			SelectedTargets = selectedTargetInfoSet;
		}
		PreprocessRecorder = new AISimulationPreprocessRecorder();
		IsLatestAction = false;
		RealTargetInformationList = null;
	}

	public void SetActor(AIVirtualCard newActor)
	{
		Actor = newActor;
	}

	public AISelectedTargetInfo GetSituationTarget(AIScriptTokenArgType whichTarget)
	{
		switch (whichTarget)
		{
		case AIScriptTokenArgType.SELECTED_TARGET:
		case AIScriptTokenArgType.TARGET_SELECT:
			return SelectedTargets.Get(0);
		case AIScriptTokenArgType.SECOND_SELECTED_TARGET:
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			return SelectedTargets.Get(1);
		case AIScriptTokenArgType.CHOICED_TARGET:
			return SelectedTargets.GetChoiceInfo();
		default:
			return null;
		}
	}

	public AISelectedTargetInfo GetBurialRiteTarget()
	{
		if (SelectedTargets != null && SelectedTargets.PreprocessTarget != null && SelectedTargets.PreprocessTarget.Type == TargetSelectType.BurialRite)
		{
			return SelectedTargets.PreprocessTarget;
		}
		return null;
	}

	public AISelectedTargetInfo GetChoiceTarget()
	{
		if (SelectedTargets != null && SelectedTargets.HasChoiceTarget)
		{
			return SelectedTargets.ChoiceTarget;
		}
		return null;
	}

	public bool IsFirstTarget(AIVirtualCard card)
	{
		if (ActionTarget != null && ActionTarget.IsSameCard(card))
		{
			return true;
		}
		return false;
	}

	public bool IsSecondTarget(AIVirtualCard card)
	{
		if (SecondActionTarget != null && SecondActionTarget.IsSameCard(card))
		{
			return true;
		}
		return false;
	}

	public bool IsTargetExists(AIScriptTokenArgType whichTarget)
	{
		return whichTarget switch
		{
			AIScriptTokenArgType.TARGET_SELECT => SelectedTargets.IsTargetExist(0), 
			AIScriptTokenArgType.SECOND_TARGET_SELECT => SelectedTargets.IsTargetExist(1), 
			_ => false, 
		};
	}

	public void SetTarget(AISelectedTargetInfo info, AIScriptTokenArgType whichTarget)
	{
		switch (whichTarget)
		{
		case AIScriptTokenArgType.TARGET_SELECT:
			SelectedTargets.Set(info, 0);
			break;
		case AIScriptTokenArgType.SECOND_TARGET_SELECT:
			SelectedTargets.Set(info, 1);
			break;
		}
	}

	public void SetMultipleTargetsInInfo(List<AIVirtualCard> targets, TargetSelectType type, AIRemovalType removalType, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo info = new AISelectedTargetInfo(targets, type, removalType);
		SetTarget(info, whichTarget);
	}

	public void SetSingleTargetInInfo(AIVirtualCard target, TargetSelectType type, AIScriptTokenArgType whichTarget)
	{
		AISelectedTargetInfo info = ((target != null) ? new AISelectedTargetInfo(target, type) : null);
		SetTarget(info, whichTarget);
	}

	public void SetChoicedTargetInInfo(AIVirtualCard target)
	{
		AISelectedTargetInfo choiceTarget = ((target != null) ? new AISelectedTargetInfo(target, TargetSelectType.Choice) : null);
		SelectedTargets.SetChoiceTarget(choiceTarget);
	}

	public void SetChoicedMultipleTargetInInfo(List<AIVirtualCard> targets)
	{
		AISelectedTargetInfo choiceTarget = ((targets != null) ? new AISelectedTargetInfo(targets, TargetSelectType.Choice) : null);
		SelectedTargets.SetChoiceTarget(choiceTarget);
	}

	public void SetDiscardInfo(AIDiscardInfo info)
	{
		DiscardInfo = info;
	}

	public bool IsSameSituation(AISituationInfo situation)
	{
		if (ActionType != situation.ActionType || !Actor.IsSameCard(situation.Actor))
		{
			return false;
		}
		if (!SelectedTargets.IsDuplicate(situation.SelectedTargets))
		{
			return false;
		}
		return true;
	}

	public void SetExecutingSkillProcess(AISkillProcessInformation processInfo)
	{
		CurrentSkillProcessInfo = processInfo;
	}

	public AISkillProcessInformation RegisterNewProcessInfo(AIVirtualCard triggerCard, AISituationTriggerInformation.TriggerType triggerType)
	{
		AISkillProcessInformation aISkillProcessInformation = new AISkillProcessInformation(new AISituationTriggerInformation(triggerCard, triggerType));
		ProcessCollection.RegisterProcess(aISkillProcessInformation);
		return aISkillProcessInformation;
	}

	public AISkillProcessInformation RegisterNewPreprocessProcessInfo(AIVirtualCard triggerCard, AISituationTriggerInformation.TriggerType triggerType)
	{
		AISkillProcessInformation aISkillProcessInformation = new AISkillProcessInformation(new AISituationTriggerInformation(triggerCard, triggerType));
		ProcessCollection.RegisterPreprocessProcessInfo(aISkillProcessInformation);
		return aISkillProcessInformation;
	}

	public void ExecuteAllSkillProcess()
	{
		ProcessCollection.ExecuteAllProcess(this);
	}

	public void SetCurrentCheckCard(AIVirtualCard card)
	{
		CurrentCheckCard = card;
	}

	public void ResetCurrentCheckCard()
	{
		CurrentCheckCard = null;
	}

	public void RegisterRealTargetInfo(List<BattleCardRealTargetInformation> info)
	{
		RealTargetInformationList = info;
	}

	public bool IsRealSkillTarget(AIVirtualCard target, AIVirtualCard owner)
	{
		if (RealTargetInformationList == null || RealTargetInformationList.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < RealTargetInformationList.Count; i++)
		{
			BattleCardRealTargetInformation battleCardRealTargetInformation = RealTargetInformationList[i];
			if (owner.IsEqual(battleCardRealTargetInformation.SkillOwner) && battleCardRealTargetInformation.IsTarget(target))
			{
				return true;
			}
		}
		return false;
	}

	public AIVirtualCardRealTargetInformation DequeueRealTargetInfo(AIVirtualCard owner, AIVirtualField field)
	{
		if (RealTargetInformationList == null || RealTargetInformationList.Count <= 0)
		{
			return null;
		}
		AIVirtualField.AIVirtualFieldSearchCardOption searchOption = new AIVirtualField.AIVirtualFieldSearchCardOption
		{
			IsOutputCannotFindError = true,
			IsSearchFromDeck = true,
			IsSearchFromBeforeLatestActionDeck = true
		};
		BattleCardRealTargetInformation battleCardRealTargetInformation = null;
		for (int i = 0; i < RealTargetInformationList.Count; i++)
		{
			BattleCardRealTargetInformation battleCardRealTargetInformation2 = RealTargetInformationList[i];
			if (owner.IsEqual(battleCardRealTargetInformation2.SkillOwner))
			{
				battleCardRealTargetInformation = battleCardRealTargetInformation2;
				break;
			}
		}
		if (battleCardRealTargetInformation != null)
		{
			RealTargetInformationList.Remove(battleCardRealTargetInformation);
			return battleCardRealTargetInformation.CreateAIVirtualTargetInformation(field, owner, searchOption);
		}
		return null;
	}

	public List<int> GetTokenIdListFromDequeuedRealTargetInfo(AIVirtualCard owner, AITokenType tokenType)
	{
		if (RealTargetInformationList == null || RealTargetInformationList.Count <= 0)
		{
			return null;
		}
		for (int i = 0; i < RealTargetInformationList.Count; i++)
		{
			BattleCardRealTargetInformation battleCardRealTargetInformation = RealTargetInformationList[i];
			if (owner.IsEqual(battleCardRealTargetInformation.SkillOwner))
			{
				List<int> result = battleCardRealTargetInformation.DequeueFirstTargetInfoAndCreateTokenIdList(owner, tokenType);
				if (!battleCardRealTargetInformation.HasAnyTarget)
				{
					RealTargetInformationList.Remove(battleCardRealTargetInformation);
				}
				return result;
			}
		}
		return null;
	}

	public void SetLatestActionSimulationParameter()
	{
		IsLatestAction = true;
	}

	public void SetLatestActionSimulationParameterFromPreAction(AISituationInfo preAction)
	{
		IsLatestAction = preAction.IsLatestAction;
	}
}
