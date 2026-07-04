using System.Collections.Generic;

namespace Wizard;

public abstract class AISelectLogicArgumentBase
{
	protected List<AIPolishConvertedExpression> _argumentList;

	public virtual AIScriptTokenArgType LogicType { get; }

	public AISelectLogicArgumentBase(List<string> args)
	{
		InitializeArgument(args);
	}

	public virtual void InitializeArgument(List<string> args)
	{
		if (args != null && args.Count > 0)
		{
			_argumentList = new List<AIPolishConvertedExpression>();
			for (int i = 0; i < args.Count; i++)
			{
				_argumentList.Add(new AIPolishConvertedExpression(args[i]));
			}
		}
	}

	public void SetSelectTarget(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, AIScriptTokenArgType whichSelectType, List<int> playPtn, AISituationInfo situation)
	{
		if (candidates == null || candidates.Count <= 0)
		{
			return;
		}
		if (selectCount <= 0)
		{
			AIConsoleUtility.LogError($"AISelectLogicArgumentBase error!! selectCount == {selectCount}");
		}
		else if (selectCount == 1)
		{
			AIVirtualCard aIVirtualCard = SelectSingleTarget(candidates, tagOwner, field, playPtn, situation, AISelectTargetPattern.Best);
			if (aIVirtualCard != null)
			{
				situation.SetSingleTargetInInfo(aIVirtualCard, TargetSelectType.Default, whichSelectType);
			}
		}
		else
		{
			List<AIVirtualCard> list = SelectMultipleSelectedTargets(candidates, selectCount, tagOwner, field, playPtn, situation, AISelectTargetPattern.Best);
			if (list != null && list.Count > 0)
			{
				situation.SetMultipleTargetsInInfo(list, TargetSelectType.Default, AIRemovalType.None, whichSelectType);
			}
		}
	}

	protected void LogNotImplementMultipleSelect()
	{
		AIConsoleUtility.LogError($"{LogicType}: 複数ターゲット選択が未対応です。");
	}

	public abstract AIVirtualCard SelectSingleTarget(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest);

	public abstract List<AIVirtualCard> SelectMultipleSelectedTargets(List<AIVirtualCard> candidates, int selectCount, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation, AISelectTargetPattern worstOrBest);
}
