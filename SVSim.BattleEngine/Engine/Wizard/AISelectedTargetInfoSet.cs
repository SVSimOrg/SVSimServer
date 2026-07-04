using System.Collections.Generic;

namespace Wizard;

public class AISelectedTargetInfoSet
{
	public static readonly int LENGTH = 2;

	private AISelectedTargetInfo[] _set;

	public AISelectedTargetInfo PreprocessTarget { get; private set; }

	public AISelectedTargetInfo ChoiceTarget { get; private set; }

	public bool HasChoiceTarget
	{
		get
		{
			if (ChoiceTarget != null)
			{
				return ChoiceTarget.HasTarget;
			}
			return false;
		}
	}

	public AISelectedTargetInfoSet()
	{
		_set = new AISelectedTargetInfo[LENGTH];
	}

	public AISelectedTargetInfoSet Clone()
	{
		return new AISelectedTargetInfoSet(this);
	}

	private AISelectedTargetInfoSet(AISelectedTargetInfoSet set)
	{
		_set = new AISelectedTargetInfo[LENGTH];
		for (int i = 0; i < LENGTH; i++)
		{
			AISelectedTargetInfo aISelectedTargetInfo = set.Get(i);
			_set[i] = aISelectedTargetInfo;
		}
		PreprocessTarget = set.PreprocessTarget;
		ChoiceTarget = set.ChoiceTarget;
	}

	public AISelectedTargetInfo Get(int index)
	{
		if (index < 0 || index > 1)
		{
			return null;
		}
		return _set[index];
	}

	public void Set(AISelectedTargetInfo info, int index)
	{
		if (index >= 0 && index <= 1)
		{
			_set[index] = info;
		}
	}

	public void SetPreprocessTarget(AISelectedTargetInfo info)
	{
		PreprocessTarget = info;
	}

	public void SetChoiceTarget(AISelectedTargetInfo info)
	{
		ChoiceTarget = info;
	}

	public bool IsTargetExist(int index)
	{
		if (index < 0 || index > 1)
		{
			return false;
		}
		return _set[index]?.HasTarget ?? false;
	}

	public bool IsAnyTargetExists()
	{
		if (PreprocessTarget != null && PreprocessTarget.HasTarget)
		{
			return true;
		}
		if (HasChoiceTarget)
		{
			return true;
		}
		AISelectedTargetInfo[] set = _set;
		foreach (AISelectedTargetInfo aISelectedTargetInfo in set)
		{
			if (aISelectedTargetInfo != null && aISelectedTargetInfo.HasTarget)
			{
				return true;
			}
		}
		return false;
	}

	public AISelectedTargetInfo GetChoiceInfo()
	{
		if (HasChoiceTarget)
		{
			return ChoiceTarget;
		}
		AISelectedTargetInfo[] set = _set;
		foreach (AISelectedTargetInfo aISelectedTargetInfo in set)
		{
			if (aISelectedTargetInfo != null && aISelectedTargetInfo.Type == TargetSelectType.Choice)
			{
				return aISelectedTargetInfo;
			}
		}
		return null;
	}

	public AISelectedTargetInfoSet GetSimilarTargetInfoSet(AIVirtualField field)
	{
		AISelectedTargetInfoSet aISelectedTargetInfoSet = new AISelectedTargetInfoSet();
		if (PreprocessTarget != null)
		{
			AISelectedTargetInfo similarTargetInfo = PreprocessTarget.GetSimilarTargetInfo(field);
			aISelectedTargetInfoSet.SetPreprocessTarget(similarTargetInfo);
		}
		if (ChoiceTarget != null)
		{
			AISelectedTargetInfo similarTargetInfo2 = ChoiceTarget.GetSimilarTargetInfo(field);
			aISelectedTargetInfoSet.SetChoiceTarget(similarTargetInfo2);
		}
		for (int i = 0; i < _set.Length; i++)
		{
			AISelectedTargetInfo aISelectedTargetInfo = _set[i];
			AISelectedTargetInfo info = null;
			if (aISelectedTargetInfo != null)
			{
				info = aISelectedTargetInfo.GetSimilarTargetInfo(field);
			}
			aISelectedTargetInfoSet.Set(info, i);
		}
		return aISelectedTargetInfoSet;
	}

	public bool IsDuplicate(AISelectedTargetInfoSet compare)
	{
		if (compare == null)
		{
			return false;
		}
		if (PreprocessTarget == null)
		{
			if (compare.PreprocessTarget != null)
			{
				return false;
			}
		}
		else if (!PreprocessTarget.IsDuplicate(compare.PreprocessTarget))
		{
			return false;
		}
		if (ChoiceTarget == null)
		{
			if (compare.ChoiceTarget != null)
			{
				return false;
			}
		}
		else if (!ChoiceTarget.IsDuplicate(compare.ChoiceTarget))
		{
			return false;
		}
		for (int i = 0; i < LENGTH; i++)
		{
			AISelectedTargetInfo aISelectedTargetInfo = _set[i];
			AISelectedTargetInfo aISelectedTargetInfo2 = compare.Get(i);
			if (aISelectedTargetInfo == null)
			{
				if (aISelectedTargetInfo2 != null)
				{
					return false;
				}
			}
			else if (!aISelectedTargetInfo.IsDuplicate(aISelectedTargetInfo2))
			{
				return false;
			}
		}
		return true;
	}

	public void UpdateRemovalType(AIScriptTokenArgType whichTarget, AIRemovalType removalType)
	{
		if (whichTarget != AIScriptTokenArgType.TARGET_SELECT && whichTarget != AIScriptTokenArgType.SECOND_TARGET_SELECT)
		{
			AIConsoleUtility.LogError($"whichTarget=={whichTarget} is invalid!!");
			return;
		}
		int num = ((whichTarget != AIScriptTokenArgType.TARGET_SELECT) ? 1 : 0);
		AISelectedTargetInfo aISelectedTargetInfo = _set[num];
		if (aISelectedTargetInfo != null)
		{
			aISelectedTargetInfo.RemovalType = removalType;
		}
	}
}
