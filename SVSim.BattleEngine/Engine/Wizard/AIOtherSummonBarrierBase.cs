using System.Collections.Generic;

namespace Wizard;

public abstract class AIOtherSummonBarrierBase : AITriggerAndTargetFiltersTagBase
{
	private AIScriptTokenArgType _selectType;

	protected AIScriptTokenArgType _stopTiming;

	protected AIScriptTokenArgType _damageType;

	private bool _isDamageTypeDefinedByMaster;

	protected abstract int _defaultDamageTypeOffset { get; }

	protected abstract int _stopTimingOffset { get; }

	protected int SELECT_TYPE_OFFSET => 1 + (_isDamageTypeDefinedByMaster ? _defaultDamageTypeOffset : _stopTimingOffset);

	protected override int NON_FILTER_FIRST_OFFSET => SELECT_TYPE_OFFSET;

	public AIOtherSummonBarrierBase(string text)
		: base(text)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_stopTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - _stopTimingOffset]);
		_damageType = AIPlayTagInitializingUtility.GetDamageTypeFromExprList(_exprList[_exprList.Count - _defaultDamageTypeOffset], out _isDamageTypeDefinedByMaster);
		_selectType = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - SELECT_TYPE_OFFSET], base.LegalSelectTypes);
		InitializeFilters();
	}

	protected override void CreateLegalSelectTypes()
	{
		base.LegalSelectTypes = new AIScriptTokenArgType[1] { AIScriptTokenArgType.ALL_SELECT };
	}

	protected override List<AIVirtualCard> GetFilteredTargets(List<AIVirtualCard> candidates, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation, bool isBlockDead = true)
	{
		return AIFilteringUtility.FilteringForStatusEffectiveAbility(candidates, tagOwner, base.TargetFilters, playPtn, situation, isAttackEffective: false, isBlockDead);
	}

	protected override void RunTagMethod(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		base.RunTagMethod(targets, field, tagOwner, playPtn, situation);
		if (_selectType == AIScriptTokenArgType.ALL_SELECT)
		{
			GiveBarrierToAllTargets(targets, field, tagOwner, playPtn, situation);
		}
	}

	protected abstract void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation);
}
