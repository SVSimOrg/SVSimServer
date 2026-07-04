using System.Collections.Generic;

namespace Wizard;

public abstract class AIChangeInplayImmediateBarrierBase : AIWhenChangeInplayTagArgument
{
	protected AIScriptTokenArgType _stopTiming;

	protected AIScriptTokenArgType _damageType;

	protected bool _isDamageTypeDefinedByMaster;

	protected abstract int _defaultDamageTypeOffset { get; }

	protected abstract int _stopTimingOffset { get; }

	protected override int SELECT_TYPE_OFFSET => 1 + (_isDamageTypeDefinedByMaster ? _defaultDamageTypeOffset : _stopTimingOffset);

	public AIChangeInplayImmediateBarrierBase(string text)
		: base(text, isImmediate: true)
	{
	}

	protected override void InitExpressions(string text)
	{
		InitExprList(text);
		_stopTiming = AIPlayTagInitializingUtility.CreateSingleArgType(_exprList[_exprList.Count - _stopTimingOffset]);
		_damageType = AIPlayTagInitializingUtility.GetDamageTypeFromExprList(_exprList[_exprList.Count - _defaultDamageTypeOffset], out _isDamageTypeDefinedByMaster);
		InitSelectType();
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

	protected override void ChangeInplayTagStartProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			GiveBarrierToAllTargets(targets, tagOwner, field, playPtn, situation);
		}
	}

	protected override void ChangeInplayTagStopProcess(List<AIVirtualCard> targets, AIVirtualField field, AIVirtualCard tagOwner, List<int> playPtn, AISituationInfo situation)
	{
		if (base.SelectType == AIScriptTokenArgType.ALL_SELECT)
		{
			DepriveBarrierFromAllTargets(targets, tagOwner, field, playPtn, situation);
		}
	}

	protected abstract void GiveBarrierToAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation);

	protected abstract void DepriveBarrierFromAllTargets(List<AIVirtualCard> targets, AIVirtualCard tagOwner, AIVirtualField field, List<int> playPtn, AISituationInfo situation);
}
