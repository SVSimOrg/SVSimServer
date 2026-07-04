using System.Collections.Generic;

namespace Wizard;

public class AIWhenPlayChangeTribe : AIWhenPlayTagArgument
{
	private CardBasePrm.TribeType _tribeType = CardBasePrm.TribeType.MAX;

	protected override int SELECT_TYPE_OFFSET => 2;

	public AIWhenPlayChangeTribe(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		base.InitExpressions(text);
		_tribeType = AIPlayTagInitializingUtility.CreateTribeType(_exprList[_exprList.Count - 1]);
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
		if (targetsFromField != null && targetsFromField.Count > 0)
		{
			switch (base.SelectType)
			{
			case AIScriptTokenArgType.ALL_SELECT:
				AITribeSimulationUtility.ChangeTribeAll(targetsFromField, _tribeType);
				break;
			case AIScriptTokenArgType.TARGET_SELECT:
				ExecuteTargetSelect(situation);
				break;
			}
		}
	}

	private void ExecuteTargetSelect(AISituationInfo situation)
	{
		if (situation != null && situation.IsTargetExists(base.SelectType))
		{
			AITribeSimulationUtility.ChangeTribeTargetSelect(_tribeType, base.SelectType, situation);
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
