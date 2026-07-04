using System.Collections.Generic;

namespace Wizard;

public class AISelectedTargetInfo
{
	public TargetSelectType Type;

	public AIRemovalType RemovalType;

	public List<AIVirtualCard> Targets { get; private set; }

	public AIVirtualCard FirstTarget => Targets[0];

	public bool HasTarget
	{
		get
		{
			if (Targets != null)
			{
				return Targets.Count > 0;
			}
			return false;
		}
	}

	public AISelectedTargetInfo(TargetSelectType type, AIRemovalType removalType = AIRemovalType.None)
	{
		Type = type;
		RemovalType = removalType;
	}

	public AISelectedTargetInfo(AIVirtualCard target, TargetSelectType type, AIRemovalType removalType = AIRemovalType.None)
	{
		AddTarget(target);
		Type = type;
		RemovalType = removalType;
	}

	public AISelectedTargetInfo(List<AIVirtualCard> targets, TargetSelectType type, AIRemovalType removalType = AIRemovalType.None)
	{
		Type = type;
		RemovalType = removalType;
		for (int i = 0; i < targets.Count; i++)
		{
			AddTarget(targets[i]);
		}
	}

	public void AddTarget(AIVirtualCard target)
	{
		if (target == null)
		{
			AIConsoleUtility.LogError("AISelectedTargetInfo.AddTarget error!! Trying to add null into target list!!!!!");
		}
		else
		{
			Targets = AIParamQuery.AddElementToList(target, Targets);
		}
	}

	public bool ContainsTarget(AIVirtualCard card)
	{
		if (Targets == null || Targets.Count <= 0)
		{
			return false;
		}
		for (int i = 0; i < Targets.Count; i++)
		{
			if (Targets[i].IsSameCard(card))
			{
				return true;
			}
		}
		return false;
	}

	public AISelectedTargetInfo GetSimilarTargetInfo(AIVirtualField field)
	{
		AISelectedTargetInfo aISelectedTargetInfo = new AISelectedTargetInfo(Type, RemovalType);
		if (Targets == null || Targets.Count <= 0)
		{
			return aISelectedTargetInfo;
		}
		for (int i = 0; i < Targets.Count; i++)
		{
			AIVirtualCard aIVirtualCard = Targets[i];
			AIVirtualCard target = ((Type == TargetSelectType.Choice) ? aIVirtualCard : field.SearchVirtualCard(aIVirtualCard));
			aISelectedTargetInfo.AddTarget(target);
		}
		return aISelectedTargetInfo;
	}

	public bool IsDuplicate(AISelectedTargetInfo compare)
	{
		if (Type != compare.Type)
		{
			return false;
		}
		if (HasTarget)
		{
			if (!compare.HasTarget || compare.Targets.Count != Targets.Count)
			{
				return false;
			}
			for (int i = 0; i < Targets.Count; i++)
			{
				if (!Targets[i].IsSameCard(compare.Targets[i]))
				{
					return false;
				}
			}
		}
		else if (compare.HasTarget)
		{
			return false;
		}
		return true;
	}

	public bool IsSelectedSameTarget(AISelectedTargetInfo otherTargetInfo)
	{
		if (!HasTarget || otherTargetInfo == null || !otherTargetInfo.HasTarget)
		{
			return false;
		}
		for (int i = 0; i < Targets.Count; i++)
		{
			if (otherTargetInfo.ContainsTarget(Targets[i]))
			{
				return true;
			}
		}
		return false;
	}
}
