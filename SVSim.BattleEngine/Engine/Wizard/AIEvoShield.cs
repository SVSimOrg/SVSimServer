using System.Collections.Generic;

namespace Wizard;

public class AIEvoShield : AIEvoTagArgument
{
	private List<AIScriptTokenArgType> _stopTimingList;

	private AIScriptTokenArgType _damageType;

	private bool _isDamageTypeDefinedByMaster;

	protected override int SELECT_TYPE_OFFSET => 1 + ((!_isDamageTypeDefinedByMaster) ? 1 : 2);

	public AIEvoShield(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_stopTimingList = AIPlayTagInitializingUtility.InitializeStopTimingList(_exprList[_exprList.Count - 1]);
		_damageType = AIPlayTagInitializingUtility.GetDamageTypeFromExprList(_exprList[_exprList.Count - 2], out _isDamageTypeDefinedByMaster);
		base.SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], base.LegalSelectTypes);
		InitializeFilter();
	}

	public override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.Filters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[2]
		{
			AIScriptTokenArgType.ALL_SELECT,
			AIScriptTokenArgType.TARGET_SELECT
		};
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		switch (base.SelectType)
		{
		case AIScriptTokenArgType.ALL_SELECT:
		{
			List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
			if (targetsFromField != null && targetsFromField.Count > 0)
			{
				AIBarrierSimulationUtility.AddMultipleStopTimingShieldToAll(targetsFromField, tagOwner, field, _damageType, _stopTimingList);
			}
			break;
		}
		case AIScriptTokenArgType.TARGET_SELECT:
			AIBarrierSimulationUtility.AddMultipleStopTimingShieldToTarget(situation, base.SelectType, tagOwner, field, _damageType, _stopTimingList);
			break;
		}
	}
}
