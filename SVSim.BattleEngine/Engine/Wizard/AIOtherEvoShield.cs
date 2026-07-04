using System.Collections.Generic;

namespace Wizard;

public class AIOtherEvoShield : AIOtherEvoTagArgument
{
	private AIScriptTokenArgType _stopTiming;

	private AIScriptTokenArgType _damageType;

	private bool _isDamageTypeDefinedByMaster;

	protected override int SELECT_TYPE_ARG_OFFSET => 1 + ((!_isDamageTypeDefinedByMaster) ? 1 : 2);

	public AIOtherEvoShield(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_stopTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - 1]);
		_damageType = AIPlayTagInitializingUtility.GetDamageTypeFromExprList(_exprList[_exprList.Count - 2], out _isDamageTypeDefinedByMaster);
		base.SelectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_ARG_OFFSET], base.LegalSelectTypes);
		InitializeFilters();
	}

	public override bool IsTargetGoingToDie(AIVirtualCard owner, AIVirtualCard candidate, AISituationInfo situation)
	{
		return false;
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			AIBarrierSimulationUtility.AddShieldToAll(targets, tagOwner, field, _damageType, _stopTiming);
		}
	}
}
