using System.Collections.Generic;

namespace Wizard;

public class AIBuffShield : AIFiltersAndSelectTypeArgument
{
	private List<AIScriptTokenArgType> _stopTimingList;

	private AIScriptTokenArgType _damageType;

	private bool _isDamageTypeDefinedByMaster;

	protected override int SELECT_TYPE_OFFSET => 1 + ((!_isDamageTypeDefinedByMaster) ? 1 : 2);

	public AIBuffShield(string text)
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
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	public override void Execute(AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation = null)
	{
		if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			List<AIVirtualCard> targetsFromField = GetTargetsFromField(tagOwner, field, playPtn, situation);
			if (targetsFromField != null && targetsFromField.Count > 0)
			{
				AIBarrierSimulationUtility.AddMultipleStopTimingShieldToAll(targetsFromField, tagOwner, field, _damageType, _stopTimingList);
			}
		}
	}
}
