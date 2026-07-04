using System.Collections.Generic;

namespace Wizard;

public class AIChangeInplayCannotPlay : AIWhenChangeInplayTagArgument
{
	private List<AIScriptTokenBase> _cannotPlayCardFilterList;

	public AIChangeInplayCannotPlay(string text, bool isImmediate)
		: base(text, isImmediate)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		if (!AIPlayTagInitializingUtility.IsInitOfFilterSet(_exprList[0]))
		{
			AIConsoleUtility.LogError("AIChangeInplayCannotPlay.InitExpressions() Error!! Filters.First is not side arg!!!!!");
			return;
		}
		int num = -1;
		for (int i = 1; i < _exprList.Count; i++)
		{
			if (AIPlayTagInitializingUtility.IsInitOfFilterSet(_exprList[i]))
			{
				num = i;
				break;
			}
		}
		if (num <= 0)
		{
			_cannotPlayCardFilterList = GetFilters(_exprList.GetRange(0, _exprList.Count - NON_FILTER_FIRST_OFFSET));
			base.Filters = new List<AIScriptTokenBase>();
		}
		else
		{
			base.Filters = GetFilters(_exprList.GetRange(0, num));
			_cannotPlayCardFilterList = GetFilters(_exprList.GetRange(num, _exprList.Count - NON_FILTER_FIRST_OFFSET - num));
		}
	}

	protected override void ChangeInplayTagStartProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		AICannotPlayInformation info = new AICannotPlayInformation(tagOwner, _cannotPlayCardFilterList);
		field.AddCannotPlayInformation(info);
	}

	protected override void ChangeInplayTagStopProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		field.RemoveCannotPlayInformation(tagOwner, _cannotPlayCardFilterList);
	}
}
