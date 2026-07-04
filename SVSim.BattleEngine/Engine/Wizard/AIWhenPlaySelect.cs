using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlaySelect : AIWhenPlayTagArgument
{
	private AISelectLogicArgumentBase _selectLogicArg;

	protected override bool _isSelectCountImplemented => true;

	public override bool IsImmediate => true;

	protected override int SELECT_COUNT_OFFSET => 1;

	public AIWhenPlaySelect(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		List<string> list = AIPlayTagInitializingUtility.SplitTagText(text);
		if (list.Count == 0)
		{
			AIConsoleUtility.LogError($"{GetType()} のargが不足しています。/n{text}");
			base.InitExpressions(text);
		}
		else
		{
			base.InitExpressions(list[0]);
			_selectLogicArg = AISelectLogicSimulationUtility.CreateSelectLogicArgument(list[1]);
		}
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (situation != null && situation.IsTargetExists(base.SelectType))
		{
			return;
		}
		if (_selectLogicArg == null)
		{
			AIConsoleUtility.LogError($"{GetType()} error!! _selectLogicArg is null");
			return;
		}
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			int selectCount = GetSelectCount(tagOwner, field, playPtn, situation);
			_selectLogicArg.SetSelectTarget(targetsFromField, selectCount, tagOwner, field, base.SelectType, playPtn, situation);
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.TARGET_SELECT,
			AIScriptTokenArgType.SECOND_TARGET_SELECT
		};
	}
}
