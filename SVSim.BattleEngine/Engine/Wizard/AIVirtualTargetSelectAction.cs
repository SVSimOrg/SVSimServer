namespace Wizard;

public class AIVirtualTargetSelectAction : AIVirtualActionInfo
{
	public bool forceLethalMode { get; set; }

	public bool IsChoiceBrave
	{
		get
		{
			if (base.ActionType == AIOperationType.PLAY && Data.CurrentFormat == Format.Avatar)
			{
				return CardMaster.IsChoiceBraveCardCheck(base.Actor.BaseId);
			}
			return false;
		}
	}

	public AIVirtualTargetSelectAction(AIVirtualCard sourceCard, AIVirtualCard original, AIOperationType operationType, AIVirtualCard target = null, AIVirtualCard secondTarget = null)
		: base(sourceCard, operationType, null)
	{
		base.OriginalCard = original;
		SelectedTargets = new AISelectedTargetInfoSet();
		if (target != null)
		{
			SelectedTargets.Set(new AISelectedTargetInfo(target, TargetSelectType.Default), 0);
		}
		if (secondTarget != null)
		{
			SelectedTargets.Set(new AISelectedTargetInfo(secondTarget, TargetSelectType.Default), 1);
		}
	}

	public AIVirtualTargetSelectAction(AIVirtualCard actor, AIVirtualCard original, AIOperationType operationType, AISelectedTargetInfoSet selectedTargetList)
		: base(actor, operationType, selectedTargetList)
	{
		base.OriginalCard = original;
	}

	public bool IsChoiceAndChangeActor(AIVirtualField field)
	{
		if (!AIChoiceTransformUtility.IsChoiceTransform(field, this))
		{
			return IsChoiceBrave;
		}
		return true;
	}

	public override ulong GetHash()
	{
		return base.GetHash() * 320041;
	}
}
