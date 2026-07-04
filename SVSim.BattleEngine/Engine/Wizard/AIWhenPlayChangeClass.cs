using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayChangeClass : AIWhenPlayTagArgument
{
	private CardBasePrm.ClanType _classType;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIWhenPlayChangeClass(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_classType = AIPlayTagInitializingUtility.CreateClassType(_exprList[_exprList.Count - 1]);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AIChangeClassSimulationUtility.ChangeClassAll(targetsFromField, _classType);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
				ExecuteTargetSelectChangeClass(targetsFromField, situation);
				break;
			}
		}
	}

	private void ExecuteTargetSelectChangeClass(List<AIVirtualCard> candidates, AISituationInfo situation)
	{
		if (situation != null && situation.IsTargetExists(base.SelectType))
		{
			AIChangeClassSimulationUtility.ChangeClassTarget(candidates, _classType, base.SelectType, situation);
		}
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT
		};
	}
}
